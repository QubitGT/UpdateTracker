using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Meta.WitAi.Data.Configuration;

public class WitConfigurationCache
{
	private ConcurrentDictionary<string, WitConfiguration> _configurations = new ConcurrentDictionary<string, WitConfiguration>();

	private ConcurrentDictionary<string, int> _references = new ConcurrentDictionary<string, int>();

	public string GetCacheId(WitConfigurationCacheKey key)
	{
		return key.clientAccessToken + "_" + key.versionTag;
	}

	public WitConfigurationCacheKey GetCacheKey(WitConfiguration configuration)
	{
		WitConfigurationCacheKey result = default(WitConfigurationCacheKey);
		result.clientAccessToken = configuration?.GetClientAccessToken();
		result.versionTag = configuration?.GetVersionTag();
		return result;
	}

	public string GetCacheId(WitConfiguration configuration)
	{
		return GetCacheId(GetCacheKey(configuration));
	}

	public WitConfiguration Get(WitConfigurationCacheKey key, Action<WitConfiguration> onSetup = null)
	{
		string cacheId = GetCacheId(key);
		if (string.IsNullOrEmpty(cacheId))
		{
			return null;
		}
		if (_configurations.TryGetValue(cacheId, out var value))
		{
			_references[cacheId]++;
			return value;
		}
		WitConfiguration witConfiguration = ScriptableObject.CreateInstance<WitConfiguration>();
		witConfiguration.SetClientAccessToken(key.clientAccessToken);
		witConfiguration.editorVersionTag = key.versionTag;
		witConfiguration.buildVersionTag = key.versionTag;
		_configurations[cacheId] = witConfiguration;
		_references[cacheId] = 1;
		onSetup?.Invoke(witConfiguration);
		return witConfiguration;
	}

	public bool Return(WitConfiguration configuration, Action<WitConfiguration> onDestroy = null)
	{
		if (configuration == null)
		{
			return false;
		}
		string cacheId = GetCacheId(configuration);
		if (string.IsNullOrEmpty(cacheId) || !_references.TryGetValue(cacheId, out var value))
		{
			return false;
		}
		value--;
		if (value > 0)
		{
			_references[cacheId] = value;
			return false;
		}
		_configurations.TryRemove(cacheId, out var _);
		_references.TryRemove(cacheId, out var _);
		onDestroy?.Invoke(configuration);
		UnityEngine.Object.Destroy(configuration);
		return true;
	}
}
