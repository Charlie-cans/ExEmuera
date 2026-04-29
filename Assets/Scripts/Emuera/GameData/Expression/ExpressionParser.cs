using System;
using System.Collections.Generic;
using MinorShift.Emuera.Sub;
using MinorShift.Emuera.GameData.Variable;
using MinorShift.Emuera.GameData.Function;
//using System.Windows.Forms;

namespace MinorShift.Emuera.GameData.Expression
{

	internal enum ArgsEndWith
	{
		None,
		EoL,
		RightParenthesis,//)结束
		RightBracket,//]结束
	}

	internal enum TermEndWith
	{
		None = 0x0000,
		EoL = 0x0001,
		Comma = 0x0002,//','结束
		RightParenthesis = 0x0004,//')'结束
		RightBracket = 0x0008,//')'结束
		Assignment = 0x0010,//')'结束

		RightParenthesis_Comma = RightParenthesis | Comma,//',' or ')'结束
		RightBracket_Comma = RightBracket | Comma,//',' or ']'结束
		Comma_Assignment = Comma | Assignment,//',' or '='结束
		RightParenthesis_Comma_Assignment = RightParenthesis | Comma | Assignment,//',' or ')' or '='结束
		RightBracket_Comma_Assignment = RightBracket | Comma | Assignment,//',' or ']' or '='结束
	}

    internal static class ExpressionParser
	{
		#region public Reduce
		/// <summary>
		/// 批量获取以逗号分隔的参数。
		/// 返回时endWith的下一个字符应为Current。终端的适当性验证由ExpressionParser进行。
		/// 调用方应妥善处理CodeEE
		/// </summary>
		/// <returns></returns>
		public static IOperandTerm[] ReduceArguments(WordCollection wc, ArgsEndWith endWith, bool isDefine)
		{
			if(wc == null)
				throw new ExeEE("传入了空流");
			List<IOperandTerm> terms = new List<IOperandTerm>();
			TermEndWith termEndWith = TermEndWith.EoL;
			switch (endWith)
			{
				case ArgsEndWith.EoL:
					termEndWith = TermEndWith.Comma;
					break;
                //case ArgsEndWith.RightBracket:
                //    termEndWith = TermEndWith.RightBracket_Comma;
                //    break;
				case ArgsEndWith.RightParenthesis:
					termEndWith = TermEndWith.RightParenthesis_Comma;
					break;
			}
			TermEndWith termEndWith_Assignment = termEndWith | TermEndWith.Assignment;
			while (true)
			{
				Word word = wc.Current;
				switch (word.Type)
				{
					case '\0':
                        if (endWith == ArgsEndWith.RightBracket)
                            throw new CodeEE("找不到与'['对应的']'");
						if (endWith == ArgsEndWith.RightParenthesis)
							throw new CodeEE("找不到与'('对应的')'");
						goto end;
					case ')':
						if (endWith == ArgsEndWith.RightParenthesis)
						{
							wc.ShiftNext();
							goto end;
						}
						throw new CodeEE("语法解析中发现意外的')'");
                    case ']':
                        if (endWith == ArgsEndWith.RightBracket)
                        {
                            wc.ShiftNext();
                            goto end;
                        }
                        throw new CodeEE("语法解析中发现意外的']'");
				}
				if(!isDefine)
					terms.Add(ReduceExpressionTerm(wc, termEndWith));
				else
				{
					terms.Add(ReduceExpressionTerm(wc, termEndWith_Assignment));
                    if (terms[terms.Count - 1] == null)
                        throw new CodeEE("函数定义的参数不能省略");
					if (wc.Current is OperatorWord)
					{//存在=
						wc.ShiftNext();
						IOperandTerm term = reduceTerm(wc, false, termEndWith, VariableCode.__NULL__);
						if (term == null)
							throw new CodeEE("'='后面没有表达式");
						if (term.GetOperandType() != terms[terms.Count - 1].GetOperandType())
							throw new CodeEE("'='前后类型不匹配");
						terms.Add(term);
					}
					else
					{
						if (terms[terms.Count - 1].GetOperandType() == typeof(Int64))
							terms.Add(new NullTerm(0));
						else
							terms.Add(new NullTerm(""));
					}
				}
				if (wc.Current.Type == ',')
					wc.ShiftNext();
			}
		end:
            IOperandTerm[] ret = new IOperandTerm[terms.Count];
			terms.CopyTo(ret);
			return ret;
		}


