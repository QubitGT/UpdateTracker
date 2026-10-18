using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.U2D;

namespace UnityEngine.AddressableAssets;

internal class DynamicResourceLocator : IResourceLocator
{
	private AddressablesImpl m_Addressables;

	private string m_AtlasSpriteProviderId;

	public string LocatorId => "DynamicResourceLocator";

	public virtual IEnumerable<object> Keys => new object[0];

	private string AtlasSpriteProviderId
	{
		get
		{
			if (!string.IsNullOrEmpty(m_AtlasSpriteProviderId))
			{
				return m_AtlasSpriteProviderId;
			}
			foreach (IResourceProvider resourceProvider in m_Addressables.ResourceManager.ResourceProviders)
			{
				if (resourceProvider is AtlasSpriteProvider)
				{
					m_AtlasSpriteProviderId = resourceProvider.ProviderId;
					return m_AtlasSpriteProviderId;
				}
			}
			return typeof(AtlasSpriteProvider).FullName;
		}
	}

	public IEnumerable<IResourceLocation> AllLocations => new IResourceLocation[0];

	public DynamicResourceLocator(AddressablesImpl addr)
	{
		m_Addressables = addr;
	}

	public bool Locate(object key, Type type, out IList<IResourceLocation> locations)
	{
		locations = null;
		if (ResourceManagerConfig.ExtractKeyAndSubKey(key, out var mainKey, out var subKey))
		{
			if (!m_Addressables.GetResourceLocations(mainKey, type, out var locations2) && type == typeof(Sprite))
			{
				m_Addressables.GetResourceLocations(mainKey, typeof(SpriteAtlas), out locations2);
			}
			if (locations2 != null && locations2.Count > 0)
			{
				locations = new List<IResourceLocation>(locations2.Count);
				foreach (IResourceLocation item in locations2)
				{
					CreateDynamicLocations(type, locations, key as string, subKey, item);
				}
				return true;
			}
		}
		return false;
	}

	internal void CreateDynamicLocations(Type type, IList<IResourceLocation> locations, string locName, string subKey, IResourceLocation mainLoc)
	{
		if (type == typeof(Sprite) && mainLoc.ResourceType == typeof(SpriteAtlas))
		{
			locations.Add(new ResourceLocationBase(locName, mainLoc.InternalId + "[" + subKey + "]", AtlasSpriteProviderId, type, mainLoc));
		}
		else if (mainLoc.HasDependencies)
		{
			locations.Add(new ResourceLocationBase(locName, mainLoc.InternalId + "[" + subKey + "]", mainLoc.ProviderId, mainLoc.ResourceType, mainLoc.Dependencies.ToArray()));
		}
		else
		{
			locations.Add(new ResourceLocationBase(locName, mainLoc.InternalId + "[" + subKey + "]", mainLoc.ProviderId, mainLoc.ResourceType));
		}
	}
}
