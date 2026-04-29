using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Drawing;
using System.Text;
//using System.Windows.Forms;
using System.IO;
using MinorShift._Library;
using MinorShift.Emuera.Sub;
//using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameProc;
//using System.Drawing.Imaging;
//using MinorShift.Emuera.Forms;
using MinorShift.Emuera.Content;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameProc.Function;
using uEmuera.Forms;
using uEmuera.Drawing;
using uEmuera.Window;

namespace MinorShift.Emuera.GameView
{
	//输入输出等待状态。
	//混淆用属性。如果使用enum.ToString()或enum.Parse()，则需设置为(Exclude=true)。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum ConsoleState
	{
		Initializing = 0,
		Quit = 5,//QUIT
		Error = 6,//由Exception导致的强制终止
		Running = 7,
		WaitInput = 20,
        Sleep = 21,//DoEvents

        //WaitKey = 1,//WAIT
        //WaitSystemInteger = 2,//System要求的输入
        //WaitInteger = 3,//INPUT
        //WaitString = 4,//INPUTS
        //WaitIntegerWithTimer = 8,
        //WaitStringWithTimer = 9,
        //Timeout = 10,
        //Timeouts = 11,
        //WaitKeyWithTimer = 12,
        //WaitKeyWithTimerF = 13,
        //WaitOneInteger = 14,
        //WaitOneString = 15,
        //WaitOneIntegerWithTimer = 16,
        //WaitOneStringWithTimer = 17,
        //WaitAnyKey = 18,

    }

	//混淆用属性。如果使用enum.ToString()或enum.Parse()，则需设置为(Exclude=true)。
	[global::System.Reflection.Obfuscation(Exclude=false)]
	internal enum ConsoleRedraw
	{
		None = 0,
		Normal = 1,
	}

	internal class ChangedEventArgs : EventArgs
	{
		public ConsoleDisplayLine ConsoleDisplayLine;

        public ChangedEventArgs(ConsoleDisplayLine cdl)
            : base()
        { ConsoleDisplayLine = cdl; }
	}

	internal class DisplayLineList : IList<ConsoleDisplayLine>
	{
        public DisplayLineList()
        {
            list = new List<ConsoleDisplayLine>();
        }

		private readonly List<ConsoleDisplayLine> list;

		public event EventHandler<ChangedEventArgs> Changed = null;

        protected virtual void OnChanged(ChangedEventArgs e)
        {
            if(Changed != null)
                Changed.Invoke(this, e);
        }

		public ConsoleDisplayLine this[int index]
		{
			get { return list[index]; }
			set { list[index] = value; }
		}

		public int Count { get { return list.Count; } }

		public bool IsReadOnly { get { return false; } }

		public void Add(ConsoleDisplayLine item)
		{
			list.Add(item);
			OnChanged(new ChangedEventArgs(item));
		}

        public void Clear() { list.Clear(); }

        public bool Contains(ConsoleDisplayLine item) { return list.Contains(item); }

        public void CopyTo(ConsoleDisplayLine[] array, int arrayIndex) { list.CopyTo(array, arrayIndex); }

        public IEnumerator<ConsoleDisplayLine> GetEnumerator() { return list.GetEnumerator(); }

        public int IndexOf(ConsoleDisplayLine item) { return list.IndexOf(item); }

        public void Insert(int index, ConsoleDisplayLine item) { list.Insert(index, item); }

        public bool Remove(ConsoleDisplayLine item) { return list.Remove(item); }

