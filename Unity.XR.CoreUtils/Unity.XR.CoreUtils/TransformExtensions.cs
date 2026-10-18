using System;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class TransformExtensions
{
	public static Pose GetLocalPose(this Transform transform)
	{
		transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
		return new Pose(localPosition, localRotation);
	}

	public static Pose GetWorldPose(this Transform transform)
	{
		transform.GetPositionAndRotation(out var position, out var rotation);
		return new Pose(position, rotation);
	}

	public static void SetLocalPose(this Transform transform, Pose pose)
	{
		transform.SetLocalPositionAndRotation(pose.position, pose.rotation);
	}

	public static void SetWorldPose(this Transform transform, Pose pose)
	{
		transform.SetPositionAndRotation(pose.position, pose.rotation);
	}

	public static Pose TransformPose(this Transform transform, Pose pose)
	{
		return pose.GetTransformedBy(transform);
	}

	public static Pose InverseTransformPose(this Transform transform, Pose pose)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		Pose result = default(Pose);
		result.position = transform.InverseTransformPoint(pose.position);
		result.rotation = Quaternion.Inverse(transform.rotation) * pose.rotation;
		return result;
	}

	public static Ray InverseTransformRay(this Transform transform, Ray ray)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		return new Ray(transform.InverseTransformPoint(ray.origin), transform.InverseTransformDirection(ray.direction));
	}
}