		/// <summary>
		/// 数值表达式或字符串表达式。处理CALL的参数等。可能返回null。
		/// 返回时endWith的字符应为Current。终端的适当性验证由调用方进行。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
        public static IOperandTerm ReduceExpressionTerm(WordCollection wc, TermEndWith endWith)
        {
			IOperandTerm term = reduceTerm(wc, false, endWith, VariableCode.__NULL__);
            return term;
        }


		///// <summary>
		///// 处理纯字符串、格式字符串、字符串表达式中的字符串表达式。
		///// 结束符号是否正确由调用方检查
		///// </summary>
		///// <param name="st"></param>
		///// <returns></returns>
		//public static IOperandTerm ReduceStringTerm(WordCollection wc, TermEndWith endWith)
		//{
		//    IOperandTerm term = reduceTerm(wc, false, endWith, VariableCode.__NULL__);
		//    if (term.GetOperandType() != typeof(string))
		//        throw new CodeEE("表达式的结果不是字符串");
		//    return term;
		//}

		public static IOperandTerm ReduceIntegerTerm(WordCollection wc, TermEndWith endwith)
		{
			IOperandTerm term = reduceTerm(wc, false, endwith, VariableCode.__NULL__);
            if (term == null)
                throw new CodeEE("无法将语法解析为表达式");
			if (term.GetOperandType() != typeof(Int64))
				throw new CodeEE("表达式的结果不是数值");
			return term;
		}

		
        /// <summary>
        /// 根据结果可能会返回SingleTerm。
        /// </summary>
        /// <returns></returns>
		public static IOperandTerm ToStrFormTerm(StrFormWord sfw)
		{
			StrForm strf = StrForm.FromWordToken(sfw);
			if(strf.IsConst)
				return new SingleTerm(strf.GetString(null));
			return new StrFormTerm(strf);
		}

		/// <summary>
		/// 批量获取以逗号分隔的CASE参数。以行尾结束。
		/// </summary>
		/// <param name="st"></param>
		/// <returns></returns>
		public static CaseExpression[] ReduceCaseExpressions(WordCollection wc)
		{
			List<CaseExpression> terms = new List<CaseExpression>();
			while (!wc.EOL)
			{
				terms.Add(reduceCaseExpression(wc));
				wc.ShiftNext();
			}
			CaseExpression[] ret = new CaseExpression[terms.Count];
			terms.CopyTo(ret);
			return ret;
		}

		public static IOperandTerm ReduceVariableArgument(WordCollection wc, VariableCode varCode)
		{
			IOperandTerm ret = reduceTerm(wc, false, TermEndWith.EoL, varCode);
			if(ret == null)
                throw new CodeEE("变量的:后面没有参数");
			return ret;
		}

		public static VariableToken ReduceVariableIdentifier(WordCollection wc, string idStr)
		{
			string subId = null;
			if (wc.Current.Type == '@')
			{
				wc.ShiftNext();
				IdentifierWord subidWT = wc.Current as IdentifierWord;
				if (subidWT == null)
					throw new CodeEE("@的使用方式不正确");
				wc.ShiftNext();
				subId = subidWT.Code;
			}
			return GlobalStatic.IdentifierDictionary.GetVariableToken(idStr, subId, true);
		}


		/// <summary>
		/// 解析一个标识符
		/// </summary>
		/// <param name="wc"></param>
		/// <param name="idStr">标识符字符串</param>
		/// <param name="varCode">如果是变量的参数则为该变量的Code。按关联数组方式使用</param>
		/// <returns></returns>
		private static IOperandTerm reduceIdentifier(WordCollection wc, string idStr, VariableCode varCode)
		{
			wc.ShiftNext();
			SymbolWord symbol = wc.Current as SymbolWord;
			if (symbol != null && symbol.Type == '.')
			{//命名空间
				throw new NotImplCodeEE();
			}
			else if (symbol != null && (symbol.Type == '(' || symbol.Type == '['))
			{//函数
				wc.ShiftNext();
				if (symbol.Type == '[')//1810 大概永远不会实现
					throw new CodeEE("使用[]的功能尚未实现");
				//处理参数
				IOperandTerm[] args = ReduceArguments(wc, ArgsEndWith.RightParenthesis, false);
				IOperandTerm mToken = GlobalStatic.IdentifierDictionary.GetFunctionMethod(GlobalStatic.LabelDictionary, idStr, args, false);
				if (mToken == null)
				{
					if (!Program.AnalysisMode)
						GlobalStatic.IdentifierDictionary.ThrowException(idStr, true);
					else
					{
                        long t = 0;
						if (GlobalStatic.tempDic.TryGetValue(idStr, out t))
							GlobalStatic.tempDic[idStr] = t + 1;
						else
							GlobalStatic.tempDic.Add(idStr, 1);
						return new NullTerm(0);
					}
				}
				return mToken;
			}
			else
			{//变量或关键字
				VariableToken id = ReduceVariableIdentifier(wc, idStr);
				if (id != null)//如果idStr是变量名，
				{
					if (varCode != VariableCode.__NULL__)//变量的参数不会再有参数
						return VariableParser.ReduceVariable(id, null, null, null);
					else
						return VariableParser.ReduceVariable(id, wc);
				}
				//如果idStr不是变量名，
				IOperandTerm refToken = GlobalStatic.IdentifierDictionary.GetFunctionMethod(GlobalStatic.LabelDictionary, idStr, null, false);
				if (refToken != null)//如果函数引用名匹配则返回该引用。实际使用时会报错
					return refToken;
				if (varCode != VariableCode.__NULL__ && GlobalStatic.ConstantData.isDefined(varCode, idStr))//存在关联数组可能性
					return new SingleTerm(idStr);
				GlobalStatic.IdentifierDictionary.ThrowException(idStr, false);
			}
			throw new ExeEE("未能抛出错误");//到此为止应该要么throw要么return。
		}

