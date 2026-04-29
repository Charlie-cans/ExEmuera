using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.GameData;

namespace MinorShift.Emuera.Sub
{
	enum LexEndWith
	{
		//无论哪种情况，遇到EoL时强制结束
		None = 0,
		EoL,//始终解析到最后
		Operator,//发现运算符时结束。赋值表达式的左边
		Question,//由三元运算符?结束。\@～～?～～#～～\@
		Percent,//由%结束。%～～%
		RightCurlyBrace,//由}结束。{～～}
		Comma,//由,结束。TIMES第一参数
		//Single,//一个Identifier即结束//1807 Single已删除
		GreaterThan,//由'>'结束。Html标签解析
	}

	enum FormStrEndWith
	{
		//无论哪种情况，遇到EoL时强制结束
		None = 0,
		EoL,//始终解析到最后
		DoubleQuotation,//由"结束。@"～～"
		Sharp,//由#结束。\@～～?～～#～～\@　的第一个
		YenAt,//由\@结束。\@～～?～～#～～\@　的第二个
		Comma,//由,结束。ANY_FORM参数
		LeftParenthesis_Bracket_Comma_Semicolon,//由[或(或,或;结束。CALLFORM类的函数名部分。
	}

	enum StrEndWith
	{
		//无论哪种情况，遇到EoL时强制结束
		None = 0,
		EoL,//始终解析到最后
		SingleQuotation,//由"结束。'～～'
		DoubleQuotation,//由"结束。"～～"
		Comma,//由,结束。PRINTV'～～,
		LeftParenthesis_Bracket_Comma_Semicolon,//由[或(或,或;结束。函数名部分。
	}

	enum LexAnalyzeFlag
	{
		None = 0,
		AnalyzePrintV = 1,//在PRINTV参数中，在'后面继续写字符串时，虽非表达式但会作为字符串显示
		AllowAssignment = 2,//表示可以使用赋值运算符的Flag。若无此Flag而在中途出现=则出错
		AllowSingleQuotationStr = 4,//用于HTML_PRINT解析。允许''括起来的字符串。
	}

	/// <summary>
	/// 1756 由TokenReader改名
	/// 虽名为Lexical，但包含语法分析
	/// </summary>
	internal static class LexicalAnalyzer
	{

		const int MAX_EXPAND_MACRO = 100;
		//readonly static IList<char> operators = new char[] { '+', '-', '*', '/', '%', '=', '!', '<', '>', '|', '&', '^', '~', '?', '#' };
		//readonly static IList<char> whiteSpaces = new char[] { ' ', '　', '\t' };
		//readonly static IList<char> endOfExpression = new char[] { ')', '}', ']', ',', ':' };
		//readonly static IList<char> startOfExpression = new char[] { '(' };
		//readonly static IList<char> stringToken = new char[] { '\"', };
		//readonly static IList<char> stringFormToken = new char[] { '@', };
		//readonly static IList<char> etcSymbol = new char[] { '[', '{', '$', '\\', };
		//readonly static IList<char> decimalDigits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', };
		readonly static IList<char> hexadecimalDigits = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F' };

	//1819 使用正则表达式稍慢。将来也想支持double。到时再考虑
		//readonly static Regex DigitsReg = new Regex("" +
		//	"(" +
		//	"((?<simple>[-]?[0-9]+)([^.xXbBeEpP]|$))" +
		//	"|(" +
		//	"(0(x|X)(?<hex>[0-9a-fA-F]+))|"+
		//	"(0(b|B)(?<bin>[01]+))|"+
		//	"(" + //base10
		//	"(?<integer>[-]?[0-9]*(?<double>[.][0-9])?)" +
		//	"(((p|P)(?<exp2>[0-9]+))|" +
		//	"((e|E)(?<exp10>[0-9]+)))?" +
		//	")"+
		//	"))"
		//	, RegexOptions.Compiled);
		//readonly static Regex idReg = new Regex(@"[^][ \t+*/%=!<>|&^~?#(){},:$\\'""@.;　-]+", RegexOptions.Compiled);
		//public static Int64 ReadInt64(StringStream st, bool retZero)
		//{
		//	Match m = DigitsReg.Match(st.RowString, st.CurrentPosition);
		//	string numstr = m.Groups["simple"].Value;
		//	if (numstr.Length > 0)
		//	{
		//		st.Jump(numstr.Length);
		//		return Convert.ToInt64(numstr, 10);
		//	}
		//	st.Jump(m.Length);
		//	if (m.Groups["bin"].Length > 0)
		//		return Convert.ToInt64(m.Groups["bin"].Value, 2);
		//	if(m.Groups["hex"].Length > 0)
		//		return Convert.ToInt64(m.Groups["hex"].Value, 16);
		//	numstr = m.Groups["number"].Value;
		//	if (numstr.Length > 0)
		//	{
		//		int exp = 0;
		//		string exp2 = m.Groups["exp2"].Value;
		//		string exp10 = m.Groups["exp10"].Value;
		//		if(m.Groups["double"].Length == 0 && exp2.Length == 0 && exp10.Length == 0)
		//		{
		//			return Convert.ToInt64(numstr,10);
		//		}
		//		double d = Convert.ToDouble(numstr);
		//		if (exp2.Length > 0)
		//		{
		//			exp = Convert.ToInt32(exp2, 10);
		//			d = d * Math.Pow(2, exp);
		//		}
		//		else if (exp10.Length > 0)
		//		{
		//			exp = Convert.ToInt32(exp10, 10);
		//			d = d * Math.Pow(10, exp);
		//		}
		//		return ((Int64)(d + 0.49));
		//	}
		//	throw new CodeEE("数字で始まるトークンが適切でありません");
		//}



