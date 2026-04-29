using System;
//using System.Drawing;
using System.Collections.Generic;
//using System.Windows.Forms;
using MinorShift._Library;
using MinorShift.Emuera.GameView;
using MinorShift.Emuera.GameData.Expression;
using System.IO;
using uEmuera;
using uEmuera.Drawing;
using uEmuera.Forms;
using uEmuera.Window;

namespace MinorShift.Emuera
{
	public static class Program
	{
		/*
		代码起始点。
		在这里创建MainWindow，
		MainWindow创建Process，
		Process创建GameBase、ConstantData、Variable。
		
		
		*.ERB的读取、执行及其他处理由Process负责，
		输入输出由MainWindow负责，
		常量的保存由ConstantData负责，
		变量的管理由Variable负责。
		 
		原本是这样规划的，但在修改过程中边界变得模糊了。
		 
		后来添加了EmueraConsole，让它负责输入输出。
        
        1750 添加DebugConsole
         由于无法将Debug完全分离，部分也由EmueraConsole负责
		
		TODO: 1819 至少想把MainWindow & Console的输入/显示组与Process & Data的数据处理组分离开

		*/
		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		//[STAThread]
		public static void Main(string[] args)
		{

			ExeDir = Sys.ExeDir;
#if UEMUERA_DEBUG
			//debugMode = true;

			//通过向ExeDir赋值变体路径来进行测试运行的代码。
			//本地路径末尾必须加\。
			//记载了本地路径时，发布前必须删除。
			ExeDir = @"";
			
#endif
			CsvDir = ExeDir + "csv/";
			if (!Directory.Exists(CsvDir)){
				CsvDir = ExeDir + "CSV/";
			}
			ErbDir = ExeDir + "erb/";
			if (!Directory.Exists(ErbDir)){
				ErbDir = ExeDir + "ERB/";
			}
			DebugDir = ExeDir + "debug/";
			if (!Directory.Exists(DebugDir)){
				DebugDir = ExeDir + "DEBUG/";
			}
			DatDir = ExeDir + "dat/";
			if (!Directory.Exists(DatDir)){
				DatDir = ExeDir + "DAT/";
			}
			ContentDir = ExeDir + "resources/";
			if (!Directory.Exists(ContentDir)){
				ContentDir = ExeDir + "RESOURCES/";
			}
			//用于错误输出
			//1815 .exe会被东方版的NG词拦截，因此移除
			//ExeName = Path.GetFileNameWithoutExtension(Sys.ExeName);

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			ConfigData.Instance.LoadConfig();
            //禁止双重启动且检测双重启动
			//if ((!Config.AllowMultipleInstances) && (Sys.PrevInstance()))
			//{
			//	MessageBox.Show("多重起動を許可する場合、emuera.configを書き換えて下さい", "既に起動しています");
			//	return;
			//}
			if (!Directory.Exists(CsvDir))
			{
				MessageBox.Show("\"" + CsvDir + "\" csvフォルダが見つかりません", "フォルダなし");
				return;
			}
			if (!Directory.Exists(ErbDir))
			{
				MessageBox.Show("\"" + ErbDir + "\" erbフォルダが見つかりません", "フォルダなし");
				return;
			}
            int argsStart = 0;
            if ((args.Length > 0)&&(args[0].Equals("-DEBUG", StringComparison.CurrentCultureIgnoreCase)))
            {
                argsStart = 1;//调试模式且解析模式时跳过第一个参数(-DEBUG)
				debugMode = true;
            }
			if(debugMode)
			{
				ConfigData.Instance.LoadDebugConfig();
				if (!Directory.Exists(DebugDir))
				{
					try
					{
						Directory.CreateDirectory(DebugDir);
					}
					catch
					{
						MessageBox.Show("debugフォルダの作成に失敗しました", "フォルダなし");
						return;
					}
				}
			}
            if (args.Length > argsStart)
            {
                AnalysisFiles = new List<string>();
                for (int i = argsStart; i < args.Length; i++)
                {
                    if (!File.Exists(args[i]) && !Directory.Exists(args[i]))
                    {
                        MessageBox.Show("与えられたファイル・フォルダは存在しません");
                        return;
                    }
                    if ((File.GetAttributes(args[i]) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        List<KeyValuePair<string, string>> fnames = Config.GetFiles(args[i] + "\\", "*.ERB");
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                        fnames.AddRange(Config.GetFiles(args[i] + "\\", "*.erb"));
#endif
                        for(int j = 0; j < fnames.Count; j++)
                        {
                            AnalysisFiles.Add(fnames[j].Value);
                        }
                    }
                    else
                    {
                        if (Path.GetExtension(args[i]).ToUpper() != ".ERB")
                        {
                            MessageBox.Show("ドロップ可能なファイルはERBファイルのみです");
                            return;
                        }
                        AnalysisFiles.Add(args[i]);
                    }
                }
                AnalysisMode = true;
            }
			MainWindow win = null;


			//while (true)
			//{
				StartTime = WinmmTimer.TickCount;
                //using (win = new MainWindow())
                //{
                    win = new MainWindow();
                    Application.Run(win);
				//	Content.AppContents.UnloadContents();
				//	if (!Reboot)
				//		break;

				//	RebootWinState = win.WindowState;
				//	if (win.WindowState == FormWindowState.Normal)
				//	{
				//		RebootClientY = win.ClientSize.Height;
				//		RebootLocation = win.Location;
				//	}
				//else
				//	{
				//		RebootClientY = 0;
				//		RebootLocation = new Point();
				//	}
				//}
				////根据条件，有时ParserMediator会在非空状态下重启
				//ParserMediator.ClearWarningList();
				//ParserMediator.Initialize(null);
				//GlobalStatic.Reset();
				////GC.Collect();
				//Reboot = false;
				//ConfigData.Instance.LoadConfig();
			//}
		}

		/// <summary>
		/// 可执行文件的目录。末尾带\的string
		/// </summary>
		public static string ExeDir { get; private set; }
		public static string CsvDir { get; private set; }
		public static string ErbDir { get; private set; }
		public static string DebugDir { get; private set; }
		public static string DatDir { get; private set; }
		public static string ContentDir { get; private set; }
		public static string ExeName { get; private set; }

		public static bool Reboot = false;
		//public static int RebootClientX = 0;
		public static int RebootClientY = 0;
        public static FormWindowState RebootWinState = FormWindowState.Normal;
		public static Point RebootLocation;

        public static bool AnalysisMode = false;
        public static List<string> AnalysisFiles = null;

		public static bool debugMode = false;
		public static bool DebugMode { get { return debugMode; } }


		public static uint StartTime { get; private set; }

	}
}