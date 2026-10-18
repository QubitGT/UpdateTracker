using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Unity.Mathematics;

[Serializable]
[Unity.IL2CPP.CompilerServices.Il2CppEagerStaticClassConstruction]
public struct AffineTransform : IEquatable<AffineTransform>, IFormattable
{
	public float3x3 rs;

	public float3 t;

	public static readonly AffineTransform identity = new AffineTransform(float3.zero, float3x3.identity);

	public static readonly AffineTransform zero;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float3 translation, quaternion rotation)
	{
		rs = math.float3x3(rotation);
		t = translation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float3 translation, quaternion rotation, float3 scale)
	{
		rs = math.mulScale(math.float3x3(rotation), scale);
		t = translation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float3 translation, float3x3 rotationScale)
	{
		rs = rotationScale;
		t = translation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float3x3 rotationScale)
	{
		rs = rotationScale;
		t = float3.zero;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(RigidTransform rigid)
	{
		rs = math.float3x3(rigid.rot);
		t = rigid.pos;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float3x4 m)
	{
		rs = math.float3x3(m.c0, m.c1, m.c2);
		t = m.c3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AffineTransform(float4x4 m)
	{
		rs = math.float3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz);
		t = m.c3.xyz;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float3x4(AffineTransform m)
	{
		return math.float3x4(m.rs.c0, m.rs.c1, m.rs.c2, m.t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator float4x4(AffineTransform m)
	{
		return math.float4x4(math.float4(m.rs.c0, 0f), math.float4(m.rs.c1, 0f), math.float4(m.rs.c2, 0f), math.float4(m.t, 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(AffineTransform rhs)
	{
		if (rs.Equals(rhs.rs))
		{
			return t.Equals(rhs.t);
		}
		return false;
	}

	public override bool Equals(object o)
	{
		if (o is AffineTransform rhs)
		{
			return Equals(rhs);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode()
	{
		return (int)math.hash(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return $"AffineTransform(({rs.c0.x}f, {rs.c1.x}f, {rs.c2.x}f,  {rs.c0.y}f, {rs.c1.y}f, {rs.c2.y}f,  {rs.c0.z}f, {rs.c1.z}f, {rs.c2.z}f), ({t.x}f, {t.y}f, {t.z}f))";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format, IFormatProvider formatProvider)
	{
		return $"AffineTransform(({rs.c0.x.ToString(format, formatProvider)}f, {rs.c1.x.ToString(format, formatProvider)}f, {rs.c2.x.ToString(format, formatProvider)}f,  {rs.c0.y.ToString(format, formatProvider)}f, {rs.c1.y.ToString(format, formatProvider)}f, {rs.c2.y.ToString(format, formatProvider)}f,  {rs.c0.z.ToString(format, formatProvider)}f, {rs.c1.z.ToString(format, formatProvider)}f, {rs.c2.z.ToString(format, formatProvider)}f), ({t.x.ToString(format, formatProvider)}f, {t.y.ToString(format, formatProvider)}f, {t.z.ToString(format, formatProvider)}f))";
	}
}
