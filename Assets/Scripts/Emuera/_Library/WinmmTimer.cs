//using System;
//using System.Runtime.InteropServices;
//using UnityEngine;

namespace MinorShift._Library
{
	/// <summary>
	/// 包装后的timer。外部只需调用此TickCount。
	/// </summary>
	internal sealed class WinmmTimer
	{
		static WinmmTimer()
		{
			instance = new WinmmTimer();
		}
		private WinmmTimer()
		{
			//mm_BeginPeriod(1);
		}
		//~WinmmTimer()
		//{
		//	mm_EndPeriod(1);
		//}

		/// <summary>
		/// 仅用于在启动时调用BeginPeriod、结束时调用EndPeriod的实例。
		/// 如果有static析构函数就不需要了
		/// </summary>
		private static volatile WinmmTimer instance;

		public static uint TickCount
        {
            get
            {
                return (uint)(System.DateTime.Now.Ticks / 10000);
            }
        }
		/// <summary>
		/// 用于当前帧渲染的毫秒数
		/// </summary>
		public static uint CurrentFrameTime;
		/// <summary>
		/// 用于固定帧渲染开始信号时间点的毫秒数的数值
		/// </summary>
		public static void FrameStart() { CurrentFrameTime =TickCount; }

        //[DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        //private static extern uint mm_GetTime();
        //[DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        //private static extern uint mm_BeginPeriod(uint uMilliseconds);
        //[DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        //private static extern uint mm_EndPeriod(uint uMilliseconds);
    }
}
