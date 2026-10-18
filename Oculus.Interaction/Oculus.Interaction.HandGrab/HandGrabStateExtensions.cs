using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public static class HandGrabStateExtensions
{
	public static Pose GetVisualWristPose(this IHandGrabState grabState)
	{
		Pose offset;
		if (grabState.HandGrabTarget.HandPose != null)
		{
			HandGrabTarget handGrabTarget = grabState.HandGrabTarget;
			offset = Pose.identity;
			return handGrabTarget.GetWorldPoseDisplaced(in offset);
		}
		Pose result = Pose.identity;
		offset = grabState.WristToGrabPoseOffset;
		PoseUtils.Inverse(in offset, ref result);
		return grabState.HandGrabTarget.GetWorldPoseDisplaced(in result);
	}

	public static Pose GetTargetGrabPose(this IHandGrabState grabState)
	{
		Pose offset;
		if (grabState.HandGrabTarget.HandPose != null)
		{
			HandGrabTarget handGrabTarget = grabState.HandGrabTarget;
			offset = grabState.WristToGrabPoseOffset;
			return handGrabTarget.GetWorldPoseDisplaced(in offset);
		}
		HandGrabTarget handGrabTarget2 = grabState.HandGrabTarget;
		offset = Pose.identity;
		return handGrabTarget2.GetWorldPoseDisplaced(in offset);
	}
}
