using System;
using System.Collections.Generic;
using System.IO;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData;

namespace MinorShift.Emuera.GameProc
{
	internal sealed partial class Process
	{
		private string[] TrainName = null;
		delegate void SystemProcess();
		Dictionary<SystemStateCode, SystemProcess> systemProcessDictionary = new Dictionary<SystemStateCode, SystemProcess>();
		private void initSystemProcess()
		{
			comAble = new int[TrainName.Length];
			systemProcessDictionary.Add(SystemStateCode.Title_Begin, new SystemProcess(this.beginTitle));
			systemProcessDictionary.Add(SystemStateCode.Openning, new SystemProcess(this.endOpenning));

			systemProcessDictionary.Add(SystemStateCode.Train_Begin, new SystemProcess(this.beginTrain));
			systemProcessDictionary.Add(SystemStateCode.Train_CallEventTrain, new SystemProcess(this.endCallEventTrain));
			systemProcessDictionary.Add(SystemStateCode.Train_CallShowStatus, new SystemProcess(this.endCallShowStatus));
			systemProcessDictionary.Add(SystemStateCode.Train_CallComAbleXX, new SystemProcess(this.endCallComAbleXX));
			systemProcessDictionary.Add(SystemStateCode.Train_CallShowUserCom, new SystemProcess(this.endCallShowUserCom));
			systemProcessDictionary.Add(SystemStateCode.Train_WaitInput, new SystemProcess(this.trainWaitInput));
			systemProcessDictionary.Add(SystemStateCode.Train_CallEventCom, new SystemProcess(this.endEventCom));
			systemProcessDictionary.Add(SystemStateCode.Train_CallComXX, new SystemProcess(this.endCallComXX));
			systemProcessDictionary.Add(SystemStateCode.Train_CallSourceCheck, new SystemProcess(this.endCallSourceCheck));
			systemProcessDictionary.Add(SystemStateCode.Train_CallEventComEnd, new SystemProcess(this.endCallEventComEnd)); ;
			systemProcessDictionary.Add(SystemStateCode.Train_DoTrain, new SystemProcess(this.doTrain));

			systemProcessDictionary.Add(SystemStateCode.AfterTrain_Begin, new SystemProcess(this.beginAfterTrain));

			systemProcessDictionary.Add(SystemStateCode.Ablup_Begin, new SystemProcess(this.beginAblup));
			systemProcessDictionary.Add(SystemStateCode.Ablup_CallShowJuel, new SystemProcess(this.endCallShowJuel));
			systemProcessDictionary.Add(SystemStateCode.Ablup_CallShowAblupSelect, new SystemProcess(this.endCallShowAblupSelect));
			systemProcessDictionary.Add(SystemStateCode.Ablup_WaitInput, new SystemProcess(this.ablupWaitInput));
			systemProcessDictionary.Add(SystemStateCode.Ablup_CallAblupXX, new SystemProcess(this.endCallAblupXX));

			systemProcessDictionary.Add(SystemStateCode.Turnend_Begin, new SystemProcess(this.beginTurnend));

			systemProcessDictionary.Add(SystemStateCode.Shop_Begin, new SystemProcess(this.beginShop));
			systemProcessDictionary.Add(SystemStateCode.Shop_CallEventShop, new SystemProcess(this.endCallEventShop));
			systemProcessDictionary.Add(SystemStateCode.Shop_CallShowShop, new SystemProcess(this.endCallShowShop));
			systemProcessDictionary.Add(SystemStateCode.Shop_WaitInput, new SystemProcess(this.shopWaitInput));
			systemProcessDictionary.Add(SystemStateCode.Shop_CallEventBuy, new SystemProcess(this.endCallEventBuy));

			systemProcessDictionary.Add(SystemStateCode.SaveGame_Begin, new SystemProcess(this.beginSaveGame));
			systemProcessDictionary.Add(SystemStateCode.SaveGame_WaitInput, new SystemProcess(this.saveGameWaitInput));
			systemProcessDictionary.Add(SystemStateCode.SaveGame_WaitInputOverwrite, new SystemProcess(this.saveGameWaitInputOverwrite));
			systemProcessDictionary.Add(SystemStateCode.SaveGame_CallSaveInfo, new SystemProcess(this.endCallSaveInfo));
			systemProcessDictionary.Add(SystemStateCode.LoadGame_Begin, new SystemProcess(this.beginLoadGame));
			systemProcessDictionary.Add(SystemStateCode.LoadGame_WaitInput, new SystemProcess(this.loadGameWaitInput));
			systemProcessDictionary.Add(SystemStateCode.LoadGameOpenning_Begin, new SystemProcess(this.beginLoadGameOpening));
			systemProcessDictionary.Add(SystemStateCode.LoadGameOpenning_WaitInput, new SystemProcess(this.loadGameWaitInput));

			//stateEndProcessDictionary.Add(ProgramState.AutoSave_Begin, new stateEndProcess(this.beginAutoSave));
			systemProcessDictionary.Add(SystemStateCode.AutoSave_CallSaveInfo, new SystemProcess(this.endAutoSaveCallSaveInfo));
			systemProcessDictionary.Add(SystemStateCode.AutoSave_CallUniqueAutosave, new SystemProcess(this.endAutoSave));

			systemProcessDictionary.Add(SystemStateCode.LoadData_DataLoaded, new SystemProcess(this.beginDataLoaded));
			systemProcessDictionary.Add(SystemStateCode.LoadData_CallSystemLoad, new SystemProcess(this.endSystemLoad));
			systemProcessDictionary.Add(SystemStateCode.LoadData_CallEventLoad, new SystemProcess(this.endEventLoad));

			systemProcessDictionary.Add(SystemStateCode.Openning_TitleLoadgame, new SystemProcess(this.endTitleLoadgame));

			systemProcessDictionary.Add(SystemStateCode.System_Reloaderb, new SystemProcess(this.endReloaderb));
			systemProcessDictionary.Add(SystemStateCode.First_Begin, new SystemProcess(this.beginFirst));


			systemProcessDictionary.Add(SystemStateCode.Normal, new SystemProcess(this.endNormal));
			return;
		}



