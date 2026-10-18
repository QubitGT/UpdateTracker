using System;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public struct BezierCurve : IEquatable<BezierCurve>
{
	public float3 P0;

	public float3 P1;

	public float3 P2;

	public float3 P3;

	public float3 Tangent0
	{
		get
		{
			return P1 - P0;
		}
		set
		{
			P1 = P0 + value;
		}
	}

	public float3 Tangent1
	{
		get
		{
			return P2 - P3;
		}
		set
		{
			P2 = P3 + value;
		}
	}

	public BezierCurve(float3 p0, float3 p1)
	{
		P0 = (P2 = p0);
		P1 = (P3 = p1);
	}

	public BezierCurve(float3 p0, float3 p1, float3 p2)
	{
		float3 @float = 2f / 3f * p1;
		P0 = p0;
		P1 = 1f / 3f * p0 + @float;
		P2 = 1f / 3f * p2 + @float;
		P3 = p2;
	}

	public BezierCurve(float3 p0, float3 p1, float3 p2, float3 p3)
	{
		P0 = p0;
		P1 = p1;
		P2 = p2;
		P3 = p3;
	}

	public BezierCurve(BezierKnot a, BezierKnot b)
		: this(a.Position, a.Position + math.rotate(a.Rotation, a.TangentOut), b.Position + math.rotate(b.Rotation, b.TangentIn), b.Position)
	{
	}

	public BezierCurve Transform(float4x4 matrix)
	{
		return new BezierCurve(math.transform(matrix, P0), math.transform(matrix, P1), math.transform(matrix, P2), math.transform(matrix, P3));
	}

	public static BezierCurve FromTangent(float3 pointA, float3 tangentOutA, float3 pointB, float3 tangentInB)
	{
		return new BezierCurve(pointA, pointA + tangentOutA, pointB + tangentInB, pointB);
	}

	public BezierCurve GetInvertedCurve()
	{
		return new BezierCurve(P3, P2, P1, P0);
	}

	public bool Equals(BezierCurve other)
	{
		if (P0.Equals(other.P0) && P1.Equals(other.P1) && P2.Equals(other.P2))
		{
			return P3.Equals(other.P3);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BezierCurve other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((P0.GetHashCode() * 397) ^ P1.GetHashCode()) * 397) ^ P2.GetHashCode()) * 397) ^ P3.GetHashCode();
	}

	public static bool operator ==(BezierCurve left, BezierCurve right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BezierCurve left, BezierCurve right)
	{
		return !left.Equals(right);
	}
}
