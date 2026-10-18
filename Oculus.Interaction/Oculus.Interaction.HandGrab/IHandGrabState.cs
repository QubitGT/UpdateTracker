using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public interface IHandGrabState
{
	bool IsGrabbing { get; }

	float FingersStrength { get; }

	float WristStrength { get; }

	Pose WristToGrabPoseOffset { get; }

	HandGrabTarget HandGrabTarget { get; }

	HandFingerFlags GrabbingFingers();
}
