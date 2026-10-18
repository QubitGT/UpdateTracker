using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets.ResourceLocators;

public class ResourceLocationMap : IResourceLocator
{
	private Dictionary<object, IList<IResourceLocation>> locations;

	public string LocatorId { get; private set; }

	public IEnumerable<IResourceLocation> AllLocations => locations.SelectMany((KeyValuePair<object, IList<IResourceLocation>> k) => k.Value);

	public Dictionary<object, IList<IResourceLocation>> Locations => locations;

	public IEnumerable<object> Keys => locations.Keys;

	public ResourceLocationMap(string id, int capacity = 0)
	{
		LocatorId = id;
		locations = new Dictionary<object, IList<IResourceLocation>>((capacity == 0) ? 100 : capacity);
	}

	public ResourceLocationMap(string id, IList<ResourceLocationData> locations)
	{
		LocatorId = id;
		if (locations == null)
		{
			return;
		}
		this.locations = new Dictionary<object, IList<IResourceLocation>>(locations.Count * 2);
		Dictionary<string, ResourceLocationBase> dictionary = new Dictionary<string, ResourceLocationBase>();
		Dictionary<string, ResourceLocationData> dictionary2 = new Dictionary<string, ResourceLocationData>();
		for (int i = 0; i < locations.Count; i++)
		{
			ResourceLocationData resourceLocationData = locations[i];
			if (resourceLocationData.Keys == null || resourceLocationData.Keys.Length < 1)
			{
				Addressables.LogErrorFormat("Address with id '{0}' does not have any valid keys, skipping...", resourceLocationData.InternalId);
			}
			else if (dictionary.ContainsKey(resourceLocationData.Keys[0]))
			{
				Addressables.LogErrorFormat("Duplicate address '{0}' with id '{1}' found, skipping...", resourceLocationData.Keys[0], resourceLocationData.InternalId);
			}
			else
			{
				ResourceLocationBase value = new ResourceLocationBase(resourceLocationData.Keys[0], Addressables.ResolveInternalId(resourceLocationData.InternalId), resourceLocationData.Provider, resourceLocationData.ResourceType)
				{
					Data = resourceLocationData.Data
				};
				dictionary.Add(resourceLocationData.Keys[0], value);
				dictionary2.Add(resourceLocationData.Keys[0], resourceLocationData);
			}
		}
		foreach (KeyValuePair<string, ResourceLocationBase> item in dictionary)
		{
			ResourceLocationData resourceLocationData2 = dictionary2[item.Key];
			if (resourceLocationData2.Dependencies != null)
			{
				string[] dependencies = resourceLocationData2.Dependencies;
				foreach (string key in dependencies)
				{
					item.Value.Dependencies.Add(dictionary[key]);
				}
				item.Value.ComputeDependencyHash();
			}
		}
		foreach (KeyValuePair<string, ResourceLocationBase> item2 in dictionary)
		{
			string[] dependencies = dictionary2[item2.Key].Keys;
			foreach (string key2 in dependencies)
			{
				Add(key2, item2.Value);
			}
		}
	}

	public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
	{
		IList<IResourceLocation> value = null;
		if (!this.locations.TryGetValue(key, out value))
		{
			locations = null;
			return false;
		}
		if (type == null)
		{
			locations = value;
			return true;
		}
		int num = 0;
		foreach (IResourceLocation item in value)
		{
			if (type.IsAssignableFrom(item.ResourceType))
			{
				num++;
			}
		}
		if (num == 0)
		{
			locations = null;
			return false;
		}
		if (num == value.Count)
		{
			locations = value;
			return true;
		}
		locations = new List<IResourceLocation>();
		foreach (IResourceLocation item2 in value)
		{
			if (type.IsAssignableFrom(item2.ResourceType))
			{
				locations.Add(item2);
			}
		}
		return true;
	}

	public void Add(object key, IResourceLocation location)
	{
		if (!locations.TryGetValue(key, out var value))
		{
			locations.Add(key, value = new List<IResourceLocation>());
		}
		value.Add(location);
	}

	public void Add(object key, IList<IResourceLocation> locations)
	{
		this.locations.Add(key, locations);
	}
}
