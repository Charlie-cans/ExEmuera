using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameProc.Function;

namespace MinorShift.Emuera.GameProc
{
	/// <summary>
	/// 抽象类，对应一条指令行
	/// </summary>
	internal abstract class LogicalLine
	{
		protected ScriptPosition position;

		//LogicalLine prevLine;
		LogicalLine nextLine;
		public ScriptPosition Position
		{
			get { return position; }
		}

		public FunctionLabelLine ParentLabelLine { get; set; }
		public LogicalLine NextLine
		{
			get { return nextLine; }
			set { nextLine = value; }
		}
		public override string ToString()
		{
			if (position == null)
				return base.ToString();
			return string.Format("{0}:{1}:{2}", position.Filename, position.LineNo, position.RowLine);
		}

		protected bool isError;
		protected string errMes = "";

		public virtual string ErrMes
		{
			get { return errMes; }
			set { errMes = value; }
		}
		public virtual bool IsError
		{
			get { return isError; }
			set { isError = value; }
		}
	}

	///// <summary>
	///// 注释行。
	///// </summary>
	//internal sealed class CommentLine : LogicalLine
	//{
	//    public CommentLine(ScriptPosition thePosition, string str)
	//    {
	//        base.position = thePosition;
	//        //comment = str;
	//    }
	//    //string comment;
	//    public override bool IsError
	//    {
	//        get { return false; }
	//    }
	//}

	/// <summary>
	/// 无效行。
	/// </summary>
	internal sealed class InvalidLine : LogicalLine
	{
		public InvalidLine(ScriptPosition thePosition, string err)
		{
			base.position = thePosition;
			errMes = err;
		}
		public override bool IsError
		{
			get { return true; }
		}
	}

	/// <summary>
	/// 指令行
	/// </summary>
	internal sealed class InstructionLine : LogicalLine
	{
		public InstructionLine(ScriptPosition thePosition, FunctionIdentifier theFunc, StringStream theArgPrimitive)
		{
			base.position = thePosition;
			this.func = theFunc;
			this.argprimitive = theArgPrimitive;
		}

		public InstructionLine(ScriptPosition thePosition, FunctionIdentifier functionIdentifier, OperatorCode assignOP, WordCollection dest, StringStream theArgPrimitive)
		{
			base.position = thePosition;
			func = functionIdentifier;
			AssignOperator = assignOP;
			assigndest = dest;
			this.argprimitive = theArgPrimitive;
		}
		readonly FunctionIdentifier func;
		StringStream argprimitive;

		WordCollection assigndest;
		public OperatorCode AssignOperator { get; private set; }
		Int64 subData = 0;
		public FunctionCode FunctionCode
		{
			get { return func.Code; }
		}
		public FunctionIdentifier Function
		{
			get { return func; }
		}
		public Argument Argument { get; set; }
		public StringStream PopArgumentPrimitive()
		{
			StringStream ret = argprimitive;
			argprimitive = null;
			return ret;
		}
		public WordCollection PopAssignmentDestStr()
		{
			WordCollection ret = assigndest;
			assigndest = null;
			return ret;
		}

		/// <summary>
		/// 记录循环的结束
		/// </summary>
		public Int64 LoopEnd
		{
			get { return subData; }
			set { subData = value; }
		}

		VariableTerm cnt;
		/// <summary>
		/// 记录循环中使用的变量
		/// </summary>
		public VariableTerm LoopCounter
		{
			get { return cnt; }
			set { cnt = value; }
		}

		Int64 step;
		/// <summary>
		/// 记录每次循环增加的值
		/// </summary>
		public Int64 LoopStep
		{
			get { return step; }
			set { step = value; }
		}

		private LogicalLine jumpto = null;
        private LogicalLine jumptoendcatch = null;
		//仅IF语句和SELECT语句使用。
		public List<InstructionLine> IfCaseList = null;
        //仅PRINTDATA语句使用。
        public List<List<InstructionLine>> dataList = null;
        //TRYCALLLIST系列使用
        public List<InstructionLine> callList = null;

		public LogicalLine JumpTo
		{
			get { return jumpto; }
			set { jumpto = value; }
		}

        public LogicalLine JumpToEndCatch
        {
            get { return jumptoendcatch; }
            set { jumptoendcatch = value; }
        }

	}

	/// <summary>
	/// 文件的开始和结束
	/// </summary>
	internal sealed class NullLine : LogicalLine { }
	
	/// <summary>
	/// 标签出错时专用的函数行类
	/// </summary>
	internal sealed class InvalidLabelLine : FunctionLabelLine
	{
		public InvalidLabelLine(ScriptPosition thePosition, string labelname, string err)
		{
			base.position = thePosition;
			LabelName = labelname;
			errMes = err;
			IsSingle = false;
			Index = -1;
			Depth = -1;
			IsMethod = false;
			MethodType = typeof(void);
		}
		public override bool IsError
		{
			get { return true; }
		}
	}

