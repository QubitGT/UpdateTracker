using System;
using System.Collections.Generic;
using System.Linq;

namespace Liv.Lck;

public class LCKPlugins
{
	private static LCKPlugins _instance;

	private static readonly object _lock = new object();

	private Dictionary<Type, ILCKPlugin> _pluginsByType;

	private Dictionary<string, ILCKPlugin> _pluginsByName;

	private bool _isInitialized;

	public static LCKPlugins Instance
	{
		get
		{
			if (_instance == null)
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new LCKPlugins();
					}
				}
			}
			return _instance;
		}
	}

	public int PluginCount => _pluginsByType.Count;

	public bool IsInitialized => _isInitialized;

	private LCKPlugins()
	{
		_pluginsByType = new Dictionary<Type, ILCKPlugin>();
		_pluginsByName = new Dictionary<string, ILCKPlugin>();
		_isInitialized = false;
	}

	public void Initialize(LckService lckService)
	{
		if (_isInitialized)
		{
			LckLog.LogWarning("LCKPlugins already initialized", "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 56);
			return;
		}
		LckLog.Log($"Initializing {_pluginsByType.Count} plugins", "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 60);
		foreach (ILCKPlugin value in _pluginsByType.Values)
		{
			try
			{
				value.Initialize(lckService);
				LckLog.Log("Initialized plugin: " + value.GetType().Name, "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 67);
			}
			catch (Exception ex)
			{
				LckLog.LogError("Failed to initialize plugin " + value.GetType().Name + ": " + ex.Message, "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 71);
			}
		}
		_isInitialized = true;
		LckLog.Log("LCKPlugins initialization complete", "Initialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 76);
	}

	internal void RegisterPlugin(ILCKPlugin plugin)
	{
		if (plugin == null)
		{
			LckLog.LogError("Attempted to register null plugin", "RegisterPlugin", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 87);
			return;
		}
		Type type = plugin.GetType();
		string pluginName = plugin.PluginName;
		if (_pluginsByType.ContainsKey(type))
		{
			LckLog.LogWarning("Plugin of type " + type.Name + " is already registered", "RegisterPlugin", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 96);
			return;
		}
		if (_pluginsByName.ContainsKey(pluginName))
		{
			LckLog.LogWarning("Plugin with name '" + pluginName + "' is already registered", "RegisterPlugin", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 102);
			return;
		}
		_pluginsByType[type] = plugin;
		_pluginsByName[pluginName] = plugin;
		LckLog.Log("Registered plugin: " + pluginName + " (" + type.Name + ")", "RegisterPlugin", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 109);
	}

	public bool HasPlugin<T>() where T : class, ILCKPlugin
	{
		return _pluginsByType.ContainsKey(typeof(T));
	}

	public bool HasPlugin(string pluginName)
	{
		return _pluginsByName.ContainsKey(pluginName);
	}

	public T GetPlugin<T>() where T : class, ILCKPlugin
	{
		if (_pluginsByType.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		return null;
	}

	public ILCKPlugin GetPlugin(string pluginName)
	{
		_pluginsByName.TryGetValue(pluginName, out var value);
		return value;
	}

	public IEnumerable<ILCKPlugin> GetAllPlugins()
	{
		return _pluginsByType.Values;
	}

	public IEnumerable<T> GetPluginsOfType<T>() where T : class, ILCKPlugin
	{
		return _pluginsByType.Values.OfType<T>();
	}

	public void UnregisterPlugin(ILCKPlugin plugin)
	{
		if (plugin != null)
		{
			Type type = plugin.GetType();
			string pluginName = plugin.PluginName;
			_pluginsByType.Remove(type);
			_pluginsByName.Remove(pluginName);
			LckLog.Log("Unregistered plugin: " + pluginName + " (" + type.Name + ")", "UnregisterPlugin", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 190);
		}
	}

	public void Clear()
	{
		_pluginsByType.Clear();
		_pluginsByName.Clear();
		_isInitialized = false;
		LckLog.Log("Cleared all registered plugins", "Clear", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPlugins.cs", 201);
	}
}
