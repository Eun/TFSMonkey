using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace TFSMonkey
{
	public class ObservableCollectionEx<T> : ObservableCollection<T>
	{
		public CancellationToken CancellationToken { get; set; }

		public Dispatcher Dispatcher { get; set; } = Application.Current.Dispatcher;
		
		public ObservableCollectionEx(CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
		}

		public void AddRangeWithoutNotification(IEnumerable<T> items)
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(new Action(() => AddRange(items)));
				return;
			}
			foreach (var i in items)
				Items.Add(i);
		}

		public void AddRange(IEnumerable<T> items)
		{
			AddRangeWithoutNotification(items);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}


		

		public T AddWithoutNotification(T item)
		{
			if (!Dispatcher.CheckAccess())
			{
				return Dispatcher.Invoke(() => AddWithoutNotification(item));
			}
			Items.Add(item);
			return item;
		}



		public T AddOrUpdateWithoutNotification(T item, Func<T, T, bool> compareFunc, Action<IList<T>, int, T> updateFunc)
		{
			int updatedIndex;
			T oldItem;
			return AddOrUpdateWithoutNotification(item, compareFunc, updateFunc, out updatedIndex, out oldItem);
		}

		public T AddOrUpdateWithoutNotification(T item, Func<T, T, bool> compareFunc, Action<IList<T>, int, T> updateFunc, out int updatedIndex, out T oldItem)
		{
			for (int i = 0, l = Items.Count; i < l; i++)
			{ 
				if (compareFunc(item, Items[i]))
				{
					oldItem = Items[i];
					updatedIndex = i;
					if (!Dispatcher.CheckAccess())
						Dispatcher.Invoke(() => { updateFunc(Items, i, item); });
					else
						updateFunc(Items, i, item);
					return Items[i];
				}
			}
			oldItem = default(T);
			updatedIndex = -1;
			if (!Dispatcher.CheckAccess())
				Dispatcher.Invoke(new Action(() => { Items.Add(item); }));
			else
				Items.Add(item);
			return item;
		}

		public T AddOrUpdate(T item, Func<T, T, bool> compareFunc, Action<IList<T>, int, T> updateFunc)
		{
			int updatedIndex;
			T oldItem;
			return AddOrUpdate(item, compareFunc, updateFunc, out updatedIndex, out oldItem);
		}

		public T AddOrUpdate(T item, Func<T, T, bool> compareFunc, Action<IList<T>, int, T> updateFunc, out int updatedIndex, out T oldItem)
		{
			var newItem = AddOrUpdateWithoutNotification(item, compareFunc, updateFunc, out updatedIndex, out oldItem);
			if (updatedIndex > -1)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, updatedIndex));
			}
			else
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
			}
			return newItem;
		}

		public void NotifyReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public void NotifyItemChanged(T item)
		{
			var index = Items.IndexOf(item);
			if (index > -1)
			{
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item, index));
			}
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (!Dispatcher.CheckAccess())
				Dispatcher.Invoke(new Action(() => base.OnCollectionChanged(e)));
			else
				base.OnCollectionChanged(e);
		}

		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (!Dispatcher.CheckAccess())
				Dispatcher.Invoke(new Action(() => base.OnPropertyChanged(e)));
			else
				base.OnPropertyChanged(e);
		}


	}
}