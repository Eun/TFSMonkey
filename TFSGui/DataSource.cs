using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using TFSCore;


namespace TFSMonkey
{
	public class DataSource
	{
		public SqliteConnection SqliteConnection { get; set; }
		public TFS Tfs { get; set; }
		public TeamProject TeamProject { get; set; }
		public Settings Settings { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

		public event EventHandler<IsWorkingEventArgs> IsWorkingChanged;

		public ObservableCollectionEx<HistoryItem> HistoryItems { get; private set; }

		public ObservableCollectionEx<WorkItem> WorkItems { get; private set; }

		private HistoryItemFactory HistoryItemFactory { get; set; }
		private WorkItemFactory WorkItemFactory { get; set; }

		private const int InitialItemSize = 500;


		private bool _isWorking;
		public bool IsWorking {
			get { return _isWorking; }
			private set
			{
				if (_isWorking != value)
				{
					_isWorking = value;
					IsWorkingChanged?.Invoke(this, new IsWorkingEventArgs(value));
				}

			}
		}

		private DB.DBBase DBLoader { get; set; }
		private DB.DBBase DBSaver { get; set; }

		public DataSource(SqliteConnection sqliteConnection, TFS tfs, TeamProject teamProject, Settings settings)
		{
			SqliteConnection = sqliteConnection;
			Tfs = tfs;
			TeamProject = teamProject;
			Settings = settings;
			HistoryItemFactory = new HistoryItemFactory();
			WorkItemFactory = new WorkItemFactory(settings);
			HistoryItems = new ObservableCollectionEx<HistoryItem>(CancellationTokenSource.Token);
			WorkItems = new ObservableCollectionEx<WorkItem>(CancellationTokenSource.Token);
			Tfs.HistoryItemFactory = HistoryItemFactory;
			Tfs.WorkItemFactory = WorkItemFactory;

			DBLoader = DB.DBBase.GetDBInstance(sqliteConnection);
			DBSaver = DB.DBBase.GetDBInstance(sqliteConnection, 0);
		}

		
		/// <summary>
		/// 
		/// </summary>
		
		public void GetData()
		{
			GetData(false, false);
		}

		private void GetData(bool skipCache, bool skipTFS)
		{
			if (IsWorking)
				return;
			IsWorking = true;

			int? lastId = null;

			if (!skipCache)
			{
				lock (SqliteConnection)
				{
					SqliteConnection.Open();
					Debug.WriteLine("Getting HistoryItems from Cache");
					var historyItems = DBLoader.GetHistoryItems();
					if (historyItems.Any())
					{
						HistoryItems.AddRangeWithoutNotification(historyItems);
						/*foreach (var item in historyItems)
						{
							item.GotWorkItems += ItemOnGotWorkItems;
						}*/

					}
					Debug.WriteLine($"Got {historyItems.Count()} HistoryItems from Cache");

					if (historyItems.Any()) lastId = historyItems.Max(x => x.ChangeSetId);


					Debug.WriteLine("Getting WorkItems from Cache");
					var workItems = DBLoader.GetWorkItems(Settings);
					if (workItems.Any())
					{
						WorkItems.AddRangeWithoutNotification(workItems);
					}

					Debug.WriteLine($"Got {workItems.Count()} WorkItems from Cache");
					SqliteConnection.Close();

					// Resolve CachedWorkItems in HistoryItems
					foreach (var item in HistoryItems)
					{
						var resolvedWorkItems = new List<WorkItem>();
						foreach (int id in item.CachedWorkItemIds)
						{
							var resolvedWorkItem = WorkItems.FirstOrDefault(x => x.Id == id);
							if (resolvedWorkItem != null)
							{
								resolvedWorkItems.Add(resolvedWorkItem);
							}
						}
						item.CachedWorkItems = resolvedWorkItems.ToArray();
						item.CachedWorkItemIds = resolvedWorkItems.Select(x => x.Id).ToArray();
					}
					HistoryItems.NotifyReset();
					WorkItems.NotifyReset();
				}
			}
			if (!skipTFS)
			{
				GetDataFromTfs(lastId, lastId.HasValue ? Int32.MaxValue : InitialItemSize);
			}
			IsWorking = false;
		}

		private void GetDataFromTfs(int? lastId, int itemSize, bool isFullCall = false)
		{
			IEnumerable<TFSCore.IHistoryItem> historyItems;

			Debug.WriteLine($"Getting HistoryItems from TFS {lastId} {itemSize}");
			historyItems = Tfs.GetHistory(TeamProject.ServerItem, lastId, itemSize);

			IList<TFSCore.IHistoryItem> modifiedHistoryItems = new List<IHistoryItem>();

			foreach (HistoryItem historyItem in historyItems)
			{
				if (CancellationTokenSource.IsCancellationRequested)
				{
					return;
				}


				modifiedHistoryItems.Add(HistoryItems.AddOrUpdateWithoutNotification(historyItem, (item, item1) => item.ChangeSetId == item1.ChangeSetId, (items, index, newItem) =>
				{
					// make sure we keep our "ExtendedProperties"
					IHistoryItem.CopyTo(items[index], newItem);
				}));
			}
			HistoryItems.NotifyReset();

			Debug.WriteLine($"Got {historyItems.Count()} HistoryItems from TFS");

			SaveDataToCache(true, false);

			Debug.WriteLine($"Resolving Workitems for {historyItems.Count()}");
			foreach (HistoryItem historyItem in modifiedHistoryItems)
			{
				if (CancellationTokenSource.IsCancellationRequested)
					return;
				foreach (WorkItem workItem in historyItem.WorkItems)
				{
					if (CancellationTokenSource.IsCancellationRequested)
						return;
					Debug.WriteLine($"Resolved Workitems for {historyItem.ChangeSetId}");
					workItem.UpdateIndexedWords(true);
					WorkItems.AddOrUpdate(workItem, (item, item1) => item.Id == item1.Id, (items, index, newItem) => items[index] = newItem);
				}
				// Updating Cache
				historyItem.UpdateIndexedWords(true);
			}

			SaveDataToCache(true, true);
			if (!isFullCall)
				GetDataFromTfs(null, Int32.MaxValue, true);
		}

		private bool RefreshScheduled = false;

		public void RefreshData()
		{
			if (RefreshScheduled)
			{
				return;
			}
			bool workedBefore = false;
			while (IsWorking)
			{
				RefreshScheduled = true;
				Debug.WriteLine("Waiting for cancelation");
				CancellationTokenSource.Cancel();
				Thread.Sleep(100);
				workedBefore = true;
				
			}
			IsWorking = true;
			RefreshScheduled = false;
			if (HistoryItems.Any() && !workedBefore)
				GetDataFromTfs(HistoryItems.Max(x => x.ChangeSetId), Int32.MaxValue);
			else
				GetDataFromTfs(null, Int32.MaxValue, true);
			IsWorking = false;
			
		}

		public void SaveDataToCache()
		{
			SaveDataToCache(true, true);
		}

		private void SaveDataToCache(bool saveHistoryItems, bool saveWorkitems)
		{
			Debug.WriteLine("Saving Data");
			lock (SqliteConnection)
			{
				SqliteConnection.Open();
				using (var transaction = SqliteConnection.BeginTransaction())
				{
					if (saveHistoryItems)
						DBSaver.SaveHistoryItems(transaction, HistoryItems);
					if (saveWorkitems)
						DBSaver.SaveWorkItems(transaction, WorkItems);
					transaction.Commit();
				}
				SqliteConnection.Close();
			}
			if (saveHistoryItems)
				Debug.WriteLine($"Saved {HistoryItems.Count} HistoryItems");
			if (saveWorkitems)
				Debug.WriteLine($"Saved {WorkItems.Count} WorkItems");
		}
	}
}

public class IsWorkingEventArgs : EventArgs
	{
		public IsWorkingEventArgs(bool isWorking)
		{
			this.IsWorking = isWorking;
		}

		public bool IsWorking { get; private set; }
	}
