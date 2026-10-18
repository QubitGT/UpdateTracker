using System;

namespace UnityEngine.XR.OpenXR.Input;

[Obsolete("OpenXR.Input.Pose is deprecated, Please use UnityEngine.InputSystem.XR.PoseState instead", false)]
public struct Pose
{
	public bool isTracked { get; set; }

	public InputTrackingState trackingState { get; set; }

	public Vector3 position { get; set; }

	public Quaternion rotation { get; set; }

	public Vector3 velocity { get; set; }

	public Vector3 angularVelocity { get; set; }
}