	/// <summary>
	/// 以@开头的标签行
	/// </summary>
	internal class FunctionLabelLine : LogicalLine, IComparable<FunctionLabelLine>
	{
		protected FunctionLabelLine() { }
		public FunctionLabelLine(ScriptPosition thePosition, string labelname, WordCollection wc)
		{
			base.position = thePosition;
			LabelName = labelname;
			IsSingle = false;
			hasPrivDynamicVar = false;
			Index = -1;
			Depth = -1;
			LocalLength = 0;
			LocalsLength = 0;
			ArgLength = 0;
			ArgsLength = 0;
			IsMethod = false;
			MethodType = typeof(void);
			this.wc = wc;

			//ArgOptional = true;
			//ArgAutoConvert = true;
		}
		WordCollection wc;
		public WordCollection PopRowArgs()
		{
			WordCollection ret = wc;
			wc = null;
			return ret;
		}

		public string LabelName { get; protected set; }
		public bool IsEvent { get; set; }
		public bool IsSystem { get; set; }
		public bool IsSingle { get; set; }
		public bool IsPri { get; set; }
		public bool IsLater { get; set; }
		public bool IsOnly { get; set; }
		public bool hasPrivDynamicVar { get; set; }
		public int LocalLength { get; set; }
		public int LocalsLength { get; set; }
		public int ArgLength { get; set; }
		public int ArgsLength { get; set; }

		//public bool ArgOptional { get; set; }
		//public bool ArgAutoConvert { get; set; }

		public bool IsMethod { get; set; }
		public Type MethodType { get; set; }
		public VariableTerm[] Arg { get; set; }
		public SingleTerm[] Def { get; set; }
        //public SingleTerm[] SubNames { get; set; }
		public int Depth { get; set; }

		#region IComparable<FunctionLabelLine> 成员
		//排序用信息
		public int Index { get; set; }
		public int FileIndex { get; set; }
		public int CompareTo(FunctionLabelLine other)
		{
			if (FileIndex != other.FileIndex)
				return FileIndex.CompareTo(other.FileIndex);
			//position == null的Line（调试命令等）不应该被排序
			if (position.LineNo != other.position.LineNo)
				return position.LineNo.CompareTo(other.position.LineNo);
			return Index.CompareTo(other.Index);
		}
		#endregion
		#region private变量
		Dictionary<string, UserDefinedVariableToken> privateVar = new Dictionary<string, UserDefinedVariableToken>();
		internal bool AddPrivateVariable(UserDefinedVariableData data)
		{
			if (privateVar.ContainsKey(data.Name))
				return false;
			UserDefinedVariableToken var = GlobalStatic.VariableData.CreatePrivateVariable(data);
			privateVar.Add(data.Name, var);
			//仅有静态变量的情况下，函数调用时无需做任何处理
			if (!data.Static)
				hasPrivDynamicVar = true;
			return true;
		}
		internal UserDefinedVariableToken GetPrivateVariable(string key)
		{
			UserDefinedVariableToken var = null;
			privateVar.TryGetValue(key, out var);
			return var;
		}

		/// <summary>
		/// 参数值确定之后、参数赋值之前调用
		/// </summary>
		internal void In()
		{
#if UEMUERA_DEBUG
			GlobalStatic.StackList.Add(this);
#endif
			foreach (UserDefinedVariableToken var in privateVar.Values)
				if (!var.IsStatic)
					var.In();
		}
		internal void Out()
		{
#if UEMUERA_DEBUG
			GlobalStatic.StackList.Remove(this);
#endif
			foreach (UserDefinedVariableToken var in privateVar.Values)
				if (!var.IsStatic)
					var.Out();
		}
		#endregion

	}

	/// <summary>
	/// 以$开头的标签行
	/// </summary>
	internal sealed class GotoLabelLine : LogicalLine, IEqualityComparer<GotoLabelLine>
	{
		public GotoLabelLine(ScriptPosition thePosition, string labelname)
		{
			base.position = thePosition;
			this.labelname = labelname;
		}
		readonly string labelname = "";
		public string LabelName
		{
			get { return labelname; }
		}

		#region IEqualityComparer<GotoLabelLine> 成员

		public bool Equals(GotoLabelLine x, GotoLabelLine y)
		{
			if ((x == null) || (y == null))
				return false;
			return ((x.ParentLabelLine == y.ParentLabelLine) && (x.labelname == y.labelname));
		}

		public int GetHashCode(GotoLabelLine obj)
		{
			return labelname.GetHashCode() ^ ParentLabelLine.GetHashCode();
		}

		#endregion
	}

}