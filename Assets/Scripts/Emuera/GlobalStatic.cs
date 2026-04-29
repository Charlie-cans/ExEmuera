using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameData;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameView;
using uEmuera.Window;

namespace MinorShift.Emuera
{
	/* 1756 创建
	 * 曾几何时，设计原则是尽量将数据设为private，只让需要的东西引用，但这已是过去的事了。
	 * 每次修改，Process.Instance.XXX之类的东西就不断增加。
	 * 嘛，对于增加这件事就认命吧，至少把那些不规范引用方式的东西集中到一处来管理，这就是本计划。
	 * 今后不再将Instance公开为public static，而是从这里引用。
	 * 不过，如果可以的话，还是希望减少从这里引用的数量。
	 */
	internal static class GlobalStatic
	{
		//这些按生成顺序排列。
		//如果从下向上引用，可能会返回null。
		//替换配置
		public static MainWindow MainWindow;
		public static EmueraConsole Console;
		public static Process Process;
		//重命名字典配置
		public static GameBase GameBaseData;
		public static ConstantData ConstantData;
		public static VariableData VariableData;
		//字符串格式
		public static VariableEvaluator VEvaluator;
		public static IdentifierDictionary IdentifierDictionary;
		public static ExpressionMediator EMediator;
		//
		public static LabelDictionary LabelDictionary;


		//用于向ERBloader传递参数解析结果的桥接变量
		//1756 从Process移来。用于Program.AnalysisMode
		public static Dictionary<string, Int64> tempDic = new Dictionary<string, long>();
#if UEMUERA_DEBUG
		public static List<FunctionLabelLine> StackList = new List<FunctionLabelLine>();
#endif
		public static void Reset()
		{
			Process = null;
			ConstantData = null;
			GameBaseData = null;
			EMediator = null;
			VEvaluator = null;
			VariableData = null;
			Console = null;
			MainWindow = null;
			LabelDictionary = null;
			IdentifierDictionary = null;
			SqliteManager.Reset();
			tempDic.Clear();
		}
	}
}
