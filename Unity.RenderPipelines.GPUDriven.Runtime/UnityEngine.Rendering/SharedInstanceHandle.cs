using System;

namespace UnityEngine.Rendering;

internal struct SharedInstanceHandle : IEquatable<SharedInstanceHandle>, IComparable<SharedInstanceHandle>
{
	public static readonly SharedInstanceHandle Invalid = new SharedInstanceHandle
	{
		index = -1
	};

	public int index { get; set; }

	public bool valid => index != -1;

	public bool Equals(SharedInstanceHandle other)
	{
		return index == other.index;
	}

	public int CompareTo(SharedInstanceHandle other)
	{
		return index.CompareTo(other.index);
	}

	public override int GetHashCode()
	{
		return index;
	}
}