		public static bool UseMacro = true;
		#region read
		public static Int64 ReadInt64(StringStream st, bool retZero)
		{
			Int64 significand = 0;
			int expBase = 0;
			int exponent = 0;
			int stStartPos = st.CurrentPosition;
			int stEndPos = st.CurrentPosition;
			int fromBase = 10;
			if (st.Current == '0')
			{
				char c = st.Next;
				if ((c == 'x') || (c == 'X'))
				{
					fromBase = 16;
					st.ShiftNext();
					st.ShiftNext();
				}
				else if ((c == 'b') || (c == 'B'))
				{
					fromBase = 2;
					st.ShiftNext();
					st.ShiftNext();
				}
				//不采用八进制，因为存在兼容性问题。
				//else if (dchar.IsDigit(c))
				//{
				//    fromBase = 8;
				//    st.ShiftNext();
				//}
			}
			if (retZero && st.Current != '+' && st.Current != '-' && !char.IsDigit(st.Current))
			{
				if (fromBase != 16)
					return 0;
				else if (!hexadecimalDigits.Contains(st.Current))
					return 0;
			}
			significand = readDigits(st, fromBase);
			if ((st.Current == 'p') || (st.Current == 'P'))
				expBase = 2;
			else if ((st.Current == 'e') || (st.Current == 'E'))
				expBase = 10;
			if (expBase != 0)
			{
				st.ShiftNext();
				unchecked { exponent = (int)readDigits(st, fromBase); }
			}
			stEndPos = st.CurrentPosition;
			if ((expBase != 0) && (exponent != 0))
			{

				double d = significand * Math.Pow(expBase, exponent);
				if ((double.IsNaN(d)) || (double.IsInfinity(d)) || (d > Int64.MaxValue) || (d < Int64.MinValue))
					throw new CodeEE("\"" + st.Substring(stStartPos, stEndPos) + "\"は64ビット符号付整数の範囲を超えています");
				significand = (Int64)d;
			}
			return significand;
		}
		//static Regex reg = new Regex(@"[0-9A-Fa-f]+", RegexOptions.Compiled);
		private static Int64 readDigits(StringStream st, int fromBase)
		{
			int start = st.CurrentPosition;
			//1756 尝试使用正则表达式但几乎没变化，故废弃
			//Match m = reg.Match(st.RowString, st.CurrentPosition);
			//st.Jump(m.Length);
			char c = st.Current;
			if ((c == '-') || (c == '+'))
			{
				st.ShiftNext();
			}
			if (fromBase == 10)
			{
				while (!st.EOS)
				{
					c = st.Current;
					if (char.IsDigit(c))
					{
						st.ShiftNext();
						continue;
					}
					break;
				}
			}
			else if (fromBase == 16)
			{
				while (!st.EOS)
				{
					c = st.Current;
					if (char.IsDigit(c) || hexadecimalDigits.Contains(c))
					{
						st.ShiftNext();
						continue;
					}
					break;
				}
			}
			else if (fromBase == 2)
			{
				while (!st.EOS)
				{
					c = st.Current;
					if (char.IsDigit(c))
					{
						if ((c != '0') && (c != '1'))
							throw new CodeEE("二進法表記の中で使用できない文字が使われています");
						st.ShiftNext();
						continue;
					}
					break;
				}
			}
			string strInt = st.Substring(start, st.CurrentPosition - start);
			try
			{
				return Convert.ToInt64(strInt, fromBase);
			}
			catch (FormatException)
			{
				throw new CodeEE("\"" + strInt + "\"は整数値に変換できません");
			}
			catch (OverflowException)
			{
				throw new CodeEE("\"" + strInt + "\"は64ビット符号付き整数の範囲を超えています");
			}
			catch (ArgumentOutOfRangeException)
			{
				if (string.IsNullOrEmpty(strInt))
					throw new CodeEE("数値として認識できる文字が必要です");
				throw new CodeEE("文字列\"" + strInt + "\"は数値として認識できません");
			}
		}

