using MinorShift._Library;
using MinorShift.Emuera.Sub;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Text;
//using System.Windows.Forms;
using uEmuera.Drawing;
using uEmuera.Forms;

namespace MinorShift.Emuera.GameView
{
	//1820 EmueraConsole中涉及displayLineList和printBuffer的部分
	//将来希望从EmueraConsole中分离
	internal sealed partial class EmueraConsole : IDisposable
	{
        private readonly DisplayLineList displayLineList;
		public bool noOutputLog = false;
		public Color bgColor = Config.BackColor;

		private readonly PrintStringBuffer printBuffer;
		readonly StringMeasure stringMeasure = new StringMeasure();

		public void ClearDisplay()
		{
			displayLineList.Clear();
			logicalLineCount = 0;
			lineNo = 0;
			lastDrawnLineNo = -1;
			verticalScrollBarUpdate();
			window.Refresh();//触发OnPaint
		}


		#region Print系

		//private bool useUserStyle = true;
		public bool UseUserStyle { get; set; }
		public bool UseSetColorStyle { get; set; }
		private StringStyle defaultStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
		private StringStyle userStyle = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
		//private StringStyle style = new StringStyle(Config.ForeColor, FontStyle.Regular, null);
		private StringStyle Style
		{
			get
			{
				if (!UseUserStyle)
					return defaultStyle;
				if (UseSetColorStyle)
					return userStyle;
				//PRINTD系列（忽略SETCOLOR）
				if (userStyle.Color == defaultStyle.Color)
					return userStyle;
				return new StringStyle(defaultStyle.Color, userStyle.FontStyle, userStyle.Fontname);
			}
		}
		//private StringStyle Style { get { return (useUserStyle ? userStyle : defaultStyle); } }
		public StringStyle StringStyle { get { return userStyle; } }
		public void SetStringStyle(FontStyle fs) { userStyle.FontStyle = fs; }
		public void SetStringStyle(Color color) { userStyle.Color = color; userStyle.ColorChanged = (color != Config.ForeColor); }
		public void SetFont(string fontname) { if (!string.IsNullOrEmpty(fontname)) userStyle.Fontname = fontname; else userStyle.Fontname = Config.FontName; }
		private DisplayLineAlignment alignment = DisplayLineAlignment.LEFT;
		public DisplayLineAlignment Alignment { get { return alignment; } set { alignment = value; } }
		public void ResetStyle()
		{
			userStyle = defaultStyle;
			alignment = DisplayLineAlignment.LEFT;
		}

		public bool EmptyLine { get { return printBuffer.IsEmpty; } }

		/// <summary>
		/// DRAWLINE用的字符串
		/// </summary>
		string stBar = null;

		uint lastBgColorChange = 0;
		bool forceTextBoxColor = false;
		public void SetBgColor(Color color)
		{
			this.bgColor = color;
			forceTextBoxColor = true;
			//如果不进行REDRAW，则不改变TextBox的颜色，仅设置标志
			//在首次重绘时匹配当前背景色
			if (redraw == ConsoleRedraw.None && window.ScrollBar.Value == window.ScrollBar.Maximum)
				return;
			uint sec = WinmmTimer.TickCount - lastBgColorChange;
			//为防止颜色变化过快，在一定时间内再次调用时强制等待
			//while (sec < 200)
			//{
			//	//Application.DoEvents();
			//	sec = WinmmTimer.TickCount - lastBgColorChange;
			//}
			RefreshStrings(true);
			lastBgColorChange = WinmmTimer.TickCount;
		}

		/// <summary>
		/// 最后一次绘制时lineNo的值
		/// </summary>
		int lastDrawnLineNo = -1;
		int lineNo = 0;
		Int64 logicalLineCount = 0;
		public long LineCount { get { return logicalLineCount; } }
		private void addRangeDisplayLine(ConsoleDisplayLine[] lineList)
		{
			for (int i = 0; i < lineList.Length; i++)
				addDisplayLine(lineList[i], false);
		}

		private void addDisplayLine(ConsoleDisplayLine line, bool force_LEFT)
		{
			if (LastLineIsTemporary)
				deleteLine(1);
			//检查不合适的Font
			AConsoleDisplayPart errorStr = null;
            AConsoleDisplayPart css = null;

            var button_count = line.Buttons.Length;
            var button_strcount = 0;
            for(var b=0; b<button_count; ++b)
			{
                ConsoleButtonString button = line.Buttons[b];

                button_strcount = button.StrArray.Length;
                for(var i=0; i<button_strcount; ++i)
				{
                    css = button.StrArray[i];
                    if (css.Error)
					{
						errorStr = css;
						break;
					}
				}
			}
			if (errorStr != null)
			{
				MessageBox.Show("Emueraの表示処理中に不適正なフォントを検出しました\n描画処理を続行できないため強制終了します", "フォント不適正");
				this.Quit();
				return;
			}
			if (force_LEFT)
				line.SetAlignment(DisplayLineAlignment.LEFT);
			else
				line.SetAlignment(alignment);
			line.LineNo = lineNo;
			displayLineList.Add(line);
			lineNo++;
			if (line.IsLogicalLine)
				logicalLineCount++;
			if (lineNo == int.MaxValue)
			{
				lastDrawnLineNo = -1;
				lineNo = 0;
			}
			if (logicalLineCount == long.MaxValue)
			{
				logicalLineCount = 0;
			}
			if (displayLineList.Count > Config.MaxLog)
				displayLineList.RemoveAt(0);
		}


