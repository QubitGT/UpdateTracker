using System;
using UnityEngine;

namespace Oculus.Interaction;

public class TransformerUtils
{
	[Serializable]
	public struct FloatRange
	{
		public float Min;

		public float Max;
	}

	[Serializable]
	public struct ConstrainedAxis
	{
		public bool ConstrainAxis;

		public FloatRange AxisRange;

		public static ConstrainedAxis Unconstrained
		{
			get
			{
				ConstrainedAxis result = default(ConstrainedAxis);
				result.ConstrainAxis = false;
				result.AxisRange = new FloatRange
				{
					Min = 1f,
					Max = 1f
				};
				return result;
			}
		}
	}

	[Serializable]
	public class PositionConstraints
	{
		public bool ConstraintsAreRelative;

		public ConstrainedAxis XAxis;

		public ConstrainedAxis YAxis;

		public ConstrainedAxis ZAxis;
	}

	[Serializable]
	public class RotationConstraints
	{
		public ConstrainedAxis XAxis;

		public ConstrainedAxis YAxis;

		public ConstrainedAxis ZAxis;
	}

	[Serializable]
	public class ScaleConstraints
	{
		public bool ConstraintsAreRelative;

		public ConstrainedAxis XAxis;

		public ConstrainedAxis YAxis;

		public ConstrainedAxis ZAxis;
	}

	public static PositionConstraints GenerateParentConstraints(PositionConstraints constraints, Vector3 initialPosition)
	{
		PositionConstraints positionConstraints;
		if (!constraints.ConstraintsAreRelative)
		{
			positionConstraints = constraints;
		}
		else
		{
			positionConstraints = new PositionConstraints();
			positionConstraints.XAxis = default(ConstrainedAxis);
			positionConstraints.YAxis = default(ConstrainedAxis);
			positionConstraints.ZAxis = default(ConstrainedAxis);
			if (constraints.XAxis.ConstrainAxis)
			{
				positionConstraints.XAxis.ConstrainAxis = true;
				positionConstraints.XAxis.AxisRange.Min = constraints.XAxis.AxisRange.Min + initialPosition.x;
				positionConstraints.XAxis.AxisRange.Max = constraints.XAxis.AxisRange.Max + initialPosition.x;
			}
			if (constraints.YAxis.ConstrainAxis)
			{
				positionConstraints.YAxis.ConstrainAxis = true;
				positionConstraints.YAxis.AxisRange.Min = constraints.YAxis.AxisRange.Min + initialPosition.y;
				positionConstraints.YAxis.AxisRange.Max = constraints.YAxis.AxisRange.Max + initialPosition.y;
			}
			if (constraints.ZAxis.ConstrainAxis)
			{
				positionConstraints.ZAxis.ConstrainAxis = true;
				positionConstraints.ZAxis.AxisRange.Min = constraints.ZAxis.AxisRange.Min + initialPosition.z;
				positionConstraints.ZAxis.AxisRange.Max = constraints.ZAxis.AxisRange.Max + initialPosition.z;
			}
		}
		return positionConstraints;
	}

	public static ScaleConstraints GenerateParentConstraints(ScaleConstraints constraints, Vector3 initialScale)
	{
		ScaleConstraints scaleConstraints;
		if (!constraints.ConstraintsAreRelative)
		{
			scaleConstraints = constraints;
		}
		else
		{
			scaleConstraints = new ScaleConstraints();
			scaleConstraints.XAxis = default(ConstrainedAxis);
			scaleConstraints.YAxis = default(ConstrainedAxis);
			scaleConstraints.ZAxis = default(ConstrainedAxis);
			if (constraints.XAxis.ConstrainAxis)
			{
				scaleConstraints.XAxis.ConstrainAxis = true;
				scaleConstraints.XAxis.AxisRange.Min = constraints.XAxis.AxisRange.Min * initialScale.x;
				scaleConstraints.XAxis.AxisRange.Max = constraints.XAxis.AxisRange.Max * initialScale.x;
			}
			if (constraints.YAxis.ConstrainAxis)
			{
				scaleConstraints.YAxis.ConstrainAxis = true;
				scaleConstraints.YAxis.AxisRange.Min = constraints.YAxis.AxisRange.Min * initialScale.y;
				scaleConstraints.YAxis.AxisRange.Max = constraints.YAxis.AxisRange.Max * initialScale.y;
			}
			if (constraints.ZAxis.ConstrainAxis)
			{
				scaleConstraints.ZAxis.ConstrainAxis = true;
				scaleConstraints.ZAxis.AxisRange.Min = constraints.ZAxis.AxisRange.Min * initialScale.z;
				scaleConstraints.ZAxis.AxisRange.Max = constraints.ZAxis.AxisRange.Max * initialScale.z;
			}
		}
		return scaleConstraints;
	}

