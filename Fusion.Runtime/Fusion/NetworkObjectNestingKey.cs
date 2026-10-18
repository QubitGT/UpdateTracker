using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct NetworkObjectNestingKey : INetworkStruct, IEquatable<NetworkObjectNestingKey>
{
	public sealed class EqualityComparer : IEqualityComparer<NetworkObjectNestingKey>
	{
		public bool Equals(NetworkObjectNestingKey x, NetworkObjectNestingKey y)
		{
			return x.Value == y.Value;
		}

		public int GetHashCode(NetworkObjectNestingKey obj)
		{
			return obj.Value;
		}
	}

	public const int SIZE = 4;

	public const int ALIGNMENT = 4;

	[FieldOffset(0)]
	public int Value;

	public bool IsNone => Value == 0;

	public bool IsValid => Value > 0;

	public NetworkObjectNestingKey(int value)
	{
		Value = value;
	}

	public bool Equals(NetworkObjectNestingKey other)
	{
		return Value == other.Value;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkObjectNestingKey other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public override string ToString()
	{
		return IsNone ? "[NestingKey:None]" : $"[NestingKey:{Value}]";
	}
}