		Int64 systemResult = 0;
		int lastCalledComable = -1;
		int lastAddCom = -1;
		//(Train.csv中的值·未定义时为-1) == comAble[(显示的值)];
		int[] comAble;//


		private void runSystemProc()
		{
			//脚本执行中不应该到这里来
			//if (!state.ScriptEnd)
			//    throw new ExeEE("不正な呼び出し");

			//目前没有传递不存在内容的处理
			//if (systemProcessDictionary.ContainsKey(state.SystemState))
			systemProcessDictionary[state.SystemState]();
			//else
			//    throw new ExeEE("未定義の状態");

		}

		void setWait()
		{
			console.ReadAnyKey();
		}

		void setWaitInput()
		{
			InputRequest req = new InputRequest();
			req.InputType = InputType.IntValue;
			req.IsSystemInput = true;
			console.WaitInput(req);
		}


		private bool callFunction(string functionName, bool force, bool isEvent)
		{
			CalledFunction call = null;
			if (isEvent)
				call = CalledFunction.CallEventFunction(this, functionName, null);
			else
				call = CalledFunction.CallFunction(this, functionName, null);
			if (call == null)
				if (!force)
					return false;
				else
					throw new CodeEE("関数\"@" + functionName + "\"が見つかりません");
			//本来非事件函数只会给出一个函数，所以不可能满足条件
			//if ((!isEvent) && (call.Count > 1))
			//    throw new ExeEE("イベント関数でない関数\"@" + functionName + "\"の候補が複数ある");
			state.IntoFunction(call, null, null);
			return true;
		}

		//从CheckState()调用的函数群。到达ScriptEnd时的处理。

