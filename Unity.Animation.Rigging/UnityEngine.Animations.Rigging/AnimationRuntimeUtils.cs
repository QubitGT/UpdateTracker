using Unity.Collections;

namespace UnityEngine.Animations.Rigging;

public static class AnimationRuntimeUtils
{
	private const float k_SqrEpsilon = 1E-08f;

	public static void SolveTwoBoneIK(AnimationStream stream, ReadWriteTransformHandle root, ReadWriteTransformHandle mid, ReadWriteTransformHandle tip, ReadOnlyTransformHandle target, ReadOnlyTransformHandle hint, float posWeight, float rotWeight, float hintWeight, AffineTransform targetOffset)
	{
		Vector3 position = root.GetPosition(stream);
		Vector3 position2 = mid.GetPosition(stream);
		Vector3 position3 = tip.GetPosition(stream);
		target.GetGlobalTR(stream, out var position4, out var rotation);
		Vector3 vector = Vector3.Lerp(position3, position4 + targetOffset.translation, posWeight);
		Quaternion rotation2 = Quaternion.Lerp(tip.GetRotation(stream), rotation * targetOffset.rotation, rotWeight);
		bool flag = hint.IsValid(stream) && hintWeight > 0f;
		Vector3 lhs = position2 - position;
		Vector3 rhs = position3 - position2;
		Vector3 vector2 = position3 - position;
		Vector3 vector3 = vector - position;
		float magnitude = lhs.magnitude;
		float magnitude2 = rhs.magnitude;
		float magnitude3 = vector2.magnitude;
		float magnitude4 = vector3.magnitude;
		float num = TriangleAngle(magnitude3, magnitude, magnitude2);
		float num2 = TriangleAngle(magnitude4, magnitude, magnitude2);
		Vector3 value = Vector3.Cross(lhs, rhs);
		if (value.sqrMagnitude < 1E-08f)
		{
			value = (flag ? Vector3.Cross(hint.GetPosition(stream) - position, rhs) : Vector3.zero);
			if (value.sqrMagnitude < 1E-08f)
			{
				value = Vector3.Cross(vector3, rhs);
			}
			if (value.sqrMagnitude < 1E-08f)
			{
				value = Vector3.up;
			}
		}
		value = Vector3.Normalize(value);
		float f = 0.5f * (num - num2);
		float num3 = Mathf.Sin(f);
		float w = Mathf.Cos(f);
		Quaternion quaternion = new Quaternion(value.x * num3, value.y * num3, value.z * num3, w);
		mid.SetRotation(stream, quaternion * mid.GetRotation(stream));
		vector2 = tip.GetPosition(stream) - position;
		root.SetRotation(stream, QuaternionExt.FromToRotation(vector2, vector3) * root.GetRotation(stream));
		if (flag)
		{
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude > 0f)
			{
				position2 = mid.GetPosition(stream);
				Vector3 position5 = tip.GetPosition(stream);
				lhs = position2 - position;
				vector2 = position5 - position;
				Vector3 vector4 = vector2 / Mathf.Sqrt(sqrMagnitude);
				Vector3 vector5 = hint.GetPosition(stream) - position;
				Vector3 from = lhs - vector4 * Vector3.Dot(lhs, vector4);
				Vector3 to = vector5 - vector4 * Vector3.Dot(vector5, vector4);
				float num4 = magnitude + magnitude2;
				if (from.sqrMagnitude > num4 * num4 * 0.001f && to.sqrMagnitude > 0f)
				{
					Quaternion q = QuaternionExt.FromToRotation(from, to);
					q.x *= hintWeight;
					q.y *= hintWeight;
					q.z *= hintWeight;
					q = QuaternionExt.NormalizeSafe(q);
					root.SetRotation(stream, q * root.GetRotation(stream));
				}
			}
		}
		tip.SetRotation(stream, rotation2);
	}

	public static void InverseSolveTwoBoneIK(AnimationStream stream, ReadOnlyTransformHandle root, ReadOnlyTransformHandle mid, ReadOnlyTransformHandle tip, ReadWriteTransformHandle target, ReadWriteTransformHandle hint, float posWeight, float rotWeight, float hintWeight, AffineTransform targetOffset)
	{
		Vector3 position = root.GetPosition(stream);
		Vector3 position2 = mid.GetPosition(stream);
		tip.GetGlobalTR(stream, out var position3, out var rotation);
		target.GetGlobalTR(stream, out var position4, out var rotation2);
		bool flag = hint.IsValid(stream);
		Vector3 hintPosition = Vector3.zero;
		if (flag)
		{
			hintPosition = hint.GetPosition(stream);
		}
		InverseSolveTwoBoneIK(position, position2, position3, rotation, ref position4, ref rotation2, ref hintPosition, flag, posWeight, rotWeight, hintWeight, targetOffset);
		target.SetPosition(stream, position4);
		target.SetRotation(stream, rotation2);
		hint.SetPosition(stream, hintPosition);
	}

	public static void InverseSolveTwoBoneIK(Vector3 rootPosition, Vector3 midPosition, Vector3 tipPosition, Quaternion tipRotation, ref Vector3 targetPosition, ref Quaternion targetRotation, ref Vector3 hintPosition, bool isHintValid, float posWeight, float rotWeight, float hintWeight, AffineTransform targetOffset)
	{
		targetPosition = ((posWeight > 0f) ? (tipPosition + targetOffset.translation) : targetPosition);
		targetRotation = ((rotWeight > 0f) ? (tipRotation * targetOffset.rotation) : targetRotation);
		if (isHintValid)
		{
			Vector3 vector = tipPosition - rootPosition;
			Vector3 vector2 = midPosition - rootPosition;
			Vector3 vector3 = tipPosition - midPosition;
			float magnitude = vector2.magnitude;
			float magnitude2 = vector3.magnitude;
			float num = Vector3.Dot(vector, vector);
			Vector3 vector4 = rootPosition;
			if (num > 1E-08f)
			{
				vector4 += Vector3.Dot(vector2 / num, vector) * vector;
			}
			Vector3 vector5 = midPosition - vector4;
			float num2 = magnitude + magnitude2;
			hintPosition = ((hintWeight > 0f) ? (vector4 + vector5.normalized * num2) : hintPosition);
		}
	}

	private static float TriangleAngle(float aLen, float aLen1, float aLen2)
	{
		return Mathf.Acos(Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2f, -1f, 1f));
	}

	public static bool SolveFABRIK(ref NativeArray<Vector3> linkPositions, ref NativeArray<float> linkLengths, Vector3 target, float tolerance, float maxReach, int maxIterations)
	{
		Vector3 vector = target - linkPositions[0];
		if (vector.sqrMagnitude > Square(maxReach))
		{
			Vector3 normalized = vector.normalized;
			for (int i = 1; i < linkPositions.Length; i++)
			{
				linkPositions[i] = linkPositions[i - 1] + normalized * linkLengths[i - 1];
			}
			return true;
		}
		int num = linkPositions.Length - 1;
		float num2 = Square(tolerance);
		if (SqrDistance(linkPositions[num], target) > num2)
		{
			Vector3 value = linkPositions[0];
			int num3 = 0;
			do
			{
				linkPositions[num] = target;
				for (int num4 = num - 1; num4 > -1; num4--)
				{
					linkPositions[num4] = linkPositions[num4 + 1] + (linkPositions[num4] - linkPositions[num4 + 1]).normalized * linkLengths[num4];
				}
				linkPositions[0] = value;
				for (int j = 1; j < linkPositions.Length; j++)
				{
					linkPositions[j] = linkPositions[j - 1] + (linkPositions[j] - linkPositions[j - 1]).normalized * linkLengths[j - 1];
				}
			}
			while (SqrDistance(linkPositions[num], target) > num2 && ++num3 < maxIterations);
			return true;
		}
		return false;
	}

	public static float SqrDistance(Vector3 lhs, Vector3 rhs)
	{
		return (rhs - lhs).sqrMagnitude;
	}

	public static float Square(float value)
	{
		return value * value;
	}

	public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t)
	{
		return Vector3.Scale(a, Vector3.one - t) + Vector3.Scale(b, t);
	}

	public static float Select(float a, float b, float c)
	{
		if (!(c > 0f))
		{
			return a;
		}
		return b;
	}

	public static Vector3 Select(Vector3 a, Vector3 b, Vector3 c)
	{
		return new Vector3(Select(a.x, b.x, c.x), Select(a.y, b.y, c.y), Select(a.z, b.z, c.z));
	}

	public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
	{
		float num = Vector3.Dot(planeNormal, planeNormal);
		float num2 = Vector3.Dot(vector, planeNormal);
		return new Vector3(vector.x - planeNormal.x * num2 / num, vector.y - planeNormal.y * num2 / num, vector.z - planeNormal.z * num2 / num);
	}

	internal static float Sum(AnimationJobCache cache, CacheIndex index, int count)
	{
		if (count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			num += cache.GetRaw(index, i);
		}
		return num;
	}

	public static float Sum(NativeArray<float> floatBuffer)
	{
		if (floatBuffer.Length == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < floatBuffer.Length; i++)
		{
			num += floatBuffer[i];
		}
		return num;
	}

	public static void PassThrough(AnimationStream stream, ReadWriteTransformHandle handle)
	{
		handle.GetLocalTRS(stream, out var position, out var rotation, out var scale);
		handle.SetLocalTRS(stream, position, rotation, scale);
	}
}
