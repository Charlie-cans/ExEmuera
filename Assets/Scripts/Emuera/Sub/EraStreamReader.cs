using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MinorShift.Emuera.Sub
{
	internal sealed class EraStreamReader : IDisposable
	{
		public EraStreamReader(bool useRename)
		{
			this.useRename = useRename;
		}

		string filepath;
		string filename;
		bool useRename = false;
		int curNo = 0;
		int nextNo = 0;
		StreamReader reader;
		FileStream stream;

		public bool Open(string path)
		{
			return Open(path, Path.GetFileName(path));
		}

		public bool Open(string path, string name)
		{
			//不会做那么不规矩的事情
			//if (disposed)
			//    throw new ExeEE("试图重用已废弃的对象");
			//if ((reader != null) || (stream != null) || (filepath != null))
			//    throw new ExeEE("试图将使用中的对象重用于其他用途");
			filepath = path;
			filename = name;
			nextNo = 0;
			curNo = 0;
			try
			{
				stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				reader = new StreamReader(stream, Config.Encode);
			}
			catch
			{
				this.Dispose();
				return false;
			}
			return true;
		}

		public string ReadLine()
		{
			nextNo++;
			curNo = nextNo;
			return reader.ReadLine();
		}

        /// <summary>
        /// 读取下一个有效行。由于通过LexicalAnalyzer引用Config，请在Config完成前不要使用。
        /// </summary>
        public StringStream ReadEnabledLine()
		{
			string line = null;
			StringStream st = new StringStream();
			curNo = nextNo;
			while (true)
			{
				line = reader.ReadLine();
				curNo++;
				nextNo++;
				if (line == null)
					return null;
				if (line.Length == 0)
					continue;

				if (useRename && (line.IndexOf("[[") >= 0) && (line.IndexOf("]]") >= 0))
				{
					foreach (KeyValuePair<string, string> pair in ParserMediator.RenameDic)
						line = line.Replace(pair.Key, pair.Value);
				}
                st.Set(line);
				LexicalAnalyzer.SkipWhiteSpace(st);
				if (st.EOS)
					continue;
				if (st.Current == '}')
					throw new CodeEE("予期しない行連結終端記号'}'が見つかりました");
				if (st.Current == '{')
				{
					if (line.Trim() != "{")
						throw new CodeEE("行連結始端記号'{'の行に'{'以外の文字を含めることはできません");
					break;
				}
				return st;
			}
			//curNo在此后不加算（将起始记号的行作为行号）
			StringBuilder b = new StringBuilder();
			while (true)
			{
				line = reader.ReadLine();
				nextNo++;
				if (line == null)
				{
					throw new CodeEE("行連結始端記号'{'が使われましたが終端記号'}'が見つかりません");
				}

				if (useRename && (line.IndexOf("[[") >= 0) && (line.IndexOf("]]") >= 0))
				{
					foreach (KeyValuePair<string, string> pair in ParserMediator.RenameDic)
						line = line.Replace(pair.Key, pair.Value);
				}
				string test = line.TrimStart();
				if (test.Length > 0)
				{
					if (test[0] == '}')
					{
						if (test.Trim() != "}")
							throw new CodeEE("行連結終端記号'}'の行に'}'以外の文字を含めることはできません");
						break;
					}
					if (test[0] == '{')
						throw new CodeEE("予期しない行連結始端記号'{'が見つかりました");
				}
				b.Append(line);
				b.Append(" ");
			}
			st.Set(b.ToString());
			LexicalAnalyzer.SkipWhiteSpace(st);
			return st;
		}

		/// <summary>
		/// 最近读取的行的行号
		/// </summary>
		public int LineNo
		{ get { return curNo; } }
		public string Filename
		{
			get
			{
				return filename;
			}
		}
		//public string Filepath
		//{
		//    get
		//    {
		//        return filepath;
		//    }
		//}

		public void Close() { this.Dispose(); }
		bool disposed = false;
		#region IDisposable メンバ

		public void Dispose()
		{
			if (disposed)
				return;
			if (reader != null)
				reader.Close();
			else if (stream != null)
				stream.Close();
			filepath = null;
			filename = null;
			reader = null;
			stream = null;
            disposed = true;
		}

		#endregion
	}
}
