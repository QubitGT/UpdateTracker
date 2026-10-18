using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class PoseExtensions
{
	public static Pose ApplyOffsetTo(this Pose pose, Pose otherPose)
	{
		Quaternion rotation = pose.rotation;
		return new Pose(rotation * otherPose.position + pose.position, rotation * otherPose.rotation);
	}

	public static Vector3 ApplyOffsetTo(this Pose pose, Vector3 position)
	{
		return pose.rotation * position + pose.position;
	}

	public static Vector3 ApplyInverseOffsetTo(this Pose pose, Vector3 position)
	{
		return Quaternion.Inverse(pose.rotation) * (position - pose.position);
	}
}
