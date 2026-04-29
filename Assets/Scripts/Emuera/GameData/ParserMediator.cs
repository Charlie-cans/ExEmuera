using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Function;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameView;
using System.IO;
using System.Text.RegularExpressions;

namespace MinorShift.Emuera
{
	// 1756 新增。汇总 Parser、LexicalAnalyzer 等需要的信息
	// 本来或许应该作为参数传递，但修改所有 Parser 的参数很麻烦，所以设为 static
	internal static class ParserMediator
	{
		/// <summary>
		/// 在 emuera.config 等中产生的警告
		/// 在 Initialize 之前发生
		/// </summary>
		/// <param name="str"></param>
		/// <param name="?"></param>
		public static void ConfigWarn(string str, ScriptPosition pos, int level, string stack)
		{
			if (level < Config.DisplayWarningLevel && !Program.AnalysisMode)
				return;
			warningList.Add(new ParserWarning(str, pos, level, stack));
		}

		static EmueraConsole console;
		public static void Initialize(EmueraConsole console)
		{
			ParserMediator.console = console;
		}

		#region Rename
		public static Dictionary<string, string> RenameDic { get; private set; }
		// 1756 从 Process.Load.cs 移入
		public static void LoadEraExRenameFile(string filepath)
		{
			if (RenameDic != null)
				RenameDic.Clear();
			// 无论如何先创建字典。字典为 null 时仅在使用 UseRenameFile 为 NO 时发生
			RenameDic = new Dictionary<string, string>();
			EraStreamReader eReader = new EraStreamReader(false);
			if ((!File.Exists(filepath)) || (!eReader.Open(filepath)))
			{
				return;
			}
			string line = null;
			ScriptPosition pos = null;
			Regex reg = new Regex(@"\\,", RegexOptions.Compiled);
			try
			{
                string[] tokens = new string[2];
                while ((line = eReader.ReadLine()) != null)
				{
					if (line.Length == 0)
						continue;
					if (line.StartsWith(";"))
						continue;
					string[] baseTokens = reg.Split(line);
					if (!baseTokens[baseTokens.Length - 1].Contains(","))
						continue;
					string[] last = baseTokens[baseTokens.Length - 1].Split(',');
					baseTokens[baseTokens.Length - 1] = last[0];
					//string[] tokens = new string[2];
					tokens[0] = string.Join(",", baseTokens);
					tokens[1] = last[1];
					pos = new ScriptPosition(eReader.Filename, eReader.LineNo, line);
					// 右侧为 ERB 中的标记，左侧为转换目标。
					string value = tokens[0].Trim();
					string key = string.Format("[[{0}]]", tokens[1].Trim());
					RenameDic[key] = value;
					pos = null;
				}
			}
			catch (Exception e)
			{
				if (pos != null)
					throw new CodeEE(e.Message, pos);
				else
					throw new CodeEE(e.Message);

			}
			finally
			{
				eReader.Close();
			}
		}
		#endregion


		public static void Warn(string str, ScriptPosition pos, int level)
		{
			Warn(str, pos, level, null);
		}

		public static void Warn(string str, ScriptPosition pos, int level, string stack)
		{
			if (level < Config.DisplayWarningLevel && !Program.AnalysisMode)
				return;
			if (console != null && !console.RunERBFromMemory)
				warningList.Add(new ParserWarning(str, pos, level, stack));
		}

		/// <summary>
		/// Parser 中的警告输出
		/// </summary>
		/// <param name="str"></param>
		/// <param name="line"></param>
		/// <param name="level">警告级别。0:轻微错误。1:可忽略的行。2:该行不执行则无害。3:致命</param>
		public static void Warn(string str, LogicalLine line, int level, bool isError, bool isBackComp)
		{
            Warn(str, line, level, isError, isBackComp, null);
		}

        public static void Warn(string str, LogicalLine line, int level, bool isError, bool isBackComp, string stack)
        {
            if (isError)
            {
                line.IsError = true;
                line.ErrMes = str;
            }
            if (level < Config.DisplayWarningLevel && !Program.AnalysisMode)
                return;
            if (isBackComp && !Config.WarnBackCompatibility)
                return;
            if (console != null && !console.RunERBFromMemory)
                warningList.Add(new ParserWarning(str, line.Position, level, stack));
            //				console.PrintWarning(str, line.Position, level);
        }
        
        private static List<ParserWarning> warningList = new List<ParserWarning>();

		public static bool HasWarning{get {return warningList.Count > 0;}}
		public static void ClearWarningList()
		{
			warningList.Clear();
		}

		public static void FlushWarningList()
		{
			for (int i = 0; i < warningList.Count; i++)
			{
				ParserWarning warning = warningList[i];
				console.PrintWarning(warning.WarningMes, warning.WarningPos, warning.WarningLevel);
                if (warning.StackTrace != null)
                {
                    string[] stacks = warning.StackTrace.Split('\n');
                    for (int j = 0; j < stacks.Length; j++)
                    {
						console.PrintSystemLine(stacks[j]);
                    }
                }
            }
			warningList.Clear();
		}

		private class ParserWarning
		{
			public ParserWarning(string mes, ScriptPosition pos, int level, string stackTrace)
			{
				WarningMes = mes;
				WarningPos = pos;
				WarningLevel = level;
                StackTrace = stackTrace;
			}
			public string WarningMes;
			public ScriptPosition WarningPos;
			public int WarningLevel;
            public string StackTrace;
		}
	}
}