		/// <summary>
		/// 仅TIMES第二参数使用。
		/// 会直接抛出Convert类发出的异常，请适当处理。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static double ReadDouble(StringStream st)
		{
			int start = st.CurrentPosition;
			//粗略读取，错误处理交给Convert类。
			//尾数小数部

			if ((st.Current == '-') || (st.Current == '+'))
			{
				st.ShiftNext();
			}
			while (!st.EOS)
			{//仮数部
				char c = st.Current;
				if (char.IsDigit(c) || (c == '.'))
				{
					st.ShiftNext();
					continue;
				}
				break;
			}
			if ((st.Current == 'e') || (st.Current == 'E'))
			{
				st.ShiftNext();
				if (st.Current == '-')
				{
					st.ShiftNext();
				}
				while (!st.EOS)
				{//指数部
					char c = st.Current;
					if (char.IsDigit(c) || (c == '.'))
					{
						st.ShiftNext();
						continue;
					}
					break;
				}
			}
			return Convert.ToDouble(st.Substring(start, st.CurrentPosition - start));
		}

		/// <summary>
		/// 获取行首单词。有宏展开。但不展开非单词的宏。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static IdentifierWord ReadFirstIdentifierWord(StringStream st)
		{
			int startpos = st.CurrentPosition;
			string str = ReadSingleIdentifier(st);
			if (string.IsNullOrEmpty(str))
				throw new CodeEE("不正な文字で行が始まっています");
			//1808a3 停止首单词的展开。禁止命令的替换。
			//if (UseMacro)
			//{
			//    int i = 0;
			//    while (true)
			//    {
			//        DefineMacro macro = GlobalStatic.IdentifierDictionary.GetMacro(str);
			//        i++;
			//        if (i > MAX_EXPAND_MACRO)
			//            throw new CodeEE("マクロの展開数が1文あたりの上限を超えました(自己参照・循環参照のおそれ)");
			//        if (macro == null)
			//            break;
			//        //如果出现非单词（单个标识符）的宏，此处不作处理
			//        if (macro.IDWord == null)
			//        {
			//            st.CurrentPosition = startpos;
			//            return null;//交由变量处理。
			//        }
			//        str = macro.IDWord.Code;
			//    }
			//}
			return new IdentifierWord(str);
		}

		/// <summary>
		/// 获取单词。有宏展开。无函数型宏展开
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static IdentifierWord ReadSingleIdentifierWord(StringStream st)
		{
			string str = ReadSingleIdentifier(st);
			if (string.IsNullOrEmpty(str))
				return null;
			if (UseMacro)
			{
				int i = 0;
				while (true)
				{
					DefineMacro macro = GlobalStatic.IdentifierDictionary.GetMacro(str);
					i++;
					if (i > MAX_EXPAND_MACRO)
						throw new CodeEE("マクロの展開数が1文あたりの上限値" + MAX_EXPAND_MACRO.ToString() + "を超えました(自己参照・循環参照のおそれ)");
					if (macro == null)
						break;
					if (macro.IDWord != null)
						throw new CodeEE("マクロ" + macro.Keyword + "はこの文脈では使用できません(1単語に置き換えるマクロのみが使用できます)");
					str = macro.IDWord.Code;
				}
			}
			return new IdentifierWord(str);
		}

        static readonly HashSet<char> kHashSet_ReadSingleIdentifier = new HashSet<char>
        {
            ' ',
            '\t',
            '+',
            '-',
            '*',
            '/',
            '%',
            '=',
            '!',
            '<',
            '>',
            '|',
            '&',
            '^',
            '~',
            '?',
            '#',
            ')',
            '}',
            ']',
            ',',
            ':',
            '(',
            '{',
            '[',
            '$',
            '\\',
            '\'',
            '\"',
            '@',
            '.',
            ';',
        };
        /// <summary>
        /// 以字符串形式获取单词。不应用宏
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public static string ReadSingleIdentifier(StringStream st)
		{
			//1819 稍慢。但将来还是想做
			//Match m = idReg.Match(st.RowString, st.CurrentPosition);
			//st.Jump(m.Length);
			//return m.Value;
			int start = st.CurrentPosition;
            char c;
			while (!st.EOS)
			{
                //switch (st.Current)
                //{
                //	case ' ':
                //	case '\t':
                //	case '+':
                //	case '-':
                //	case '*':
                //	case '/':
                //	case '%':
                //	case '=':
                //	case '!':
                //	case '<':
                //	case '>':
                //	case '|':
                //	case '&':
                //	case '^':
                //	case '~':
                //	case '?':
                //	case '#':
                //	case ')':
                //	case '}':
                //	case ']':
                //	case ',':
                //	case ':':
                //	case '(':
                //	case '{':
                //	case '[':
                //	case '$':
                //	case '\\':
                //	case '\'':
                //	case '\"':
                //	case '@':
                //	case '.':
                //	case ';'://コメントに関しては直後に行われるであろうSkipWhiteSpaceなどが対応する。
                //		goto end;
                //	case '　':
                //		if (!Config.SystemAllowFullSpace)
                //			throw new CodeEE("予期しない全角スペースを発見しました(この警告はシステムオプション「" + Config.GetConfigName(ConfigCode.SystemAllowFullSpace) + "」により無視できます)");
                //		goto end;
                //}

                c = st.Current;
                if(kHashSet_ReadSingleIdentifier.Contains(c))
                    goto end;
                else if(c == '　')
                {
                    if(!Config.SystemAllowFullSpace)
                	    throw new CodeEE("予期しない全角スペースを発見しました(この警告はシステムオプション「" + Config.GetConfigName(ConfigCode.SystemAllowFullSpace) + "」により無視できます)");
                    goto end;
                }
                st.ShiftNext();
			}
		end:
			return st.Substring(start, st.CurrentPosition - start);
		}

