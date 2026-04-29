using System;
using System.Collections.Generic;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Variable;

namespace MinorShift.Emuera.GameProc
{
	//1756 解除内部类限制，改为通用开放


	//混淆用属性。如果要进行enum.ToString()或enum.Parse()操作，请设置为(Exclude=true)。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum SystemStateCode
	{
		__CAN_SAVE__ = 0x10000,//是否可以调用存档/读档画面？
		__CAN_BEGIN__ = 0x20000,//是否可以调用BEGIN命令？
		Title_Begin = 0,//初始状态
		Openning = 1,//等待首次输入
		Train_Begin = 0x10,//从BEGIN TRAIN。
		Train_CallEventTrain = 0x11,//@EVENTTRAIN调用中。可跳过
		Train_CallShowStatus = 0x12,//@SHOW_STATUS调用中
		Train_CallComAbleXX = 0x13,//@COM_ABLExx调用中。跳过时设为RETURN 1。
		Train_CallShowUserCom = 0x14,//@SHOW_USERCOM调用中
		Train_WaitInput = 0x15,//等待输入状态。如果选择可执行则从EVENTCOM到COMxx，否则将RESULT传递给@USERCOM
		Train_CallEventCom = 0x16 | __CAN_BEGIN__,//@EVENTCOM调用中

		Train_CallComXX = 0x17 | __CAN_BEGIN__,//@COMxx调用中
		Train_CallSourceCheck = 0x18 | __CAN_BEGIN__,//@SOURCE_CHECK调用中
		Train_CallEventComEnd = 0x19 | __CAN_BEGIN__,//@EVENTCOMEND调用中。可跳过。返回Train_CallEventTrain。@USERCOM调用中也是这里

		Train_DoTrain = 0x1A,

		AfterTrain_Begin = 0x20 | __CAN_BEGIN__,//从BEGIN AFTERTRAIN。调用@EVENTEND后转移到Normal。

		Ablup_Begin = 0x30,//从BEGIN ABLUP。
		Ablup_CallShowJuel = 0x31,//@SHOW_JUEL
		Ablup_CallShowAblupSelect = 0x32,//@SHOW_ABLUP_SELECT
		Ablup_WaitInput = 0x33,//
		Ablup_CallAblupXX = 0x34 | __CAN_BEGIN__,//如果没有@ABLUPxx，则将RESULT传递给@USERABLUP。返回Ablup_CallShowJuel。

		Turnend_Begin = 0x40 | __CAN_BEGIN__,//从BEGIN TURNEND。调用@EVENTTURNEND后转移到Normal。

		Shop_Begin = 0x50 | __CAN_SAVE__,//从BEGIN SHOP
		Shop_CallEventShop = 0x51 | __CAN_BEGIN__ | __CAN_SAVE__,//@EVENTSHOP调用中。可跳过
		Shop_CallShowShop = 0x52 | __CAN_SAVE__,//@SHOW_SHOP调用中
		Shop_WaitInput = 0x53 | __CAN_SAVE__,//等待输入状态。如果存在道具则从EVENTBUY到BOUGHT，否则将RESULT传递给@USERSHOP
		Shop_CallEventBuy = 0x54 | __CAN_BEGIN__ | __CAN_SAVE__,//@USERSHOP或@EVENTBUY调用中

		SaveGame_Begin = 0x100,//从SAVEGAME
		SaveGame_WaitInput = 0x101,//等待输入
		SaveGame_WaitInputOverwrite = 0x102,//等待覆盖许可
		SaveGame_CallSaveInfo = 0x103,//@SAVEINFO调用中。共20次。
		LoadGame_Begin = 0x110,//从LOADGAME
		LoadGame_WaitInput = 0x111,//等待输入
		LoadGameOpenning_Begin = 0x120,//首次选择[1]时。
		LoadGameOpenning_WaitInput = 0x121,//等待输入


		//AutoSave_Begin = 0x200,
		AutoSave_CallSaveInfo = 0x201,
		AutoSave_CallUniqueAutosave = 0x202,
		AutoSave_Skipped = 0x203,

		LoadData_DataLoaded = 0x210,//数据加载后立即
		LoadData_CallSystemLoad = 0x211 | __CAN_BEGIN__,//数据加载后立即
		LoadData_CallEventLoad = 0x212 | __CAN_BEGIN__,//@EVENTLOAD调用中。可跳过

		Openning_TitleLoadgame = 0x220,

		System_Reloaderb = 0x230,
		First_Begin = 0x240,

		Normal = 0xFFFF | __CAN_BEGIN__ | __CAN_SAVE__,//没有特别情况时。到达ScriptEnd时出错
	}

