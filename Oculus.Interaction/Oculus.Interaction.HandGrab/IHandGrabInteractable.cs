using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public interface IHandGrabInteractable : IRelativeToRef
{
	HandAlignType HandAlignment { get; }

	bool UsesHandPose { get; }

	float Slippiness { get; }

	GrabTypeFlags SupportedGrabTypes { get; }

	GrabbingRule PinchGrabRules { get; }

	GrabbingRule PalmGrabRules { get; }

	bool SupportsHandedness(Handedness handedness);

	IMovement GenerateMovement(in Pose from, in Pose to);

	[Obsolete("Use CalculateBestPose with offset instead")]
	bool CalculateBestPose(Pose userPose, float handScale, Handedness handedness, ref HandGrabResult result);

	void CalculateBestPose(in Pose userPose, in Pose offset, Transform relativeTo, float handScale, Handedness handedness, ref HandGrabResult result);
}