		/// <summary>
		/// 读取直到找到endWith。起始点和终端的检查由调用方执行。
		/// 支持转义。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static string ReadString(StringStream st, StrEndWith endWith)
		{
			StringBuilder buffer = new StringBuilder(100);
			while (true)
			{
				switch (st.Current)
				{
					case '\0':
						goto end;
					case '\"':
						if (endWith == StrEndWith.DoubleQuotation)
							goto end;
						break;
					case '\'':
						if (endWith == StrEndWith.SingleQuotation)
							goto end;
						break;
					case ',':
						if ((endWith == StrEndWith.Comma) || (endWith == StrEndWith.LeftParenthesis_Bracket_Comma_Semicolon))
							goto end;
						break;
					case '(':
					case '[':
					case ';':
						if (endWith == StrEndWith.LeftParenthesis_Bracket_Comma_Semicolon)
							goto end;
						break;
					case '\\'://转义处理
						st.ShiftNext();//跳过\
						switch (st.Current)
						{
							case StringStream.EndOfString:
								throw new CodeEE("エスケープ文字\\の後に文字がありません");
							case '\n': break;
							case 's': buffer.Append(' '); break;
							case 'S': buffer.Append('　'); break;
							case 't': buffer.Append('\t'); break;
							case 'n': buffer.Append('\n'); break;
							default: buffer.Append(st.Current); break;
						}
						st.ShiftNext();//跳过\的下一个字符
						continue;
				}
				buffer.Append(st.Current);
				st.ShiftNext();
			}
		end:
			return buffer.ToString();
		}

		/// <summary>
		/// 失败则抛出CodeEE。不依赖OperatorManager
		/// 有时会返回OperatorCode.Assignment。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static OperatorCode ReadOperator(StringStream st, bool allowAssignment)
		{
			char cur = st.Current;
			st.ShiftNext();
			char next = st.Current;
			switch (cur)
			{
				case '+':
					if (next == '+')
					{
						st.ShiftNext();
						return OperatorCode.Increment;
					}
					return OperatorCode.Plus;
				case '-':
					if (next == '-')
					{
						st.ShiftNext();
						return OperatorCode.Decrement;
					}
					return OperatorCode.Minus;
				case '*':
					return OperatorCode.Mult;
				case '/':
					return OperatorCode.Div;
				case '%':
					return OperatorCode.Mod;
				case '=':
					if (next == '=')
					{
						st.ShiftNext();
						return OperatorCode.Equal;
					}
					if (allowAssignment)
						return OperatorCode.Assignment;
					throw new CodeEE("予期しない代入演算子'='を発見しました(等価比較には'=='を使用してください)");
				case '!':
					if (next == '=')
					{
						st.ShiftNext();
						return OperatorCode.NotEqual;
					}
					else if (next == '&')
					{
						st.ShiftNext();
						return OperatorCode.Nand;
					}
					else if (next == '|')
					{
						st.ShiftNext();
						return OperatorCode.Nor;
					}
					return OperatorCode.Not;
				case '<':
					if (next == '=')
					{
						st.ShiftNext();
						return OperatorCode.LessEqual;
					}
					else if (next == '<')
					{
						st.ShiftNext();
						return OperatorCode.LeftShift;
					}
					return OperatorCode.Less;
				case '>':
					if (next == '=')
					{
						st.ShiftNext();
						return OperatorCode.GreaterEqual;
					}
					else if (next == '>')
					{
						st.ShiftNext();
						return OperatorCode.RightShift;
					}
					return OperatorCode.Greater;
				case '|':
					if (next == '|')
					{
						st.ShiftNext();
						return OperatorCode.Or;
					}
					return OperatorCode.BitOr;
				case '&':
					if (next == '&')
					{
						st.ShiftNext();
						return OperatorCode.And;
					}
					return OperatorCode.BitAnd;
				case '^':
					if (next == '^')
					{
						st.ShiftNext();
						return OperatorCode.Xor;
					}
					return OperatorCode.BitXor;
				case '~':
					return OperatorCode.BitNot;
				case '?':
					return OperatorCode.Ternary_a;
				case '#':
					return OperatorCode.Ternary_b;

			}
			throw new CodeEE("'" + cur + "'は演算子として認識できません");
		}

