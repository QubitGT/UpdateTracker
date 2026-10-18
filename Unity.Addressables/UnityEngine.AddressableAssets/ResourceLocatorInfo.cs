using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets;

public class ResourceLocatorInfo
{
	public IResourceLocator Locator { get; private set; }

	public string LocalHash { get; private set; }

	public IResourceLocation CatalogLocation { get; private set; }

	internal bool ContentUpdateAvailable { get; set; }

	public IResourceLocation HashLocation => CatalogLocation.Dependencies[0];

	public bool CanUpdateContent
	{
		get
		{
			if (!string.IsNullOrEmpty(LocalHash) && CatalogLocation != null && CatalogLocation.HasDependencies)
			{
				return CatalogLocation.Dependencies.Count == 3;
			}
			return false;
		}
	}

	public ResourceLocatorInfo(IResourceLocator loc, string localHash, IResourceLocation remoteCatalogLocation)
	{
		Locator = loc;
		LocalHash = localHash;
		CatalogLocation = remoteCatalogLocation;
	}

	internal void UpdateContent(IResourceLocator locator, string hash, IResourceLocation loc)
	{
		LocalHash = hash;
		CatalogLocation = loc;
		Locator = locator;
	}
}
