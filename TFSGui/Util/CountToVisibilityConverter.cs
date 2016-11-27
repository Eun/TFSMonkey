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
	[ValueConversion(typeof(ICollection), typeof(Visibility))]
	public sealed class CountToVisibilityConverter : IValueConverter
	{
		public Visibility TrueValue { get; set; }
		public Visibility FalseValue { get; set; }


		public CountToVisibilityConverter()
		{
			// set defaults
			TrueValue = Visibility.Visible;
			FalseValue = Visibility.Collapsed;
		}

		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (value == null)
				return FalseValue;
			if (!(value is ICollection))
				return null;
			return ((ICollection)value).Count > 0 ? TrueValue : FalseValue;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