		public void deleteLine(int argNum)
		{
			int delNum = 0;
			int num = argNum;
			while (delNum < num)
			{
				if (displayLineList.Count == 0)
					break;
				ConsoleDisplayLine line = displayLineList[displayLineList.Count - 1];
				displayLineList.RemoveAt(displayLineList.Count - 1);
				lineNo--;
				if (line.IsLogicalLine)
				{
					delNum++;
					logicalLineCount--;
				}
			}
			if (lineNo < 0)
				lineNo += int.MaxValue;
			lastDrawnLineNo = -1;
			//RefreshStrings(true);
		}

		public bool LastLineIsTemporary
		{
			get
			{
				if (displayLineList.Count == 0)
					return false;
				return displayLineList[displayLineList.Count - 1].IsTemporary;
			}
		}

        //检查是否为空行
        public bool LastLineIsEmpty
        {
            get
            {
                if (displayLineList.Count == 0)
                    return false;
                return string.IsNullOrEmpty(displayLineList[displayLineList.Count - 1].ToString().Trim());
            }
        }

        //重写最后一行＋添加下一行时设置为重用该行
        public void PrintTemporaryLine(string str)
		{
			PrintSingleLine(str, true);
		}

		//仅重写最后一行
		private void changeLastLine(string str)
		{
			deleteLine(1);
			PrintSingleLine(str, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="position"></param>
		/// <param name="level">警告级别.0:轻微错误.1:可忽略的行.2:如果该行不执行则无害.3:致命</param>
		public void PrintWarning(string str, ScriptPosition position, int level)
		{
			if (level < Config.DisplayWarningLevel && !Program.AnalysisMode)
				return;
			//仅警告强制显示
			bool b = force_temporary;
			force_temporary = false;
			if (position != null)
			{
				if (position.LineNo >= 0)
				{
					PrintErrorButton(string.Format("警告Lv{0}:{1}:{2}行目:{3}", level, position.Filename, position.LineNo, str), position);
					if (position.RowLine != null)
						PrintError(position.RowLine);
				}
				else
					PrintErrorButton(string.Format("警告Lv{0}:{1}:{2}", level, position.Filename, str), position);

			}
			else
			{
				PrintError(string.Format("警告Lv{0}:{1}", level, str));
			}
			force_temporary = b;
		}



		/// <summary>
		/// 忽略用户指定的字体。不考虑窗口大小，确保一行写完。系统用。
		/// </summary>
		/// <param name="str"></param>
		public void PrintSystemLine(string str)
		{
			PrintFlush(false);
			//RefreshStrings(false);
			UseUserStyle = false;
			PrintSingleLine(str, false);
		}
		public void PrintError(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (Program.DebugMode)
			{
				this.DebugPrint(str);
				this.DebugNewLine();
			}
			PrintFlush(false);
			UseUserStyle = false;
			ConsoleDisplayLine dispLine = PrintPlainwithSingleLine(str);
			if (dispLine == null)
				return;
			addDisplayLine(dispLine, true);
			RefreshStrings(false);
		}

		internal void PrintErrorButton(string str, ScriptPosition pos)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (Program.DebugMode)
			{
				this.DebugPrint(str);
				this.DebugNewLine();
			}
			UseUserStyle = false;
			ConsoleDisplayLine dispLine = printBuffer.AppendAndFlushErrButton(str, Style, ErrorButtonsText, pos, stringMeasure);
			if (dispLine == null)
				return;
			addDisplayLine(dispLine, true);
			RefreshStrings(false);
		}

		/// <summary>
		/// 1813 考虑到用途，将传统的PrintLine分割为PrintSingleLine和PrintSystemLine
		/// </summary>
		/// <param name="str"></param>
		public void PrintSingleLine(string str) { PrintSingleLine(str, false); }
		public void PrintSingleLine(string str, bool temporary)
		{
			if (string.IsNullOrEmpty(str))
				return;
			PrintFlush(false);
			printBuffer.Append(str, Style);
			ConsoleDisplayLine dispLine = BufferToSingleLine(true, temporary);
			if (dispLine == null)
				return;
			addDisplayLine(dispLine, false);
			RefreshStrings(false);
		}

		public void Print(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (str.Contains("\n"))
			{
				int newline = str.IndexOf('\n');
				string upper = str.Substring(0, newline);
				printBuffer.Append(upper, Style);
				NewLine();
				if (newline < str.Length - 1)
				{
					string lower = str.Substring(newline + 1);
					Print(lower);
				}
				return;
			}
			printBuffer.Append(str, Style);
			return;
		}

		
		public void PrintImg(string str)
		{
			printBuffer.Append(new ConsoleImagePart(str, null, 0, 0, 0));
		}

		public void PrintShape(string type, int[] param)
		{
			ConsoleShapePart part = ConsoleShapePart.CreateShape(type, param, userStyle.Color, userStyle.ButtonColor, false);
			printBuffer.Append(part);
		}

		public void PrintHtml(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			if (!this.Enabled)
				return;
			if (!printBuffer.IsEmpty)
			{
				ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, force_temporary);
				addRangeDisplayLine(dispList);
			}
			addRangeDisplayLine(HtmlManager.Html2DisplayLine(str, stringMeasure, this));
			RefreshStrings(false);
		}

