using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Unity.Mathematics;

[Serializable]
[Unity.IL2CPP.CompilerServices.Il2CppEagerStaticClassConstruction]
public struct half : IEquatable<half>, IFormattable
{
	public ushort value;

	public static readonly half zero;

	public static float MaxValue => 65504f;

	public static float MinValue => -65504f;

	public static half MaxValueAsHalf => new half(MaxValue);

	public static half MinValueAsHalf => new half(MinValue);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public half(half x)
	{
		value = x.value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public half(float v)
	{
		value = (ushort)math.f32tof16(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public half(double v)
	{
		value = (ushort)math.f32tof16((float)v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator half(float v)
	{
		return new half(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static explicit operator half(double v)
	{
		return new half(v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float(half d)
	{
		return math.f16tof32(d.value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator double(half d)
	{
		return math.f16tof32(d.value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(half lhs, half rhs)
	{
		return lhs.value == rhs.value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(half lhs, half rhs)
	{
		return lhs.value != rhs.value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(half rhs)
	{
		return value == rhs.value;
	}

	public override bool Equals(object o)
	{
		if (o is half rhs)
		{
			return Equals(rhs);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return math.f16tof32(value).ToString();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format, IFormatProvider formatProvider)
	{
		return math.f16tof32(value).ToString(format, formatProvider);
	}
}
