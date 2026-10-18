using System;
using System.Collections.Generic;
using System.Linq;

namespace Liv.Lck;

public static class LCKPluginIntegration
{
	public static void InitializePlugins(LckService lckService)
	{
		if (lckService == null)
		{
			LckLog.LogError("Cannot initialize plugins with null LCK service", "InitializePlugins", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 20);
		}
		else
		{
			LCKPlugins.Instance.Initialize(lckService);
		}
	}

	public static void ShutdownPlugins()
	{
		foreach (ILCKPlugin allPlugin in LCKPlugins.Instance.GetAllPlugins())
		{
			try
			{
				allPlugin.Shutdown();
			}
			catch (Exception ex)
			{
				LckLog.LogError("Failed to shutdown plugin " + allPlugin.PluginName + ": " + ex.Message, "ShutdownPlugins", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 41);
			}
		}
	}

	public static T GetPlugin<T>() where T : class, ILCKPlugin
	{
		return LCKPlugins.Instance.GetPlugin<T>();
	}

	public static bool HasPlugin<T>() where T : class, ILCKPlugin
	{
		return LCKPlugins.Instance.HasPlugin<T>();
	}

	public static void LogPluginInfo()
	{
		IEnumerable<ILCKPlugin> allPlugins = LCKPlugins.Instance.GetAllPlugins();
		LckLog.Log($"Registered plugins ({allPlugins.Count()}):", "LogPluginInfo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 72);
		foreach (ILCKPlugin item in allPlugins)
		{
			LckLog.Log("  - " + item.PluginName + " v" + item.PluginVersion + " (" + item.GetType().Name + ")", "LogPluginInfo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 76);
		}
	}
}
