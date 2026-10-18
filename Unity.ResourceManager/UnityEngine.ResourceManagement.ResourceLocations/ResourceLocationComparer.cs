using System.Collections.Generic;

namespace UnityEngine.ResourceManagement.ResourceLocations;

public class ResourceLocationComparer : IEqualityComparer<IResourceLocation>
{
	public bool Equals(IResourceLocation x, IResourceLocation y)
	{
		return GetHashCode(x) == GetHashCode(y);
	}

	public int GetHashCode(IResourceLocation obj)
	{
		return obj.InternalId.GetHashCode() * 31 + obj.ResourceType.GetHashCode();
	}
}