		#endregion

		#region private reduce
		private static CaseExpression reduceCaseExpression(WordCollection wc)
		{
			CaseExpression ret = new CaseExpression();
			IdentifierWord id = wc.Current as IdentifierWord;
			if ((id != null) && (id.Code.Equals("IS", Config.SCVariable)))
			{
				wc.ShiftNext();
				ret.CaseType = CaseExpressionType.Is;
				OperatorWord opWT = wc.Current as OperatorWord;
				if (opWT == null)
					throw new CodeEE("IS关键字后面没有运算符");

				OperatorCode op = opWT.Code;
				if (!OperatorManager.IsBinary(op))
					throw new CodeEE("IS关键字后面的运算符不是二元运算符");
				wc.ShiftNext();
				ret.Operator = op;
				ret.LeftTerm = reduceTerm(wc, false, TermEndWith.Comma, VariableCode.__NULL__);
				if (ret.LeftTerm == null)
					throw new CodeEE("IS关键字后面没有表达式");
				Type type = ret.LeftTerm.GetOperandType();
				return ret;
			}
			ret.LeftTerm = reduceTerm(wc, true, TermEndWith.Comma, VariableCode.__NULL__);
			if (ret.LeftTerm == null)
				throw new CodeEE("CASE的参数不能省略");
			id = wc.Current as IdentifierWord;
			if ((id != null) && (id.Code.Equals("TO", Config.SCVariable)))
			{
				ret.CaseType = CaseExpressionType.To;
				wc.ShiftNext();
				ret.RightTerm = reduceTerm(wc, true, TermEndWith.Comma, VariableCode.__NULL__);
				if (ret.RightTerm == null)
					throw new CodeEE("TO关键字后面没有表达式");
				id = wc.Current as IdentifierWord;
				if ((id != null) && (id.Code.Equals("TO", Config.SCVariable)))
					throw new CodeEE("TO关键字被使用了两次");
				if (ret.LeftTerm.GetOperandType() != ret.RightTerm.GetOperandType())
					throw new CodeEE("TO关键字前后类型不匹配");
				return ret;
			}
			ret.CaseType = CaseExpressionType.Normal;
			return ret;
		}


