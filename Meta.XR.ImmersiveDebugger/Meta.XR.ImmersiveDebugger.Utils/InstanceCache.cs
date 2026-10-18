using System;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.ImmersiveDebugger.UserInterface;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Utils;

internal class InstanceCache
{
	internal readonly Dictionary<Type, List<InstanceHandle>> CacheData = new Dictionary<Type, List<InstanceHandle>>();

	private readonly List<InstanceHandle> _emptyCache = new List<InstanceHandle>();

	public event Action<Type> OnCacheChangedForTypeEvent;

	public event Func<InstanceHandle, IInspector> OnInstanceAdded;

	public event Action<InstanceHandle> OnInstanceRemoved;

	public List<InstanceHandle> GetCacheDataForClass(Type classType)
	{
		CacheData.TryGetValue(classType, out var value);
		return value ?? _emptyCache;
	}

	private List<InstanceHandle> FetchObjectsHandlesOfType(Type classType)
	{
		return (from obj in UnityEngine.Object.FindObjectsByType(classType, FindObjectsSortMode.None)
			select new InstanceHandle(classType, obj)).ToList();
	}

	public void RegisterClassType(Type classType)
	{
		if (!CacheData.ContainsKey(classType))
		{
			CacheData[classType] = new List<InstanceHandle>();
		}
	}

	public void RegisterClassTypes(IEnumerable<Type> types)
	{
		foreach (Type type in types)
		{
			RegisterClassType(type);
		}
	}

	internal void RetrieveInstances()
	{
		foreach (KeyValuePair<Type, List<InstanceHandle>> item in new Dictionary<Type, List<InstanceHandle>>(CacheData))
		{
			Type key = item.Key;
			List<InstanceHandle> value = item.Value;
			bool flag = false;
			foreach (InstanceHandle item2 in FetchObjectsHandlesOfType(key))
			{
				if (!value.Contains(item2))
				{
					value.Add(item2);
					flag = true;
					this.OnInstanceAdded?.Invoke(item2);
				}
			}
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (!value[num].Valid)
				{
					this.OnInstanceRemoved?.Invoke(value[num]);
					value.RemoveAt(num);
					flag = true;
				}
			}
			if (flag)
			{
				CacheData[key] = value;
				this.OnCacheChangedForTypeEvent?.Invoke(key);
			}
		}
	}

	internal void RegisterHandle(InstanceHandle handle)
	{
		RegisterClassType(handle.Type);
		if (CacheData.TryGetValue(handle.Type, out var value))
		{
			value.Add(handle);
		}
	}

	internal void UnregisterHandle(InstanceHandle handle)
	{
		if (CacheData.TryGetValue(handle.Type, out var value))
		{
			value.Remove(handle);
		}
	}
}