	//混淆用属性。如果要进行enum.ToString()或enum.Parse()操作，请设置为(Exclude=true)。
	[global::System.Reflection.Obfuscation(Exclude = false)]
	internal enum BeginType
	{
		NULL = 0,
		SHOP = 2,
		TRAIN = 3,
		AFTERTRAIN = 4,
		ABLUP = 5,
		TURNEND = 6,
		FIRST = 7,
		TITLE = 8,
	}

	internal sealed class ProcessState
	{
		public ProcessState(EmueraConsole console)
		{
			if (Program.DebugMode)//如果不是DebugMode则无需知道
				this.console = console;
		}
		readonly EmueraConsole console = null;
		readonly List<CalledFunction> functionList = new List<CalledFunction>();
		private LogicalLine currentLine;
		//private LogicalLine nextLine;
		public int lineCount = 0;
        public int currentMin = 0;
        //private bool sequential;

		public bool ScriptEnd
		{
			get
			{
                return functionList.Count == currentMin;
            }
		}

        public int functionCount
        {
            get
            {
                return functionList.Count;
            }
        }

		SystemStateCode sysStateCode = SystemStateCode.Title_Begin;
		BeginType begintype = BeginType.NULL;
		public bool isBegun { get { return (begintype != BeginType.NULL) ? true : false; } }

        public LogicalLine CurrentLine { get { return currentLine; } set { currentLine = value; } }
        public LogicalLine ErrorLine
		{
			get
			{
				//if (RunningLine != null)
				//	return RunningLine;
				return currentLine;
			}
		}

		//在IF语句中检查ELSEIF语句内容等情况下，当CurrentLine与正在处理的Line不同时设置
		//public LogicalLine RunningLine { get; set; }
		//1755a 调用源消失
		//public bool Sequential { get { return sequential; } }
		public CalledFunction CurrentCalled
		{
			get
			{
				//没有执行函数的状态除部分系统INPUT外不存在，因此从逻辑上讲只能通过GOTO系列处理到达此处，无法满足前提
				//if (functionList.Count == 0)
				//    throw new ExeEE("実行中関数がない");
				return functionList[functionList.Count - 1];
			}
		}
		public SystemStateCode SystemState
		{
			get { return sysStateCode; }
			set { sysStateCode = value; }
		}

		public void ShiftNextLine()
		{
            currentLine = currentLine.NextLine;
            //nextLine = nextLine.NextLine;
            //RunningLine = null;
            //sequential = true;
			//GlobalStatic.Process.lineCount++;
			lineCount++;
		}

		/// <summary>
		/// 函数内的移动。不是JUMP而是GOTO或IF语句等
		/// </summary>
		/// <param name="line"></param>
		public void JumpTo(LogicalLine line)
		{
            currentLine = line;
            lineCount++;
            //sequential = false;
			//ShfitNextLine();
		}

		public void SetBegin(string keyword)
		{//应该已经Trim和ToUpper过了
			switch (keyword)
			{
				case "SHOP":
					SetBegin(BeginType.SHOP); return;
				case "TRAIN":
					SetBegin(BeginType.TRAIN); return;
				case "AFTERTRAIN":
					SetBegin(BeginType.AFTERTRAIN); return;
				case "ABLUP":
					SetBegin(BeginType.ABLUP); return;
				case "TURNEND":
					SetBegin(BeginType.TURNEND); return;
				case "FIRST":
					SetBegin(BeginType.FIRST); return;
				case "TITLE":
					SetBegin(BeginType.TITLE); return;
			}
			throw new CodeEE("BEGINのキーワード\"" + keyword + "\"は未定義です");
		}

