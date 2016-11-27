using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TFSMonkey
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		static App()
		{
			/*AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
				Debug.WriteLine($"Loading { new AssemblyName(args.Name).Name}.dll");
				using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"TFSMonkey.{new AssemblyName(args.Name).Name}.dll"))
				{
					
					var assemblyData = new byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}
			};*/
		}
	}
}
