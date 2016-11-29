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
	public class CombinedViewModel
	{
		public XmlLanguage Language { get; set; } = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
		public string Text => "";

		public string Name => Assembly.GetExecutingAssembly().GetName().Name;
		public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
	}
}