		void beginTitle()
		{
			//如果连续调教命令处理中的状态被遗留下来，在这里清除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			console.ResetStyle();
			deleteAllPrevState();
			if (Program.AnalysisMode)
			{
				console.PrintSystemLine("ファイル解析終了：Analysis.logに出力します");
				console.OutputLog(Program.ExeDir + "Analysis.log");
				console.noOutputLog = true;
				console.PrintSystemLine("エンターキーもしくはクリックで終了します");
				uEmuera.Media.SystemSounds.Asterisk.Play();
				console.ThrowTitleError(false);
				return;
			}
			if ((!noError) && (!Config.CompatiErrorLine))
			{
				console.PrintSystemLine("ERBコードに解釈不可能な行があるためEmueraを終了します");
				console.PrintSystemLine("※互換性オプション「" + Config.GetConfigName(ConfigCode.CompatiErrorLine) + "」により強制的に動作させることができます");
				console.PrintSystemLine("emuera.logにログを出力します");
				console.OutputLog(Program.ExeDir + "emuera.log");
				console.noOutputLog = true;
				console.PrintSystemLine("エンターキーもしくはクリックで終了します");
				//System.Media.SystemSounds.Asterisk.Play();
				console.ThrowTitleError(true);
				return;
			}
			if (callFunction("SYSTEM_TITLE", false, false))
			{//自定义
				state.SystemState = SystemStateCode.Normal;
				return;
			}
			//标准的标题画面
			console.PrintBar();
			console.NewLine();
			console.Alignment = GameView.DisplayLineAlignment.CENTER;
			console.PrintSingleLine(gamebase.ScriptTitle);
			if (gamebase.ScriptVersion != 0)
				console.PrintSingleLine(gamebase.ScriptVersionText);
			console.PrintSingleLine(gamebase.ScriptAutherName);
			console.PrintSingleLine("(" + gamebase.ScriptYear + ")");
			console.NewLine();
			console.PrintSingleLine(gamebase.ScriptDetail);
			console.Alignment = GameView.DisplayLineAlignment.LEFT;

			console.PrintBar();
			console.NewLine();
			console.PrintSingleLine("[0] " + Config.TitleMenuString0);
			console.PrintSingleLine("[1] " + Config.TitleMenuString1);
			openingInput();
			return;
		}

		void openingInput()
		{
			setWaitInput();
			state.SystemState = SystemStateCode.Openning;
			return;
		}

		void endOpenning()
		{
			if (systemResult == 0)
			{//[0] 从头开始
				vEvaluator.ResetData();
				//vEvaluator.AddCharacter(0, false);
				vEvaluator.AddCharacterFromCsvNo(0);
				if (gamebase.DefaultCharacter > 0)
					//vEvaluator.AddCharacter(gamebase.DefaultCharacter, false);
					vEvaluator.AddCharacterFromCsvNo(gamebase.DefaultCharacter);
				console.PrintBar();
				console.NewLine();
				beginFirst();
			}
			else if (systemResult == 1)
			{
				if (callFunction("TITLE_LOADGAME", false, false))
				{//自定义
					state.SystemState = SystemStateCode.Openning_TitleLoadgame;
				}
				else
				{//标准的LOADGAME
					beginLoadGameOpening();
				}
			}
			else//输入不正确则重写选择项，要求正确选择。
			{//改为与RESUELASTLINE相同的处理
				console.deleteLine(1);
				console.PrintTemporaryLine("無効な値です");
				console.updatedGeneration = true;
				openingInput();
				//beginTitle();
			}

		}

		void beginFirst()
		{
			state.SystemState = SystemStateCode.Normal;
			//如果连续调教命令处理中的状态被遗留下来，在这里清除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			callFunction("EVENTFIRST", true, true);
		}

		void endTitleLoadgame()
		{
			beginTitle();
		}

		void beginTrain()
		{
			vEvaluator.UpdateInBeginTrain();
			state.SystemState = SystemStateCode.Train_CallEventTrain;
			//调用EVENTTRAIN后转移到Train_CallEventTrain。
			if (!callFunction("EVENTTRAIN", false, true))
			{
				//如果不存在则跳过，视为Train_CallEventTrain已完成。
				endCallEventTrain();
			}
		}

		List<Int64> coms = new List<long>();
		bool isCTrain = false;
		int count = 0;
		bool skipPrint = false;
		public bool SkipPrint { get { return skipPrint; } set { skipPrint = value; } }
		void endCallEventTrain()
		{
			if (vEvaluator.NEXTCOM >= 0)
			{//NEXTCOM的处理
				state.SystemState = SystemStateCode.Train_CallEventCom;
				vEvaluator.SELECTCOM = vEvaluator.NEXTCOM;
				vEvaluator.NEXTCOM = 0;
				//由于代入的是0而非-1，所以除非ERB侧修改否则会无限循环，但这是eramaker以来的规范。
				callEventCom();
				return;
			}
			else
			{
				//if (!isCTrain)
				//{
				//调用SHOW_STATUS后转移到Train_CallShowStatus。
				if (isCTrain)
					skipPrint = true;
				callFunction("SHOW_STATUS", true, false);
				state.SystemState = SystemStateCode.Train_CallShowStatus;
				//}
				//else
				//{
				//如果是连续调教模式则进入COMABLE处理
				//	endCallShowStatus();
				//}
			}
		}

		void endCallShowStatus()
		{
			//SHOW_STATUS结束后重置ComAbleXX的调用状态，然后转移到Train_CallComAbleXX。
			state.SystemState = SystemStateCode.Train_CallComAbleXX;
			lastCalledComable = -1;
			lastAddCom = -1;
			printComCount = 0;
			for (int i = 0; i < comAble.Length; i++)
				comAble[i] = -1;
			endCallComAbleXX();
		}

