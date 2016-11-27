using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TFSMonkey.Util
{
	[ValueConversion(typeof(string), typeof(string))]
	public sealed class RemoveNewLinesConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (!(value is string))
				return "";
			return ((string) value).Replace('\n', ' ').Replace('\r', ' ');
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
