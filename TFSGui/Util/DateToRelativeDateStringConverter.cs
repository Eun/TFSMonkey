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
	[ValueConversion(typeof(DateTime), typeof(string))]
	public sealed class DateToRelativeDateStringConverter : IValueConverter
	{

		public DateToRelativeDateStringConverter()
		{
		}

		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (!(value is DateTime))
				return "";
			return ((DateTime)value).ToRelativeDateString();
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