		string getTrainComString(int trainCode, int comNo)
		{
			string trainName = TrainName[trainCode];
			return string.Format("{0}[{1,3}]", trainName, comNo);
		}

		int printComCount = 0;
		void endCallComAbleXX()
		{
			//添加选项。如果RESULT为0则只增加选项编号而不添加。
			if ((lastCalledComable >= 0) && (TrainName[lastCalledComable] != null))
			{
				lastAddCom++;
				if (vEvaluator.RESULT != 0)
				{
					comAble[lastAddCom] = lastCalledComable;
					if (!isCTrain)
					{
						console.PrintC(getTrainComString(lastCalledComable, lastAddCom), true);
						printComCount++;
						if ((Config.PrintCPerLine > 0) && (printComCount % Config.PrintCPerLine == 0))
							console.PrintFlush(false);
					}
					console.RefreshStrings(false);
				}
			}
			//ComAbleXX的调用。跳过train.csv中未定义的，如果找不到ComAbleXX则按RETURN 1处理。
			while (++lastCalledComable < TrainName.Length)
			{
				if (TrainName[lastCalledComable] == null)
					continue;
				string comName = string.Format("COM_ABLE{0}", lastCalledComable);
				if (!callFunction(comName, false, false))
				{
					lastAddCom++;
					if (Config.ComAbleDefault == 0)
						continue;
					comAble[lastAddCom] = lastCalledComable;
					if (!isCTrain)
					{
						console.PrintC(getTrainComString(lastCalledComable, lastAddCom), true);
						printComCount++;
						if ((Config.PrintCPerLine > 0) && (printComCount % Config.PrintCPerLine == 0))
							console.PrintFlush(false);
					}
					continue;
				}
				console.RefreshStrings(false);
				return;
			}
			//全部搜索完毕后结束，调用SHOW_USERCOM。
			if (lastCalledComable >= TrainName.Length)
			{
				state.SystemState = SystemStateCode.Train_CallShowUserCom;
				//if (!isCTrain)
				//{
				console.PrintFlush(false);
				console.RefreshStrings(false);
				callFunction("SHOW_USERCOM", true, false);
				//}
				//else
				//	endCallShowUserCom();
			}
		}

		void endCallShowUserCom()
		{
			if (skipPrint)
				skipPrint = false;
			vEvaluator.UpdateAfterShowUsercom();
			if (!isCTrain)
			{
				//设为等待数值输入状态并转移到Train_WaitInput。
				setWaitInput();

				state.SystemState = SystemStateCode.Train_WaitInput;
			}
			else
			{
				if (count < coms.Count)
				{
					systemResult = coms[count];
					count++;
					trainWaitInput();
				}
			}
		}

		void trainWaitInput()
		{
			int selectCom = -1;
			if (!isCTrain)
			{
				if ((systemResult >= 0) && (systemResult < comAble.Length))
					selectCom = comAble[systemResult];
			}
			else
			{
				for (int i = 0; i < comAble.Length; i++)
				{
					if (comAble[i] == systemResult)
						selectCom = (int)systemResult;
				}
				console.PrintSingleLine(string.Format("＜コマンド連続実行：{0}/{1}＞", count, coms.Count));
			}
			//TrainName已定义且可用（COMABLE返回了非0）
			if (selectCom >= 0)
			{
				vEvaluator.SELECTCOM = selectCom;
				callEventCom();
			}
			else
			{//未被定义或不可用。
				if (isCTrain)
					console.PrintSingleLine("コマンドを実行できませんでした");
				vEvaluator.RESULT = systemResult;
				state.SystemState = SystemStateCode.Train_CallEventComEnd;
				callFunction("USERCOM", true, false);
				//COM中需要的操作全部在USERCOM内完成。
			}
		}

		private Int64 doTrainSelectCom = -1;
		void doTrain()
		{
			vEvaluator.UpdateAfterShowUsercom();
			vEvaluator.SELECTCOM = doTrainSelectCom;
			callEventCom();
		}

		void callEventCom()
		{
			vEvaluator.UpdateAfterInputCom();
			state.SystemState = SystemStateCode.Train_CallEventCom;
			if (!callFunction("EVENTCOM", false, true))
				endEventCom();
			return;
		}