		public void SetBegin(BeginType type)
		{
			string errmes = "";
			switch (type)
			{
				case BeginType.SHOP:
				case BeginType.TRAIN:
				case BeginType.AFTERTRAIN:
				case BeginType.ABLUP:
				case BeginType.TURNEND:
				case BeginType.FIRST:
					if ((sysStateCode & SystemStateCode.__CAN_BEGIN__) != SystemStateCode.__CAN_BEGIN__)
					{
						errmes = "BEGIN";
						goto err;
					}
					break;
				//1.729 使BEGIN TITLE可以在任何地方使用
				case BeginType.TITLE:
					break;
				//在BEGIN处理中已检查
				//default:
				//    throw new ExeEE("不適当なBEGIN呼び出し");
			}
			begintype = type;
			return;
		err:
			CalledFunction func = functionList[0];
			string funcName = func.FunctionName;
			throw new CodeEE("@" + funcName + "中で" + errmes + "命令を実行することはできません");
		}

		public void SaveLoadData(bool saveData)
		{

			if (saveData)
				sysStateCode = SystemStateCode.SaveGame_Begin;
			else
				sysStateCode = SystemStateCode.LoadGame_Begin;
			//ClearFunctionList();
			return;
		}

		public void ClearFunctionList()
		{
			if (Program.DebugMode && !isClone && GlobalStatic.Process.MethodStack() == 0)
				console.DebugClearTraceLog();

            CalledFunction called = null;
            var count = functionList.Count;
			for(var i=0; i<count; ++i)
            {
                called = functionList[i];
                if(called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
            }
			functionList.Clear();
			begintype = BeginType.NULL;
		}

		public bool calledWhenNormal = true;
		/// <summary>
		/// 通过BEGIN命令的程序状态变化
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public void Begin()
		{
			//来自@EVENTSHOP的调用暂时丢弃
			if (sysStateCode == SystemStateCode.Shop_CallEventShop)
				return;

			switch (begintype)
			{
				case BeginType.SHOP:
					if (sysStateCode == SystemStateCode.Normal)
						calledWhenNormal = true;
					else
						calledWhenNormal = false;
					sysStateCode = SystemStateCode.Shop_Begin;
					break;
				case BeginType.TRAIN:
					sysStateCode = SystemStateCode.Train_Begin;
					break;
				case BeginType.AFTERTRAIN:
					sysStateCode = SystemStateCode.AfterTrain_Begin;
					break;
				case BeginType.ABLUP:
					sysStateCode = SystemStateCode.Ablup_Begin;
					break;
				case BeginType.TURNEND:
					sysStateCode = SystemStateCode.Turnend_Begin;
					break;
				case BeginType.FIRST:
					sysStateCode = SystemStateCode.First_Begin;
					break;
				case BeginType.TITLE:
					sysStateCode = SystemStateCode.Title_Begin;
					break;
				//因为在设置时已经判断过了，所以不应该到这里来
				//default:
				//    throw new ExeEE("不適当なBEGIN呼び出し");
			}
			if (Program.DebugMode)
			{
				console.DebugClearTraceLog();
				console.DebugAddTraceLog("BEGIN:" + begintype.ToString());
			}
            CalledFunction called = null;
            var count = functionList.Count;
            for(var i = 0; i < count; ++i)
            {
                called = functionList[i];
                if(called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
            }
			functionList.Clear();
			begintype = BeginType.NULL;
			return;
		}

		/// <summary>
		/// 由系统强制执行的BEGIN
		/// </summary>
		/// <param name="type"></param>
		public void Begin(BeginType type)
		{
			begintype = type;
			sysStateCode = SystemStateCode.Title_Begin;
			Begin();
		}

		public LogicalLine GetCurrentReturnAddress
		{
			get
			{
                if (functionList.Count == currentMin)
                    return null;
				return functionList[functionList.Count - 1].ReturnAddress;
			}
		}

        public LogicalLine GetReturnAddressSequensial(int curerntDepth)
        {
            if (functionList.Count == currentMin)
                return null;
            return functionList[functionList.Count - curerntDepth - 1].ReturnAddress;
        }

		public string Scope
		{
			get
			{
				//因为只能从脚本执行处理中调用，所以这里不应该...不存在才对
				//if (functionList.Count == 0)
				//{
				//    throw new ExeEE("実行中の関数が存在しません");
				//}
				if (functionList.Count == 0)
					return null;//1756 因为现在也可以从调试命令调用了
				return functionList[functionList.Count - 1].FunctionName;
			}
		}

		public void Return(Int64 ret)
		{
			if (IsFunctionMethod)
			{
				ReturnF(null);
				return;
			}
			//sequential = false;//无论如何都不是顺序的。
			//调用源全部是脚本处理
			//if (functionList.Count == 0)
			//{
			//    throw new ExeEE("実行中の関数が存在しません");
			//}
			CalledFunction called = functionList[functionList.Count - 1];
			if (called.IsJump)
			{//JUMP的情况。立即RETURN RESULT。
                if (called.TopLabel.hasPrivDynamicVar)
                    called.TopLabel.Out();
				functionList.Remove(called);
				if (Program.DebugMode)
					console.DebugRemoveTraceLog();
				Return(ret);
				return;
			}
			if (!called.IsEvent)
			{
                if (called.TopLabel.hasPrivDynamicVar)
                    called.TopLabel.Out();
                currentLine = null;
            }
			else
			{
                if (called.CurrentLabel.hasPrivDynamicVar)
                    called.CurrentLabel.Out();
				//带有#Single标志的函数返回了1。
				//1752 修正为检查是否等于1而非非0
				//1756 修正为按#PRI或#LATER的组结束，而非全部结束
                if (called.IsOnly)
                    called.FinishEvent();
				else if ((called.HasSingleFlag) && (ret == 1))
					called.ShiftNextGroup();
				else
                    called.ShiftNext();//前进到下一个同名函数。
                currentLine = called.CurrentLabel;//移动到函数起点(@~~)。如果没有可调用的函数则为null
                if (called.CurrentLabel != null)
                {
                    lineCount++;
                    if (called.CurrentLabel.hasPrivDynamicVar)
                        called.CurrentLabel.In();
                }
            }
			if (Program.DebugMode)
				console.DebugRemoveTraceLog();
			//函数结束
            if (currentLine == null)
            {
                currentLine = called.ReturnAddress;
                functionList.RemoveAt(functionList.Count - 1);
				if (currentLine == null)
				{
					//此时functionList应该是空的
					//functionList.Clear();//全部结束。将处理返回给stateEndProcess
					if (begintype != BeginType.NULL)//如果执行了BEGIN XX
					{
						Begin();
					}
					return;
				}
                lineCount++;
                //ShfitNextLine();
                return;
			}
			else if (Program.DebugMode)
			{
				FunctionLabelLine label = called.CurrentLabel;
				console.DebugAddTraceLog("CALL :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
			}
            lineCount++;
            //ShfitNextLine();
            return;
		}

		public void IntoFunction(CalledFunction call, UserDefinedFunctionArgument srcArgs, ExpressionMediator exm)
		{

			if (call.IsEvent)
			{
                CalledFunction called = null;
                var count = functionList.Count;
                for(var i = 0; i < count; ++i)
                {
                    called = functionList[i];
					if (called.IsEvent)
						throw new CodeEE("EVENT関数の解決前にCALLEVENT命令が行われました");
				}
			}
			if (Program.DebugMode)
			{
				FunctionLabelLine label = call.CurrentLabel;
				if (call.IsJump)
					console.DebugAddTraceLog("JUMP :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
				else
					console.DebugAddTraceLog("CALL :@" + label.LabelName + ":" + label.Position.ToString() + "行目");
			}
            if (srcArgs != null)
            {
                //确定参数值
                srcArgs.SetTransporter(exm);
                //更新私有变量
                if (call.TopLabel.hasPrivDynamicVar)
                    call.TopLabel.In();
                //向更新后的变量代入参数
                for (int i = 0; i < call.TopLabel.Arg.Length; i++)
                {
                    if (srcArgs.Arguments[i] != null)
                    {
						if (call.TopLabel.Arg[i].Identifier.IsReference)
							((ReferenceToken)(call.TopLabel.Arg[i].Identifier)).SetRef(srcArgs.TransporterRef[i]);
                        else if (srcArgs.Arguments[i].GetOperandType() == typeof(Int64))
                            call.TopLabel.Arg[i].SetValue(srcArgs.TransporterInt[i], exm);
                        else
                            call.TopLabel.Arg[i].SetValue(srcArgs.TransporterStr[i], exm);
                    }
                }
            }
            else//到这里来的是来自系统的调用=只有没有参数的函数 虽然感觉可以放到if嵌套外面，不过...
            {
                //更新私有变量
                if (call.TopLabel.hasPrivDynamicVar)
                    call.TopLabel.In();
            }
			functionList.Add(call);
			//sequential = false;
            currentLine = call.CurrentLabel;
            lineCount++;
            //ShfitNextLine();
        }

		#region userdifinedmethod
		public bool IsFunctionMethod
		{
			get
			{
                return functionList[currentMin].TopLabel.IsMethod;
            }
		}

		public SingleTerm MethodReturnValue = null;

		public void ReturnF(SingleTerm ret)
		{
			//读取时应该已经检查过了
			//if (!IsFunctionMethod)
			//    throw new ExeEE("ReturnFと#FUNCTIONのチェックがおかしい");
			//sequential = false;//无论如何都不是顺序的。
			//调用源只有RETURNF命令或函数结束时
			//if (functionList.Count == 0)
			//    throw new ExeEE("実行中の関数が存在しません");
			//非事件调用，因此这不可能发生
			//else if (functionList.Count != 1)
			//    throw new ExeEE("関数が複数ある");
			if (Program.DebugMode)
			{
				console.DebugRemoveTraceLog();
			}
			//Out在GetValue侧执行
			//functionList[0].TopLabel.Out();
            currentLine = functionList[functionList.Count - 1].ReturnAddress;
            functionList.RemoveAt(functionList.Count - 1);
            //nextLine = null;
            MethodReturnValue = ret;
            return;
		}

		#endregion

		bool isClone = false;
        public bool IsClone { get { return isClone; } set { isClone = value; } }

		// 因为没有需要functionList副本的调用源，所以决定不复制。
		public ProcessState Clone()
		{
			ProcessState ret = new ProcessState(console);
			ret.isClone = true;
			//反正要删除，所以不需要复制
			//foreach (CalledFunction func in functionList)
			//	ret.functionList.Add(func.Clone());
			ret.currentLine = this.currentLine;
            //ret.nextLine = this.nextLine;
            //ret.sequential = this.sequential;
			ret.sysStateCode = this.sysStateCode;
			ret.begintype = this.begintype;
			//ret.MethodReturnValue = this.MethodReturnValue;
			return ret;

		}
		//public ProcessState CloneForFunctionMethod()
		//{
		//    ProcessState ret = new ProcessState(console);
		//    ret.isClone = true;

		//    //反正要删除，所以不需要复制
		//    //foreach (CalledFunction func in functionList)
		//    //	ret.functionList.Add(func.Clone());
		//    ret.currentLine = this.currentLine;
		//    ret.nextLine = this.nextLine;
		//    //ret.sequential = this.sequential;
		//    ret.sysStateCode = this.sysStateCode;
		//    ret.begintype = this.begintype;
		//    //ret.MethodReturnValue = this.MethodReturnValue;
		//    return ret;
		//}
	}
}