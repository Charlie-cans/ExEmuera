using System;
using System.Collections.Generic;
using System.Text;

namespace MinorShift.Emuera.GameProc
{
	enum InputType
	{
		EnterKey = 1,//回车键或点击
		AnyKey = 2,//任意输入即可
		IntValue = 3,//整数值。是否OneInput由其他变量决定
		StrValue = 4,//字符串。
		Void = 5,//无法输入。只能等待→跳过中或宏执行中时视为未发生

		//1823
		PrimitiveMouseKey = 11,

	}


	// 1819添加 将输入/显示系统与Data/Process系统解耦的计划之一
	// 尽可能在中间加入缓冲层。最终目标是放到单独线程中

	//每次实例化并丢弃这个类合适吗 是否应该复用
	internal sealed class InputRequest
	{
		public InputRequest()
		{
			ID = LastRequestID++;
		}
		public readonly Int64 ID;
		public InputType InputType;
		public bool NeedValue
		{ 
			get 
			{ 
				return (InputType == InputType.IntValue || InputType == InputType.StrValue
					|| InputType == InputType.PrimitiveMouseKey); 
			} 
		}
		public bool OneInput = false;
		public bool StopMesskip = false;
		public bool IsSystemInput = false;

		public bool HasDefValue = false;
		public long DefIntValue;
		public string DefStrValue;

		public long Timelimit = -1;
		public bool DisplayTime;
		public string TimeUpMes;

		static Int64 LastRequestID = 0;
	}
}
