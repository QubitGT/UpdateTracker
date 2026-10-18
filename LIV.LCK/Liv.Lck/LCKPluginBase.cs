using System;

namespace Liv.Lck;

public abstract class LCKPluginBase : ILCKPlugin
{
	protected LckService LckService { get; private set; }

	protected bool IsInitialized { get; private set; }

	public abstract string PluginName { get; }

	public abstract string PluginVersion { get; }

	protected LCKPluginBase()
	{
		LCKPlugins.Instance.RegisterPlugin(this);
	}

	public void Initialize(LckService lckService)
	{
		if (IsInitialized)
		{
			LckLog.LogWarning("Plugin " + PluginName + " is already initialized", "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 47);
			return;
		}
		LckService = lckService;
		try
		{
			OnInitialize();
			IsInitialized = true;
			LckLog.Log("Plugin " + PluginName + " initialized successfully", "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 57);
		}
		catch (Exception ex)
		{
			LckLog.LogError("Failed to initialize plugin " + PluginName + ": " + ex.Message, "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 61);
			throw;
		}
	}

	public void Shutdown()
	{
		if (!IsInitialized)
		{
			LckLog.LogWarning("Plugin " + PluginName + " is not initialized", "Shutdown", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 73);
			return;
		}
		try
		{
			OnShutdown();
			IsInitialized = false;
			LckService = null;
			LckLog.Log("Plugin " + PluginName + " shutdown successfully", "Shutdown", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 82);
		}
		catch (Exception ex)
		{
			LckLog.LogError("Failed to shutdown plugin " + PluginName + ": " + ex.Message, "Shutdown", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginBase.cs", 86);
			throw;
		}
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnShutdown()
	{
	}

	protected bool HasPlugin<T>() where T : class, ILCKPlugin
	{
		return LCKPlugins.Instance.HasPlugin<T>();
	}

	protected bool HasPlugin(string pluginName)
	{
		return LCKPlugins.Instance.HasPlugin(pluginName);
	}

	protected T GetPlugin<T>() where T : class, ILCKPlugin
	{
		return LCKPlugins.Instance.GetPlugin<T>();
	}

	protected ILCKPlugin GetPlugin(string pluginName)
	{
		return LCKPlugins.Instance.GetPlugin(pluginName);
	}
}
