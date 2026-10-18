using System;
using UnityEngine;

namespace Unity.Cinemachine.TargetTracking;

[Serializable]
public struct TrackerSettings
{
	[Tooltip("The coordinate space to use when interpreting the offset from the target.  This is also used to set the camera's Up vector, which will be maintained when aiming the camera.")]
	public BindingMode BindingMode;

	[Tooltip("How aggressively the camera tries to maintain the offset, per axis.  Small numbers are more responsive, rapidly translating the camera to keep the target's offset.  Larger numbers give a more heavy slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
	public Vector3 PositionDamping;

	public AngularDampingMode AngularDampingMode;

	[Tooltip("How aggressively the camera tries to track the target's rotation, per axis.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public Vector3 RotationDamping;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to track the target's rotation.  Small numbers are more responsive.  Larger numbers give a more heavy slowly responding camera.")]
	public float QuaternionDamping;

	public static TrackerSettings Default
	{
		get
		{
			TrackerSettings result = default(TrackerSettings);
			result.BindingMode = BindingMode.WorldSpace;
			result.PositionDamping = Vector3.one;
			result.AngularDampingMode = AngularDampingMode.Euler;
			result.RotationDamping = Vector3.one;
			result.QuaternionDamping = 1f;
			return result;
		}
	}

	public void Validate()
	{
		PositionDamping.x = Mathf.Max(0f, PositionDamping.x);
		PositionDamping.y = Mathf.Max(0f, PositionDamping.y);
		PositionDamping.z = Mathf.Max(0f, PositionDamping.z);
		RotationDamping.x = Mathf.Max(0f, RotationDamping.x);
		RotationDamping.y = Mathf.Max(0f, RotationDamping.y);
		RotationDamping.z = Mathf.Max(0f, RotationDamping.z);
		QuaternionDamping = Mathf.Max(0f, QuaternionDamping);
	}
}
