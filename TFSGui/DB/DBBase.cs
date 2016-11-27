using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace TFSMonkey.DB
{
	public abstract class DBBase
	{

		public abstract int Version { get; }

		protected SqliteConnection SqliteConnection { get; }
		public abstract IEnumerable<HistoryItem> GetHistoryItems();
		public abstract void SaveHistoryItems(SqliteTransaction transaction, IEnumerable<HistoryItem> items);
		public abstract IEnumerable<WorkItem> GetWorkItems(Settings settings);
		public abstract void SaveWorkItems(SqliteTransaction transaction, IEnumerable<WorkItem> items);
		public abstract Settings GetSettings();
		public abstract void SaveSettings(SqliteTransaction transaction, Settings settings);


		public abstract IEnumerable<string> GetServers();

		public abstract void SaveServer(string server);

		protected DBBase(SqliteConnection sqliteConnection)
		{
			SqliteConnection = sqliteConnection;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sqliteConnection"></param>
		/// <param name="version">Pass 0 for the current version, Pass -1 to detect from sqliteconnection</param>
		/// <returns></returns>
		public static DBBase GetDBInstance(SqliteConnection sqliteConnection, int version = -1)
		{
			if (version == -1)
				version = GetVersion(sqliteConnection);
			switch (version)
			{
				case 1:
					return new DB1(sqliteConnection);
				default:
					return new DB2(sqliteConnection);
			}
		}


		private static int GetVersion(SqliteConnection sqliteConnection)
		{
			try
			{
				using (var cmd = new SqliteCommand("SELECT `Version` FROM Version WHERE `Id` = 0", sqliteConnection))
				{
					using (var rdr = cmd.ExecuteReader())
					{
						if (rdr.Read())
						{
							return rdr.GetInt32(0);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				return 0;
			}
			return 0;
		}

		public void SaveVersionInfo(SqliteTransaction transaction = null)
		{
			new SqliteCommand("CREATE TABLE IF NOT EXISTS Version (`Id` INTEGER PRIMARY KEY, `Version` INTEGER)", SqliteConnection, transaction).ExecuteNonQuery();
			new SqliteCommand($"INSERT OR REPLACE INTO Version (`Id`, `Version`) VALUES (0, \"{Version}\")", SqliteConnection, transaction).ExecuteNonQuery();
		}

		
	}
}
