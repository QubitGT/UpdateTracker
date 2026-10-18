using System;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

[Serializable]
public struct GtCameraDockSettings
{
	public bool forceFov;

	[Range(30f, 110f)]
	public float fov;

	public bool forceOrientation;

	public bool landscapeMode;

	public bool forceCameraFacing;

	public bool isFront;

	public CameraMode GetEnforcedMode()
	{
		return CameraMode.Selfie;
	}
}
