using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
public class NetworkedWeavedStringAttribute : Attribute
{
	public int Capacity { get; }

	public string CacheFieldName { get; }

	public NetworkedWeavedStringAttribute(int capacity, string cacheFieldName)
	{
		Capacity = capacity;
		CacheFieldName = cacheFieldName;
	}
}
