using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct Ptr : IEquatable<Ptr>, INetworkStruct
{
	public sealed class EqualityComparer : IEqualityComparer<Ptr>
	{
		public bool Equals(Ptr x, Ptr y)
		{
			return x.Address == y.Address;
		}

		public int GetHashCode(Ptr obj)
		{
			return obj.Address;
		}
	}

	public const int SIZE = 4;

	[FieldOffset(0)]
	public int Address;

	public static Ptr Null => default(Ptr);

	public bool Equals(Ptr other)
	{
		return Address == other.Address;
	}

	public override bool Equals(object obj)
	{
		return obj is Ptr other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Address;
	}

	public override string ToString()
	{
		return $"0x{Address:X}";
	}

	public static implicit operator bool(Ptr a)
	{
		return a.Address != 0;
	}

	public static bool operator ==(Ptr a, Ptr b)
	{
		return a.Address == b.Address;
	}

	public static bool operator !=(Ptr a, Ptr b)
	{
		return a.Address != b.Address;
	}

	public static Ptr operator +(Ptr p, int v)
	{
		p.Address += v;
		return p;
	}

	public static Ptr operator -(Ptr p, int v)
	{
		p.Address -= v;
		return p;
	}
}
