using System.IO;

namespace MinorShift._Library
{
	public static class Sys
	{
		static Sys()
		{}
        public static void SetWorkFolder(string folder)
        {
            _WorkFolder = folder;
        }
        public static string WorkFolder { get { return _WorkFolder; } }
        private static string _WorkFolder;

        public static void SetSourceFolder(string folder)
        {
            ExeDir = uEmuera.Utils.NormalizePath(_WorkFolder + "/" + folder + "/");
        }
        
		/// <summary>
		/// 可执行文件的路径
		/// </summary>
		//public static readonly string ExePath;

		/// <summary>
		/// 可执行文件的目录。末尾带\的string
		/// </summary>
		public static string ExeDir { get; private set; }

		/// <summary>
		/// 可执行文件的名称。不含目录
		/// </summary>
		//public static readonly string ExeName;

		/// <summary>
		/// 防止双重启动。如果已有同名exe在运行则返回true
		/// </summary>
		/// <returns></returns>
		public static bool PrevInstance()
		{
            //string thisProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //if (System.Diagnostics.Process.GetProcessesByName(thisProcessName).Length > 1)
            //{
            //	return true;
            //}
            return false;
		}
	}
}

