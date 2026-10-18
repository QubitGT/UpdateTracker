using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct CameraTarget
{
	[Tooltip("Object for the camera to follow")]
	public Transform TrackingTarget;

	[Tooltip("Object for the camera to look at")]
	public Transform LookAtTarget;

	public bool CustomLookAtTarget;
}
