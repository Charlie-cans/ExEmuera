using System;
using System.Collections.Generic;

namespace MinorShift.Emuera.Sub
{
	[Serializable]
    internal abstract class EmueraException : ApplicationException
	{
		protected EmueraException(string errormes, ScriptPosition position)
			: base(errormes)
		{
			Position = position;
		}
		protected EmueraException(string errormes)
			: base(errormes)
		{
			Position = null;
		}
		public ScriptPosition Position;
	}

	/// <summary>
	/// 被认为由emuera本体引起的错误
	/// </summary>
    [Serializable]
    internal sealed class ExeEE : EmueraException
	{
		public ExeEE(string errormes)
			: base(errormes)
		{
		}
		public ExeEE(string errormes, ScriptPosition position)
			: base(errormes, position)
		{
		}
	}

	/// <summary>
	/// 被认为由脚本侧引起的错误
	/// </summary>
    [Serializable]
    internal class CodeEE : EmueraException
	{
		public CodeEE(string errormes, ScriptPosition position)
			: base(errormes, position)
		{
		}
		public CodeEE(string errormes)
			: base(errormes)
		{
		}
	}

	/// <summary>
	/// 被认为由脚本侧引起的错误中，与未定义标识符相关的错误
	/// </summary>
	[Serializable]
	internal class IdentifierNotFoundCodeEE : CodeEE
	{
		public IdentifierNotFoundCodeEE(string errormes, ScriptPosition position)
			: base(errormes, position)
		{
		}
		public IdentifierNotFoundCodeEE(string errormes)
			: base(errormes)
		{
		}
	}

	/// <summary>
	/// 未实现错误
	/// </summary>
    [Serializable]
    internal sealed class NotImplCodeEE : CodeEE
	{
		public NotImplCodeEE(ScriptPosition position)
			: base("この機能は現バージョンでは使えません", position)
		{
		}
		public NotImplCodeEE()
			: base("この機能は現バージョンでは使えません")
		{
		}
	}

	/// <summary>
	/// Save、Load过程中的错误
	/// </summary>
    [Serializable]
    internal sealed class FileEE : EmueraException
	{
		public FileEE(string errormes)
			: base(errormes)
		{ }
	}

	/// <summary>
	/// 用于显示错误位置的位置数据。由于是格式化前的数据，除错误显示外不应引用。
	/// </summary>
	internal sealed class ScriptPosition : IEquatable<ScriptPosition>, IEqualityComparer<ScriptPosition>
	{
		public ScriptPosition(string srcLine)
		{
			LineNo = -1;
            RowLine = srcLine;
			Filename = "";
		}
		public ScriptPosition(string srcFile, int srcLineNo, string srcLine)
		{
			LineNo = srcLineNo;
            RowLine = srcLine;
            if (srcFile == null)
				Filename = "";
            else
                Filename = srcFile;
		}
		public ScriptPosition(string srcFile, int srcLineNo)
		{
			LineNo = srcLineNo;
            RowLine = "";
            if (srcFile == null)
				Filename = "";
            else
                Filename = srcFile;
		}
		public readonly int LineNo;
		public readonly string RowLine;
		public readonly string Filename;
		public override string ToString()
		{
			if(LineNo == -1)
				return base.ToString();
			return Filename + ":" + LineNo.ToString();
		}

		#region IEqualityComparer<ScriptPosition> メンバ

		public bool Equals(ScriptPosition x, ScriptPosition y)
		{
			if((x == null)||(y == null))
				return false;
			return ((x.Filename == y.Filename) && (x.LineNo == y.LineNo));
		}

		public int GetHashCode(ScriptPosition obj)
		{
			return Filename.GetHashCode() ^ LineNo.GetHashCode();
		}

		#endregion

		#region IEquatable<ScriptPosition> メンバ

		public bool Equals(ScriptPosition other)
		{
			return this.Equals(this, other);
		}

		#endregion
	}
}
