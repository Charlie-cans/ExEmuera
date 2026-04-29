using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameData
{
	internal static class SqliteManager
	{
		private const string SQLITE_DLL = "sqlite3";
		private const int SQLITE_OK = 0;
		private const int SQLITE_ROW = 100;
		private const int SQLITE_DONE = 101;
		private const int SQLITE_NULL = 5;
		private const int SQLITE_OPEN_READWRITE = 2;
		private const int SQLITE_OPEN_CREATE = 4;
		private const int SQLITE_OPEN_MEMORY = 128;
		private static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);

		// .NET Standard 2.0 没有 PtrToStringUTF8 / LPUTF8Str 方法。使用 byte[] 作为输入，手动解码输出。
		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_open_v2(byte[] filename, out IntPtr db, int flags, byte[] zVfs);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_close_v2(IntPtr db);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_prepare_v2(IntPtr db, byte[] sql, int nByte, out IntPtr stmt, out IntPtr tail);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_step(IntPtr stmt);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_finalize(IntPtr stmt);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_bind_text(IntPtr stmt, int index, byte[] text, int nByte, IntPtr destructor);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr sqlite3_column_text(IntPtr stmt, int iCol);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern long sqlite3_column_int64(IntPtr stmt, int iCol);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_column_count(IntPtr stmt);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_column_type(IntPtr stmt, int iCol);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr sqlite3_errmsg(IntPtr db);

		[DllImport(SQLITE_DLL, CallingConvention = CallingConvention.Cdecl)]
		private static extern int sqlite3_exec(IntPtr db, byte[] sql, IntPtr callback, IntPtr arg, out IntPtr errmsg);

		private static Dictionary<string, IntPtr> databases = new Dictionary<string, IntPtr>();

		private static byte[] U8(string s) => s != null ? Encoding.UTF8.GetBytes(s + '\0') : null;
		private static string P8(IntPtr p) { if (p == IntPtr.Zero) return ""; int len = 0; while (Marshal.ReadByte(p, len) != 0) len++; var buf = new byte[len]; Marshal.Copy(p, buf, 0, len); return Encoding.UTF8.GetString(buf); }
		private static string P8n(IntPtr p) { if (p == IntPtr.Zero) return null; int len = 0; while (Marshal.ReadByte(p, len) != 0) len++; var buf = new byte[len]; Marshal.Copy(p, buf, 0, len); return Encoding.UTF8.GetString(buf); }

		private static void CheckError(int rc, IntPtr db, string context)
		{
			if (rc != SQLITE_OK && rc != SQLITE_ROW && rc != SQLITE_DONE)
			{
				string msg = P8(sqlite3_errmsg(db)) ?? "unknown error";
				throw new CodeEE($"SQLite error in {context}: {msg} (code {rc})");
			}
		}

		public static void Connect(string dbName)
		{
			if (databases.ContainsKey(dbName))
				Disconnect(dbName);
			// 持久化文件数据库，路径为 <游戏目录>/dat/<dbName>.db
			string dir = Path.Combine(_Library.Sys.ExeDir, "dat");
			Directory.CreateDirectory(dir);
			string dbPath = Path.Combine(dir, dbName + ".db");
			int rc = sqlite3_open_v2(U8(dbPath), out IntPtr db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, null);
			if (rc != SQLITE_OK)
				throw new CodeEE($"SQL_CONNECT '{dbName}' failed to open {dbPath} (code {rc})");
			sqlite3_exec(db, U8("PRAGMA journal_mode=OFF"), IntPtr.Zero, IntPtr.Zero, out _);
			databases[dbName] = db;
		}

		public static void Disconnect(string dbName)
		{
			if (databases.TryGetValue(dbName, out var db))
			{
				sqlite3_close_v2(db);
				databases.Remove(dbName);
			}
		}

		public static void ExecuteNonQuery(string dbName, string sql)
		{
			var db = GetDb(dbName);
			int rc = sqlite3_exec(db, U8(sql), IntPtr.Zero, IntPtr.Zero, out IntPtr errmsg);
			if (rc != SQLITE_OK)
			{
				string msg = P8(errmsg);
				throw new CodeEE($"SQL error: {msg}");
			}
		}

		public static long ExecuteScalarLong(string dbName, string sql)
		{
			var db = GetDb(dbName);
			int rc = sqlite3_prepare_v2(db, U8(sql), -1, out IntPtr stmt, out _);
			CheckError(rc, db, "ExecuteScalarLong");
			try
			{
				rc = sqlite3_step(stmt);
				if (rc == SQLITE_ROW && sqlite3_column_count(stmt) > 0)
					return sqlite3_column_int64(stmt, 0);
				return 0;
			}
			finally { sqlite3_finalize(stmt); }
		}

		public static string ExecuteScalarString(string dbName, string sql)
		{
			var db = GetDb(dbName);
			int rc = sqlite3_prepare_v2(db, U8(sql), -1, out IntPtr stmt, out _);
			CheckError(rc, db, "ExecuteScalarString");
			try
			{
				rc = sqlite3_step(stmt);
				if (rc == SQLITE_ROW && sqlite3_column_count(stmt) > 0)
				{
					if (sqlite3_column_type(stmt, 0) == SQLITE_NULL)
						return "";
					return P8(sqlite3_column_text(stmt, 0));
				}
				return "";
			}
			finally { sqlite3_finalize(stmt); }
		}

		public static void ExecuteReader(string dbName, string sql, string outVarName)
		{
			var db = GetDb(dbName);
			int rc = sqlite3_prepare_v2(db, U8(sql), -1, out IntPtr stmt, out _);
			CheckError(rc, db, "ExecuteReader");
			try
			{
				var resultsArray = GlobalStatic.VEvaluator.RESULTS_ARRAY;
				int idx = 0;
				while ((rc = sqlite3_step(stmt)) == SQLITE_ROW)
				{
					int colCount = sqlite3_column_count(stmt);
					for (int col = 0; col < colCount; col++)
					{
						if (idx >= resultsArray.Length) break;
						resultsArray[idx++] = sqlite3_column_type(stmt, col) == SQLITE_NULL ? "" : P8(sqlite3_column_text(stmt, col));
					}
					if (idx >= resultsArray.Length) break;
				}
			}
			finally { sqlite3_finalize(stmt); }
		}

		public static void ImportMapXml(string dbName, string tableName, string xmlPath)
		{
			var db = GetDb(dbName);

			string fullPath = xmlPath;
			if (!Path.IsPathRooted(xmlPath))
			{
				string gameRoot = Path.GetDirectoryName(Program.CsvDir.TrimEnd('/', '\\'));
				fullPath = Path.Combine(gameRoot, xmlPath);
			}
			if (!File.Exists(fullPath))
				return;

			sqlite3_exec(db, U8($"CREATE TABLE IF NOT EXISTS {tableName} (k TEXT PRIMARY KEY, v TEXT)"), IntPtr.Zero, IntPtr.Zero, out _);
			sqlite3_exec(db, U8("BEGIN"), IntPtr.Zero, IntPtr.Zero, out _);

			try
			{
				string xml = File.ReadAllText(fullPath, Encoding.UTF8);
				int pos = 0;
				while (true)
				{
					int pStart = xml.IndexOf("<p>", pos, StringComparison.Ordinal);
					if (pStart < 0) break;
					int pEnd = xml.IndexOf("</p>", pStart + 3, StringComparison.Ordinal);
					if (pEnd < 0) break;
					string pContent = xml.Substring(pStart + 3, pEnd - pStart - 3);

					int kStart = pContent.IndexOf("<k>", StringComparison.Ordinal);
					int kEnd = pContent.IndexOf("</k>", StringComparison.Ordinal);
					int vStart = pContent.IndexOf("<v>", StringComparison.Ordinal);
					int vEnd = pContent.IndexOf("</v>", StringComparison.Ordinal);

					if (kStart >= 0 && kEnd > kStart)
					{
						string key = pContent.Substring(kStart + 3, kEnd - kStart - 3);
						string value = (vStart >= 0 && vEnd > vStart) ? pContent.Substring(vStart + 3, vEnd - vStart - 3) : "";
						key = key.Replace("'", "''");
						value = value.Replace("'", "''");
						sqlite3_exec(db, U8($"INSERT OR IGNORE INTO {tableName} (k, v) VALUES ('{key}', '{value}')"), IntPtr.Zero, IntPtr.Zero, out _);
					}
					pos = pEnd + 4;
				}
				sqlite3_exec(db, U8("COMMIT"), IntPtr.Zero, IntPtr.Zero, out _);
			}
			catch
			{
				sqlite3_exec(db, U8("ROLLBACK"), IntPtr.Zero, IntPtr.Zero, out _);
				throw;
			}
		}

		private static IntPtr GetDb(string dbName)
		{
			if (databases.TryGetValue(dbName, out var db))
				return db;
			throw new CodeEE($"SQL database '{dbName}' is not connected");
		}

		public static void Reset()
		{
			foreach (var kv in databases)
				sqlite3_close_v2(kv.Value);
			databases.Clear();
		}
	}
}