		void endEventCom()
		{
			long selectCom = vEvaluator.SELECTCOM;
			string comName = string.Format("COM{0}", selectCom);
			state.SystemState = SystemStateCode.Train_CallComXX;
			callFunction(comName, true, false);
		}

		void endCallComXX()
		{
			//执行失败
			if (vEvaluator.RESULT == 0)
			{
				//Com结束。
				endCallEventComEnd();
			}
			else
			{//如果成功则转移到SOURCE_CHECK。
				state.SystemState = SystemStateCode.Train_CallSourceCheck;
				callFunction("SOURCE_CHECK", true, false);
			}
		}

		void endCallSourceCheck()
		{
			//SOURCE在这里重置
			vEvaluator.UpdateAfterSourceCheck();
			//调用EVENTCOMEND后转移到Train_CallEventComEnd。
			state.SystemState = SystemStateCode.Train_CallEventComEnd;
			//如果EVENTCOMEND不存在，或EVENTCOMEND内没有执行WAIT系命令，则在EVENTCOMEND后添加WAIT。
			NeedWaitToEventComEnd = true;
			if (!callFunction("EVENTCOMEND", false, true))
			{
				//如果找不到则跳过，视为Train_CallEventComEnd已结束。
				endCallEventComEnd();
			}
		}
		public bool NeedWaitToEventComEnd = false;
		bool needCheck = true;
		void endCallEventComEnd()
		{
			if (console.LastLineIsTemporary && !isCTrain && needCheck)
			{
                if (console.LastLineIsEmpty)
                {
                    console.deleteLine(2);
                    console.PrintTemporaryLine("無効な値です");
                }
				console.updatedGeneration = true;
				endCallShowUserCom();
			}
			else
			{
				if (isCTrain && count == coms.Count)
				{
					isCTrain = false;
					skipPrint = false;
					coms.Clear();
					count = 0;
					if (callFunction("CALLTRAINEND", false, false))
					{
						needCheck = false;
						return;
					}
				}
				needCheck = true;
				////1.701	这里不需要WAIT。
				////setWait();
				//1.703 果然还是有需要的情况
				if (NeedWaitToEventComEnd)
					setWait();
				NeedWaitToEventComEnd = false;
				//从SHOW_STATUS重新开始。
				//处理与Train_CallEventTrain相同。
				endCallEventTrain();
			}
		}

		void beginAfterTrain()
		{
			//因为在连续调教模式中可能会到这里，所以在这里解除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			state.SystemState = SystemStateCode.Normal;
			//调用EVENTEND。因为exe侧不需要再掌握状态，所以转移到Normal。
			callFunction("EVENTEND", true, true);
		}

		void beginAblup()
		{
			//如果连续调教命令处理中的状态被遗留下来，在这里清除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			state.SystemState = SystemStateCode.Ablup_CallShowJuel;
			//调用SHOW_JUEL后转移到Ablup_CallShowJuel。
			callFunction("SHOW_JUEL", true, false);
		}

		void endCallShowJuel()
		{
			state.SystemState = SystemStateCode.Ablup_CallShowAblupSelect;
			//调用SHOW_ABLUP_SELECT后转移到Ablup_CallAblupSelect。
			callFunction("SHOW_ABLUP_SELECT", true, false);
		}

		void endCallShowAblupSelect()
		{
			//设为等待数值输入状态并转移到Ablup_WaitInput。
			setWaitInput();
			state.SystemState = SystemStateCode.Ablup_WaitInput;
		}

		void ablupWaitInput()
		{
			//即使未定义，若小于100则调用ABLUP，不调用USERABLUP。否则[99]反弹刻印等功能就无法实现。
			if ((systemResult >= 0) && (systemResult < 100))
			{
				state.SystemState = SystemStateCode.Ablup_CallAblupXX;
				string ablName = string.Format("ABLUP{0}", systemResult);
				if (!callFunction(ablName, false, false))
				{
					//如果找不到则结束
					console.deleteLine(1);
					console.PrintTemporaryLine("無効な値です");
					console.updatedGeneration = true;
					endCallShowAblupSelect();
				}
			}
			else
			{
				vEvaluator.RESULT = systemResult;
				state.SystemState = SystemStateCode.Ablup_CallAblupXX;
				callFunction("USERABLUP", true, false);
			}
		}

		void endCallAblupXX()
		{
			if (console.LastLineIsTemporary)
			{
                if (console.LastLineIsEmpty)
                {
                    console.deleteLine(2);
                    console.PrintTemporaryLine("無効な値です");
                }
				console.updatedGeneration = true;
				endCallShowAblupSelect();
			}
			else
				beginAblup();
		}

