using System;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[Serializable]
public struct BezierKnot : ISerializationCallbackReceiver, IEquatable<BezierKnot>
{
	public float3 Position;

	public float3 TangentIn;

	public float3 TangentOut;

	public quaternion Rotation;

	public BezierKnot(float3 position)
		: this(position, 0f, 0f, quaternion.identity)
	{
	}

	public BezierKnot(float3 position, float3 tangentIn, float3 tangentOut)
		: this(position, tangentIn, tangentOut, quaternion.identity)
	{
	}

	public BezierKnot(float3 position, float3 tangentIn, float3 tangentOut, quaternion rotation)
	{
		Position = position;
		TangentIn = tangentIn;
		TangentOut = tangentOut;
		Rotation = rotation;
	}

	public BezierKnot Transform(float4x4 matrix)
	{
		quaternion quaternion = math.mul(new quaternion(matrix), Rotation);
		quaternion q = math.inverse(quaternion);
		return new BezierKnot(math.transform(matrix, Position), math.rotate(q, math.rotate(matrix, math.rotate(Rotation, TangentIn))), math.rotate(q, math.rotate(matrix, math.rotate(Rotation, TangentOut))), quaternion);
	}

	public static BezierKnot operator +(BezierKnot knot, float3 rhs)
	{
		return new BezierKnot(knot.Position + rhs, knot.TangentIn, knot.TangentOut, knot.Rotation);
	}

	public static BezierKnot operator -(BezierKnot knot, float3 rhs)
	{
		return new BezierKnot(knot.Position - rhs, knot.TangentIn, knot.TangentOut, knot.Rotation);
	}

	internal BezierKnot BakeTangentDirectionToRotation(bool mirrored, BezierTangent main = BezierTangent.Out)
	{
		if (mirrored)
		{
			float num = math.length((main == BezierTangent.In) ? TangentIn : TangentOut);
			return new BezierKnot(Position, new float3(0f, 0f, 0f - num), new float3(0f, 0f, num), SplineUtility.GetKnotRotation(math.mul(Rotation, (main == BezierTangent.In) ? (-TangentIn) : TangentOut), math.mul(Rotation, math.up())));
		}
		return new BezierKnot(Position, new float3(0f, 0f, 0f - math.length(TangentIn)), new float3(0f, 0f, math.length(TangentOut)), Rotation = SplineUtility.GetKnotRotation(math.mul(Rotation, (main == BezierTangent.In) ? (-TangentIn) : TangentOut), math.mul(Rotation, math.up())));
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (math.lengthsq(Rotation) == 0f)
		{
			Rotation = quaternion.identity;
		}
	}

	public override string ToString()
	{
		return $"{{{Position}, {TangentIn}, {TangentOut}, {Rotation}}}";
	}

	public bool Equals(BezierKnot other)
	{
		if (Position.Equals(other.Position) && TangentIn.Equals(other.TangentIn) && TangentOut.Equals(other.TangentOut))
		{
			return Rotation.Equals(other.Rotation);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BezierKnot other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Position, TangentIn, TangentOut, Rotation);
	}
}