		/// <summary>
		/// 失败则抛出CodeEE。不依赖OperatorManager
		/// "="时返回OperatorCode.Assignment。"=="时返回Equal
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static OperatorCode ReadAssignmentOperator(StringStream st)
		{
			OperatorCode ret = OperatorCode.NULL;
			char cur = st.Current;
			st.ShiftNext();
			char next = st.Current;
			switch (cur)
			{
				case '+':
					if (next == '+')
						ret = OperatorCode.Increment;
					else if (next == '=')
						ret = OperatorCode.Plus;
					break;
				case '-':
					if (next == '-')
						ret = OperatorCode.Decrement;
					else if (next == '=')
						ret = OperatorCode.Minus;
					break;
				case '*':
					if (next == '=')
						ret = OperatorCode.Mult;
					break;
				case '/':
					if (next == '=')
						ret = OperatorCode.Div;
					break;
				case '%':
					if (next == '=')
						ret = OperatorCode.Mod;
					break;
				case '=':
					if (next == '=')
					{
						ret = OperatorCode.Equal;
						break;
					}
					return OperatorCode.Assignment;
				case '\'':
					if (next == '=')
					{
						ret = OperatorCode.AssignmentStr;
						break;
					}
					throw new CodeEE("\"\'\"は代入演算子として認識できません");
				case '<':
					if (next == '<')
					{
						st.ShiftNext();
						if (st.Current == '=')
						{
							ret = OperatorCode.LeftShift;
							break;
						}
						throw new CodeEE("'<'は代入演算子として認識できません");
					}
					break;
				case '>':
					if (next == '>')
					{
						st.ShiftNext();
						if (st.Current == '=')
						{
							ret = OperatorCode.RightShift;
							break;
						}
						throw new CodeEE("'>'は代入演算子として認識できません");
					}
					break;
				case '|':
					if (next == '=')
						ret = OperatorCode.BitOr;
					break;
				case '&':
					if (next == '=')
						ret = OperatorCode.BitAnd;
					break;
				case '^':
					if (next == '=')
						ret = OperatorCode.BitXor;
					break;
			}
			if (ret == OperatorCode.NULL)
				throw new CodeEE("'" + cur + "'は代入演算子として認識できません");
			st.ShiftNext();
			return ret;
		}



		/// <summary>
		/// 用于Console的文字显示。不得用于词法分析或语法分析
		/// </summary>
		public static int SkipAllSpace(StringStream st)
		{
			int count = 0;
			while (true)
			{
				switch (st.Current)
				{
					case ' ':
					case '\t':
					case '　':
						count++;
						st.ShiftNext();
						continue;
				}
				return count;
			}
		}

		public static bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '　';
		}

		/// <summary>
		/// 用于词法分析和语法分析。跳过空白字符和注释。
		/// </summary>
		public static int SkipWhiteSpace(StringStream st)
		{
			int count = 0;
			while (true)
			{
				switch (st.Current)
				{
					case ' ':
					case '\t':
						count++;
						st.ShiftNext();
						continue;
					case '　':
						if (!Config.SystemAllowFullSpace)
							return count;
						goto case ' ';
					case ';':
						if (st.CurrentEqualTo(";#;") && Program.DebugMode)
						{
							st.Jump(3);
							continue;
						}
						else if (st.CurrentEqualTo(";!;"))
						{
							st.Jump(3);
							continue;
						}
						st.Seek(0, System.IO.SeekOrigin.End);
						return count;
				}
				return count;
			}
		}

		/// <summary>
		/// 用于词法分析和语法分析。跳过字符串前面的半角空格。性质上仅查看半角空格。
		/// </summary>
		public static int SkipHalfSpace(StringStream st)
		{
			int count = 0;
			while (st.Current == ' ')
			{
				count++;
				st.ShiftNext();
			}
			return count;
		}
		#endregion

		#region analyse
		
