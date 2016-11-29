using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TFSMonkey.Views
{
	/// <summary>
	/// Interaction logic for CombinedPage.xaml
	/// </summary>
	public partial class CombinedPage : Page
	{
		public CombinedPage()
		{
			DataContext = new CombinedViewModel();
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
