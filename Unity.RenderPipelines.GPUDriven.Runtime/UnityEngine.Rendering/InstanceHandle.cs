using System;

namespace UnityEngine.Rendering;

internal struct InstanceHandle : IEquatable<InstanceHandle>, IComparable<InstanceHandle>
{
	public static readonly InstanceHandle Invalid = new InstanceHandle
	{
		index = -1
	};

	public int index { get; private set; }

	public int instanceIndex => index >> 1;

	public InstanceType type => (InstanceType)((long)index & 1L);

	public bool valid => index != -1;

	public static InstanceHandle Create(int instanceIndex, InstanceType instanceType)
	{
		InstanceHandle result = default(InstanceHandle);
		result.index = (instanceIndex << 1) | (int)instanceType;
		return result;
	}

	public static InstanceHandle FromInt(int value)
	{
		InstanceHandle result = default(InstanceHandle);
		result.index = value;
		return result;
	}

	public bool Equals(InstanceHandle other)
	{
		return index == other.index;
	}

	public int CompareTo(InstanceHandle other)
	{
		return index.CompareTo(other.index);
	}

	public override int GetHashCode()
	{
		return index;
	}
}
