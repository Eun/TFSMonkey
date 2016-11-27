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
	[ValueConversion(typeof(string), typeof(bool))]
	public sealed class StringToBooleanConverter : IValueConverter
	{
		public bool TrueValue { get; set; }
		public bool FalseValue { get; set; }


		public StringToBooleanConverter()
		{
			// set defaults
			TrueValue = true;
			FalseValue = false;
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
