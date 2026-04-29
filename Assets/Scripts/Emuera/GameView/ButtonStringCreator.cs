using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameView
{
	internal sealed class ButtonPrimitive
	{
		public string Str = "";
		public Int64 Input;
		public bool CanSelect = false;
		public override string ToString()
		{
			return Str;
		}
	}

	internal static class ButtonStringCreator
	{
		public static List<string> Split(string printBuffer)
		{
			List<ButtonPrimitive> list = syn(printBuffer);
			List<string> ret = new List<string>();
            for(var i=0; i < list.Count; ++i)
				ret.Add(list[i].Str);
			return ret;
		}
		public static List<ButtonPrimitive> SplitButton(string printBuffer)
		{
			return syn(printBuffer);
		}

		private static List<ButtonPrimitive> syn(string printBuffer)
		{
			string printString = printBuffer.ToString();
			List<ButtonPrimitive> ret = new List<ButtonPrimitive>();
			if (printString.Length == 0)
				goto nonButton;
			List<string> strs = null;
			if ((!printString.Contains("[")) || (!printString.Contains("]")))
				goto nonButton;
			strs = lex(new StringStream(printString));
			if (strs == null)
				goto nonButton;
			bool beforeButton = false;//在第一个按钮（如"[1]"）之前存在文本
			bool afterButton = false;//在最后一个按钮（如"[1]"）之后存在文本
			int buttonCount = 0;
			Int64 inpL = 0;
			for (int i = 0; i < strs.Count; i++)
			{
				if (strs[i].Length == 0)
					continue;
				char c = strs[i][0];
				if (LexicalAnalyzer.IsWhiteSpace(c))
				{//只是空白
				}
				//非数值不转为按钮。
				//else if ((c == '[') && (!isSymbols(strArray[i])))
				else if (isButtonCore(strs[i], ref inpL))
				{//[]包围的字符串。在此阶段不判断是否作为选项的核心。
					buttonCount++;
					afterButton = false;
				}
				else
				{//可能成为选项说明的字符串
                    afterButton = true;
					if (buttonCount == 0)
						beforeButton = true;
				}
			}
			if (buttonCount <= 1)
			{
				ButtonPrimitive button = new ButtonPrimitive();
				button.Str = printBuffer.ToString();
				button.CanSelect = (buttonCount >= 1);
				button.Input = inpL;
				ret.Add(button);
				return ret;
			}
			buttonCount = 0;
			bool alignmentRight = !beforeButton && afterButton;//说明固定在按钮右侧
			bool alignmentLeft = beforeButton && !afterButton;//说明固定在按钮左侧
			bool alignmentEtc = !alignmentRight && !alignmentLeft;//随机应变
			bool canSelect = false;
			Int64 input = 0;

			int state = 0;
			StringBuilder buffer = new StringBuilder();
			VoidMethod reduce = delegate
			{
				if (buffer.Length == 0)
					return;
				ButtonPrimitive button = new ButtonPrimitive();
				button.Str = buffer.ToString();
				button.CanSelect = canSelect;
				button.Input = input;
				ret.Add(button);
				buffer.Remove(0, buffer.Length);
				canSelect = false;
				input = 0;
			};
			for (int i = 0; i < strs.Count; i++)
			{
				if (strs[i].Length == 0)
					continue;
				char c = strs[i][0];
				if (LexicalAnalyzer.IsWhiteSpace(c))
				{//只是空白
					if (((state & 3) == 3) && (alignmentEtc) && (strs[i].Length >= 2))
					{//如果包含核心和说明的内容已完成，则生成按钮。
						//单个字符以下的空格忽略。应对角色购买界面
                        reduce();
						buffer.Append(strs[i]);
						state = 0;
					}
					else
					{
						buffer.Append(strs[i]);
					}
					continue;
				}
				if(isButtonCore(strs[i], ref inpL))
				{
					buttonCount++;
					if (((state & 1) == 1) || alignmentRight)
					{//buffer已包含核心，或强制右对齐
						reduce();
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						state = 1;
					}//((state & 2) == 2) || 
					else if (alignmentLeft)
					{//buffer已包含说明，或强制左对齐
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						reduce();
						state = 0;
					}
					else
					{//buffer为空或空白字符串
						buffer.Append(strs[i]);
						input = inpL;
						canSelect = true;
						state = 1;
					}
					continue;
				}
				//else
				//{//選択肢の説明になるかもしれない文字列
					
					buffer.Append(strs[i]);
					state |= 2;
				//}
				
			};
			reduce();
			return ret;
		nonButton:
			ret = new List<ButtonPrimitive>();
			ButtonPrimitive singleButton = new ButtonPrimitive();
			singleButton.Str = printString;
			ret.Add(singleButton);
			return ret;
		}
		readonly static Regex numReg = new Regex(@"\[\s*([0][xXbB])?[+-]?[0-9]+([eEpP][0-9]+)?\s*\]");

		/// <summary>
		/// 检查带[]的字符串是否可视为数值
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static bool isNumericWord(string str)
		{
			return numReg.IsMatch(str);
		}

		/// <summary>
		/// 判断是否成为按钮的核心。目前仅限整数。
		/// 由于使用try-catch，性能略重。
		/// </summary>
		/// <param name="str"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		private static bool isButtonCore(string str, ref long input)
		{
			if((str == null)||(str.Length < 3)||(str[0] != '[')||(str[str.Length-1] != ']'))
				return false;
			if (!isNumericWord(str))
				return false;
			string buttonStr = str.Substring(1, str.Length - 2);
			StringStream stInt = new StringStream(buttonStr);
			LexicalAnalyzer.SkipAllSpace(stInt);
			try
			{
				input = LexicalAnalyzer.ReadInt64(stInt, false);
			}
			catch
			{
				return false; 
			}
			return true;
		}


		delegate void VoidMethod();

		/// <summary>
		/// 词法分割
		/// 将"[1] あ [2] いうえ "分割为"[1]"," ", "あ"," ","[2]"," ","いうえ"," "
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		private static List<string> lex(StringStream st)
		{
			List<string> strs = new List<string>();
			int state = 0;
			int startIndex = 0;
			VoidMethod reduce = delegate
			{
				if (st.CurrentPosition == startIndex)
					return;
				int length = st.CurrentPosition - startIndex;
				strs.Add(st.Substring(startIndex, length));
				startIndex = st.CurrentPosition;
			};
			while (!st.EOS)
			{
				if (st.Current == '[')
				{
					if (state == 1)//"["内部
						goto unanalyzable;
					reduce();
					state = 1;
					st.ShiftNext();
				}
				else if (st.Current == ']')
				{
					if (state != 1)//"["外部
						goto unanalyzable;
					st.ShiftNext();
					reduce();
					state = 0;
				}
				else if ((state == 0) && (LexicalAnalyzer.IsWhiteSpace(st.Current)))
				{
					reduce();
					LexicalAnalyzer.SkipAllSpace(st);
					reduce();
				}
				else
				{
					st.ShiftNext();
				}
			}
			reduce();
			return strs;
		unanalyzable:
			return null;
		}

	}
}