		/// <summary>
		/// 解析器主体
		/// </summary>
		/// <param name="wc"></param>
		/// <param name="allowKeywordTo">是否允许找到TO关键字</param>
		/// <param name="endWith">结束記号</param>
		/// <returns></returns>
        private static IOperandTerm reduceTerm(WordCollection wc, bool allowKeywordTo, TermEndWith endWith, VariableCode varCode)
        {
            TermStack stack = new TermStack();
            //int termCount = 0;
            int ternaryCount = 0;
            OperatorCode formerOp = OperatorCode.NULL;
			bool varArg = varCode != VariableCode.__NULL__;
			do
			{
				Word token = wc.Current;
				switch (token.Type)
				{
					case '\0':
						goto end;
					case '"'://LiteralStringWT
						stack.Add(((LiteralStringWord)token).Str);
						break;
					case '0'://LiteralIntegerWT
						stack.Add(((LiteralIntegerWord)token).Int);
						break;
					case 'F'://FormattedStringWT
						stack.Add(ToStrFormTerm((StrFormWord)token));
						break;
					case 'A'://IdentifierWT
						{
							string idStr = (((IdentifierWord)token).Code);
							if (idStr.Equals("TO", Config.SCVariable))
							{
								if (allowKeywordTo)
									goto end;
								else
									throw new CodeEE("TO关键字不能在此处使用");
							}
							else if (idStr.Equals("IS", Config.SCVariable))
								throw new CodeEE("IS关键字不能在此处使用");
							stack.Add(reduceIdentifier(wc, idStr, varCode));
							continue;
						}

					case '='://OperatorWT
						{
							if (varArg)
								throw new CodeEE("在读取变量参数时发现了意外的运算符");
							OperatorCode op = ((OperatorWord)token).Code;
							if (op == OperatorCode.Assignment)
							{
								if ((endWith & TermEndWith.Assignment) == TermEndWith.Assignment)
									goto end;
								throw new CodeEE("表达式中使用了赋值运算符'='(请使用'=='进行等价比较)");
							}

							if (formerOp == OperatorCode.Equal || formerOp == OperatorCode.Greater || formerOp == OperatorCode.Less
								|| formerOp == OperatorCode.GreaterEqual || formerOp == OperatorCode.LessEqual || formerOp == OperatorCode.NotEqual)
							{
								if (op == OperatorCode.Equal || op == OperatorCode.Greater || op == OperatorCode.Less
								|| op == OperatorCode.GreaterEqual || op == OperatorCode.LessEqual || op == OperatorCode.NotEqual)
								{
									ParserMediator.Warn("（構文上の注意）比較演算子が連続しています。", GlobalStatic.Process.GetScaningLine(), 0, false, false);
								}
							}
							stack.Add(op);
							formerOp = op;
							if (op == OperatorCode.Ternary_a)
								ternaryCount++;
							else if (op == OperatorCode.Ternary_b)
							{
								if (ternaryCount > 0)
									ternaryCount--;
								else
									throw new CodeEE("対応する'?'のない'#'です");
							}
							break;
						}
					case '(':
						wc.ShiftNext();
                        IOperandTerm inTerm = reduceTerm(wc, false, TermEndWith.RightParenthesis, VariableCode.__NULL__);
                        if (inTerm == null)
                            throw new CodeEE("かっこ\"(\"～\")\"の中に式が含まれていません");
						stack.Add(inTerm);
						if (wc.Current.Type != ')')
							throw new CodeEE("対応する')'のない'('です");
						//termCount++;
						wc.ShiftNext();
						continue;
					case ')':
						if ((endWith & TermEndWith.RightParenthesis) == TermEndWith.RightParenthesis)
							goto end;
						throw new CodeEE("構文解釈中に予期しない記号'" + token.Type + "'を発見しました");
					case ']':
						if ((endWith & TermEndWith.RightBracket) == TermEndWith.RightBracket)
							goto end;
						throw new CodeEE("構文解釈中に予期しない記号'" + token.Type + "'を発見しました");
					case ',':
						if ((endWith & TermEndWith.Comma) == TermEndWith.Comma)
							goto end;
						throw new CodeEE("構文解釈中に予期しない記号'" + token.Type + "'を発見しました");
					case 'M':
						throw new ExeEE("マクロ解決失敗");
					default:
						throw new CodeEE("構文解釈中に予期しない記号'" + token.Type + "'を発見しました");
				}
				//termCount++;
				wc.ShiftNext();
			} while (!varArg);
		end:
            if (ternaryCount > 0)
                throw new CodeEE("'?'と'#'の数が正しく対応していません");
            return stack.ReduceAll();
        }
        
		#endregion

