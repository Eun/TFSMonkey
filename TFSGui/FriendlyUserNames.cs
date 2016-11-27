using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSMonkey
{
	class FriendlyUserNames
	{
		private static ConcurrentDictionary<string, string> friendlyAssignedNames = new ConcurrentDictionary<string, string>();

		public static string Resolve(string name)
		{
			string resolvedName;
			if (friendlyAssignedNames.TryGetValue(name, out resolvedName))
			{
				return resolvedName;
			}

			resolvedName = name;

			if (name.Contains(@"\") && System.Environment.UserDomainName?.Length > 0)
			{
				

				var domainAndUser = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
				if (domainAndUser.Length == 2)
				{

					var context = new DirectoryContext(DirectoryContextType.Domain, domainAndUser.First().Replace("\\", ""));
					using (var root = Domain.GetDomain(context).GetDirectoryEntry())
					{
						using (var ds = new DirectorySearcher(root, $"(&(objectclass=user)(sAMAccountName={domainAndUser.Last().Replace("\\", "")}))"))
						{
							var src = ds.FindAll();
							foreach (SearchResult sr in src)
							{
								if (sr.Properties.Contains("name"))
								{
									resolvedName = sr.Properties["name"][0] as string;
									break;
								}
							}
						}
					}
				}
			}

			friendlyAssignedNames.TryAdd(name, resolvedName);
			return resolvedName;
		}
	}
}
