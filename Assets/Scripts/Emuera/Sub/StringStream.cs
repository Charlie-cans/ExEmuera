using System;
using System.IO;

namespace MinorShift.Emuera.Sub
{
	/// <summary>
	/// 用于逐字评估字符串的类
	/// </summary>
	internal sealed class StringStream
	{
        public StringStream(string s)
		{
			source = s;
			if (source == null)
				source = "";
			pointer = 0;
		}

        public StringStream()
        { }
        public void Set(string s)
        {
            source = s;
            if(source == null)
                source = "";
            pointer = 0;
        }

		string source;
		public const char EndOfString = '\0';
		int pointer;
		public string RowString
		{
			get
			{
				return source;
			}
		}

		public int CurrentPosition
		{
			get { return pointer; }
			set { pointer = value; }
		}
		public char Current
		{
			get
			{
				if (pointer >= source.Length)
					return EndOfString;
				return source[pointer];
			}
		}
		
		public void AppendString(string str)
		{
			if (pointer > source.Length)
				pointer = source.Length;
			source += " " + str;
		}
		
		/// <summary>
		/// 已到达字符串末尾
		/// </summary>
		public bool EOS { get { return pointer >= source.Length; } }

		///用于变量分隔符"[["和"]]"的预读等
		public char Next
		{
			get
			{
				if (pointer + 1 >= source.Length)
					return EndOfString;
				return source[pointer + 1];
			}
		}

		public string Substring()
		{
			if (pointer >= source.Length)
				return "";
			else if (pointer == 0)
				return source;
			return source.Substring(pointer);
		}

		public string Substring(int start, int length)
		{
			if (start >= source.Length || length == 0)
				return "";
			if (start + length > source.Length)
				length = source.Length - start;
			return source.Substring(start, length);
		}

		internal void Replace(int start, int count, string src)
		{
			//前提是参数传递了正确的数值
			source = (source.Remove(start, count)).Insert(start, src);
			pointer = start;
		}

		public void ShiftNext()
		{
			pointer++;
		}

        public void Jump(int skip)
        {
            pointer += skip;
        }

		/// <summary>
		/// 検索文字列の相対位置を返す。見つからない場合、負の値。
		/// </summary>
		/// <param name="str"></param>
		public int Find(string str)
		{
			return source.IndexOf(str, pointer) - pointer;
		}

		/// <summary>
		/// 検索文字列の相対位置を返す。見つからない場合、負の値。
		/// </summary>
		public int Find(char c)
		{
			return source.IndexOf(c, pointer) - pointer;
		}

		public override string ToString()
		{
			if (source == null)
				return "";
			return source;
		}

		public bool CurrentEqualTo(string rother)
		{
			if (pointer + rother.Length > source.Length)
				return false;

			for (int i = 0;  i < rother.Length;i++)
			{
				if (source[pointer + i] != rother[i])
					return false;
			}
			return true;
		}

		public bool TripleSymbol()
		{
			if (pointer + 3 > source.Length)
				return false;
			return (source[pointer] == source[pointer + 1]) && (source[pointer] == source[pointer + 2]);
		}


		public bool CurrentEqualTo(string rother, StringComparison comp)
		{
			if (pointer + rother.Length > source.Length)
				return false;
			string sub = source.Substring(pointer, rother.Length);
			return sub.Equals(rother, comp);
		}

		public void Seek(int offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin)
				pointer = offset;
			else if (origin == SeekOrigin.Current)
				pointer = pointer + offset;
			else if (origin == SeekOrigin.End)
				pointer = source.Length + offset;
			if (pointer < 0)
				pointer = 0;
		}
	}
}