        public void RemoveAt(int index) { list.RemoveAt(index); }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return list.GetEnumerator(); }
    }

	internal sealed partial class EmueraConsole :IDisposable
	{
		#pragma warning disable CS0414
		public EmueraConsole(MainWindow parent)
		{
			window = parent;

			//1.713 在此阶段不得使用setStBar
			//setStBar(StaticConfig.DrawLineString);
			state = ConsoleState.Initializing;
			if (Config.FPS > 0)
				msPerFrame = 1000 / (uint)Config.FPS;
			//displayLineList = new List<ConsoleDisplayLine>();
            displayLineList = new DisplayLineList();
            //if (Program.DebugMode)
            //{
            //    debuglog = new StreamWriter(Program.DebugDir + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log", true, Encoding.UTF8)
            //    {
            //        AutoFlush = true,
            //    };

            //    void logging(object sender, ChangedEventArgs e)
            //    {
            //        var s = e.ConsoleDisplayLine.ToString();
            //        debuglog.WriteLine(s);
            //    }
            //    displayLineList.Changed += logging;
            //}

			printBuffer = new PrintStringBuffer(this);

			timer = new Timer();
			timer.Enabled = false;
			timer.Tick += new EventHandler(tickTimer);
			timer.Interval = 10;
			CBG_Clear();//添加字符串绘制用占位符

			redrawTimer = new Timer();
			redrawTimer.Enabled = false;//TODO:1824 添加动画用重绘定时器启用函数
			redrawTimer.Tick += new EventHandler(tickRedrawTimer);
			redrawTimer.Interval = 10;
        }
#region 1823 cbg相关
		private readonly List<ClientBackGroundImage> cbgList = new List<ClientBackGroundImage>();
		private GraphicsImage cbgButtonMap = null;
		private int selectingCBGButtonInt = -1;
		private int lastSelectingCBGButtonInt = -1;
		//ConsoleButtonString selectingButton = null;
		//ConsoleButtonString lastSelectingButton = null;
		class ClientBackGroundImage : IComparable<ClientBackGroundImage>
		{
			/// <summary>
			/// zdepth == 0是字符串用占位符，不得在其他地方使用
			/// </summary>
			/// <param name="zdepth"></param>
			internal ClientBackGroundImage(int zdepth)
			{ this.zdepth = zdepth; }
			public ASprite Img = null;
			public ASprite ImgB = null;
			public int x;
			public int y;
			public readonly int zdepth;
			public bool isButton = false;
			public int buttonValue;
			public string tooltipString = null;
			public int CompareTo(ClientBackGroundImage other)
			{
				if (other == null)
					return -1;
				//按逆序排序
				return -zdepth.CompareTo(other.zdepth);
			}
		}
		public void CBG_Clear()
		{
			for(var i=0; i<cbgList.Count; ++i)
			{
                ClientBackGroundImage cimg = cbgList[i];
                //将一次性匿名Image予以dispose
                if (cimg.Img != null && cimg.Img.Name.Length == 0)
					cimg.Img.Dispose();
			}
			cbgList.Clear();
			CBG_ClearBMap();
			cbgList.Add(new ClientBackGroundImage(0));
		}

		public void CBG_ClearRange(int zmin, int zmax)
		{
			if (zmin > zmax)
				return;
			for (int i = 0; i < cbgList.Count;i++)
			{
				ClientBackGroundImage cimg = cbgList[i];
				if (cimg.zdepth < zmin || cimg.zdepth > zmax || cimg.zdepth == 0)//0是占位符，因此不删除
					continue;

				//将一次性匿名Image予以dispose
				if (cimg.Img != null && cimg.Img.Name.Length == 0)
					cimg.Img.Dispose();
				cbgList.RemoveAt(i);
				i--;
			}
		}

		public void CBG_ClearButton()
		{
			for (int i = 0; i < cbgList.Count; i++)
			{
				ClientBackGroundImage cimg = cbgList[i];
				if (!cimg.isButton)
					continue;

				//将一次性匿名Image予以dispose
				if (cimg.Img != null && cimg.Img.Name.Length == 0)
					cimg.Img.Dispose();
				cbgList.RemoveAt(i);
				i--;
			}
			CBG_ClearBMap();
		}

		public void CBG_ClearBMap()
		{
			cbgButtonMap = null;
			selectingCBGButtonInt = -1;
			lastSelectingCBGButtonInt = -1;
		}

		public bool CBG_SetGraphics(GraphicsImage gra, int x, int y, int zdepth)
		{
			if (gra == null || !gra.IsCreated)
				return false;
			return CBG_SetImage(new SpriteG("", gra, new Rectangle(0, 0, gra.Width, gra.Height)), x, y, zdepth);
		}
		public bool CBG_SetImage(ASprite image, int x, int y, int zdepth)
		{
			if (image == null || !image.IsCreated)
				return false;
			if (zdepth == 0)
				throw new ArgumentOutOfRangeException();
			ClientBackGroundImage cbg = new ClientBackGroundImage(zdepth);
			cbg.Img = image;
			cbg.x = x;
			cbg.y = y;
			//cbg.zdepth = zdepth;
			cbgList.Add(cbg);
			cbgList.Sort();
			return true;
		}

		public bool CBG_SetButtonMap(GraphicsImage gra)
		{
			if (gra == null || !gra.IsCreated)
				return false;
			if (cbgButtonMap == gra)
				return false;
			cbgButtonMap = gra;
			selectingCBGButtonInt = -1;
			lastSelectingCBGButtonInt = -1;
			return true;
		}

		public bool CBG_SetButtonImage(int buttonValue, ASprite imageN, ASprite imageB, int x, int y, int zdepth, string tooltip = null)
		{
			if (zdepth == 0)
				throw new ArgumentOutOfRangeException();
			ClientBackGroundImage cbg = new ClientBackGroundImage(zdepth);
			cbg.Img = imageN;
			cbg.ImgB = imageB;
			cbg.x = x;
			cbg.y = y;
			//cbg.zdepth = zdepth;
			cbg.isButton = true;
			cbg.buttonValue = buttonValue;
			cbg.tooltipString = tooltip;
			cbgList.Add(cbg);
			cbgList.Sort();
			return true;
		}
		public int ClientWidth { get { return UnityEngine.Screen.width; } }
		public int ClientHeight { get { return UnityEngine.Screen.height; } }
#endregion

		const string ErrorButtonsText = "__openFileWithDebug__";
        private readonly MainWindow window;

		MinorShift.Emuera.GameProc.Process emuera;
		ConsoleState state = ConsoleState.Initializing;
		public bool Enabled { get { return window.Created; } }

		/// <summary>
		/// 当前Emuera是否处于活动状态
		/// </summary>
		internal bool IsActive
		{
            get
            {
                return !(
                    window == null || 
                    !window.Created 
                    //|| Form.ActiveForm == null
                    );
            }
        }

		/// <summary>
		/// 脚本是否正在继续运行
		/// 输入相关（包括消息跳过和宏）应参照IsInProcess
		/// </summary>
		internal bool IsRunning
		{
			get
			{
				if (state == ConsoleState.Initializing)
					return true;
				return (state == ConsoleState.Running || runningERBfromMemory);
			}
		}

		internal bool IsInProcess
		{
			get
			{
				if (state == ConsoleState.Initializing)
					return true;
				if (state == ConsoleState.Sleep)
					return true;
				if (inProcess)
					return true;
				return (state == ConsoleState.Running || runningERBfromMemory);
			}
		}

		internal bool IsError
		{
			get
			{
				return state == ConsoleState.Error;
			}
		}

		internal bool IsWaitingEnterKey
		{
			get
			{
				if ((state == ConsoleState.Quit) || (state == ConsoleState.Error))
					return true;
				if(state == ConsoleState.WaitInput)
					return (inputReq.InputType == InputType.AnyKey || inputReq.InputType == InputType.EnterKey);
				return false;
			}
		}

        internal bool IsWaitAnyKey
        {
            get
			{
				return (state == ConsoleState.WaitInput && inputReq.InputType == InputType.AnyKey);
            }
        }

        internal bool IsWaintingOnePhrase
        {
            get
            {
				return (state == ConsoleState.WaitInput && inputReq.OneInput);
            }
        }

		internal bool IsRunningTimer
		{
			get
			{
				return (state == ConsoleState.WaitInput && inputReq.Timelimit > 0 && !isTimeout);
			}
		}

		internal bool IsWaitingPrimitive
		{
			get
			{
				if (state == ConsoleState.WaitInput)
					return (inputReq.InputType == InputType.PrimitiveMouseKey);
				return false;
			}
		}
		
		internal string SelectedString
		{
			get
			{
				if (selectingButton == null)
					return null;
				if (state == ConsoleState.Error)
					return selectingButton.Inputs;
				if (state != ConsoleState.WaitInput)
					return null;
				if (inputReq.InputType == InputType.IntValue && (selectingButton.IsInteger))
					return selectingButton.Input.ToString();
				if (inputReq.InputType == InputType.StrValue)
					return selectingButton.Inputs;
				return null;
			}
		}

		public void Initialize()
		{
			GlobalStatic.Console = this;
			GlobalStatic.MainWindow = window;
            emuera = new GameProc.Process(this);
			GlobalStatic.Process = emuera;
			if (Program.DebugMode && Config.DebugShowWindow)
			{
				OpenDebugDialog();
				window.Focus();
			}
			ClearDisplay();
			if (!emuera.Initialize())
			{
				state = ConsoleState.Error;
				OutputLog(null);
				PrintFlush(false);
				RefreshStrings(true);
				return;
			}
			callEmueraProgram("");
			RefreshStrings(true);
		}
		

        public void Quit() { state = ConsoleState.Quit; }
		public void ThrowTitleError(bool error)
		{
			state = ConsoleState.Error;
			notToTitle = true;
			byError = error;
		}
		public void ThrowError(bool playSound)
		{
			if (playSound)
				uEmuera.Media.SystemSounds.Hand.Play();
			forceUpdateGeneration();
			UseUserStyle = false;
			PrintFlush(false);
			RefreshStrings(false);
			state = ConsoleState.Error;
		}

        public bool notToTitle = false;
        public bool byError = false;
        //public ScriptPosition ErrPos = null;

		#region button相关
		bool lastButtonIsInput = true;
        public bool updatedGeneration = false;
		int lastButtonGeneration = 0;//最后添加的选项的世代。与此世代不一致的选项无法选择。
		int newButtonGeneration = 0;//下一次添加的选项的世代。每次Input或Inputs递增
		//public int LastButtonGeneration { get { return lastButtonGeneration; } }
		public int NewButtonGeneration { get { return newButtonGeneration; } }
        public void UpdateGeneration() { lastButtonGeneration = newButtonGeneration; updatedGeneration = true; }
        public void forceUpdateGeneration() { newButtonGeneration++; lastButtonGeneration = newButtonGeneration; updatedGeneration = true; }
        LogicalLine lastInputLine;

		private void newGeneration()
		{
            //不要求输入值时应该不需要更新
			if (state != ConsoleState.WaitInput || !inputReq.NeedValue)
				return;
            if (!updatedGeneration && emuera.getCurrentLine != lastInputLine)
            {
                //如果在没有按钮的情况下来到下一个输入，则强制更新世代
                lastButtonGeneration = newButtonGeneration;
            }
            else
                updatedGeneration = false;
            lastInputLine = emuera.getCurrentLine;
			//防止选择旧选项。使INPUT中使用的选项不能转用于INPUTS。
			if (inputReq.InputType == InputType.IntValue)
			{
				if (lastButtonGeneration == newButtonGeneration)
					unchecked { newButtonGeneration++; }
				else if (!lastButtonIsInput)
					lastButtonGeneration = newButtonGeneration;
				lastButtonIsInput = true;
			}
			if (inputReq.InputType == InputType.StrValue)
			{
				if (lastButtonGeneration == newButtonGeneration)
					unchecked { newButtonGeneration++; }
				else if (lastButtonIsInput)
					lastButtonGeneration = newButtonGeneration;
				lastButtonIsInput = false;
			}
		}

		/// <summary>
		/// 选中的按钮。必须与INPUT或INPUTS对应
		/// </summary>
		ConsoleButtonString selectingButton = null;
		ConsoleButtonString lastSelectingButton = null;
		public ConsoleButtonString SelectingButton { get { return selectingButton; } }
		public bool ButtonIsSelected(ConsoleButtonString button) { return selectingButton == button; }

		/// <summary>
		/// 已显示ToolTip的标志
		/// </summary>
		bool tooltipUsed = false;
		/// <summary>
		/// 鼠标正下方的文本。也可以是按钮。
		/// 用于ToolTip显示。忽略世代，在历史记录中也显示
		/// </summary>
		ConsoleButtonString pointingString = null;
		ConsoleButtonString lastPointingString = null;
		#endregion

		#region Input & Timer相关

		//bool hasDefValue = false;
		//Int64 defNum;
		//string defStr;

		private InputRequest inputReq = null;
		public void Await(int time)
		{
			if (!Enabled || state != ConsoleState.Running)
			{
				this.Quit();
				return;
			}
			RefreshStrings(true);
			state = ConsoleState.Sleep;
			emuera.UpdateCheckInfiniteLoopState();

			//System.Windows.Forms.Application.DoEvents();
			//if (time > 0)
			//	System.Threading.Thread.Sleep(time);

			////如果在DoEvents()期间窗口被关闭就结束了。
			//if (!Enabled || state != ConsoleState.Sleep)
			//{
			//	ReadAnyKey();
			//	return;
			//}

			state = ConsoleState.Running;
		}

		public void WaitInput(InputRequest req)
		{
			state = ConsoleState.WaitInput;
			inputReq = req;
			if (req.Timelimit > 0)
			{
				if (req.OneInput)
					window.update_lastinput();
				presetTimer();
//				setTimer();
			}
			//updateMousePosition();
			//Point point = window.MainPicBox.PointToClient(Control.MousePosition);
			//if (window.MainPicBox.ClientRectangle.Contains(point))
			//{
			//	PrintFlush(false);
			//	MoveMouse(point);
			//}
		}

		public void ReadAnyKey(bool anykey = false, bool stopMesskip = false)
		{
			InputRequest req = new InputRequest();
			if (!anykey)
				req.InputType = InputType.EnterKey;
			else
				req.InputType = InputType.AnyKey;
			req.StopMesskip = stopMesskip;
			inputReq = req;
			state = ConsoleState.WaitInput;
			emuera.NeedWaitToEventComEnd = false;
		}


		/// <summary>
		/// INPUT期间的动画用定时器
		/// </summary>
		Timer redrawTimer = null;

		private void tickRedrawTimer(object sender, EventArgs e)
		{
			if (!redrawTimer.Enabled)
				return;
			//不在等待INPUT时，或带有定时器的INPUT状态时，交给其他处理
			if (state != ConsoleState.WaitInput || timer.Enabled)
			{
				return;
			}
			window.Refresh();//触发OnPaint
		}

		/// <summary>
		/// 动画用定时器的设置。指定0以下的值则停止定时器
		/// </summary>
		public void setRedrawTimer(uint tickcount)
		{
			if (tickcount <= 0)
			{
				redrawTimer.Enabled = false;
				return;
			}
			if (tickcount < 10)
				tickcount = 10;
			redrawTimer.Interval = tickcount;
			redrawTimer.Enabled = true;
		}



		Timer timer = null;
		Int64 timerID = -1;
		Int64 timer_startTime;//当前定时器启动时的毫秒数（以WinmmTimer.TickCount为基准）
		Int64 timer_nextDisplayTime;//TINPUT系列中下次显示剩余时间时的TickCount毫秒数
		Int64 timer_endTime;//当前定时器结束时的TickCount毫秒数
        bool wait_timeout = false;
        bool isTimeout = false;
        public bool IsTimeOut { get { return isTimeout; } }

		/// <summary>
		/// 1824 在TINPUT时不直接设置定时器，而是在第一次重绘完成后设置定时器（否则会陷入TINPUT和重绘的死循环）
		/// </summary>
		bool need_settimer = false;

		private void presetTimer()
		{
			need_settimer = true;
			if (inputReq.DisplayTime)
			{
				//如果不足100ms，会瞬间显示剩余0并结束
				//timer_nextDisplayTime = timer_startTime + 100;
				long start = inputReq.Timelimit / 100;
				string timeString1 = "残り ";
				string timeString2 = ((double)start / 10.0).ToString();
				PrintSingleLine(timeString1 + timeString2);
			}
		}
		private void setTimer()
		{
			isTimeout = false;
			timerID = inputReq.ID;
			timer.Enabled = true;
			timer_startTime = WinmmTimer.TickCount;
			timer_endTime = timer_startTime + inputReq.Timelimit;
			//if (inputReq.DisplayTime)
			//设置下次显示剩余时间的时机。如果inputReq.DisplayTime不为true，则仅设置但不被引用（应该
			timer_nextDisplayTime = timer_startTime + 100;

		}
        public void NeedSetTimer()
        {
            if(need_settimer)
            {
                need_settimer = false;
                setTimer();
            }
        }

		//通用
		private void tickTimer(object sender, EventArgs e)
		{
			if (!timer.Enabled)
				return;
			if (state != ConsoleState.WaitInput || inputReq.Timelimit <= 0 || timerID != inputReq.ID)
			{
#if UEMUERA_DEBUG
				throw new ExeEE("");
#else
				stopTimer();
				return;
#endif
			}
			long curtime = WinmmTimer.TickCount;
			if (curtime >= timer_endTime)
			{
				endTimer();
				return;
			}
			if (inputReq.DisplayTime && curtime >= timer_nextDisplayTime)
			{
				//由于显示耗时导致定时器停止，因此下次绘制在100ms后。某些情况下显示会一跳0.2。
				timer_nextDisplayTime = curtime + 100;
				long time = (timer_endTime - curtime) / 100;
				string timeString1 = "残り ";
				string timeString2 = ((double)time / 10.0).ToString();
				changeLastLine(timeString1 + timeString2);
			}
		}

		private void stopTimer()
		{
			//if (state == ConsoleState.WaitKeyWithTimerF && countTime < timeLimit)
			//{
			//	wait_timeout = true;
			//	while (countTime < timeLimit)
			//	{
			//		Application.DoEvents();
			//	}
			//	wait_timeout = false;
			//}
			timer.Enabled = false;
            //timer.Dispose();
		}

		/// <summary>
		/// 仅从tickTimer调用
		/// </summary>
		private void endTimer()
		{
            if (wait_timeout)
                return;
			stopTimer();
            isTimeout = true;
			if(IsWaitingPrimitive)
			{
				//callEmueraProgram在调用方执行。
				InputMouseKey(4, 0, 0, 0,0);
				return;
			}
			if (inputReq.DisplayTime)
				changeLastLine(inputReq.TimeUpMes);
			else if (inputReq.TimeUpMes != null)
				PrintSingleLine(inputReq.TimeUpMes);
			callEmueraProgram("");//默认输入的处理在callEmueraProgram侧进行
			if (state == ConsoleState.WaitInput && inputReq.NeedValue)
			{
				Point point = window.MainPicBox.PointToClient(Control.MousePosition);
				if (window.MainPicBox.ClientRectangle.Contains(point))
					MoveMouse(point);
			}
			RefreshStrings(true);
		}

        public void forceStopTimer()
        {
            if (timer.Enabled)
            {
                timer.Enabled = false;
            }
        }
		#endregion

		#region Call相关
		/// <summary>
		/// 脚本执行。不会调用RefreshStrings，需要调用方执行
		/// </summary>
		/// <param name="str"></param>
		private void callEmueraProgram(string str)
		{
			//不进行输入字符串的显示处理时str == null
			if (str != null)
			{
				//将INPUT字符串PRINT的处理等
				if (!doInputToEmueraProgram(str))
					return;
				if (state == ConsoleState.Error)
					return;
			}
			state = ConsoleState.Running;
			emuera.DoScript();
			if (state == ConsoleState.Running)
			{//如果为Running，则Process应该继续处理
				state = ConsoleState.Error;
                PrintError("emueraのエラー：プログラムの状態を特定できません");
			}
			if (state == ConsoleState.Error && !noOutputLog)
				OutputLog(Program.ExeDir + "emuera.log");
			PrintFlush(false);
			//1819 Refresh由调用方执行
			//RefreshStrings(false);
			newGeneration();
		}

		private bool doInputToEmueraProgram(string str)
		{
			if (state == ConsoleState.WaitInput)
			{
				Int64 inputValue;

				switch (inputReq.InputType)
				{
					case InputType.IntValue:
						if (string.IsNullOrEmpty(str) && inputReq.HasDefValue && !IsRunningTimer)
						{
							inputValue = inputReq.DefIntValue;
							str = inputValue.ToString();
						}
						else if (!Int64.TryParse(str, out inputValue))
							return false;
						if (inputReq.IsSystemInput)
							emuera.InputSystemInteger(inputValue);
						else
							emuera.InputInteger(inputValue);
						break;
					case InputType.StrValue:
						if (string.IsNullOrEmpty(str) && inputReq.HasDefValue && !IsRunningTimer)
							str = inputReq.DefStrValue;
						//空输入和超时
						if (str == null)
							str = "";
						emuera.InputString(str);
						break;
				}
				stopTimer();
			}
			Print(str);
			PrintFlush(false);
			return true;
		}
		#endregion

		#region 输入相关
		readonly string[] spliter = new string[] { "\\n", "\r\n", "\n", "\r" };//真正的换行符应该不会出现，但以防万一

		public bool MesSkip = false;
		private bool inProcess = false;
		volatile public bool KillMacro = false;
		
		internal void MouseWheel(Point point, int delta)
		{
			if (!IsWaitingPrimitive)
				return;
			//point是以客户端左上角为基准的坐标。
			//将clientPoint替换为以客户端左下角为基准的坐标
			Point clientPoint = point;
			clientPoint.Y = point.Y - ClientHeight;
			InputMouseKey(2, delta, clientPoint.X, clientPoint.Y, 0);
		}

		internal void MouseDown(Point point, MouseButtons button)
		{
			if (!IsWaitingPrimitive)
				return;
			//point是以客户端左上角为基准的坐标。
			//将clientPoint替换为以客户端左下角为基准的坐标
			Point clientPoint = point;
			clientPoint.Y = point.Y - ClientHeight;
			int buttonNum = -1;
			if(cbgButtonMap != null && cbgButtonMap.IsCreated)
			{
				//替换为以地图图像左上角为基准的坐标
				Point mapPoint = clientPoint;
				mapPoint.Y = clientPoint.Y + cbgButtonMap.Height;
				if(mapPoint.X >= 0 && mapPoint.Y >= 0 && mapPoint.X < cbgButtonMap.Width && mapPoint.Y < cbgButtonMap.Height)
				{
					Color c = cbgButtonMap.Bitmap.GetPixel(mapPoint.X, mapPoint.Y);
					if(c.A == 255)
					{
						buttonNum = c.ToArgb() & 0xFFFFFF;
					}
				}

			}
			InputMouseKey(1, (int)button, clientPoint.X, clientPoint.Y, buttonNum);
		}

		//1823 捕获按键输入
		internal void PressPrimitiveKey(int keycode, int keydata, int keymod)
		{
			if (IsWaitingPrimitive)
				InputMouseKey(3, keycode, keydata, 0, 0);
		}

		//1823 捕获按键输入
		internal void InputMouseKey(int type, int result1, int result2, int result3, int result4)
		{
			emuera.InputResult5(type, result1, result2, result3, result4);

			inProcess = true;
			try
			{
				//1823 Esc键、宏、右键均不可用。仅发送按下的键。
				callEmueraProgram(null);
				if (state == ConsoleState.WaitInput && inputReq.NeedValue)
				{
					Point point = window.MainPicBox.PointToClient(Control.MousePosition);
					if (window.MainPicBox.ClientRectangle.Contains(point))
						MoveMouse(point);
				}
			}
			finally
			{
				inProcess = false;
			}
			RefreshStrings(true);
		}

		public void PressEnterKey(bool keySkip, string str, bool changedByMouse)
		{
			MesSkip = keySkip;
			if ((state == ConsoleState.Running) || (state == ConsoleState.Initializing))
				return;
			else if ((state == ConsoleState.Quit))
			{
				window.Close();
				return;
			}
			else if (state == ConsoleState.Error)
			{
				if (str == ErrorButtonsText && selectingButton != null && selectingButton.ErrPos != null)
				{
					openErrorFile(selectingButton.ErrPos);
					return;
				}
				window.Close();
				return;
			}
#if UEMUERA_DEBUG
			if (state != ConsoleState.WaitInput || inputReq == null)
				throw new ExeEE("");
#endif
			KillMacro = false;
			try
			{
				string[] text;
				if(changedByMouse)//1823 如果是通过鼠标输入的，则不进行宏解析
				{ text = new string[] { str }; }
				else
				{
					if (str.StartsWith("@") && !inputReq.OneInput)
					{
						doSystemCommand(str);
						return;
					}
					if (inputReq.InputType == InputType.Void)
						return;
					if (timer.Enabled &&
						(inputReq.InputType == InputType.AnyKey || inputReq.InputType == InputType.EnterKey))
						stopTimer();
					//if((inputReq.InputType == InputType.IntValue || inputReq.InputType == InputType.StrValue)
					if (str.Contains("("))
						str = parseInput(new StringStream(str), false);
					text = str.Split(spliter, StringSplitOptions.None);
				}
				
				inProcess = true;
				for (int i = 0; i < text.Length; i++)
				{
					string inputs = text[i];
					if (inputs.IndexOf("\\e") >= 0)
					{
						inputs = inputs.Replace("\\e", "");//去除\e
						MesSkip = true;
					}

					if (inputReq.OneInput && (!Config.AllowLongInputByMouse || !changedByMouse) && inputs.Length > 1)
						inputs = inputs.Remove(1);
					//1819 TODO:在输入无效类（强制等待TWAIT）中，是停止跳过和宏还是保持原样
					//目前保持原样。虽然在强制等待中无法开始跳过，但如果已在跳过状态则可以跳过。
					if (inputReq.InputType == InputType.Void)
					{
						i--;
						inputs = "";
					}
					callEmueraProgram(inputs);
					RefreshStrings(false);
					while (MesSkip && state == ConsoleState.WaitInput)
					{
						//TODO:可以允许输入无效吗？可以在宏中跳过停止吗？
						if (inputReq.NeedValue)
							break;
						if (inputReq.StopMesskip)
							break;
						callEmueraProgram("");
						RefreshStrings(false);
						//如果不调用DoEvent，甚至连绘制处理都不会执行
						//Application.DoEvents();
						//Esc同时是宏停止和跳过开始键，因此即使通过Esc停止了跳过也会立即重新开始，所以没什么意义
						//if (KillMacro)
						//	goto endMacro;
					}
					MesSkip = false;
					if (state != ConsoleState.WaitInput)
						break;
					//在宏循环时不会发生等待处理，因此在此处处理系统队列
					//Application.DoEvents();
#if UEMUERA_DEBUG
					if (state != ConsoleState.WaitInput || inputReq == null)
						throw new ExeEE("");
#endif
					if (KillMacro)
						goto endMacro;
				}
			}
			finally
			{
				inProcess = false;
			}
			endMacro:
			if(state == ConsoleState.WaitInput && inputReq.NeedValue)
			{
				Point point = window.MainPicBox.PointToClient(Control.MousePosition);
				if (window.MainPicBox.ClientRectangle.Contains(point))
					MoveMouse(point);
			}
			RefreshStrings(true);
		}

		private void openErrorFile(ScriptPosition pos)
		{
			ProcessStartInfo pInfo = new ProcessStartInfo();
			pInfo.FileName = Config.TextEditor;
			string fname = pos.Filename.ToUpper();
			if (fname.EndsWith(".CSV"))
			{
				if (fname.Contains(Program.CsvDir.ToUpper()))
					fname = fname.Replace(Program.CsvDir.ToUpper(), "");
				fname = Program.CsvDir + fname;
			}
			else
			{
				//在解析模式下，查看的文件不一定在ERB\下，且拥有完整路径，因此不需要进行此修正
				if (!Program.AnalysisMode)
				{
					if (fname.Contains(Program.ErbDir.ToUpper()))
						fname = fname.Replace(Program.ErbDir.ToUpper(), "");
					fname = Program.ErbDir + fname;
				}
			}
			switch (Config.EditorType)
			{
				case TextEditorType.SAKURA:
					pInfo.Arguments = "-Y=" + pos.LineNo.ToString() + " \"" + fname + "\"";
					break;
				case TextEditorType.TERAPAD:
					pInfo.Arguments = "/jl=" + pos.LineNo.ToString() + " \"" + fname + "\"";
					break;
				case TextEditorType.EMEDITOR:
					pInfo.Arguments = "/l " + pos.LineNo.ToString() + " \"" + fname + "\"";
					break;
				case TextEditorType.USER_SETTING:
					if (Config.EditorArg != "" && Config.EditorArg != null)
						pInfo.Arguments = Config.EditorArg + pos.LineNo.ToString() + " \"" + fname + "\"";
					else
						pInfo.Arguments = fname;
					break;
			}
			try
			{
				System.Diagnostics.Process.Start(pInfo);
			}
			catch (System.ComponentModel.Win32Exception)
			{
				uEmuera.Media.SystemSounds.Hand.Play();
				PrintError("エディタを開くことができませんでした");
				forceUpdateGeneration();
			}
			return;
		}

        string parseInput(StringStream st, bool isNest)
        {
            StringBuilder sb = new StringBuilder(20);
            StringBuilder num = new StringBuilder(20);
            bool hasRet = false;
            int res = 0;
            while (!st.EOS && (!isNest || st.Current != ')'))
            {
                if (st.Current == '(')
                {
                    st.ShiftNext();
                    string tstr = parseInput(st, true);

                    if (!st.EOS)
                    {
                        st.ShiftNext();
                        if (st.Current == '*')
                        {
                            st.ShiftNext();
                            while (char.IsNumber(st.Current))
                            {
                                num.Append(st.Current);
                                st.ShiftNext();
                            }
                            if (num.ToString() != "" && num.ToString() != null)
                            {
                                int.TryParse(num.ToString(), out res);
                                for (int i = 0; i < res; i++)
                                    sb.Append(tstr);
                                num.Remove(0, num.Length);
                            }
                        }
                        else
                            sb.Append(tstr);
                        continue;
                    }
                    else
                    {
                        sb.Append(tstr);
                        break;
                    }
                }
                else if (st.Current == '\\')
                {
                    st.ShiftNext();
                    switch (st.Current)
                    {
                        case 'n':
                            if (!hasRet)
                                sb.Append('\n');
                            else
                                hasRet = false;
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'e':
                            sb.Append("\\e\n");
                            hasRet = true;
                            break;
                        case '\n':
                            break;
                        default:
                            sb.Append(st.Current);
                            break;
                    }
                }
                else
                    sb.Append(st.Current);
                st.ShiftNext();
            }
            return sb.ToString();
        }


		bool runningERBfromMemory = false;
		/// <summary>
		/// 来自普通控制台的Debug命令，以及调试窗口的变量监视等，
		/// 正在执行不存在*.ERB文件的脚本
		/// 1750 从IsDebug改名
		/// </summary>
		public bool RunERBFromMemory { get { return runningERBfromMemory; } set { runningERBfromMemory = value; } }
		void doSystemCommand(string command)
		{
			if(timer.Enabled)
			{
				PrintError("タイマー系命令の待ち時間中はコマンドを入力できません");
				PrintError("");//可能会被定时器显示处理清除
				RefreshStrings(true);
				return;
			}
			if (IsInProcess)
			{
				PrintError("スクリプト実行中はコマンドを入力できません");
				RefreshStrings(true);
				return;
			}
			StringComparison sc = Config.SCVariable;
			Print(command);
			PrintFlush(false);
			RefreshStrings(true);
			string com = command.Substring(1);
			if (com.Length == 0)
				return;
			if (com.Equals("REBOOT", sc))
			{
				window.Reboot();
				return;
			}
			else if (com.Equals("OUTPUT", sc) || com.Equals("OUTPUTLOG", sc))
			{
				this.OutputLog(Program.ExeDir + "emuera.log");
				return;
			}
			else if ((com.Equals("QUIT", sc)) || (com.Equals("EXIT", sc)))
			{
				window.Close();
				return;
			}
			else if (com.Equals("CONFIG", sc))
			{
				window.ShowConfigDialog();
				return;
			}
			else if (com.Equals("DEBUG", sc))
			{
				if (!Program.DebugMode)
				{
					PrintError("デバッグウインドウは-Debug引数付きで起動したときのみ使えます");
					RefreshStrings(true);
					return;
				}
				OpenDebugDialog();
			}
			else
			{
				if (!Config.UseDebugCommand)
				{
					PrintError("デバッグコマンドを使用できない設定になっています");
					RefreshStrings(true);
					return;
				}
				//将处理移至DebugMode相关
				DebugCommand(com, Config.ChangeMasterNameIfDebug, false);
				PrintFlush(false);
			}
			RefreshStrings(true);
		}
		#endregion

		#region 描画系
		uint lastUpdate = 0;
		uint msPerFrame = 1000 / 60;//60FPS
		ConsoleRedraw redraw = ConsoleRedraw.Normal;
        public ConsoleRedraw Redraw { get { return redraw; } }
		public void SetRedraw(Int64 i)
		{
			if ((i & 1) == 0)
				redraw = ConsoleRedraw.None;
			else
				redraw = ConsoleRedraw.Normal;
			if ((i & 2) != 0)
				RefreshStrings(true);
		}

		string debugTitle = null;
		public void SetWindowTitle(string str)
		{
			if (Program.DebugMode)
			{
				debugTitle = str;
				window.Text = str + " (Debug Mode)";
			}
			else
				window.Text = str;
		}

        public void SetEmueraVersionInfo(string str)
        {
            window.TextBox.Text = str;
        }
		public string GetWindowTitle()
		{
			if (Program.DebugMode && debugTitle != null)
				return debugTitle;
			return window.Text;
		}


		/// <summary>
		/// 从1818以前的RefreshStrings中提取selectingButton部分
		/// 在此触发OnPaint
		/// </summary>
		public void RefreshStrings(bool force_Paint)
		{
			bool isBackLog = window.ScrollBar.Value != window.ScrollBar.Maximum;
			//日志显示不受REDRAW设置的影响
			if ((redraw == ConsoleRedraw.None) && (!force_Paint) && (!isBackLog))
				return;
			//选中按钮的适应性检查
			if (selectingButton != null)
			{
				//历史显示中选项无效→使已移出屏幕的按钮也能从历史中选择
				//if (isBackLog)
				//	selectingButton = null;
				//如果不是等待数值或字符串输入的状态则无效
				if(state != ConsoleState.Error && state != ConsoleState.WaitInput)
					selectingButton = null;
				else if((state == ConsoleState.WaitInput) && !inputReq.NeedValue)
					selectingButton = null;
				//如果选项不是最新的则无效
				else if (selectingButton.Generation != lastButtonGeneration)
					selectingButton = null;
			}
			if (!force_Paint)
			{//如果是force则一定重绘。
				//如果不在历史显示中、已显示最后一行、且选中按钮未更改，则无需更新
				if ((!isBackLog) && (lastDrawnLineNo == lineNo) && (lastSelectingButton == selectingButton))
					return;
				//Environment.TickCount分辨率太差，因此调用winmm的定时器
				uint sec = WinmmTimer.TickCount - lastUpdate;
				//如果还未到重写时机，则等待下一次更新
				//但是，在等待输入等暂时没有更新时机的情况下，尝试强制重写
				if (sec < msPerFrame && (state == ConsoleState.Running || state == ConsoleState.Initializing))
					return;
			}
			if (forceTextBoxColor)
			{
				uint sec = WinmmTimer.TickCount - lastBgColorChange;
				//为防止颜色变化过快，在一定时间内再次调用时强制等待
				//while (sec < 200)
				//{
				//	//Application.DoEvents();
				//	sec = WinmmTimer.TickCount - lastBgColorChange;
				//}
				window.TextBox.BackColor = this.bgColor;
				lastBgColorChange = WinmmTimer.TickCount;
			}
			verticalScrollBarUpdate();
			window.Refresh();//触发OnPaint

		}

		///// <summary>
		///// 将1818以前RefreshStrings的后半与m_RefreshStrings融合
		///// 仅使用全清法，因此变得简洁。双缓冲应由OnPaint自动处理
		///// </summary>
		///// <param name="graph"></param>
		//public void OnPaint(Graphics graph)
		//{
		//	//因为如果在绘制过程中Emuera被关闭，可能会访问已废弃的PictureBox
		//	//虽然刚收到OnPaint的graph，应该没问题，但以防万一
		//	if (!this.Enabled)
		//		return;

		//	//应该在发出绘制命令的Refresh时执行，还是在OnPaint开始时执行，还是在OnPaint结束时执行
		//	lastUpdate = WinmmTimer.TickCount;

		//	bool isBackLog = window.ScrollBar.Value != window.ScrollBar.Maximum;
		//	int pointY = window.MainPicBox.Height - Config.LineHeight;

		//	int bottomLineNo = window.ScrollBar.Value - 1;
		//	if (displayLineList.Count - 1 < bottomLineNo)
		//		bottomLineNo = displayLineList.Count - 1;//1820 虽然觉得这个处理不需要，但因为有错误报告，所以保留
		//	int topLineNo = bottomLineNo - (pointY / Config.LineHeight + 1);
		//	if (topLineNo < 0)
		//		topLineNo = 0;
		//	pointY -= (bottomLineNo - topLineNo) * Config.LineHeight;

            
		//	if (Config.TextDrawingMode == TextDrawingMode.WINAPI)
		//	{
		//		GDI.GDIStart(graph, this.bgColor);
		//		GDI.FillRect(new Rectangle(0, 0, window.MainPicBox.Width, window.MainPicBox.Height));
		//		//for (int i = bottomLineNo; i >= topLineNo; i--)
		//		//{
		//		//	displayLineList[i].GDIDrawTo(pointY, isBackLog);
		//		//	pointY -= Config.LineHeight;
		//		//}
		//		//1820a12 改为从上到下绘制
		//		for (int i =topLineNo ; i <= bottomLineNo; i++)
		//		{
		//			displayLineList[i].GDIDrawTo(pointY, isBackLog);
		//			pointY += Config.LineHeight;
		//		}
		//		GDI.GDIEnd(graph);
		//	}
		//	else
		//	{
		//		graph.Clear(this.bgColor);
		//		//for (int i = bottomLineNo; i >= topLineNo; i--)
		//		//{
		//		//	displayLineList[i].DrawTo(graph, pointY, isBackLog, true, Config.TextDrawingMode);
		//		//	pointY -= Config.LineHeight;
		//		//}
		//		//1820a12 改为从上到下绘制
		//		for (int i =topLineNo ; i <= bottomLineNo; i++)
		//		{
		//			displayLineList[i].DrawTo(graph, pointY, isBackLog, true, Config.TextDrawingMode);
		//			pointY += Config.LineHeight;
		//		}

		//	}

		//	//ToolTip绘制

		//	if (lastPointingString != pointingString)
		//	{
		//		if (tooltipUsed)
		//			window.ToolTip.RemoveAll();
		//		if (pointingString != null && !string.IsNullOrEmpty(pointingString.Title))
		//		{
		//			window.ToolTip.SetToolTip(window.MainPicBox, pointingString.Title);
		//			tooltipUsed = true;
		//		}
		//		lastPointingString = pointingString;
		//	}
		//	if (isBackLog)
		//		lastDrawnLineNo = -1;
		//	else
		//		lastDrawnLineNo = lineNo;
		//	lastSelectingButton = selectingButton;
		//	/*调试用。假设绘制极其沉重的环境
		//	System.Threading.Thread.Sleep(50);
		//	*/
		//	forceTextBoxColor = false;
		//}

		public void SetToolTipColor(Color foreColor, Color backColor)
		{
			window.ToolTip.ForeColor = foreColor;
			window.ToolTip.BackColor = backColor;

		}
		public void SetToolTipDelay(int delay)
		{
			window.ToolTip.InitialDelay = delay;
		}

        int tooltip_duration = 0;
        public void SetToolTipDuration(int duration)
        {
            tooltip_duration = duration;
        }


        //private Graphics getGraphics()
        //{
        //	//想删除但怕出问题，所以保留
        //	if (!window.Created)
        //		throw new ExeEE("存在しないウィンドウにアクセスした");
        //	//if (Config.UseImageBuffer)
        //	//	return Graphics.FromImage(window.MainPicBox.Image);
        //	//else
        //		return window.MainPicBox.CreateGraphics();
        //}

        #endregion

        #region DebugMode系
        DebugDialog dd = null;
		public DebugDialog DebugDialog { get { return dd; } }
		StringBuilder dConsoleLog = new StringBuilder("");
		public string DebugConsoleLog { get { return dConsoleLog.ToString(); } }
		List<string> dTraceLogList = new List<string>();
#pragma warning disable CS0414 // 字段 'EmueraConsole.dTraceLogChanged' 已被赋值，但值从未被使用。
		bool dTraceLogChanged = true;
#pragma warning restore CS0414 // 字段 'EmueraConsole.dTraceLogChanged' 已被赋值，但值从未被使用。
		public string GetDebugTraceLog(bool force)
		{
			//if (!dTraceLogChanged && !force)
			//	return null;
			StringBuilder builder = new StringBuilder("");
			LogicalLine line = emuera.GetScaningLine();
			builder.AppendLine("*実行中の行");
			if ((line == null) || (line.Position == null))
			{
				builder.AppendLine("ファイル名:なし");
				builder.AppendLine("行番号:なし 関数名:なし");
				builder.AppendLine("");
			}
			else
			{
				builder.AppendLine("ファイル名:" + line.Position.Filename);
				builder.AppendLine("行番号:" + line.Position.LineNo.ToString() + " 関数名:" + line.ParentLabelLine.LabelName);
				builder.AppendLine("");
			}
			builder.AppendLine("*スタックトレース");
			for (int i = dTraceLogList.Count - 1; i >= 0; i--)
			{
				builder.AppendLine(dTraceLogList[i]);
			}
			return builder.ToString();
		}
		public void OpenDebugDialog()
		{
			if (!Program.DebugMode)
				return;
			if (dd != null)
			{
				if (dd.Created)
				{
					dd.Focus();
					return;
				}
				else
				{
					dd.Dispose();
					dd = null;
				}
			}
			dd = new DebugDialog();
			dd.SetParent(this, emuera);
			dd.Show();
		}

		public void DebugPrint(string str)
		{
			if (!Program.DebugMode)
				return;
			dConsoleLog.Append(str);
		}

		public void DebugClear()
		{
			dConsoleLog.Remove(0, dConsoleLog.Length);
		}

		public void DebugNewLine()
		{
			if (!Program.DebugMode)
				return;
			dConsoleLog.Append(Environment.NewLine);
		}

		public void DebugAddTraceLog(string str)
		{
			//如果Emuera未以调试模式启动则忽略
			//如果正在执行ERB文件以外的内容（调试命令、变量监视）则忽略
			if (!Program.DebugMode || runningERBfromMemory)
				return;
			dTraceLogChanged = true;
			dTraceLogList.Add(str);
		}
		public void DebugRemoveTraceLog()
		{
			if (!Program.DebugMode || runningERBfromMemory)
				return;
			dTraceLogChanged = true;
			if(dTraceLogList.Count > 0)
				dTraceLogList.RemoveAt(dTraceLogList.Count-1);
		}
		public void DebugClearTraceLog()
		{
			if (!Program.DebugMode || runningERBfromMemory)
				return;
			dTraceLogChanged = true;
			dTraceLogList.Clear();
		}

		public void DebugCommand(string com, bool munchkin, bool outputDebugConsole)
		{
			ConsoleState temp_state = state;
			runningERBfromMemory = true;
            //为防脚本等失败而进行的预防性保存
            GlobalStatic.Process.saveCurrentState(false);
            try
			{
				LogicalLine line = null;
				if (!com.StartsWith("@") && !com.StartsWith("\"") && !com.StartsWith("\\"))
					line = LogicalLineParser.ParseLine(com, null);
				if (line == null || (line is InvalidLine))
				{
					WordCollection wc = LexicalAnalyzer.Analyse(new StringStream(com), LexEndWith.EoL, LexAnalyzeFlag.None);
					IOperandTerm term = ExpressionParser.ReduceExpressionTerm(wc, TermEndWith.EoL);
					if (term == null)
						throw new CodeEE("解釈不能なコードです");
					if (term.GetOperandType() == typeof(Int64))
					{
						if (outputDebugConsole)
							com = "DEBUGPRINTFORML {" + com + "}";
						else
							com = "PRINTVL " + com;
					}
					else
					{
						if (outputDebugConsole)
							com = "DEBUGPRINTFORML %" + com + "%";
						else
							com = "PRINTFORMSL " + com;
					}
					line = LogicalLineParser.ParseLine(com, null);
				}
				if (line == null)
					throw new CodeEE("解釈不能なコードです");
				if (line is InvalidLine)
					throw new CodeEE(line.ErrMes);
				if (!(line is InstructionLine))
					throw new CodeEE("デバッグコマンドで使用できるのは代入文か命令文だけです");
				InstructionLine func = (InstructionLine)line;
				if (func.Function.IsFlowContorol())
					throw new CodeEE("フロー制御命令は使用できません");
				//如果查看__METHOD_SAFE__的话可能不需要
				if (func.Function.IsWaitInput())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				//1750 因为和__METHOD_SAFE__条件基本相同
				if (!func.Function.IsMethodSafe())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				//1756 不能在SIF之后出现的东西在这里也不可用。
				if (func.Function.IsPartial())
					throw new CodeEE(func.Function.Name + "命令は使用できません");
				switch (func.FunctionCode)
				{//遗漏处理
					//相反，OUTPUTLOG、QUIT在DebugCommand之前捕获
					case FunctionCode.PUTFORM:
					case FunctionCode.UPCHECK:
					case FunctionCode.CUPCHECK:
					case FunctionCode.SAVEDATA:
						throw new CodeEE(func.Function.Name + "命令は使用できません");
				}
				ArgumentParser.SetArgumentTo(func);
				if (func.IsError)
					throw new CodeEE(func.ErrMes);
				emuera.DoDebugNormalFunction(func, munchkin);
				if (func.FunctionCode == FunctionCode.SET)
				{
					if (!outputDebugConsole)
						PrintSingleLine(com);
					//DebugWindow那边会稍显冗长，可能不需要
				}
			}
			catch (Exception e)
			{
				if (outputDebugConsole)
				{
					DebugPrint(e.Message);
					DebugNewLine();
				}
				else
					PrintError(e.Message);
				emuera.clearMethodStack();
			}
			finally
			{
                //确实恢复为原始状态
                GlobalStatic.Process.loadPrevState();
                runningERBfromMemory = false;
				state = temp_state;
			}
		}
		#endregion

		#region Window.Form系

		internal Point GetMousePosition()
		{
            //if (window == null || !window.Created)
            //	return new Point();
            ////获取以客户端左上角为基准的坐标
            //Point pos = window.MainPicBox.PointToClient(Cursor.Position);
            ////替换为以客户端左下角为基准的坐标
            //pos.Y = pos.Y - ClientHeight;
            //return pos;
            return Point.Empty;
		}

		/// <summary>
		/// 将鼠标位置反映到按钮的选择状态
		/// </summary>
		/// <param name="point"></param>
		/// <returns>此后是否需要RefreshStrings</returns>
		public bool MoveMouse(Point point)
		{
            return false;
		//	if (cbgButtonMap != null && cbgButtonMap.IsCreated)
		//	{
		//		//point是以客户端左上角为基准的坐标。
		//		//将clientPoint替换为以客户端左下角为基准的坐标
		//		Point clientPoint = point;
		//		clientPoint.Y = point.Y - ClientHeight;
		//		int buttonNum = -1;
		//		//替换为以地图图像左上角为基准的坐标
		//		Point mapPoint = clientPoint;
		//		mapPoint.Y = mapPoint.Y + cbgButtonMap.Height;
		//		if (mapPoint.X >= 0 && mapPoint.Y >= 0 && mapPoint.X < cbgButtonMap.Width && mapPoint.Y < cbgButtonMap.Height)
		//		{
		//			Color c = cbgButtonMap.Bitmap.GetPixel(mapPoint.X, mapPoint.Y);
		//			if (c.A == 255)
		//			{
		//				buttonNum = c.ToArgb() & 0xFFFFFF;
		//			}
		//		}
		//		if (buttonNum >= 0)
		//		{
		//			bool ret = (pointingString != null || selectingButton != null || buttonNum != selectingCBGButtonInt);
		//			selectingCBGButtonInt = buttonNum;
		//			pointingString = null;
		//			selectingButton = null;
		//			return ret;
		//		}
		//		else if (selectingCBGButtonInt >= 0)
		//		{
		//			selectingCBGButtonInt = -1;
		//			pointingString = null;
		//			selectingButton = null;
		//			return true;
		//		}
		//	}
		//	selectingCBGButtonInt = -1;
		//	ConsoleButtonString select = null;
		//	ConsoleButtonString pointing = null;
		//	bool canSelect = false;
		//	//数値か文字列の入力待ち状態でなければ選択中にはならない
		//	if (state == ConsoleState.Error)
		//		canSelect = true;
		//	else if (state == ConsoleState.WaitInput && inputReq.NeedValue)
		//		canSelect = true;
		//	//脚本执行中忽略//输入・宏处理中忽略
		//	if(this.IsInProcess)
		//		goto end;
		//	//历史显示中忽略
		//	//if (window.ScrollBar.Value != window.ScrollBar.Maximum)
		//	//	goto end;
		//	int pointX = point.X;
		//	int pointY = point.Y;
		//	ConsoleDisplayLine curLine = null;

		//	int bottomLineNo = window.ScrollBar.Value - 1;
		//	if (displayLineList.Count - 1 < bottomLineNo)
		//		bottomLineNo = displayLineList.Count - 1;//1820 虽然觉得这个处理不需要，但因为有错误报告，所以保留
		//	int topLineNo = bottomLineNo - (window.MainPicBox.Height/ Config.LineHeight);
		//	if (topLineNo < 0)
		//		topLineNo = 0;
		//	int relPointY = pointY - window.MainPicBox.Height;
		//	//从下向上搜索，一旦发现即停止
		//	for (int i = bottomLineNo; i >= topLineNo; i--)
		//	{
		//		relPointY += Config.LineHeight;
		//		curLine = displayLineList[i];
				
		//		for (int b = 0; b < curLine.Buttons.Length; b++)
		//		{
		//			ConsoleButtonString button = curLine.Buttons[curLine.Buttons.Length - b - 1];
		//			if(button == null || button.StrArray == null)
		//				continue;
		//			if ((button.PointX <= pointX) && (button.PointX + button.Width >= pointX))
		//			{
		//				//if (relPointY >= 0 && relPointY <= Config.FontSize)
		//				//{
		//				//	pointing = button;
		//				//	if(pointing.IsButton)
		//				//		goto breakfor;
		//				//}
		//				foreach(AConsoleDisplayPart part in button.StrArray)
		//				{
		//					if(part == null)
		//						continue;
		//					if ((part.PointX <= pointX) && (part.PointX + part.Width >= pointX)
		//						&& (relPointY >= part.Top) && (relPointY <= part.Bottom))
		//					{
		//						pointing = button;
		//						if (pointing.IsButton)
		//							goto breakfor;
		//					}
		//				}
		//			}
		//		}
		//	}


		//	//int posy_bottom2up = window.MainPicBox.Height - pointY;
		//	//int logNum = window.ScrollBar.Maximum - window.ScrollBar.Value;
		//	////显示中的最下面行号
		//	//int curBottomLineNo = displayLineList.Count - logNum;
		//	//int curPointingLineNo = curBottomLineNo - (posy_bottom2up / Config.LineHeight + 1);
		//	//if ((curPointingLineNo < 0) || (curPointingLineNo >= displayLineList.Count))
		//	//	curLine = null;
		//	//else
		//	//	curLine =  displayLineList[curPointingLineNo];
		//	//if (curLine == null)
		//	//	goto end;
			
		//	//pointing = curLine.GetPointingButton(pointX);
		//breakfor:
		//	if ((pointing == null) || (pointing.Generation != lastButtonGeneration))
		//		canSelect = false;
		//	else if (!pointing.IsButton)
		//		canSelect = false;
		//	else if ((state == ConsoleState.WaitInput && inputReq.InputType == InputType.IntValue) && (!pointing.IsInteger))
		//		canSelect = false;
		//end:
		//	if (canSelect)
		//		select = pointing;
		//	bool needRefresh = select != selectingButton || pointing != pointingString;
		//	pointingString = pointing;
		//	selectingButton = select;
		//	return needRefresh;
		}


		public void LeaveMouse()
		{
			bool needRefresh = selectingButton != null || pointingString != null;
			selectingButton = null;
			pointingString = null;
			if(needRefresh)
			{
				RefreshStrings(true);
			}
		}

		private void verticalScrollBarUpdate()
		{
			int max = displayLineList.Count;
			int move = max - window.ScrollBar.Maximum;
			if (move == 0)
				return;
			if (move > 0)
			{
				window.ScrollBar.Maximum = max;
				window.ScrollBar.Value += move;
			}
			else
			{
				if (max > window.ScrollBar.Value)
					window.ScrollBar.Value = max;
				window.ScrollBar.Maximum = max;
			}
			window.ScrollBar.Enabled = max > 0;
		}
		#endregion

		public void GotoTitle()
		{
			//if (state == ConsoleState.Error)
			//{
			//    MessageBox.Show("エラー発生時はこの機能は使えません");
			//}
            forceStopTimer();
			ClearDisplay();
            redraw = ConsoleRedraw.Normal;
            UseUserStyle = false;
            userStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
            uEmuera.Utils.ResourcePrepareSimple();
            emuera.BeginTitle();
			ReadAnyKey(false, false);
			callEmueraProgram("");
			RefreshStrings(true);
		}

		bool force_temporary = false;
        bool timer_suspended = false;
		ConsoleState prevState;
		InputRequest prevReq;

		public void ReloadErb()
		{
			if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
            if (timer.Enabled)
            {
				timer.Enabled = false;
                timer_suspended = true;
            }
            prevState = state;
			prevReq = inputReq;
			state = ConsoleState.Initializing;
			PrintSingleLine("ERB再読み込み中……", true);
			force_temporary = true;
			emuera.ReloadErb();
			force_temporary = false;
            PrintSingleLine("再読み込み完了", true);
			RefreshStrings(true);
            //防止强制切换按钮世代
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }

		public void ReloadErbFinished()
		{
			state = prevState;
			inputReq = prevReq;
			PrintSingleLine(" ");
            if (timer_suspended)
            {
                timer_suspended = false;
                timer.Enabled = true;
            }
		}

		public void ReloadPartialErb(List<string> path)
		{
			if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
            if (timer.Enabled)
            {
				timer.Enabled = false;
                timer_suspended = true;
            }
			prevState = state;
			prevReq = inputReq;
			state = ConsoleState.Initializing;
            PrintSingleLine("ERB再読み込み中……", true);
			force_temporary = true;
			emuera.ReloadPartialErb(path);
			force_temporary = false;
            PrintSingleLine("再読み込み完了", true);
			RefreshStrings(true);
            //防止强制切换按钮世代
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }

		public void ReloadFolder(string erbPath)
		{
            if (state == ConsoleState.Error)
			{
				MessageBox.Show("エラー発生時はこの機能は使えません");
				return;
			}
			if (state == ConsoleState.Initializing)
			{
				MessageBox.Show("初期化中はこの機能は使えません");
				return;
			}
            if (timer.Enabled)
            {
				timer.Enabled = false;
                timer_suspended = true;
            }
            List<string> paths = new List<string>();
			SearchOption op = SearchOption.AllDirectories;
			if (!Config.SearchSubdirectory)
				op = SearchOption.TopDirectoryOnly;
			var fnames = new List<string>(Directory.GetFiles(erbPath, "*.ERB", op));
#if UNITY_ANDROID && !UNITY_EDITOR
            fnames.AddRange(Directory.GetFiles(erbPath, "*.erb", op));
#endif
            for (int i = 0; i < fnames.Count; i++)
				if (Path.GetExtension(fnames[i]).ToUpper() == ".ERB")
					paths.Add(fnames[i]);
            fnames.Clear();

            bool notRedraw = false;
            if (redraw == ConsoleRedraw.None)
            {
                notRedraw = true;
                redraw = ConsoleRedraw.Normal;
            }
			prevState = state;
			prevReq = inputReq;
			state = ConsoleState.Initializing;
            PrintSingleLine("ERB再読み込み中……", true);
			force_temporary = true;
            emuera.ReloadPartialErb(paths);
			force_temporary = false;
            PrintSingleLine("再読み込み完了", true);
			RefreshStrings(true);
            //防止强制切换按钮世代
            updatedGeneration = true;
            if (notRedraw)
                redraw = ConsoleRedraw.None;
        }

		public void Dispose()
		{
			if(timer != null)
				timer.Dispose();
			//timer = null;
			//stringMeasure.Dispose();
		}
	}
}