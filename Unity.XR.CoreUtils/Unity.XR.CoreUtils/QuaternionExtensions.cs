using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class QuaternionExtensions
{
	public static Quaternion ConstrainYaw(this Quaternion rotation)
	{
		rotation.x = 0f;
		rotation.z = 0f;
		return rotation;
	}

	public static Quaternion ConstrainYawNormalized(this Quaternion rotation)
	{
		rotation.x = 0f;
		rotation.z = 0f;
		rotation.Normalize();
		return rotation;
	}

	public static Quaternion ConstrainYawPitchNormalized(this Quaternion rotation)
	{
		Vector3 eulerAngles = rotation.eulerAngles;
		eulerAngles.z = 0f;
		return Quaternion.Euler(eulerAngles);
	}
}
