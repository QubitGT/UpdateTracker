using UnityEngine;

namespace Liv.Lck.Smoothing;

public static class SmoothingUtils
{
	public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
	{
		return SmoothDampQuaternion(current, target, ref currentVelocity, smoothTime, Time.deltaTime);
	}

	internal static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime, float deltaTime)
	{
		if (deltaTime == 0f)
		{
			return current;
		}
		if (smoothTime == 0f)
		{
			return target;
		}
		Vector3 eulerAngles = current.eulerAngles;
		Vector3 eulerAngles2 = target.eulerAngles;
		return Quaternion.Euler(Mathf.SmoothDampAngle(eulerAngles.x, eulerAngles2.x, ref currentVelocity.x, smoothTime, float.PositiveInfinity, deltaTime), Mathf.SmoothDampAngle(eulerAngles.y, eulerAngles2.y, ref currentVelocity.y, smoothTime, float.PositiveInfinity, deltaTime), Mathf.SmoothDampAngle(eulerAngles.z, eulerAngles2.z, ref currentVelocity.z, smoothTime, float.PositiveInfinity, deltaTime));
	}
}
