using System;
using System.Collections.Generic;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi;

[Serializable]
public abstract class PluggableBase<T>
{
	private static Type[] _pluginTypes;

	[SerializeField]
	protected List<T> LoadedPlugins;

	protected static void CheckForPlugins()
	{
		if (_pluginTypes == null)
		{
			FindPlugins();
		}
	}

	protected void EnsurePluginsAreLoaded()
	{
		CheckForPlugins();
		LoadedPlugins = new List<T>(BuildPlugins());
	}

	private static void FindPlugins()
	{
		_pluginTypes = ReflectionUtils.GetAllAssignableTypes<T>();
	}

	private static IEnumerable<T> BuildPlugins()
	{
		T[] array = new T[_pluginTypes.Length];
		for (int i = 0; i < array.Length; i++)
		{
			if (Activator.CreateInstance(_pluginTypes[i]) is T val)
			{
				array[i] = val;
			}
		}
		return array;
	}

	public TPluginType Get<TPluginType>() where TPluginType : T
	{
		if (LoadedPlugins == null)
		{
			EnsurePluginsAreLoaded();
		}
		return (TPluginType)(object)LoadedPlugins.Find((T path) => path is TPluginType);
	}

	public TPluginType[] GetAll<TPluginType>() where TPluginType : T
	{
		if (LoadedPlugins == null)
		{
			EnsurePluginsAreLoaded();
		}
		if (LoadedPlugins == null)
		{
			return Array.Empty<TPluginType>();
		}
		List<TPluginType> list = new List<TPluginType>();
		foreach (T loadedPlugin in LoadedPlugins)
		{
			if (loadedPlugin is TPluginType item)
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}
}
