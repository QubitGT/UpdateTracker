using System;
using UnityEngine;

namespace Oculus.Interaction.Throw;

[Obsolete]
public class VelocityCalculatorUtilMethods
{
	public static Vector3 ToLinearVelocity(Vector3 startPosition, Vector3 destinationPosition, float deltaTime)
	{
		if (!(Mathf.Abs(deltaTime) > Mathf.Epsilon))
		{
			return Vector3.zero;
		}
		return (destinationPosition - startPosition) / deltaTime;
	}

	public static Vector3 ToAngularVelocity(Quaternion startQuaternion, Quaternion destinationQuaternion, float deltaTime)
	{
		if (startQuaternion.Equals(destinationQuaternion) || deltaTime == 0f)
		{
			return Vector3.zero;
		}
		return DeltaRotationToAngularVelocity(destinationQuaternion * Quaternion.Inverse(startQuaternion), deltaTime);
	}

	public static Quaternion AngularVelocityToQuat(Vector3 angularVelocity)
	{
		return Quaternion.AngleAxis(angularVelocity.magnitude, angularVelocity.normalized);
	}

	public static (float, Vector3) QuatToAngleAxis(Quaternion inputQuat)
	{
		inputQuat.ToAngleAxis(out var angle, out var axis);
		if (float.IsInfinity(axis.x))
		{
			axis = Vector3.zero;
			angle = 0f;
		}
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return (angle, axis);
	}

	public static Vector3 QuatToAngularVeloc(Quaternion inputQuat)
	{
		_ = Vector3.zero;
		(float, Vector3) tuple = QuatToAngleAxis(inputQuat);
		var (num, _) = tuple;
		return tuple.Item2 * num;
	}

	public static Vector3 DeltaRotationToAngularVelocity(Quaternion deltaRotation, float deltaTime)
	{
		var (num, vector) = QuatToAngleAxis(deltaRotation);
		if (!(Mathf.Abs(deltaTime) > Mathf.Epsilon))
		{
			return Vector3.zero;
		}
		return vector * num * (MathF.PI / 180f) / deltaTime;
	}

	public static (Vector3, Vector3) GetVelocityAndAngularVelocity(TransformSample startSample, TransformSample endSample, float duration)
	{
		return (ToLinearVelocity(startSample.Position, endSample.Position, duration), ToAngularVelocity(startSample.Rotation, endSample.Rotation, duration));
	}
}