		private int printCWidth = -1;
		private int printCWidthL = -1;
		private int printCWidthL2 = -1;
		public void PrintC(string str, bool alignmentRight)
		{
			if (string.IsNullOrEmpty(str))
				return;

			printBuffer.Append(CreateTypeCString(str, alignmentRight), Style, true);
		}

		private void calcPrintCWidth(StringMeasure stringMeasure)
		{
			string str = new string(' ', Config.PrintCLength);
			Font font = Config.Font;
			printCWidth = stringMeasure.GetDisplayLength(str, font);

			str += " ";
			printCWidthL = stringMeasure.GetDisplayLength(str, font);

			str += " ";
			printCWidthL2 = stringMeasure.GetDisplayLength(str, font);
		}

		private string CreateTypeCString(string str, bool alignmentRight)
		{
			if (printCWidth == -1)
				calcPrintCWidth(stringMeasure);
			int length = 0;
			int width = 0;
            if(str != null)
                //length = Config.Encode.GetByteCount(str);
                length = uEmuera.Utils.GetByteCount(str);
            int printcLength = Config.PrintCLength;
			Font font = null;
			try
			{
				font = new Font(Style.Fontname, Config.Font.Size, Style.FontStyle, GraphicsUnit.Pixel);
			}
			catch
			{
				return str;
			}

			if ((alignmentRight) && (length < printcLength))
			{
				str = new string(' ', printcLength - length) + str;
				width = stringMeasure.GetDisplayLength(str, font);
				while (width > printCWidth)
				{
					if (str[0] != ' ')
						break;
					str = str.Remove(0, 1);
					width = stringMeasure.GetDisplayLength(str, font);
				}
			}
			else if ((!alignmentRight) && (length < printcLength + 1))
			{
				str += new string(' ', printcLength + 1 - length);
				width = stringMeasure.GetDisplayLength(str, font);
				while (width > printCWidthL)
				{
					if (str[str.Length - 1] != ' ')
						break;
					str = str.Remove(str.Length - 1, 1);
					width = stringMeasure.GetDisplayLength(str, font);
				}
			}
			return str;
		}

