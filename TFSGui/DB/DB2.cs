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
	public class DB2 : DB1
	{

		public override int Version => 2;

		public override IEnumerable<HistoryItem> GetHistoryItems()
		{
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `ChangeSetId`, `Committer`, `CommitterDisplayName`, `Comment`, `CreationDate`, `Owner`, `OwnerDisplayName`, `WorkItems`, `IndexedWords` FROM HistoryItems{Version} ORDER BY `ChangeSetId` DESC", SqliteConnection))
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
								OwnerDisplayName = rdr.GetString(6),
								CachedWorkItemIds = rdr.GetString(7).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => Int32.Parse(x)).ToArray(),
								IndexedWords = rdr.GetString(8)
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
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS HistoryItems{Version} (`ChangeSetId` INTEGER PRIMARY KEY, `Committer` TEXT, `CommitterDisplayName` TEXT, `Comment` TEXT, `CreationDate` INTEGER, `Owner` TEXT, `OwnerDisplayName` TEXT, `WorkItems` TEXT, `IndexedWords` TEXT)", SqliteConnection, transaction).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO HistoryItems{Version} (`ChangeSetId`, `Committer`, `CommitterDisplayName`, `Comment`, `CreationDate`, `Owner`, `OwnerDisplayName`, `WorkItems`, `IndexedWords`) VALUES (@ChangeSetId, @Committer, @CommitterDisplayName, @Comment, @CreationDate, @Owner, @OwnerDisplayName, @WorkItems, @IndexedWords)", SqliteConnection, transaction))
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
					cmd.Parameters.AddWithValue("@WorkItems", string.Join("|", item.CachedWorkItemIds.Select(x=> x)));
					cmd.Parameters.AddWithValue("@IndexedWords", item.IndexedWords);
					cmd.ExecuteNonQuery();
				}
			}
			SaveVersionInfo(transaction);
		}


		public override IEnumerable<WorkItem> GetWorkItems(Settings settings)
		{
			try
			{
				using (var cmd = new SqliteCommand($"SELECT `Id`, `Title`, `State`, `AssignedTo`, `WorkItemType`, `ChangedDate`, `IndexedWords` FROM WorkItems{Version} Order By `Id` DESC", SqliteConnection))
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
								IndexedWords = rdr.GetString(6),
								Settings = settings,
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
			new SqliteCommand($"CREATE TABLE IF NOT EXISTS WorkItems{Version} (`Id` INTEGER PRIMARY KEY, `Title` TEXT, `State` TEXT, `AssignedTo` TEXT, `WorkItemType` TEXT, `ChangedDate` INTEGER, `IndexedWords` TEXT)", SqliteConnection, transaction).ExecuteNonQuery();
			using (var cmd = new SqliteCommand($"INSERT OR REPLACE INTO WorkItems{Version} (`Id`, `Title`, `State`, `AssignedTo`, `WorkItemType`, `ChangedDate`, `IndexedWords`) VALUES (@Id, @Title, @State, @AssignedTo, @WorkItemType, @ChangedDate, @IndexedWords)", SqliteConnection, transaction))
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
					cmd.Parameters.AddWithValue("@IndexedWords", item.IndexedWords);
					cmd.ExecuteNonQuery();
				}
			}
			SaveVersionInfo(transaction);
		}

		public DB2(SqliteConnection sqliteConnection) : base(sqliteConnection)
		{
		}
	}
}
