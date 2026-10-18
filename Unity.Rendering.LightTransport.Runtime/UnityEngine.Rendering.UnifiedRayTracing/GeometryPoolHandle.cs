using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct GeometryPoolHandle : IEquatable<GeometryPoolHandle>
{
	public int index;

	public static readonly GeometryPoolHandle Invalid = new GeometryPoolHandle
	{
		index = -1
	};

	public readonly bool valid => index != -1;

	public bool Equals(GeometryPoolHandle other)
	{
		return index == other.index;
	}
}
