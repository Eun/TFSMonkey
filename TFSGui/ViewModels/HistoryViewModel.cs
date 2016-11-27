using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using TFSCore;
using TFSMonkey.Annotations;
using TFSMonkey.Util;

namespace TFSMonkey
{
	public class HistoryViewModel : INotifyPropertyChanged
	{
		#region Properties
		public XmlLanguage Language { get; set; } = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
		public string Text => "";
		
		public ICollectionView HistoryItems { get; private set; }
		#region Filter Properties
		private ICollection<string> _historySearchTerms = new List<string>();
		private string _historySearchTerm;
		public string HistorySearchTerm
		{
			get { return _historySearchTerm; }
			set
			{
				_historySearchTerm = value;
				_historySearchTerms = value.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				RaisePropertyChanged(nameof(HistorySearchTerm));
				HistoryItems?.Refresh();
			}
		}
		#endregion

		public bool IsWorking => MainViewModel.Instance.DataSource.IsWorking;

		public int[] ColumnWidths => MainViewModel.Instance.Settings.HistoryColumnWidths;

		#endregion

		#region Commands
		public ICommand TextBoxTextChanged { get; } = new RelayCommand((param) =>
		{
			(param as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
		});

		public ICommand RefreshData { get; set; }

		#endregion

		#region Construtor
		public HistoryViewModel()
		{
			
			HistoryItems = CollectionViewSource.GetDefaultView(MainViewModel.Instance.DataSource.HistoryItems);
			HistoryItems.SortDescriptions.Add(new SortDescription("ChangeSetId", ListSortDirection.Descending));
			HistoryItems.Filter = HistoryFilter;

			MainViewModel.Instance.DataSource.IsWorkingChanged += (sender, args) => RaisePropertyChanged(nameof(IsWorking));
			MainViewModel.Instance.DataSource.HistoryItems.CollectionChanged += (sender, args) => {
				RaisePropertyChanged(nameof(HistoryItems));
			};

			RefreshData = new RelayCommand(refreshData);
		}

		#endregion

		#region Commands
		private void refreshData(object param)
		{
			new Task(MainViewModel.Instance.DataSource.RefreshData).Start();
		}
		#endregion
		
		#region Filters
		private bool HistoryFilter(object item)
		{

			if (_historySearchTerms.Count == 0)
				return true;
			var historyItem = (item as HistoryItem);
			if (historyItem == null)
				return true;

			return _historySearchTerms.All(x => historyItem.IndexedWords.Contains(x));

		}
		#endregion


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}		
	}
}