		void beginTurnend()
		{
			//如果连续调教命令处理中的状态被遗留下来，在这里清除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			//调用EVENTTURNEND后转移到Normal
			callFunction("EVENTTURNEND", true, true);
			state.SystemState = SystemStateCode.Normal;
		}

		void beginShop()
		{
			//如果连续调教命令处理中的状态被遗留下来，在这里清除
			if (isCTrain)
				if (ClearCommands())
					return;
			skipPrint = false;
			state.SystemState = SystemStateCode.Shop_CallEventShop;
			//调用EVENTSHOP后转移到Shop_CallEventShop。
			if (!callFunction("EVENTSHOP", false, true))
			{
				//如果不存在则跳过，视为Shop_CallEventShop已结束。
				endCallEventShop();
			}
		}

		void endCallEventShop()
		{
			saveTarget = -1;
			if (Config.AutoSave && state.calledWhenNormal)
				beginAutoSave();
			else
			{
				state.SystemState = SystemStateCode.AutoSave_Skipped;
				endAutoSaveCallSaveInfo();
			}
		}

		void beginAutoSave()
		{
			if (callFunction("SYSTEM_AUTOSAVE", false, false))
			{//如果@SYSTEM_AUTOSAVE存在则使用它。
				state.SystemState = SystemStateCode.AutoSave_CallUniqueAutosave;
				return;
			}
			saveTarget = AutoSaveIndex;
			vEvaluator.SAVEDATA_TEXT = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " ";
			state.SystemState = SystemStateCode.AutoSave_CallSaveInfo;
			if (!callFunction("SAVEINFO", false, false))
				endAutoSaveCallSaveInfo();//如果不存在则跳过
		}

		void endAutoSaveCallSaveInfo()
		{
			if (saveTarget == AutoSaveIndex)
			{
				if (!vEvaluator.SaveTo(saveTarget, vEvaluator.SAVEDATA_TEXT))
				{
					console.PrintError("オートセーブ中に予期しないエラーが発生しました");
					console.PrintError("オートセーブをスキップします");
					console.ReadAnyKey();
				}
			}
			endAutoSave();
		}

		void endAutoSave()
		{
			if (state.isBegun)
			{
				state.Begin();
				return;
			}
			state.SystemState = SystemStateCode.Shop_CallShowShop;
			//调用SHOW_SHOP后转移到Shop_CallShowShop
			callFunction("SHOW_SHOP", true, false);
		}

		void endCallShowShop()
		{
			//设为等待数值输入状态并转移到Shop_WaitInput。
			setWaitInput();
			state.SystemState = SystemStateCode.Shop_WaitInput;
		}

		//与PRINT_SHOPITEM是独立的。
		//即使有BOUGHT为100以上的道具且ITEMSALES为TRUE，也强制进入@USERSHOP。
		void shopWaitInput()
		{
			if ((systemResult >= 0) && (systemResult < Config.MaxShopItem))
			{
				if (vEvaluator.ItemSales(systemResult))
				{
					if (vEvaluator.BuyItem(systemResult))
					{
						state.SystemState = SystemStateCode.Shop_CallEventBuy;
						//调用EVENTBUY后转移到Shop_CallEventBuy
						if (!callFunction("EVENTBUY", false, true))
							endCallEventBuy();
						return;
					}
					else
					{
						//console.Print("お金が足りません。");
						//console.NewLine();
						console.deleteLine(1);
						console.PrintTemporaryLine("お金が足りません。");
					}
				}
				else
				{
					//console.Print("売っていません。");
					//console.NewLine();
					console.deleteLine(1);
					console.PrintTemporaryLine("売っていません。");
				}
				//购买失败时，返回endCallEventShop()。
				//endCallEventShop();
				endCallShowShop();
				return;
			}
			else
			{
				//更新RESULT
				vEvaluator.RESULT = systemResult;

				//调用USERSHOP后转移到Shop_CallEventBuy
				callFunction("USERSHOP", true, false);
				state.SystemState = SystemStateCode.Shop_CallEventBuy;
				return;
			}
		}

		void endCallEventBuy()
		{
			if (console.LastLineIsTemporary)
			{
                if (console.LastLineIsEmpty)
                {
                    console.deleteLine(2);
                    console.PrintTemporaryLine("無効な値です");
                }
				console.updatedGeneration = true;
				endCallShowShop();
			}
			else
			{
				//回到开头
				endAutoSave();
			}
		}


