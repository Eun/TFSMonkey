using System.Windows;
using System.Windows.Navigation;

namespace TFSMonkey.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			DataContext = new MainViewModel();
			InitializeComponent();
		}

		private void Frame_OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			if (e.NavigationMode != NavigationMode.New)
			{
				e.Cancel = true;
			}
		}
	}
}