		/// <summary>
		/// 只能解析函数声明和表达式。不要传入FORM字符串或普通字符串
		/// return时endWith的字符应该位于Current。终端的适当性验证由调用方执行。
		/// </summary>
		/// <returns></returns>
		public static WordCollection Analyse(StringStream st, LexEndWith endWith, LexAnalyzeFlag flag)
		{
			WordCollection ret = new WordCollection();
			int nestBracketS = 0;
			//int nestBracketM = 0;
			int nestBracketL = 0;
			while (true)
			{
				switch (st.Current)
				{
					case '\n':
					case '\0':
						goto end;
					case ' ':
					case '\t':
						st.ShiftNext();
						continue;
					case '　':
						if (!Config.SystemAllowFullSpace)
							throw new CodeEE("字句解析中に予期しない全角スペースを発見しました(この警告はシステムオプション「" + Config.GetConfigName(ConfigCode.SystemAllowFullSpace) + "」により無視できます)");
						st.ShiftNext();
						continue;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						ret.Add(new LiteralIntegerWord(ReadInt64(st, false)));
						break;
					case '>':
						if(endWith == LexEndWith.GreaterThan)
							goto end;
						goto case '+';
					case '+':
					case '-':
					case '*':
					case '/':
					case '%':
					case '=':
					case '!':
					case '<':
					case '|':
					case '&':
					case '^':
					case '~':
					case '?':
					case '#':
						if ((nestBracketS == 0) && (nestBracketL == 0))
						{
							if (endWith == LexEndWith.Operator)
								goto end;//应该是赋值运算符。调用方应该会检查
							else if ((endWith == LexEndWith.Percent) && (st.Current == '%'))
								goto end;
							else if ((endWith == LexEndWith.Question) && (st.Current == '?'))
								goto end;
						}
						ret.Add(new OperatorWord(ReadOperator(st, (flag & LexAnalyzeFlag.AllowAssignment) == LexAnalyzeFlag.AllowAssignment)));
						break;
					case ')': ret.Add(new SymbolWord(')')); nestBracketS--; st.ShiftNext(); continue;
					case ']': ret.Add(new SymbolWord(']')); nestBracketL--; st.ShiftNext(); continue;
					case '(': ret.Add(new SymbolWord('(')); nestBracketS++; st.ShiftNext(); continue;
					case '[':
						if (st.Next == '[')
						{
							//throw new CodeEE("字句解析中に予期しない文字'[['を発見しました");
							////1808alpha006 rename处理变更
							//1808beta009 仅此处恢复
							//因为在当前处理下来到这里时rename失败已确定，但为了恢复警告内容
							if (ParserMediator.RenameDic == null)
								throw new CodeEE("字句解析中に予期しない文字\"[[\"を発見しました");
							int start = st.CurrentPosition;
							int find = st.Find("]]");
							if (find <= 2)
							{
								if (find == 2)
									throw new CodeEE("空の[[]]です");
								else
									throw new CodeEE("対応する\"]]\"のない\"[[\"です");
							}
							string key = st.Substring(start, find + 2);
							//1810 至此未能替换的内容强制报错
							//因为连那些在行连接前无法替换、通过行连接变得可以替换的内容也被替换了
							throw new CodeEE("字句解析中に置換(rename)できない符号" + key + "を発見しました");
							//string value = null;
							//if (!ParserMediator.RenameDic.TryGetValue(key, out value))
							//    throw new CodeEE("字句解析中に置換(rename)できない符号" + key + "を発見しました");
							//st.Replace(start, find + 2, value);
							//continue;//从该处重新开始解析
						}
						ret.Add(new SymbolWord('[')); nestBracketL++; st.ShiftNext(); continue;
					case ':': ret.Add(new SymbolWord(':')); st.ShiftNext(); continue;
					case ',':
						if ((endWith == LexEndWith.Comma) && (nestBracketS == 0))// && (nestBracketL == 0))
							goto end;
						ret.Add(new SymbolWord(',')); st.ShiftNext(); continue;
					//case '}': ret.Add(new SymbolWT('}')); nestBracketM--; continue;
					//case '{': ret.Add(new SymbolWT('{')); nestBracketM++; continue;
					case '\'':
						if ((flag & LexAnalyzeFlag.AllowSingleQuotationStr) == LexAnalyzeFlag.AllowSingleQuotationStr)
						{
							st.ShiftNext();
							ret.Add(new LiteralStringWord(ReadString(st, StrEndWith.SingleQuotation)));
							if (st.Current != '\'')
								throw new CodeEE("\'が閉じられていません");
							st.ShiftNext();
							break;
						}
						if ((flag & LexAnalyzeFlag.AnalyzePrintV) != LexAnalyzeFlag.AnalyzePrintV)
						{
							//AssignmentStr用特殊处理 正在搜索赋值语句的赋值运算符且'=の場合のみ許可
							if ((endWith == LexEndWith.Operator) && (nestBracketS == 0) && (nestBracketL == 0) && st.Next == '=' )
								goto end;
							throw new CodeEE("字句解析中に予期しない文字'" + st.Current + "'を発見しました");
						}
						st.ShiftNext();
						ret.Add(new LiteralStringWord(ReadString(st, StrEndWith.Comma)));
						if (st.Current == ',')
							goto case ',';//如果有后续则进入,的处理。否则应为行终端
						goto end;
					case '}':
						if (endWith == LexEndWith.RightCurlyBrace)
							goto end;
						throw new CodeEE("字句解析中に予期しない文字'" + st.Current + "'を発見しました");
					case '\"':
						st.ShiftNext();
						ret.Add(new LiteralStringWord(ReadString(st, StrEndWith.DoubleQuotation)));
						if (st.Current != '\"')
							throw new CodeEE("\"が閉じられていません");
						st.ShiftNext();
						break;
					case '@':
						if (st.Next != '\"')
						{
							ret.Add(new SymbolWord('@'));
							st.ShiftNext();
							continue;
						}
						st.ShiftNext();
						st.ShiftNext();
						ret.Add(AnalyseFormattedString(st, FormStrEndWith.DoubleQuotation, false));
						if (st.Current != '\"')
							throw new CodeEE("\"が閉じられていません");
						st.ShiftNext();
						break;
					case '.':
						ret.Add(new SymbolWord('.'));
						st.ShiftNext();
						continue;

					case '\\':
						if (st.Next != '@')
							throw new CodeEE("字句解析中に予期しない文字'" + st.Current + "'を発見しました");
						{
							st.Jump(2);
							ret.Add(new StrFormWord(new string[] { "", "" }, new SubWord[] { AnalyseYenAt(st) }));
						}
						break;
					case '{':
					case '$':
						throw new CodeEE("字句解析中に予期しない文字'" + st.Current + "'を発見しました");
					case ';'://1807 行中注释
						if (st.CurrentEqualTo(";#;") && Program.DebugMode)
						{
							st.Jump(3);
							break;
						}
						else if (st.CurrentEqualTo(";!;"))
						{
							st.Jump(3);
							break;
						}
						st.Seek(0, System.IO.SeekOrigin.End);
						goto end;
					default:
						{
							ret.Add(new IdentifierWord(ReadSingleIdentifier(st)));
							break;
						}
				}
			}
		end:
			if ((nestBracketS != 0) || (nestBracketL != 0))
			{
				if (nestBracketS < 0)
					throw new CodeEE("字句解析中に対応する'('のない')'を発見しました");
				else if (nestBracketS > 0)
					throw new CodeEE("字句解析中に対応する')'のない'('を発見しました");
				if (nestBracketL < 0)
					throw new CodeEE("字句解析中に対応する'['のない']'を発見しました");
				else if (nestBracketL > 0)
					throw new CodeEE("字句解析中に対応する']'のない'['を発見しました");
			}
			if (UseMacro)
				return expandMacro(ret);
			return ret;

		}

