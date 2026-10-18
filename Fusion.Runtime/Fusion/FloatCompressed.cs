using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct FloatCompressed : INetworkStruct, IEquatable<FloatCompressed>
{
	[FieldOffset(0)]
	public int valueEncoded;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator FloatCompressed(float v)
	{
		FloatCompressed result = default(FloatCompressed);
		result.valueEncoded = FloatUtils.Compress(v);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float(FloatCompressed q)
	{
		return FloatUtils.Decompress(q.valueEncoded);
	}

	public bool Equals(FloatCompressed other)
	{
		return valueEncoded == other.valueEncoded;
	}

	public override bool Equals(object obj)
	{
		return obj is FloatCompressed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return valueEncoded;
	}

	public static bool operator ==(FloatCompressed left, FloatCompressed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(FloatCompressed left, FloatCompressed right)
	{
		return !left.Equals(right);
	}
}
