using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Microsoft.Data.Sqlite;

namespace TFSMonkey
{
	public class Settings
	{
		public List<NameColorIcon> States;

		public List<NameColorIcon> WorkItemTypes;
		public int[] HistoryColumnWidths { get; set; }
		public int[] WorkItemColumnWidths { get; set; }


		public static Settings LoadSettings(SqliteConnection sqliteConnection)
		{
			lock (sqliteConnection)
			{
				sqliteConnection.Open();
				Debug.WriteLine("Getting Settings from DB");
				var db = DB.DBBase.GetDBInstance(sqliteConnection);
				var settings = db.GetSettings();
				Debug.WriteLine($"Got Settings from db");
				sqliteConnection.Close();
				return settings;
			}
		}

		public static void SaveSettings(SqliteConnection sqliteConnection, Settings settings)
		{
			Debug.WriteLine("Saving settings");
			lock (sqliteConnection)
			{
				sqliteConnection.Open();

				using (var transaction = sqliteConnection.BeginTransaction())
				{
					var db = DB.DBBase.GetDBInstance(sqliteConnection, 0);
					db.SaveSettings(transaction, settings);
					transaction.Commit();
				}
				sqliteConnection.Close();
			}
		}


		public Settings()
		{
			States = new List<NameColorIcon>()
			{
				new NameColorIcon("Resolved", Color.FromArgb(0xff, 0x8b, 0xc3, 0x4a), "check"),
				new NameColorIcon("Active", Color.FromArgb(0xff, 0xff, 0xc1, 0x07), "cogs"),
				new NameColorIcon("Closed", Color.FromArgb(0xff, 0xf4, 0x43, 0x36), "times"),
			};

			WorkItemTypes = new List<NameColorIcon>()
			{
				new NameColorIcon("Bug", Color.FromArgb(0xff, 0x00, 0x96, 0x88), "bug"),
				new NameColorIcon("Scenario", Color.FromArgb(0xff, 0x79, 0x55, 0x48), "rocket"),
				new NameColorIcon("Task", Color.FromArgb(0xff, 0x21, 0x96, 0xf3), "file-text-o"),
			};

			HistoryColumnWidths = new int[]
			{
				60,	  // ChangeSetId
				160,  // CommitterDisplayName
				160,  // CreationDate
				500,  // Comment
				500,  // Workitems
			};

			WorkItemColumnWidths = new int[]
			{
				0,     // State
				60,    // ID
				160,  // AssignedTo
				160,  // ChangedDate
				0,    // WorkitemType
				500,  // Title
				60,   //State
				100,   // WorkitemType
			};
		}


		public Color GetStateColor(string stateName)
		{
			foreach (var state in States)
			{
				if (string.Compare(stateName, state.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					return state.Color;
				}
			}
			return Color.FromArgb(0,0,0,0);
		}

		public string GetStateIcon(string stateName)
		{
			foreach (var state in States)
			{
				if (string.Compare(stateName, state.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					return state.Icon;
				}
			}
			return "";
		}

		public Color GetWorkItemTypeColor(string workItemTypeName)
		{
			foreach (var workItemType in WorkItemTypes)
			{
				if (string.Compare(workItemTypeName, workItemType.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					return workItemType.Color;
				}
			}
			return Color.FromArgb(0, 0, 0, 0);
		}

		public string GetWorkItemTypeIcon(string workItemTypeName)
		{
			foreach (var workItemType in WorkItemTypes)
			{
				if (string.Compare(workItemTypeName, workItemType.Name, StringComparison.CurrentCultureIgnoreCase) == 0)
				{
					return workItemType.Icon;
				}
			}
			return "";
		}

		public static Dictionary<string, string> ToDictonary(Settings settings)
		{
			var dict = new Dictionary<string, string>();
			dict.Add("StateNames1", string.Join("|",settings.States.Select(x=> x.Name)));
			dict.Add("StateColors1", string.Join("|", settings.States.Select(x => $"#{x.Color.A:X2}{x.Color.R:X2}{x.Color.G:X2}{x.Color.B:X2}")));
			dict.Add("StateIcons1", string.Join("|", settings.States.Select(x => x.Icon)));
			dict.Add("WorkItemTypeNames1", string.Join("|", settings.WorkItemTypes.Select(x => x.Name)));
			dict.Add("WorkItemTypeColors1", string.Join("|", settings.WorkItemTypes.Select(x => $"#{x.Color.A:X2}{x.Color.R:X2}{x.Color.G:X2}{x.Color.B:X2}")));
			dict.Add("WorkItemTypeIcons1", string.Join("|", settings.WorkItemTypes.Select(x => x.Icon)));
			dict.Add("HistoryColumnWidths1", string.Join("|", settings.HistoryColumnWidths.Select(x => x.ToString())));
			dict.Add("WorkItemColumnWidths1", string.Join("|", settings.WorkItemColumnWidths.Select(x => x.ToString())));
			return dict;
		}

		public static Settings FromDictonary(Dictionary<string, string> dict)
		{
			var settings = new Settings();

			string str1, str2, str3;
			if (dict.TryGetValue("StateNames1", out str1) && (dict.TryGetValue("StateColors1", out str2)) && (dict.TryGetValue("StateIcons1", out str3)))
			{
				var names = str1.Split(new[] {'|'}, StringSplitOptions.RemoveEmptyEntries);
				var colors = str2.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				var icons = str3.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if (names.Length == colors.Length && names.Length == icons.Length)
				{
					settings.States.Clear();
					for (int i = 0; i < names.Length; i++)
					{
						var color = ColorConverter.ConvertFromString(colors[i]);
						if (color is Color)
						{
							settings.States.Add(new NameColorIcon(names[i], (Color)color, icons[i]));
						}
					}
				}
			}

			if (dict.TryGetValue("WorkItemTypeNames1", out str1) && (dict.TryGetValue("WorkItemTypeColors1", out str2)) && (dict.TryGetValue("WorkItemTypeIcons1", out str3)))
			{
				var names = str1.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				var colors = str2.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				var icons = str3.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				if (names.Length == colors.Length && names.Length == icons.Length)
				{
					settings.WorkItemTypes.Clear();
					for (int i = 0; i < names.Length; i++)
					{
						var color = ColorConverter.ConvertFromString(colors[i]);
						if (color is Color)
						{
							settings.WorkItemTypes.Add(new NameColorIcon(names[i], (Color)color, icons[i]));
						}
					}
				}
			}

			if (dict.TryGetValue("HistoryColumnWidths1", out str1))
			{
				var widths = str1.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < widths.Length && i < 5; i++)
				{
					int width;
					if (Int32.TryParse(widths[i], out width))
					{
						settings.HistoryColumnWidths[i] = width;
					}
				}
			}

			if (dict.TryGetValue("WorkItemColumnWidths1", out str1))
			{
				var widths = str1.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < widths.Length && i < 7; i++)
				{
					int width;
					if (Int32.TryParse(widths[i], out width))
					{
						settings.WorkItemColumnWidths[i] = width;
					}
				}
			}

			return settings;
		}
	}
}