		internal void PrintButton(string str, string p)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(str, Style, p);
		}
		internal void PrintButton(string str, long p)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(str, Style, p);
		}
		internal void PrintButtonC(string str, string p, bool isRight)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(CreateTypeCString(str, isRight), Style, p);
		}
		internal void PrintButtonC(string str, long p, bool isRight)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendButton(CreateTypeCString(str, isRight), Style, p);
		}

		internal void PrintPlain(string str)
		{
			if (string.IsNullOrEmpty(str))
				return;
			printBuffer.AppendPlainText(str, Style);
		}

		public void NewLine()
		{
			PrintFlush(true);
			RefreshStrings(false);
		}

		public ConsoleDisplayLine BufferToSingleLine(bool force, bool temporary)
		{
			if (!this.Enabled)
				return null;
			if (!force && printBuffer.IsEmpty)
				return null;
			if (force && printBuffer.IsEmpty)
				printBuffer.Append(" ", Style);
			ConsoleDisplayLine dispLine = printBuffer.FlushSingleLine(stringMeasure, temporary | force_temporary);
			return dispLine;
		}

		internal ConsoleDisplayLine PrintPlainwithSingleLine(string str)
		{
			if (!this.Enabled)
				return null;
			if (string.IsNullOrEmpty(str))
				return null;
			printBuffer.AppendPlainText(str, Style);
			ConsoleDisplayLine dispLine = printBuffer.FlushSingleLine(stringMeasure, false);
			return dispLine;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="force">即使缓冲区为空也进行换行</param>
		public void PrintFlush(bool force)
		{
			if (!this.Enabled)
				return;
			if (!force && printBuffer.IsEmpty)
				return;
			if (force && printBuffer.IsEmpty)
				printBuffer.Append(" ", Style);
			ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, force_temporary);
			//ConsoleDisplayLine[] dispList = printBuffer.Flush(stringMeasure, temporary | force_temporary);
			addRangeDisplayLine(dispList);
			//1819 绘制命令已分离
			//RefreshStrings(false);
		}

		/// <summary>
		/// 对应DRAWLINE命令。如果可以更改其字体会变得麻烦，因此固定为Regular。
		/// </summary>
		public void PrintBar()
		{
			//已在初始阶段设置，无需查看
			//if (stBar == null)
			//    setStBar(StaticConfig.DrawLineString);

			//1806beta001 废弃CompatiDRAWLINE，迁移至CompatiLinefeedAs1739
			//CompatiLinefeedAs1739的处理在PrintStringBuffer.cs中进行
			//if (Config.CompatiDRAWLINE)
			//	PrintFlush(false);
			StringStyle ss = userStyle;
			userStyle.FontStyle = FontStyle.Regular;
			Print(stBar);
			userStyle = ss;
		}

		public void printCustomBar(string barStr)
		{
			if (string.IsNullOrEmpty(barStr))
				throw new CodeEE("空文字列によるDRAWLINEが行われました");
			StringStyle ss = userStyle;
			userStyle.FontStyle = FontStyle.Regular;
			Print(getStBar(barStr));
			userStyle = ss;
		}

		public string getDefStBar()
		{
			return stBar;
		}

		public string getStBar(string barStr)
		{
            if(barStr.Length == 1)
                return uEmuera.Utils.GetStBar(barStr[0], Config.Font);

			StringBuilder bar = new StringBuilder();
			bar.Append(barStr);
			int width = 0;
			Font font = Config.Font;
			while (width < Config.DrawableWidth)
			{//逐字增加直到越过边界
				bar.Append(barStr);
				width = stringMeasure.GetDisplayLength(bar.ToString(), font);
			}
			while (width > Config.DrawableWidth)
			{//一旦越过边界，就逐字减少直到不再超过（用于应对barStr为多字符字符串的情况）
				bar.Remove(bar.Length - 1, 1);
				width = stringMeasure.GetDisplayLength(bar.ToString(), font);
			}
			return bar.ToString();
		}

		public void setStBar(string barStr)
		{
			stBar = getStBar(barStr);
		}
		#endregion


		private bool outputLog(string fullpath)
		{
			StreamWriter writer = null;
			try
			{
				writer = new StreamWriter(fullpath, false, Encoding.UTF8);
				foreach (ConsoleDisplayLine line in displayLineList)
				{
					writer.WriteLine(line.ToString());
				}
			}
			catch (Exception)
			{
				MessageBox.Show("ログの出力に失敗しました", "ログ出力失敗");
				return false;
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
			return true;
		}


		public bool OutputLog(string filename)
		{
            if (filename == null)
                filename = Program.ExeDir + "emuera.log";

            if (!filename.StartsWith(Program.ExeDir, StringComparison.CurrentCultureIgnoreCase))
            {
                MessageBox.Show("ログファイルは実行ファイル以下のディレクトリにのみ保存できます", "ログ出力失敗");
                return false;
            }

			if (outputLog(filename))
			{
				if (window.Created)
				{
					PrintSystemLine("※※※ログファイルを" + filename + "に出力しました※※※");
					RefreshStrings(true);
				}
				return true;
			}
			else
				return false;
		}

		public void GetDisplayStrings(StringBuilder builder)
		{
			if (displayLineList.Count == 0)
				return;
			for (int i = 0; i < displayLineList.Count; i++)
			{
				builder.AppendLine(displayLineList[i].ToString());
			}
		}

		public ConsoleDisplayLine[] GetDisplayLines(Int64 lineNo)
		{
			if (lineNo < 0 || lineNo > displayLineList.Count)
				return null;
			int count = 0;
			List<ConsoleDisplayLine> list = new List<ConsoleDisplayLine>();
			for (int i = displayLineList.Count - 1; i >= 0; i--)
			{
				if (count == lineNo)
					list.Insert(0, displayLineList[i]);
				if (displayLineList[i].IsLogicalLine)
					count++;
				if (count > lineNo)
					break;
			}
			if (list.Count == 0)
				return null;
			ConsoleDisplayLine[] ret = new ConsoleDisplayLine[list.Count];
			list.CopyTo(ret);
			return ret;
		}
		public ConsoleDisplayLine[] PopDisplayingLines()
		{
			if (!this.Enabled)
				return null;
			if (printBuffer.IsEmpty)
				return null;
			return  printBuffer.Flush(stringMeasure, force_temporary);
		}
		
	}
}
