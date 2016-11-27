using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;

namespace TFSMonkey.DB
{
	public class DB1 : DBBase
	{

		public override int Version => 1;

		public override IEnumerable<HistoryItem> GetHistoryItems()
		{
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `ChangeSetId`, `Committer`, `CommitterDisplayName`, `Comment`, `CreationDate`, `Owner`, `OwnerDisplayName` FROM HistoryItems{Version} ORDER BY `ChangeSetId` DESC", SqliteConnection))
				{
					using (var rdr = cmd.ExecuteReader())
					{
						var items = new List<HistoryItem>();
						while (rdr.Read())
						{
							items.Add(new HistoryItem
							{
								ChangeSetId = rdr.GetInt32(0),
								Committer = rdr.GetString(1),
								CommitterDisplayName = rdr.GetString(2),
								Comment = rdr.GetString(3),
								CreationDate = DateTimeOffset.FromUnixTimeSeconds(rdr.GetInt32(4)).LocalDateTime,
								Owner = rdr.GetString(5),
								OwnerDisplayName = rdr.GetString(6)
							});
						}
						return items;
					}
				}
			}
			catch
			{
				return new HistoryItem[0];
			}
		}

		public override void SaveHistoryItems(SqliteTransaction transaction, IEnumerable<HistoryItem> items)
		{
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS HistoryItems{Version} (`ChangeSetId` INTEGER PRIMARY KEY, `Committer` TEXT, `CommitterDisplayName` TEXT, `Comment` TEXT, `CreationDate` INTEGER, `Owner` TEXT, `OwnerDisplayName` TEXT)", SqliteConnection, transaction).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO HistoryItems{Version} (`ChangeSetId`, `Committer`, `CommitterDisplayName`, `Comment`, `CreationDate`, `Owner`, `OwnerDisplayName`) VALUES (@ChangeSetId, @Committer, @CommitterDisplayName, @Comment, @CreationDate, @Owner, @OwnerDisplayName)", SqliteConnection, transaction))
			{
				foreach (HistoryItem item in items)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@ChangeSetId", item.ChangeSetId);
					cmd.Parameters.AddWithValue("@Committer", item.Committer);
					cmd.Parameters.AddWithValue("@CommitterDisplayName", item.CommitterDisplayName);
					cmd.Parameters.AddWithValue("@Comment", item.Comment);
					cmd.Parameters.AddWithValue("@CreationDate", new DateTimeOffset(item.CreationDate).ToUnixTimeSeconds());
					cmd.Parameters.AddWithValue("@Owner", item.Owner);
					cmd.Parameters.AddWithValue("@OwnerDisplayName", item.OwnerDisplayName);
					cmd.ExecuteNonQuery();
				}
			}
			SaveVersionInfo(transaction);
		}

		public override IEnumerable<WorkItem> GetWorkItems(Settings settings)
		{
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `Id`, `Title`, `State`, `AssignedTo`, `WorkItemType`, `ChangedDate` FROM WorkItems{Version} Order By `Id` DESC", SqliteConnection))
				{
					using (var rdr = cmd.ExecuteReader())
					{
						var items = new List<WorkItem>();
						while (rdr.Read())
						{
							items.Add(new WorkItem
							{
								Id = rdr.GetInt32(0),
								Title = rdr.GetString(1),
								State = rdr.GetString(2),
								AssignedTo = rdr.GetString(3),
								WorkItemType = rdr.GetString(4),
								ChangedDate = DateTimeOffset.FromUnixTimeSeconds(rdr.GetInt32(5)).LocalDateTime,
								Settings = settings
							});


						}
						Debug.WriteLine($"Got {items.Count} WorkItems from Cache");
						return items;
					}
				}
			}
			catch
			{
				return new WorkItem[0];
			}
		}

		public override void SaveWorkItems(SqliteTransaction transaction, IEnumerable<WorkItem> items)
		{
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS WorkItems{Version} (`Id` INTEGER PRIMARY KEY, `Title` TEXT, `State` TEXT, `AssignedTo` TEXT, `WorkItemType` TEXT, `ChangedDate` INTEGER)", SqliteConnection, transaction).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO WorkItems{Version} (`Id`, `Title`, `State`, `AssignedTo`, `WorkItemType`, `ChangedDate`) VALUES (@Id, @Title, @State, @AssignedTo, @WorkItemType, @ChangedDate)", SqliteConnection, transaction))
			{
				foreach (WorkItem item in items)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@Id", item.Id);
					cmd.Parameters.AddWithValue("@Title", item.Title);
					cmd.Parameters.AddWithValue("@State", item.State);
					cmd.Parameters.AddWithValue("@AssignedTo", item.AssignedTo);
					cmd.Parameters.AddWithValue("@WorkItemType", item.WorkItemType);
					cmd.Parameters.AddWithValue("@ChangedDate", new DateTimeOffset(item.ChangedDate).ToUnixTimeSeconds());
					cmd.ExecuteNonQuery();
				}
			}
			SaveVersionInfo(transaction);
		}

		public override Settings GetSettings()
		{
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `Setting`, `Value` FROM Settings{Version}", SqliteConnection))
				{
					using (var rdr = cmd.ExecuteReader())
					{
						var dict = new Dictionary<string, string>();
						while (rdr.Read())
						{
							dict.Add(rdr.GetString(0), rdr.GetString(1));
						}
						return Settings.FromDictonary(dict);
					}
				}
			}
			catch
			{
				return new Settings();
			}
		}

		public override void SaveSettings(SqliteTransaction transaction, Settings settings)
		{
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS Settings{Version} (`Setting` TEXT PRIMARY KEY, `Value` TEXT)", SqliteConnection, transaction).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO Settings{Version} (`Setting`, `Value`) VALUES (@Setting, @Value)", SqliteConnection, transaction))
			{
				var dict = Settings.ToDictonary(settings);
				foreach (var entry in dict)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.AddWithValue("@Setting", entry.Key);
					cmd.Parameters.AddWithValue("@Value", entry.Value);
					cmd.ExecuteNonQuery();
				}
			}
			SaveVersionInfo(transaction);
		}

		public override IEnumerable<string> GetServers()
		{
			var items = new List<string>();
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `Url` FROM Servers{Version}", SqliteConnection))
				{
					using (var rdr = cmd.ExecuteReader())
					{
						while (rdr.Read())
						{
							items.Add(rdr.GetString(0));
						}
					}
				}
			}
			catch
			{

			}
			return items;
		}

		public override void SaveServer(string server)
		{
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS Servers{Version} (`Url` TEXT PRIMARY KEY)", SqliteConnection).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO Servers{Version} (`Url`) VALUES (@Url)", SqliteConnection))
			{
				cmd.Prepare();
				cmd.Parameters.AddWithValue("@Url", server);
				cmd.ExecuteNonQuery();
			}
			SaveVersionInfo();
		}


		

		public DB1(SqliteConnection sqliteConnection) : base(sqliteConnection)
		{
		}
	}
}
