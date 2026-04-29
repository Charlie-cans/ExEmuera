using System;

namespace MinorShift.Emuera.PluginSystem
{
	// 插件方法参数
	public class PluginMethodParameter
	{
		public bool IsString;
		public string StrValue;
		public long IntValue;

		public PluginMethodParameter(string s) { IsString = true; StrValue = s; IntValue = 0; }
		public PluginMethodParameter(long v) { IsString = false; StrValue = v.ToString(); IntValue = v; }
	}

	// 插件方法接口 - 外部DLL实现此接口
	public interface IPluginMethod
	{
		string Name { get; }
		string Description { get; }
		void Execute(PluginMethodParameter[] args);
	}

	// 插件清单基类 - 外部DLL继承此类
	public abstract class BasePluginManifest
	{
		public abstract string PluginName { get; }
		public abstract string PluginVersion { get; }
		public abstract IPluginMethod[] GetPluginMethods();
	}
}
