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
	/// Interaction logic for ConnectPage.xaml
	/// </summary>
	public partial class ConnectPage
	{
		public ConnectPage()
		{
			DataContext = new ConnectViewModel();
			InitializeComponent();
		}
	}
}
