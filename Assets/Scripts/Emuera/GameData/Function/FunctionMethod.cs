using System;
using System.Collections.Generic;
using System.Text;
using MinorShift.Emuera.GameData.Expression;
using MinorShift.Emuera.Sub;

namespace MinorShift.Emuera.GameData.Function
{
	internal abstract class FunctionMethod
	{
		public Type ReturnType { get; protected set; }
		protected Type[] argumentTypeArray;
		protected string Name { get; private set; }

		//参数数量和类型是否匹配的测试
		//不正确时返回错误消息。
		//参数数量不定或允许省略参数时需要override。
		public virtual string CheckArgumentType(string name, IOperandTerm[] arguments)
		{
			if (arguments.Length != argumentTypeArray.Length)
				return string.Format(Properties.Resources.SyntaxErrMesMethodDefaultArgumentNum0, name);
			for (int i = 0; i < argumentTypeArray.Length; i++)
			{
				if (arguments[i] == null)
					return string.Format(Properties.Resources.SyntaxErrMesMethodDefaultArgumentNotNullable0, name, i+1);
				if (argumentTypeArray[i] != typeof(void) && argumentTypeArray[i] != arguments[i].GetOperandType())
					return string.Format(Properties.Resources.SyntaxErrMesMethodDefaultArgumentType0, name, i + 1);
			}
			return null;
		}
		
		//参数全部为常量时是否可以将Method解体。引用RAND或Chara等的情况不可
		public bool CanRestructure { get; protected set; }

		//FunctionMethod是否拥有固有的Restructure()
		public bool HasUniqueRestructure { get; protected set; }

		//实际的计算。
		public virtual Int64 GetIntValue(ExpressionMediator exm, IOperandTerm[] arguments) { throw new ExeEE("戻り値の型が違う or 未実装"); }
		public virtual string GetStrValue(ExpressionMediator exm, IOperandTerm[] arguments) { throw new ExeEE("戻り値の型が違う or 未実装"); }
		public virtual SingleTerm GetReturnValue(ExpressionMediator exm, IOperandTerm[] arguments)
		{
			if (ReturnType == typeof(Int64))
				return new SingleTerm(GetIntValue(exm, arguments));
			else
				return new SingleTerm(GetStrValue(exm, arguments));
		}

		/// <summary>
		/// 返回值表示整体是否可以Restructure
		/// </summary>
		/// <param name="exm"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		public virtual bool UniqueRestructure(ExpressionMediator exm, IOperandTerm[] arguments)
		{ throw new ExeEE("未実装？"); }


		internal void SetMethodName(string name)
		{
			Name = name;
		}
	}
}