		/// <summary>
        /// 表达式解析用类
        /// </summary>
        private class TermStack
        {
            /// <summary>
            /// 接下来应出现的内容的类型。
            /// 如果是(前置)一元运算符或等待值则为0，等待二元/三元运算符则为1，等待值则为2，等待与++、--、!对应的值则为3。
            /// </summary>
            int state = 0;
            bool hasBefore = false;
            bool hasAfter = false;
            bool waitAfter = false;
            Stack<Object> stack = new Stack<Object>();
            public void Add(OperatorCode op)
            {
                if (state == 2 || state == 3)
                    throw new CodeEE("式が異常です");
                if (state == 0)
                {
                    if (!OperatorManager.IsUnary(op))
                        throw new CodeEE("式が異常です");
                    stack.Push(op);
                    if (op == OperatorCode.Plus || op == OperatorCode.Minus || op == OperatorCode.BitNot)
                        state = 2;
                    else
                        state = 3;
                    return;
                }
                if (state == 1)
                {
                    //如果是一元后置运算符则进入特殊处理
                    if (OperatorManager.IsUnaryAfter(op))
                    {
                        if (hasAfter)
                        {
                            hasAfter = false;
                            throw new CodeEE("後置の単項演算子が複数存在しています");
                        }
                        if (hasBefore)
                        {
                            hasBefore = false;
                            throw new CodeEE("インクリメント・デクリメントを前置・後置両方同時に使うことはできません");
                        }
                        stack.Push(op);
                        reduceUnaryAfter();
                        //如果一元前置运算符在等待处理，在此处解决
                        if (waitAfter)
                            reduceUnary();
                        hasBefore = false;
                        hasAfter = true;
                        waitAfter = false;
                        return;
                    }
                    if (!OperatorManager.IsBinary(op) && !OperatorManager.IsTernary(op))
                        throw new CodeEE("式が異常です");
                    //先解决未解决的前置运算符
                    if (waitAfter)
                        reduceUnary();
                    int priority = OperatorManager.GetPriority(op);
                    //如果前一个计算的优先级相同或更高，则进行归约。
                    while (lastPriority() >= priority)
                    {
                        this.reduceLastThree();
                    }
                    stack.Push(op);
                    state = 0;
                    waitAfter = false;
                    hasBefore = false;
                    hasAfter = false;
                    return;
                }
                throw new CodeEE("式が異常です");
            }
            public void Add(Int64 i) { Add(new SingleTerm(i)); }
            public void Add(string s) { Add(new SingleTerm(s)); }
            public void Add(IOperandTerm term)
            {
                stack.Push(term);
                if (state == 1)
                    throw new CodeEE("式が異常です");
                if (state == 2)
                    waitAfter = true;
                if (state == 3)
                {
                    reduceUnary();
                    hasBefore = true;
                }
                state = 1;
                return;
            }


            private int lastPriority()
            {
                if (stack.Count < 3)
                    return -1;
                object temp = (object)stack.Pop();
                OperatorCode opCode = (OperatorCode)stack.Peek();
                int priority = OperatorManager.GetPriority(opCode);
                stack.Push(temp);
                return priority;
            }

            public IOperandTerm ReduceAll()
            {
                if (stack.Count == 0)
                    return null;
                if (state != 1)
                    throw new CodeEE("式が異常です");
                //一元运算符的等待未解决时在此处解决
                if (waitAfter)
                    reduceUnary();
                waitAfter = false;
                hasBefore = false;
                hasAfter = false;
                while (stack.Count > 1)
                {
                    reduceLastThree();
                }
                IOperandTerm retTerm = (IOperandTerm)stack.Pop();
                return retTerm;
            }

            private void reduceUnary()
            {
                //if (stack.Count < 2)
                //    throw new ExeEE("不合时宜的调用");
                IOperandTerm operand = (IOperandTerm)stack.Pop();
                OperatorCode op = (OperatorCode)stack.Pop();
                IOperandTerm newTerm = OperatorMethodManager.ReduceUnaryTerm(op, operand);
                stack.Push(newTerm);
            }

            private void reduceUnaryAfter()
            {
                //if (stack.Count < 2)
                //    throw new ExeEE("不合时宜的调用");
                OperatorCode op = (OperatorCode)stack.Pop();
                IOperandTerm operand = (IOperandTerm)stack.Pop();
                
                IOperandTerm newTerm = OperatorMethodManager.ReduceUnaryAfterTerm(op, operand);
                stack.Push(newTerm);
				
            }
            private void reduceLastThree()
            {
                //if (stack.Count < 2)
                //    throw new ExeEE("不合时宜的调用");
                IOperandTerm right = (IOperandTerm)stack.Pop();//後から入れたほうが右側
                OperatorCode op = (OperatorCode)stack.Pop();
                IOperandTerm left = (IOperandTerm)stack.Pop();
                if (OperatorManager.IsTernary(op))
                {
                    if (stack.Count > 1)
                    {
                        reduceTernary(left, right, op);
                        return;
                    }
                    throw new CodeEE("式の数が不足しています");
                }
                
                IOperandTerm newTerm = OperatorMethodManager.ReduceBinaryTerm(op, left, right);
                stack.Push(newTerm);
			}

            private void reduceTernary(IOperandTerm left, IOperandTerm right, OperatorCode op)
            {
                OperatorCode newOp = (OperatorCode)stack.Pop();
				IOperandTerm newLeft = (IOperandTerm)stack.Pop();
				
                IOperandTerm newTerm = OperatorMethodManager.ReduceTernaryTerm(newOp, newLeft, left, right);
                stack.Push(newTerm);
            }

			SingleTerm GetSingle(IOperandTerm oprand)
			{
				return (SingleTerm)oprand;
			}
        }

    }
}