		private static WordCollection expandMacro(WordCollection wc)
		{
			//宏展开
			wc.Pointer = 0;
			int count = 0;
			while (!wc.EOL)
			{
				IdentifierWord word = wc.Current as IdentifierWord;
				if (word == null)
				{
					wc.ShiftNext();
					continue;
				}
				string idStr = word.Code;
				DefineMacro macro = GlobalStatic.IdentifierDictionary.GetMacro(idStr);
				if (macro == null)
				{
					wc.ShiftNext();
					continue;
				}
				count++;
				if (count > MAX_EXPAND_MACRO)
					throw new CodeEE("マクロの展開数が1文あたりの上限" + MAX_EXPAND_MACRO.ToString() + "を超えました(自己参照・循環参照のおそれ)");
				if (!macro.HasArguments)
				{
					wc.Remove();
					wc.InsertRange(macro.Statement);
					continue;
				}
				//函数型宏
				wc = expandFunctionlikeMacro(macro, wc);
			}
			wc.Pointer = 0;
			return wc;
		}

		private static WordCollection expandFunctionlikeMacro(DefineMacro macro, WordCollection wc)
		{
			int macroStart = wc.Pointer;
			wc.ShiftNext();
			SymbolWord symbol = wc.Current as SymbolWord;
			if (symbol == null || symbol.Type != '(')
				throw new CodeEE("関数形式のマクロ" + macro.Keyword + "に引数がありません");
			WordCollection macroWC = macro.Statement.Clone();
			WordCollection[] args = new WordCollection[macro.ArgCount];
			//参数部分读取循环
			for (int i = 0; i < macro.ArgCount; i++)
			{
				int macroNestBracketS = 0;
				args[i] = new WordCollection();
				while (true)
				{
					wc.ShiftNext();
					if (wc.EOL)
						throw new CodeEE("関数形式のマクロ" + macro.Keyword + "の用法が正しくありません");
					symbol = wc.Current as SymbolWord;
					if (symbol == null)
					{
						args[i].Add(wc.Current);
						continue;
					}
					switch (symbol.Type)
					{
						case '(': macroNestBracketS++; break;
						case ')':
							if (macroNestBracketS > 0)
							{
								macroNestBracketS--;
								break;
							}
							if (i != macro.ArgCount - 1)
								throw new CodeEE("関数形式のマクロ" + macro.Keyword + "の引数の数が正しくありません");
							goto exitfor;
						case ',':
							if (macroNestBracketS == 0)
								goto exitwhile;
							break;
					}
					args[i].Add(wc.Current);
				}
			exitwhile:
				if (args[i].Collection.Count == 0)
					throw new CodeEE("関数形式のマクロ" + macro.Keyword + "の引数を省略することはできません");
				continue;
			}
		//参数部分读取循环終端
		exitfor:
			symbol = wc.Current as SymbolWord;
			if (symbol == null || symbol.Type != ')')
				throw new CodeEE("関数形式のマクロ" + macro.Keyword + "の用法が正しくありません");
			int macroLength = wc.Pointer - macroStart + 1;
			wc.Pointer = macroStart;
			for (int j = 0; j < macroLength; j++)
				wc.Collection.RemoveAt(macroStart);
			while (!macroWC.EOL)
			{
				MacroWord w = macroWC.Current as MacroWord;
				if (w == null)
				{
					macroWC.ShiftNext();
					continue;
				}
				macroWC.Remove();
				macroWC.InsertRange(args[w.Number]);
				macroWC.Pointer += args[w.Number].Collection.Count;
			}
			wc.InsertRange(macroWC);
			wc.Pointer = macroStart;
			return wc;
		}

