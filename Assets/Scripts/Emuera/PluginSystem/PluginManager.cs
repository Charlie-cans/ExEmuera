using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MinorShift.Emuera.PluginSystem
{
	// 插件管理器 - 单例模式，管理所有已加载的插件方法
	public class PluginManager
	{
		private static PluginManager _instance;
		public static PluginManager Instance
		{
			get
			{
				if (_instance == null) _instance = new PluginManager();
				return _instance;
			}
		}

		private Dictionary<string, IPluginMethod> methods = new Dictionary<string, IPluginMethod>(StringComparer.OrdinalIgnoreCase);
		private bool loaded = false;

		private PluginManager() { }

		// 从Plugins目录加载所有插件DLL
		public void LoadPlugins()
		{
			if (loaded) return;
			loaded = true;

			string pluginsDir = Path.Combine(_Library.Sys.ExeDir, "Plugins");
			if (!Directory.Exists(pluginsDir)) return;

			foreach (string dllPath in Directory.GetFiles(pluginsDir, "*.dll"))
			{
				try
				{
					Assembly asm = Assembly.LoadFrom(dllPath);
					Type[] types = asm.GetTypes();
					foreach (Type t in types)
					{
						if (t.IsAbstract || !typeof(BasePluginManifest).IsAssignableFrom(t)) continue;
						BasePluginManifest manifest = (BasePluginManifest)Activator.CreateInstance(t);
						IPluginMethod[] pluginMethods = manifest.GetPluginMethods();
						if (pluginMethods != null)
						{
							foreach (IPluginMethod method in pluginMethods)
							{
								if (!string.IsNullOrEmpty(method.Name))
									methods[method.Name] = method;
							}
						}
					}
				}
				catch (Exception e) { UnityEngine.Debug.Log("[PluginManager] 加载插件失败: " + dllPath + " - " + e.Message); }
			}
		}

		// 查找插件方法（不区分大小写）
		public IPluginMethod GetMethod(string name)
		{
			methods.TryGetValue(name, out var m);
			return m;
		}

		// 检查方法是否存在
		public bool HasMethod(string name)
		{
			return methods.ContainsKey(name);
		}

		// 获取所有已注册的方法名
		public string[] GetMethodNames()
		{
			string[] names = new string[methods.Count];
			methods.Keys.CopyTo(names, 0);
			return names;
		}

		// 重置（用于重启动）
		public void Reset()
		{
			methods.Clear();
			loaded = false;
		}
	}
}
