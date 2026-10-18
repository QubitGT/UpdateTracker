using System;

namespace UnityEngine.Rendering;

internal struct GPUInstanceIndex : IEquatable<GPUInstanceIndex>, IComparable<GPUInstanceIndex>
{
	public static readonly GPUInstanceIndex Invalid = new GPUInstanceIndex
	{
		index = -1
	};

	public int index { get; set; }

	public bool valid => index != -1;

	public bool Equals(GPUInstanceIndex other)
	{
		return index == other.index;
	}

	public int CompareTo(GPUInstanceIndex other)
	{
		return index.CompareTo(other.index);
	}

	public override int GetHashCode()
	{
		return index;
	}
}
