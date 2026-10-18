using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(2)]
public struct Vector2Compressed : INetworkStruct, IEquatable<Vector2Compressed>
{
	[FieldOffset(0)]
	public int xEncoded;

	[FieldOffset(4)]
	public int yEncoded;

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

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector2Compressed(Vector2 v)
	{
		Vector2Compressed result = default(Vector2Compressed);
		result.xEncoded = FloatUtils.Compress(v.x);
		result.yEncoded = FloatUtils.Compress(v.y);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Vector2(Vector2Compressed q)
	{
		Vector2 result = default(Vector2);
		result.x = FloatUtils.Decompress(q.xEncoded);
		result.y = FloatUtils.Decompress(q.yEncoded);
		return result;
	}

	public bool Equals(Vector2Compressed other)
	{
		return xEncoded == other.xEncoded && yEncoded == other.yEncoded;
	}

	public override bool Equals(object obj)
	{
		return obj is Vector2Compressed other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (xEncoded * 397) ^ yEncoded;
	}

	public static bool operator ==(Vector2Compressed left, Vector2Compressed right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Vector2Compressed left, Vector2Compressed right)
	{
		return !left.Equals(right);
	}
}