		void beginDataLoaded()
		{
			state.SystemState = SystemStateCode.LoadData_CallSystemLoad;
			
			if (!callFunction("SYSTEM_LOADEND", false, false))
				endSystemLoad();//如果不存在则跳过
		}
		void endSystemLoad()
		{
			state.SystemState = SystemStateCode.LoadData_CallEventLoad;
			//调用EVENTLOAD后转移到LoadData_CallEventLoad。
			if (!callFunction("EVENTLOAD", false, true))
			{
				//如果不存在则跳过，视为Train_CallEventTrain已完成。
				endAutoSave();
			}
		}

		void endEventLoad()
		{
			//如果在@EVENTLOAD中执行了BEGIN命令，就不会到这里来。
			//如果到了这里，则视为BEGIN SHOP。不进行自动存档。
			endAutoSave();
		}

		void beginSaveGame()
		{
			console.PrintSingleLine("何番にセーブしますか？");
			state.SystemState = SystemStateCode.SaveGame_Begin;
			printSaveDataText();
		}

		void beginLoadGame()
		{
			console.PrintSingleLine("何番をロードしますか？");
			state.SystemState = SystemStateCode.LoadGame_Begin;
			printSaveDataText();
		}

		void beginLoadGameOpening()
		{
			console.PrintSingleLine("何番をロードしますか？");
			state.SystemState = SystemStateCode.LoadGameOpenning_Begin;
			printSaveDataText();
		}

		bool[] dataIsAvailable = new bool[21];
		bool isFirstTime = true;
		const int AutoSaveIndex = 99;
		int page = 0;
		void printSaveDataText()
		{
			if (isFirstTime)
			{
				isFirstTime = false;
				dataIsAvailable = new bool[Config.SaveDataNos + 1];
			}
			int dataNo = 0;
			for (int i = 0; i < page; i++)
			{
				console.PrintFlush(false);
				console.Print(string.Format("[{0, 2}] セーブデータ{0, 2}～{1, 2}を表示", i * 20, i * 20 + 19));
			}
			for (int i = 0; i < 20; i++)
			{
				dataNo = page * 20 + i;
				if (dataNo == dataIsAvailable.Length - 1)
					break;
				dataIsAvailable[dataNo] = false;
				console.PrintFlush(false);
				console.Print(string.Format("[{0, 2}] ", dataNo));
				if (!writeSavedataTextFrom(dataNo))
					continue;
				dataIsAvailable[dataNo] = true;
			}
			for (int i = page; i < ((dataIsAvailable.Length - 2) / 20); i++)
			{
				console.PrintFlush(false);
				console.Print(string.Format("[{0, 2}] セーブデータ{0, 2}～{1, 2}を表示", (i + 1) * 20, (i + 1) * 20 + 19));
			}
			//自动存档的处理另外分离（出于显示处理的需要）
			dataIsAvailable[dataIsAvailable.Length - 1] = false;
			if (state.SystemState != SystemStateCode.SaveGame_Begin)
			{
				dataNo = AutoSaveIndex;
				console.PrintFlush(false);
				console.Print(string.Format("[{0, 2}] ", dataNo));
				if (writeSavedataTextFrom(dataNo))
					dataIsAvailable[dataIsAvailable.Length - 1] = true;
			}
			console.RefreshStrings(false);
			//绘制全部结束
			console.PrintSingleLine("[100] 戻る");
			setWaitInput();
			if (state.SystemState == SystemStateCode.SaveGame_Begin)
				state.SystemState = SystemStateCode.SaveGame_WaitInput;
			else if (state.SystemState == SystemStateCode.LoadGame_Begin)
				state.SystemState = SystemStateCode.LoadGame_WaitInput;
			else// if (state.SystemState == SystemStateCode.LoadGameOpenning_Begin)
				state.SystemState = SystemStateCode.LoadGameOpenning_WaitInput;
			//因为已被正确处理，所以不会到这里
			//else
			//    throw new ExeEE("異常な状態");
		}

