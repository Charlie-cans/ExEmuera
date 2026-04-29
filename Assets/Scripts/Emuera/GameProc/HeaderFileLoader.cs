using System;
using System.Collections.Generic;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameProc.Function;
using MinorShift._Library;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Function;

namespace MinorShift.Emuera.GameProc
{
	internal sealed class HeaderFileLoader
	{
		public HeaderFileLoader(EmueraConsole main, IdentifierDictionary idDic, Process proc)
		{
			output = main;
			parentProcess = proc;
			this.idDic = idDic;
		}
		readonly Process parentProcess;
		readonly EmueraConsole output;
		readonly IdentifierDictionary idDic;

		bool noError = true;
		Queue<DimLineWC> dimlines;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="erbDir"></param>
		/// <param name="displayReport"></param>
		/// <returns></returns>
		public bool LoadHeaderFiles(string headerDir, bool displayReport)
		{
			List<KeyValuePair<string, string>> headerFiles = Config.GetFiles(headerDir, "*.ERH");
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            headerFiles.AddRange(Config.GetFiles(headerDir, "*.erh"));
#endif
            bool noError = true;
			dimlines = new Queue<DimLineWC>();
			try
			{
				for (int i = 0; i < headerFiles.Count; i++)
				{
					string filename = headerFiles[i].Key;
					string file = headerFiles[i].Value;
					if (displayReport)
						output.PrintSystemLine(filename + "読み込み中・・・");
					noError = loadHeaderFile(file, filename);
					if (!noError)
						break;
					//System.Windows.Forms.//Application.DoEvents();
				}
				//即使发生错误，也检查已成功读取的部分
				if (dimlines.Count > 0)
				{
					//如果不是&=，这里发生的错误无法被捕获
					noError &= analyzeSharpDimLines();
				}

				dimlines.Clear();
			}
			finally
			{
				ParserMediator.FlushWarningList();
			}
			return noError;
		}


		private bool loadHeaderFile(string filepath, string filename)
		{
			StringStream st = null;
			ScriptPosition position = null;
			//EraStreamReader eReader = new EraStreamReader(false);
			//1815修正 _rename.csv的应用
			//从eramakerEX的规范来说应用到.ERH是不合适的，但已经成了Emuera的规范也只能接受
			EraStreamReader eReader = new EraStreamReader(true);

			if (!eReader.Open(filepath, filename))
			{
				throw new CodeEE(eReader.Filename + "のオープンに失敗しました");
				//return false;
			}
			try
			{
				while ((st = eReader.ReadEnabledLine()) != null)
				{
					if (!noError)
						return false;
					position = new ScriptPosition(filename, eReader.LineNo, st.RowString);
					LexicalAnalyzer.SkipWhiteSpace(st);
					if (st.Current != '#')
						throw new CodeEE("ヘッダーの中に#で始まらない行があります", position);
					st.ShiftNext();
					string sharpID = LexicalAnalyzer.ReadSingleIdentifier(st);
					if (sharpID == null)
					{
						ParserMediator.Warn("解釈できない#行です", position, 1);
						return false;
					}
					if (Config.ICFunction)
						sharpID = sharpID.ToUpper();
					LexicalAnalyzer.SkipWhiteSpace(st);
					switch (sharpID)
					{
						case "DEFINE":
							analyzeSharpDefine(st, position);
							break;
						case "FUNCTION":
						case "FUNCTIONS":
							analyzeSharpFunction(st, position, sharpID == "FUNCTIONS");
							break;
						case "DIM":
						case "DIMS":
							//1822 #DIM暂时保留，稍后批量处理
							{
								WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
								dimlines.Enqueue(new DimLineWC(wc, sharpID == "DIMS", false, position));
							}
							//analyzeSharpDim(st, position, sharpID == "DIMS");
							break;
						default:
							throw new CodeEE("#" + sharpID + "は解釈できないプリプロセッサです", position);
					}
				}
			}
			catch (CodeEE e)
			{
				if (e.Position != null)
					position = e.Position;
				ParserMediator.Warn(e.Message, position, 2);
				return false;
			}
			finally
			{
				eReader.Close();
			}
			return true;
		}

		//#define FOO (～～)     id to wc
		//#define BAR($1) (～～)     idwithargs to wc(replaced)
		//#diseble FOOBAR             
		//#dim piyo, i
		//#dims puyo, j
		//static List<string> keywordsList = new List<string>();