		/// <summary>
		/// 从@"等之后开始
		/// return时endWith的字符应该位于Current。终端的适当性验证由调用方执行。
		/// </summary>
		/// <returns></returns>
		public static StrFormWord AnalyseFormattedString(StringStream st, FormStrEndWith endWith, bool trim)
		{
			List<string> strs = new List<string>();
			List<SubWord> SWTs = new List<SubWord>();
			StringBuilder buffer = new StringBuilder(100);
			while (true)
			{
				char cur = st.Current;
				switch (cur)
				{
					case '\n':
					case '\0':
						goto end;
					case '\"':
						if (endWith == FormStrEndWith.DoubleQuotation)
							goto end;
						buffer.Append(cur);
						break;
					case '#':
						if (endWith == FormStrEndWith.Sharp)
							goto end;
						buffer.Append(cur);
						break;
					case ',':
						if ((endWith == FormStrEndWith.Comma) || (endWith == FormStrEndWith.LeftParenthesis_Bracket_Comma_Semicolon))
							goto end;
						buffer.Append(cur);
						break;
					case '(':
					case '[':
					case ';':
						if (endWith == FormStrEndWith.LeftParenthesis_Bracket_Comma_Semicolon)
							goto end;
						buffer.Append(cur);
						break;
					case '%':
						strs.Add(buffer.ToString());
						buffer.Remove(0, buffer.Length);
						st.ShiftNext();
						SWTs.Add(new PercentSubWord(Analyse(st, LexEndWith.Percent, LexAnalyzeFlag.None)));
						if (st.Current != '%')
							throw new CodeEE("\'%\'が使われましたが対応する\'%\'が見つかりません");
						break;
					case '{':
						strs.Add(buffer.ToString());
						buffer.Remove(0, buffer.Length);
						st.ShiftNext();
						SWTs.Add(new CurlyBraceSubWord(Analyse(st, LexEndWith.RightCurlyBrace, LexAnalyzeFlag.None)));
						if (st.Current != '}')
							throw new CodeEE("\'{\'が使われましたが対応する\'}\'が見つかりません");
						break;
					case '*':
					case '+':
					case '=':
					case '/':
					case '$':
						if (!Config.SystemIgnoreTripleSymbol && st.TripleSymbol())
						{
							strs.Add(buffer.ToString());
							buffer.Remove(0, buffer.Length);
							st.Jump(3);
							SWTs.Add(new TripleSymbolSubWord(cur));
							continue;
						}
						else
							buffer.Append(cur);
						break;
					case '\\'://使用转义字符

						st.ShiftNext();
						cur = st.Current;
						switch (cur)
						{
							case '\0':
								throw new CodeEE("エスケープ文字\\の後に文字がありません");
							case '\n': break;
							case 's': buffer.Append(' '); break;
							case 'S': buffer.Append('　'); break;
							case 't': buffer.Append('\t'); break;
							case 'n': buffer.Append('\n'); break;
							case '@'://\@～～?～～#～～\@
								{
									if ((endWith == FormStrEndWith.YenAt) || (endWith == FormStrEndWith.Sharp))
										goto end;
									strs.Add(buffer.ToString());
									buffer.Remove(0, buffer.Length);
									st.ShiftNext();
									SWTs.Add(AnalyseYenAt(st));
									continue;
								}
							default:
								buffer.Append(cur);
								st.ShiftNext();
								continue;
						}
						break;
					default:
						buffer.Append(cur);
						break;
				}
				st.ShiftNext();
			}
		end:
			strs.Add(buffer.ToString());

			string[] retStr = new string[strs.Count];
			SubWord[] retSWTs = new SubWord[SWTs.Count];
			strs.CopyTo(retStr);
			SWTs.CopyTo(retSWTs);
			if (trim && retStr.Length > 0)
			{
				retStr[0] = retStr[0].TrimStart(new char[] { ' ', '\t' });
				retStr[retStr.Length - 1] = retStr[retStr.Length - 1].TrimEnd(new char[] { ' ', '\t' });
			}
			return new StrFormWord(retStr, retSWTs);
		}



		/// <summary>
		/// 从@之后开始、@之后为Current
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static YenAtSubWord AnalyseYenAt(StringStream st)
		{
			WordCollection w = Analyse(st, LexEndWith.Question, LexAnalyzeFlag.None);
			if (st.Current != '?')
				throw new CodeEE("\'\\@\'が使われましたが対応する\'?\'が見つかりません");
			st.ShiftNext();
			StrFormWord left = AnalyseFormattedString(st, FormStrEndWith.Sharp, true);
			if (st.Current != '#')
			{
				if (st.Current != '@')
					throw new CodeEE("\'\\@\',\'?\'が使われましたが対応する\'#\'が見つかりません");
				st.ShiftNext();
				ParserMediator.Warn("\'\\@\',\'?\'が使われましたが対応する\'#\'が見つかりません", GlobalStatic.Process.GetScaningLine(), 1, false, false);
				return new YenAtSubWord(w, left, null);
			}
			st.ShiftNext();
			StrFormWord right = AnalyseFormattedString(st, FormStrEndWith.YenAt, true);
			if (st.Current != '@')
				throw new CodeEE("\'\\@\',\'?\',\'#\'が使われましたが対応する\'\\@\'が見つかりません");
			st.ShiftNext();
			return new YenAtSubWord(w, left, right);
		}

		#endregion

	}
}