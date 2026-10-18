using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(4)]
public struct QuaternionCompressed : INetworkStruct, IEquatable<QuaternionCompressed>
{
	[FieldOffset(0)]
	public int xEncoded;

	[FieldOffset(4)]
	public int yEncoded;

	[FieldOffset(8)]
	public int zEncoded;

	[FieldOffset(12)]
	public int wEncoded;

	public float X
	{
		get
		{
			return FloatUtils.Decompress(xEncoded);
		}
		set
		{
			xEncoded = FloatUtils.Compress(value);
		}
	}

	public float Y
	{
		get
		{
			return FloatUtils.Decompress(yEncoded);
		}
		set
		{
			yEncoded = FloatUtils.Compress(value);
		}
	}

	public float Z
	{
		get
		{
			return FloatUtils.Decompress(zEncoded);
		}
		set
		{
			zEncoded = FloatUtils.Compress(value);
		}
	}

	public float W
	{
		get
		{
			return FloatUtils.Decompress(wEncoded);
		}
		set
		{
			wEncoded = FloatUtils.Compress(value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator QuaternionCompressed(Quaternion v)
	{
		QuaternionCompressed result = default(QuaternionCompressed);
		result.xEncoded = FloatUtils.Compress(v.x);
		result.yEncoded = FloatUtils.Compress(v.y);
		result.zEncoded = FloatUtils.Compress(v.z);
		result.wEncoded = FloatUtils.Compress(v.w);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Quaternion(QuaternionCompressed q)
	{
		Quaternion result = default(Quaternion);
		result.x = FloatUtils.Decompress(q.xEncoded);
		result.y = FloatUtils.Decompress(q.yEncoded);
		result.z = FloatUtils.Decompress(q.zEncoded);
		result.w = FloatUtils.Decompress(q.wEncoded);
		return result;
	}

	public bool Equals(QuaternionCompressed other)
	{
		return xEncoded == other.xEncoded && yEncoded == other.yEncoded && zEncoded == other.zEncoded && wEncoded == other.wEncoded;
	}

	public override bool Equals(object obj)
	{
		return obj is QuaternionCompressed other && Equals(other);
	}

	public override int GetHashCode()
	{
		int num = xEncoded;
		num = (num * 397) ^ yEncoded;
		num = (num * 397) ^ zEncoded;
		return (num * 397) ^ wEncoded;
	}

	public static bool operator ==(QuaternionCompressed left, QuaternionCompressed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(QuaternionCompressed left, QuaternionCompressed right)
	{
		return !left.Equals(right);
	}
}
