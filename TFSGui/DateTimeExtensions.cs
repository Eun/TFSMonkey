using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSMonkey
{
	internal static class DateTimeExtensions
	{
		public static string ToRelativeDateString(this DateTime date)
		{
			return GetRelativeDateValue(date, DateTime.Now);
		}
		public static string ToRelativeDateStringUtc(this DateTime date)
		{
			return GetRelativeDateValue(date, DateTime.UtcNow);
		}
		private static string GetRelativeDateValue(DateTime date, DateTime comparedTo)
		{
			TimeSpan diff = comparedTo.Subtract(date);
			if (diff.TotalDays >= 365)
				return date.ToString("d MMMM yyyy");
			else if (diff.TotalDays >= 7)
				return date.ToString("d MMMM");
			else if (diff.TotalDays > 1)
				return string.Format("{0:N0} days ago", diff.TotalDays);
			else if (diff.TotalDays == 1)
				return "yesterday";
			else if (diff.TotalHours >= 2)
				return string.Format("{0:N0} hours ago", diff.TotalHours);
			else if (diff.TotalHours >= 1)
				return "1 hour ago";
			else if (diff.TotalMinutes >= 5)
				return string.Format("{0:N0} minutes ago", diff.TotalMinutes);
			if (diff.TotalMinutes >= 1)
				return "a few minutes ago";
			else
				return "less than a minute ago";
		}

	}
}
