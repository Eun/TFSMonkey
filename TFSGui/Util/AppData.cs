using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSMonkey.Util
{
	class AppData
	{
		public static string GetAppPath()
		{
			var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrEmpty(appData))
				return null;
			appData = System.IO.Path.Combine(appData, "TFSMonkey");
			if (!System.IO.Directory.Exists(appData))
			{
				if (!System.IO.Directory.CreateDirectory(appData).Exists)
				{
					return null;
				}
			}
			return appData;
		}

		public static string SanitizePath(string path)
		{
			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

			foreach (char c in invalid)
			{
				path = path.Replace(c.ToString(), "");
			}
			return path;
		}

	}
}
