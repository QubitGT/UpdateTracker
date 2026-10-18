using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public interface IOVRCameraRigRef
{
	OVRCameraRig CameraRig { get; }

	OVRHand LeftHand { get; }

	OVRHand RightHand { get; }

	Transform LeftController { get; }

	Transform RightController { get; }

	event Action<bool> WhenInputDataDirtied;
}
