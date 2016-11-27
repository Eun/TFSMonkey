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
	[ValueConversion(typeof(string), typeof(Visibility))]
	public sealed class StringToVisibilityConverter : IValueConverter
	{
		public Visibility TrueValue { get; set; }
		public Visibility FalseValue { get; set; }


		public StringToVisibilityConverter()
		{
			// set defaults
			TrueValue = Visibility.Visible;
			FalseValue = Visibility.Collapsed;
		}

		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (!(value is string))
				return FalseValue;
			return string.IsNullOrEmpty((string) value) ? FalseValue : TrueValue;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