		private void analyzeSharpDefine(StringStream st, ScriptPosition position)
		{
			//在调用前执行LexicalAnalyzer.SkipWhiteSpace(st)。
			string srcID = LexicalAnalyzer.ReadSingleIdentifier(st);
			if (srcID == null)
				throw new CodeEE("置換元の識別子がありません", position);
			if (Config.ICVariable)
				srcID = srcID.ToUpper();

            //如果不在这里进行名称重复检查，会导致严重后果
            string errMes = "";
            int errLevel = -1;
            idDic.CheckUserMacroName(ref errMes, ref errLevel, srcID);
            if (errLevel >= 0)
            {
                ParserMediator.Warn(errMes, position, errLevel);
                if (errLevel >= 2)
                {
                    noError = false;
                    return;
                }
            }
            
            bool hasArg = st.Current == '(';//指定参数时，后面必须紧跟(。空格也不允许。
			//1808a3 允许赋值运算符（用于函数声明）
			WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
			if (wc.EOL)
			{
				//throw new CodeEE("置換先の式がありません", position);
				//1808a3 允许空宏
				DefineMacro nullmac = new DefineMacro(srcID, new WordCollection(), 0);
				idDic.AddMacro(nullmac);
				return;
			}

			List<string> argID = new List<string>();
			if (hasArg)//函数型宏的参数解析
			{
				wc.ShiftNext();//跳过'('
				if (wc.Current.Type == ')')
					throw new CodeEE("関数型マクロの引数を0個にすることはできません", position);
				while (!wc.EOL)
				{
					IdentifierWord word = wc.Current as IdentifierWord;
					if (word == null)
						throw new CodeEE("置換元の引数指定の書式が間違っています", position);
					word.SetIsMacro();
					string id = word.Code;
					if (argID.Contains(id))
						throw new CodeEE("置換元の引数に同じ文字が2回以上使われています", position);
					argID.Add(id);
					wc.ShiftNext();
					if (wc.Current.Type == ',')
					{
						wc.ShiftNext();
						continue;
					}
					if (wc.Current.Type == ')')
						break;
					throw new CodeEE("置換元の引数指定の書式が間違っています", position);
				}
				if (wc.EOL)
					throw new CodeEE("')'が閉じられていません", position);

				wc.ShiftNext();
			}
			if (wc.EOL)
				throw new CodeEE("置換先の式がありません", position);
			WordCollection destWc = new WordCollection();
			while (!wc.EOL)
			{
				destWc.Add(wc.Current);
				wc.ShiftNext();
			}
			if (hasArg)//函数型宏的参数设置
			{
				while (!destWc.EOL)
				{
					IdentifierWord word = destWc.Current as IdentifierWord;
					if (word == null)
					{
						destWc.ShiftNext();
						continue;
					}
					for (int i = 0; i < argID.Count; i++)
					{
						if (string.Equals(word.Code, argID[i], Config.SCVariable))
						{
							destWc.Remove();
							destWc.Insert(new MacroWord(i));
							break;
						}
					}
					destWc.ShiftNext();
				}
				destWc.Pointer = 0;
			}
			if (hasArg)//1808a3 禁用函数型宏
				throw new CodeEE("関数型マクロは宣言できません", position);
			DefineMacro mac = new DefineMacro(srcID, destWc, argID.Count);
			idDic.AddMacro(mac);
		}

		//private void analyzeSharpDim(StringStream st, ScriptPosition position, bool dims)
		//{
		//	//WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
		//	//UserDefinedVariableData data = UserDefinedVariableData.Create(wc, dims, false, position);
		//	//if (data.Reference)
		//	//	throw new NotImplCodeEE();
		//	//VariableToken var = null;
		//	//if (data.CharaData)
		//	//	var = parentProcess.VEvaluator.VariableData.CreateUserDefCharaVariable(data);
		//	//else
		//	//	var = parentProcess.VEvaluator.VariableData.CreateUserDefVariable(data);
		//	//idDic.AddUseDefinedVariable(var);
		//}

		//1822 仅汇总#DIM，稍后处理
		private bool analyzeSharpDimLines()
		{
			bool noError = true;
			bool tryAgain = true;
			while (dimlines.Count > 0)
			{
				int count = dimlines.Count;
				for (int i = 0; i < count; i++)
				{
					DimLineWC dimline = dimlines.Dequeue();
					try
					{
						UserDefinedVariableData data = UserDefinedVariableData.Create(dimline);
						if (data.Reference)
							throw new NotImplCodeEE();
						VariableToken var = null;
						if (data.CharaData)
							var = parentProcess.VEvaluator.VariableData.CreateUserDefCharaVariable(data);
						else
							var = parentProcess.VEvaluator.VariableData.CreateUserDefVariable(data);
						idDic.AddUseDefinedVariable(var);
					}
					catch (IdentifierNotFoundCodeEE e)
					{
						//如果通过重试有希望解决，则添加到队列末尾
						if (tryAgain)
						{
							dimline.WC.Pointer = 0;
							dimlines.Enqueue(dimline);
						}
						else
						{
							ParserMediator.Warn(e.Message, dimline.SC, 2);
							noError = true;
						}
					}
					catch (CodeEE e)
					{
						ParserMediator.Warn(e.Message, dimline.SC, 2);
						noError = false;
					}
				}
				if (dimlines.Count == count)
					tryAgain = false;
			}
			return noError;
		}

		private void analyzeSharpFunction(StringStream st, ScriptPosition position, bool funcs)
		{
			throw new NotImplCodeEE();
			//WordCollection wc = LexicalAnalyzer.Analyse(st, LexEndWith.EoL, LexAnalyzeFlag.AllowAssignment);
			//UserDefinedFunctionData data = UserDefinedFunctionData.Create(wc, funcs, position);
			//idDic.AddRefMethod(UserDefinedRefMethod.Create(data));
		}
	}
}