		int saveTarget = -1;
		void saveGameWaitInput()
		{
			if (systemResult == 100)
			{
				//如果是取消则恢复之前的状态
				loadPrevState();
				return;
			}
			else if (((int)systemResult / 20) != page && systemResult != AutoSaveIndex && (systemResult >= 0 && systemResult < dataIsAvailable.Length - 1))
			{
				page = (int)systemResult / 20;
				state.SystemState = SystemStateCode.SaveGame_Begin;
				printSaveDataText();
				return;
			}
			bool available = false;
			if ((systemResult >= 0) && (systemResult < dataIsAvailable.Length - 1))
				available = dataIsAvailable[systemResult];
			else
			{//重新输入
				console.deleteLine(1);
				console.PrintTemporaryLine("無効な値です");
				console.updatedGeneration = true;
				setWaitInput();
				return;
			}
			saveTarget = (int)systemResult;
			//如果有已有数据则显示选项并转移到SaveGame_WaitInputOverwrite。
			if (available)
			{
				console.PrintSingleLine("既にデータが存在します。上書きしますか？");
				console.PrintC("[0] はい", false);
				console.PrintC("[1] 否", false);
				setWaitInput();
				state.SystemState = SystemStateCode.SaveGame_WaitInputOverwrite;
				return;
			}
			//如果没有已有数据则视为选择了"是"并直接跳转
			systemResult = 0;
			saveGameWaitInputOverwrite();
		}

		void saveGameWaitInputOverwrite()
		{
			if (systemResult == 1)//否
			{
				beginSaveGame();
				return;
			}
			else if (systemResult != 0)//也不是"是"
			{//重新输入
				console.deleteLine(1);
				console.PrintTemporaryLine("無効な値です");
				console.updatedGeneration = true;
				setWaitInput();
				return;
			}
			vEvaluator.SAVEDATA_TEXT = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " ";
			state.SystemState = SystemStateCode.SaveGame_CallSaveInfo;
			if (!callFunction("SAVEINFO", false, false))
				endCallSaveInfo();//如果不存在则跳过
		}

		void endCallSaveInfo()
		{
			if (!vEvaluator.SaveTo(saveTarget, vEvaluator.SAVEDATA_TEXT))
			{
				console.PrintError("セーブ中に予期しないエラーが発生しました");
				console.ReadAnyKey();
			}
			loadPrevState();
		}

		void loadGameWaitInput()
		{
			if (systemResult == 100)
			{//如果是取消
				//如果是开场则返回开场
				if (state.SystemState == SystemStateCode.LoadGameOpenning_WaitInput)
				{
					beginTitle();
					return;
				}
				//如果从其他地方来则恢复之前的状态
				loadPrevState();
				return;
			}
			else if (((int)systemResult / 20) != page && systemResult != AutoSaveIndex && (systemResult >= 0 && systemResult < dataIsAvailable.Length - 1))
			{
				page = (int)systemResult / 20;
				if (state.SystemState == SystemStateCode.LoadGameOpenning_WaitInput)
					state.SystemState = SystemStateCode.LoadGameOpenning_Begin;
				else
					state.SystemState = SystemStateCode.LoadGame_Begin;
				printSaveDataText();
				return;
			}
			bool available = false;
			if ((systemResult >= 0) && (systemResult < dataIsAvailable.Length - 1))
				available = dataIsAvailable[systemResult];
			else if (systemResult == AutoSaveIndex)
				available = dataIsAvailable[dataIsAvailable.Length - 1];
			else
			{//重新输入
				console.deleteLine(1);
				console.PrintTemporaryLine("無効な値です");
				console.updatedGeneration = true;
				setWaitInput();
				return;
			}
			if (!available)
			{
				console.PrintSingleLine(systemResult.ToString());
				console.PrintError("データがありません");
				if (state.SystemState == SystemStateCode.LoadGameOpenning_WaitInput)
				{
					beginLoadGameOpening();
					return;
				}
				beginLoadGame();
				return;
			}

			if (!vEvaluator.LoadFrom((int)systemResult))
				throw new ExeEE("ファイルのロード中に予期しないエラーが発生しました");
			deletePrevState();
			beginDataLoaded();
		}


		void endNormal()
		{
			throw new CodeEE("予期しないスクリプト終端です");
		}

		void endReloaderb()
		{
			loadPrevState();
			console.ReloadErbFinished();
		}

		private bool writeSavedataTextFrom(int saveIndex)
		{
			EraDataResult result = vEvaluator.CheckData(saveIndex, EraSaveFileType.Normal);
			console.Print(result.DataMes);
			console.NewLine();
			return result.State == EraDataState.OK;
		}

		//1808 移动到vEvaluator.SaveTo()等处
		//private bool loadFrom(int dataIndex)
		//private bool saveTo(int saveIndex, string saveText)
		//private string getSaveDataPath(int index)
	}

}