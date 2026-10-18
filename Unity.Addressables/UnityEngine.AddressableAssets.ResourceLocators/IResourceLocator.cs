using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.AddressableAssets.ResourceLocators;

public interface IResourceLocator
{
	string LocatorId { get; }

	IEnumerable<object> Keys { get; }

	IEnumerable<IResourceLocation> AllLocations { get; }

	bool Locate(object key, Type type, out IList<IResourceLocation> locations);
}