	public static Vector3 GetConstrainedTransformPosition(Vector3 unconstrainedPosition, PositionConstraints positionConstraints, Transform relativeTransform = null)
	{
		Vector3 vector = unconstrainedPosition;
		if (relativeTransform != null)
		{
			vector = relativeTransform.InverseTransformPoint(vector);
		}
		if (positionConstraints.XAxis.ConstrainAxis)
		{
			vector.x = Mathf.Clamp(vector.x, positionConstraints.XAxis.AxisRange.Min, positionConstraints.XAxis.AxisRange.Max);
		}
		if (positionConstraints.YAxis.ConstrainAxis)
		{
			vector.y = Mathf.Clamp(vector.y, positionConstraints.YAxis.AxisRange.Min, positionConstraints.YAxis.AxisRange.Max);
		}
		if (positionConstraints.ZAxis.ConstrainAxis)
		{
			vector.z = Mathf.Clamp(vector.z, positionConstraints.ZAxis.AxisRange.Min, positionConstraints.ZAxis.AxisRange.Max);
		}
		if (relativeTransform != null)
		{
			vector = relativeTransform.TransformPoint(vector);
		}
		return vector;
	}

	public static Quaternion GetConstrainedTransformRotation(Quaternion unconstrainedRotation, RotationConstraints rotationConstraints, Transform relativeTransform = null)
	{
		if (relativeTransform != null)
		{
			unconstrainedRotation = Quaternion.Inverse(relativeTransform.rotation) * unconstrainedRotation;
		}
		Vector3 eulerAngles = unconstrainedRotation.eulerAngles;
		float num = eulerAngles.x;
		float num2 = eulerAngles.y;
		float num3 = eulerAngles.z;
		if (rotationConstraints.XAxis.ConstrainAxis)
		{
			num = ClampAngle(num, rotationConstraints.XAxis.AxisRange.Min, rotationConstraints.XAxis.AxisRange.Max);
		}
		if (rotationConstraints.YAxis.ConstrainAxis)
		{
			num2 = ClampAngle(num2, rotationConstraints.YAxis.AxisRange.Min, rotationConstraints.YAxis.AxisRange.Max);
		}
		if (rotationConstraints.ZAxis.ConstrainAxis)
		{
			num3 = ClampAngle(num3, rotationConstraints.ZAxis.AxisRange.Min, rotationConstraints.ZAxis.AxisRange.Max);
		}
		Quaternion quaternion = Quaternion.Euler(num, num2, num3);
		if (relativeTransform != null)
		{
			quaternion = relativeTransform.rotation * quaternion;
		}
		return quaternion.normalized;
		static float ClampAngle(float angle, float min, float max)
		{
			if (min == max)
			{
				return min;
			}
			if (min <= max)
			{
				if (angle >= min && angle <= max)
				{
					return angle;
				}
			}
			else if (angle >= min || angle <= max)
			{
				return angle;
			}
			if (Mathf.Abs(Mathf.DeltaAngle(angle, min)) <= Mathf.Abs(Mathf.DeltaAngle(max, angle)))
			{
				return min;
			}
			return max;
		}
	}

	public static Vector3 GetConstrainedTransformScale(Vector3 unconstrainedScale, ScaleConstraints scaleConstraints)
	{
		Vector3 result = unconstrainedScale;
		if (scaleConstraints.XAxis.ConstrainAxis)
		{
			result.x = Mathf.Clamp(result.x, scaleConstraints.XAxis.AxisRange.Min, scaleConstraints.XAxis.AxisRange.Max);
		}
		if (scaleConstraints.YAxis.ConstrainAxis)
		{
			result.y = Mathf.Clamp(result.y, scaleConstraints.YAxis.AxisRange.Min, scaleConstraints.YAxis.AxisRange.Max);
		}
		if (scaleConstraints.ZAxis.ConstrainAxis)
		{
			result.z = Mathf.Clamp(result.z, scaleConstraints.ZAxis.AxisRange.Min, scaleConstraints.ZAxis.AxisRange.Max);
		}
		return result;
	}

	public static Pose WorldToLocalPose(Pose worldPose, Matrix4x4 worldToLocal)
	{
		return new Pose(worldToLocal.MultiplyPoint3x4(worldPose.position), worldToLocal.rotation * worldPose.rotation);
	}

	public static Pose AlignLocalToWorldPose(Matrix4x4 localToWorld, Pose local, Pose world)
	{
		Pose a = new Pose(localToWorld.MultiplyPoint3x4(local.position), localToWorld.rotation * local.rotation);
		Pose result = default(Pose);
		PoseUtils.Inverse(in a, ref result);
		Pose b = new Pose(localToWorld.GetPosition(), localToWorld.rotation);
		Pose b2 = PoseUtils.Multiply(in result, in b);
		return PoseUtils.Multiply(in world, in b2);
	}

	public static float WorldToLocalMagnitude(float magnitude, Matrix4x4 worldToLocal)
	{
		return worldToLocal.MultiplyVector(magnitude * Vector3.forward).magnitude;
	}

	public static float LocalToWorldMagnitude(float magnitude, Matrix4x4 localToWorld)
	{
		return localToWorld.MultiplyVector(magnitude * Vector3.forward).magnitude;
	}

	public static Vector3 ConstrainAlongDirection(Vector3 position, Vector3 origin, Vector3 direction, FloatConstraint min, FloatConstraint max)
	{
		if (!min.Constrain && !max.Constrain)
		{
			return position;
		}
		float num = Vector3.Dot(position - origin, direction);
		float num2 = num;
		if (min.Constrain)
		{
			num2 = Mathf.Max(num2, min.Value);
		}
		if (max.Constrain)
		{
			num2 = Mathf.Min(num2, max.Value);
		}
		float num3 = num2 - num;
		return position + direction * num3;
	}
}
