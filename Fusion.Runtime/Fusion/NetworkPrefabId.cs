using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Serialization;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[InlineHelp]
[NetworkStructWeaved(1)]
public struct NetworkPrefabId : INetworkStruct, IEquatable<NetworkPrefabId>, IComparable, IComparable<NetworkPrefabId>
{
	public sealed class EqualityComparer : IEqualityComparer<NetworkPrefabId>
	{
		public bool Equals(NetworkPrefabId x, NetworkPrefabId y)
		{
			return x.RawValue == y.RawValue;
		}

		public int GetHashCode(NetworkPrefabId obj)
		{
			return (int)obj.RawValue;
		}
	}

	public const int SIZE = 4;

	public const int ALIGNMENT = 4;

	public const int MAX_INDEX = 2147483646;

	[FieldOffset(0)]
	[FormerlySerializedAs("Value")]
	public uint RawValue;

	public bool IsNone => RawValue == 0;

	public bool IsValid => RawValue != 0;

	public int AsIndex => (int)(RawValue - 1);

	public static NetworkPrefabId FromIndex(int index)
	{
		if (index < 0 || index >= int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		NetworkPrefabId result = default(NetworkPrefabId);
		result.RawValue = (uint)(index + 1);
		return result;
	}

	public static NetworkPrefabId FromRaw(uint value)
	{
		NetworkPrefabId result = default(NetworkPrefabId);
		result.RawValue = value;
		return result;
	}

	public bool Equals(NetworkPrefabId other)
	{
		return RawValue == other.RawValue;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkPrefabId other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (int)RawValue;
	}

	public override string ToString()
	{
		return ToString(brackets: true, prefix: true);
	}

	int IComparable.CompareTo(object obj)
	{
		if (obj is NetworkPrefabId other)
		{
			return CompareTo(other);
		}
		return -1;
	}

	public string ToString(bool brackets, bool prefix)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (brackets)
		{
			stringBuilder.Append('[');
		}
		if (IsValid)
		{
			if (prefix)
			{
				stringBuilder.Append("Index:");
			}
			stringBuilder.Append(AsIndex);
		}
		else
		{
			stringBuilder.Append("Invalid");
		}
		if (brackets)
		{
			stringBuilder.Append(']');
		}
		return stringBuilder.ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkPrefabId a, NetworkPrefabId b)
	{
		return a.RawValue == b.RawValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkPrefabId a, NetworkPrefabId b)
	{
		return a.RawValue != b.RawValue;
	}

	public int CompareTo(NetworkPrefabId other)
	{
		return RawValue.CompareTo(other.RawValue);
	}
}
