using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct Tick : IComparable<Tick>, IEquatable<Tick>
{
	public sealed class RelationalComparer : IComparer<Tick>
	{
		public int Compare(Tick x, Tick y)
		{
			return x.Raw.CompareTo(y.Raw);
		}
	}

	public sealed class EqualityComparer : IEqualityComparer<Tick>
	{
		public bool Equals(Tick x, Tick y)
		{
			return x.Raw == y.Raw;
		}

		public int GetHashCode(Tick obj)
		{
			return obj.Raw;
		}
	}

	public const int SIZE = 4;

	public const int ALIGNMENT = 4;

	[FieldOffset(0)]
	public int Raw;

	public Tick Next(int increment)
	{
		Tick result = default(Tick);
		result.Raw = Raw + increment;
		return result;
	}

	public bool Equals(Tick other)
	{
		return Raw == other.Raw;
	}

	public int CompareTo(Tick other)
	{
		return Raw.CompareTo(other.Raw);
	}

	public override bool Equals(object obj)
	{
		return obj is Tick other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Raw;
	}

	public override string ToString()
	{
		return $"[Tick:{(int)this}]";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(Tick a, Tick b)
	{
		return a.Raw > b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(Tick a, Tick b)
	{
		return a.Raw >= b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(Tick a, Tick b)
	{
		return a.Raw < b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(Tick a, Tick b)
	{
		return a.Raw <= b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Tick a, Tick b)
	{
		return a.Raw == b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Tick a, Tick b)
	{
		return a.Raw != b.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Tick(int value)
	{
		Tick result = default(Tick);
		result.Raw = ((value >= 0) ? value : 0);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator int(Tick value)
	{
		return value.Raw;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator bool(Tick value)
	{
		return value.Raw > 0;
	}
}
