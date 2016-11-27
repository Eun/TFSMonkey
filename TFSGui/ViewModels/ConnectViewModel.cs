using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using TFSCore;
using TFSMonkey.Annotations;
using TFSMonkey.Util;

namespace TFSMonkey
{
	public class ConnectViewModel : INotifyPropertyChanged
	{
		#region Properties
		public TFS Tfs { get; set; }
		public TeamProject TeamProject { get; set; }

		public XmlLanguage Language { get; set; } = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);

		public string Text => "";
		public string Title { get; set; } = "TFSMonkey";


		public ICollectionView HistoryItems { get; private set; }

		public ICollectionView WorkItems { get; private set; }

		public bool IsProjectSelected { get; set; }

		#region IsWorking Property
		public bool IsWorking => IsServersWorking || IsProjectsWorking;

		private bool _isServersWorking;
		public bool IsServersWorking
		{
			get { return _isServersWorking; }
			set
			{
				_isServersWorking = value;
				RaisePropertyChanged(nameof(IsWorking));
				RaisePropertyChanged(nameof(IsServersWorking));
			}
		}

		private bool _isProjectsWorking;
		public bool IsProjectsWorking
		{
			get { return _isProjectsWorking; }
			set
			{
				_isProjectsWorking = value;
				RaisePropertyChanged(nameof(IsWorking));
				RaisePropertyChanged(nameof(IsProjectsWorking));
			}
		}
		#endregion


		private string appPath;

		#endregion

		#region Commands
		public ICommand ServerChanged { get; set; }
		public ICommand ProjectChanged { get; set; }
		public ICommand ProjectSelected { get; set; }
		public ICommand ShutdownCommand { get; } = new RelayCommand((param) =>
		{
			Application.Current.Shutdown();
		});

		#endregion




		private string _selectedServer;

		public string SelectedServer
		{
			get { return _selectedServer; }
			set
			{
				_selectedServer = value;

				RaisePropertyChanged(nameof(SelectedServer));
				serverChanged(value);
			}
		}

		public IEnumerable<TeamProject> Projects { get; set; } = new TeamProject[0];

		public List<string> Servers { get; set; } = new List<string>();


		public ConnectViewModel()
		{
			ServerChanged = new RelayCommand(serverChanged);
			ProjectChanged = new RelayCommand(projectChanged);
			ProjectSelected = new RelayCommand(projectSelected);

			// Load Servers
			appPath = Util.AppData.GetAppPath();
			if (string.IsNullOrEmpty(appPath))
			{
				MessageBox.Show("Could not create data directory", "Problem", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown();
				return;
			}

			new Task(() =>
			{
				LoadServers();
			}).Start();
		}


		#region Commands
		private void serverChanged(object param)
		{
			if (IsServersWorking || IsProjectsWorking)
				return;
			var url = param as string;
			if (url != null)
			{
				Uri result;
				if (Uri.TryCreate(url, UriKind.Absolute, out result))
				{
					new Task(() =>
					{
						LoadProjects(result);
					}).Start();
				}
			}
		}

		private void projectChanged(object param)
		{
			if (IsProjectsWorking)
				return;
			TeamProject = (param as TeamProject);
			IsProjectSelected = true;
			RaisePropertyChanged(nameof(IsProjectSelected));
		}

		private void projectSelected(object param)
		{
			if (TeamProject != null)
			{
				MainViewModel.Instance.ShowProject(Tfs, TeamProject);
			}
			else
			{
				MessageBox.Show("Invalid Project selected!", "", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		#endregion



		private void LoadServers()
		{
			IsServersWorking = true;
			using (var sqliteConnection = new SqliteConnection("Data Source=" + System.IO.Path.Combine(appPath, "servers.sqlite")))
			{
				
				sqliteConnection.Open();
				var db = DB.DBBase.GetDBInstance(sqliteConnection);
				Servers.AddRange(db.GetServers());
				sqliteConnection.Close();
			}
			IsServersWorking = false;

			RaisePropertyChanged(nameof(Servers));
			SelectedServer = Servers.FirstOrDefault();

			// load tfs urls from regeistry
			new Task(() =>
			{
				var registryServers = new List<string>();
				try
				{
					var root = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\Instances");

					if (root == null)
						return;
					foreach (var instance in root.GetSubKeyNames())
					{
						var key = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\Instances\{instance}\Collections");
						if (key == null)
							continue;
						foreach (var item in key.GetSubKeyNames())
						{
							var itemKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\VisualStudio\14.0\TeamFoundation\Instances\{instance}\Collections\{item}");
							if (itemKey == null)
								continue;
							var uri = itemKey.GetValue("Uri", null) as string;
							if (uri == null)
								continue;
							Uri result;
							if (Uri.TryCreate(uri, UriKind.Absolute, out result))
							{
								registryServers.Add(result.AbsoluteUri.TrimEnd('/'));
							}
						}
					}
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}

				bool addedServer = false;
				foreach (var server in registryServers)
				{
					if (Servers.FirstOrDefault(x => string.Compare(x, server, StringComparison.InvariantCultureIgnoreCase) == 0) == null)
					{
						Servers.Add(server);
						addedServer = true;
					}
				}
				if (addedServer)
				{ 
					RaisePropertyChanged(nameof(Servers));
					if (SelectedServer == null)
					{
						SelectedServer = Servers.FirstOrDefault();
					}
				}
			}).Start();

		}

		private void LoadProjects(Uri uri)
		{
			IsProjectsWorking = true;
			try
			{
				Tfs = new TFS(uri);
				Projects = Tfs.GetTeamProjects();

				if (!Servers.Select(server => server.ToLowerInvariant()).ToList().Contains(uri.AbsoluteUri.ToLowerInvariant()))
				{
					Servers.Add(uri.AbsoluteUri.TrimEnd('/'));
					RaisePropertyChanged(nameof(Servers));		
				}
				using (var sqliteConnection = new SqliteConnection("Data Source=" + System.IO.Path.Combine(appPath, "servers.sqlite")))
				{
					sqliteConnection.Open();
					var db = DB.DBBase.GetDBInstance(sqliteConnection, 0);
					db.SaveServer(uri.AbsoluteUri.TrimEnd('/'));
					sqliteConnection.Close();
				}
				RaisePropertyChanged(nameof(Projects));
			}
			catch (Exception e)
			{
				Projects = new List<TeamProject>();
				RaisePropertyChanged(nameof(Projects));
				MessageBox.Show(e.Message, "TFSMonkey", MessageBoxButton.OK, MessageBoxImage.Error);
				Tfs = null;
			}
			IsProjectsWorking = false;

		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		
	}
}