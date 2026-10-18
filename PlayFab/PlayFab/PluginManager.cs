using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.Json;

namespace PlayFab;

public class PluginManager
{
	private Dictionary<PluginContractKey, IPlayFabPlugin> plugins = new Dictionary<PluginContractKey, IPlayFabPlugin>(new PluginContractKeyComparator());

	private static readonly PluginManager Instance = new PluginManager();

	private PluginManager()
	{
	}

	public static T GetPlugin<T>(PluginContract contract, string instanceName = "") where T : IPlayFabPlugin
	{
		return (T)Instance.GetPluginInternal(contract, instanceName);
	}

	public static void SetPlugin(IPlayFabPlugin plugin, PluginContract contract, string instanceName = "")
	{
		Instance.SetPluginInternal(plugin, contract, instanceName);
	}

	private IPlayFabPlugin GetPluginInternal(PluginContract contract, string instanceName)
	{
		PluginContractKey pluginContractKey = default(PluginContractKey);
		pluginContractKey._pluginContract = contract;
		pluginContractKey._pluginName = instanceName;
		PluginContractKey key = pluginContractKey;
		if (!plugins.TryGetValue(key, out var value))
		{
			value = contract switch
			{
				PluginContract.PlayFab_Serializer => CreatePlugin<SimpleJsonInstance>(), 
				PluginContract.PlayFab_Transport => CreatePlayFabTransportPlugin(), 
				_ => throw new ArgumentException("This contract is not supported", "contract"), 
			};
			plugins[key] = value;
		}
		return value;
	}

	private void SetPluginInternal(IPlayFabPlugin plugin, PluginContract contract, string instanceName)
	{
		if (plugin == null)
		{
			throw new ArgumentNullException("plugin", "Plugin instance cannot be null");
		}
		PluginContractKey pluginContractKey = default(PluginContractKey);
		pluginContractKey._pluginContract = contract;
		pluginContractKey._pluginName = instanceName;
		PluginContractKey key = pluginContractKey;
		plugins[key] = plugin;
	}

	private IPlayFabPlugin CreatePlugin<T>() where T : IPlayFabPlugin, new()
	{
		return (IPlayFabPlugin)Activator.CreateInstance(typeof(T));
	}

	private ITransportPlugin CreatePlayFabTransportPlugin()
	{
		ITransportPlugin transportPlugin = null;
		if (PlayFabSettings.RequestType == WebRequestType.HttpWebRequest)
		{
			transportPlugin = new PlayFabWebRequest();
		}
		if (transportPlugin == null)
		{
			transportPlugin = new PlayFabUnityHttp();
		}
		return transportPlugin;
	}
}
