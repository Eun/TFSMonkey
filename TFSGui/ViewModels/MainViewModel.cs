using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public class MainViewModel : INotifyPropertyChanged
	{
		#region Properties
		private TFS Tfs { get; set; }
		private TeamProject TeamProject { get; set; }
		public XmlLanguage Language { get; set; } = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
		public string Text => "";
		public string Title { get; set; } = "TFSMonkey";
		private string appPath;
		private SqliteConnection SqliteConnection { get; set; }
		public Uri Page { get; private set; }

		public DataSource DataSource { get; private set; }

		public Settings Settings { get; private set; }
		public event ExitEventHandler Exit;

		public static MainViewModel Instance { get; private set; }
		

		#endregion

		#region Constructor
		public MainViewModel()
		{
			Instance = this;

			ShowConnectPage();
			// Load Servers
			appPath = Util.AppData.GetAppPath();
			if (string.IsNullOrEmpty(appPath))
			{
				MessageBox.Show("Could not create data directory", "Problem", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
				return;
			}

			Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
			{
				Application.Current.Exit += (sender, args) =>
				{
					Exit?.Invoke(sender,args);
					if (DataSource != null)
					{
						DataSource.CancellationTokenSource.Cancel();
						if (DataSource.IsWorking)
							DataSource.SaveDataToCache();
					}
					if (SqliteConnection != null)
						Settings.SaveSettings(SqliteConnection, Settings);
					Application.Current.Shutdown(0);
				};
			}));

		}
		#endregion


		public static void ShowConnectPage()
		{
			Instance.Page = new Uri("/Views/ConnectPage.xaml", UriKind.Relative);
			Instance.RaisePropertyChanged(nameof(Page));
		}

		public void ShowProject(TFS tfs, TeamProject teamProject)
		{
			Tfs = tfs;
			TeamProject = teamProject;

			Title = teamProject.Name + "- TFSMonkey";
			RaisePropertyChanged(nameof(Title));

			SqliteConnection = new SqliteConnection("Data Source=" + System.IO.Path.Combine(appPath, $"{AppData.SanitizePath(Tfs.Url)}-{AppData.SanitizePath(TeamProject.ServerItem)}.sqlite"));
			
			Settings = Settings.LoadSettings(SqliteConnection);

			DataSource = new DataSource(SqliteConnection, tfs, TeamProject, Settings);

			Page = new Uri("/Views/CombinedPage.xaml", UriKind.Relative);
			RaisePropertyChanged(nameof(Page));

			Task.Factory.StartNew(() =>
			{
				DataSource.GetData();
				new Timer(state =>
				{
					DataSource.RefreshData();
				}).Change(1000*60*10, 1000*60*10);
			});

		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		
	}
}