using System;
using System.Collections.Generic;
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
using FontAwesome.WPF;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using TFSCore;
using TFSMonkey.Annotations;
using TFSMonkey.Util;

namespace TFSMonkey
{
	public class WorkItemsViewModel : INotifyPropertyChanged
	{
		#region Properties
		public XmlLanguage Language { get; set; } = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
		public string Text => "";
		public ICollectionView WorkItems { get; private set; }
		#region Filter Properties
		private ICollection<string> _workItemSearchTerms = new List<string>();
		private string _workItemSearchTerm;

		public string WorkItemSearchTerm
		{
			get { return _workItemSearchTerm; }
			set
			{
				_workItemSearchTerm = value;
				_workItemSearchTerms = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				RaisePropertyChanged(nameof(WorkItemSearchTerm));
				UpdateDetails();
				WorkItems?.Refresh();
			}
		}
		#endregion

		public bool IsWorking => MainViewModel.Instance.DataSource.IsWorking;

		public int[] ColumnWidths => MainViewModel.Instance.Settings.WorkItemColumnWidths;

		public IEnumerable<NameColorIcon> States => MainViewModel.Instance.Settings.States;
		public IEnumerable<NameColorIcon> WorkItemTypes => MainViewModel.Instance.Settings.WorkItemTypes;

		#endregion

		#region Commands
		public ICommand TextBoxTextChanged { get; } = new RelayCommand((param) =>
		{
			(param as TextBox)?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
		});

		public ICommand RefreshData { get; set; }

		#endregion

		

		#region Construtor
		public WorkItemsViewModel()
		{
			WorkItems = CollectionViewSource.GetDefaultView(MainViewModel.Instance.DataSource.WorkItems);
			WorkItems.SortDescriptions.Add(new SortDescription("ChangedDate", ListSortDirection.Descending));
			WorkItems.Filter = WorkItemFilter;

			MainViewModel.Instance.DataSource.IsWorkingChanged += (sender, args) => RaisePropertyChanged(nameof(IsWorking));
			MainViewModel.Instance.DataSource.WorkItems.CollectionChanged += (sender, args) =>
			{
				UpdateDetails();
				RaisePropertyChanged(nameof(WorkItems));
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
		private bool WorkItemFilter(object item)
		{

			if (_workItemSearchTerms.Count == 0)
				return true;
			var workItem = (item as WorkItem);
			if (workItem == null)
				return true;

			return (_workItemSearchTerms.All(x =>
				workItem.Id.ToString().IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1 ||
				workItem.Title.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1 ||
				workItem.AssignedTo.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1 ||
				workItem.State.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1 ||
				workItem.WorkItemType.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1
			));
		}
		#endregion
		
		private void UpdateDetails()
		{
			
			foreach (var state in States)
			{
				int count = 0;
				foreach (IWorkItem workItem in WorkItems)
				{
					if (string.Compare(workItem.State, state.Name, StringComparison.InvariantCultureIgnoreCase) == 0 && WorkItemFilter(workItem))
					{
						++count;
					}
				}
				state.Count = count;
			}

			foreach (var state in WorkItemTypes)
			{
				int count = 0;
				foreach (IWorkItem workItem in WorkItems)
				{
					if (string.Compare(workItem.WorkItemType, state.Name, StringComparison.InvariantCultureIgnoreCase) == 0 && WorkItemFilter(workItem))
					{
						++count;
					}
				}
				state.Count = count;
			}
			RaisePropertyChanged(nameof(States));
			RaisePropertyChanged(nameof(WorkItemTypes));
		}
		
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}