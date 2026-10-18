using Oculus.Interaction.Grab;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class HandGrabResult
{
	public bool HasHandPose;

	public HandPose HandPose;

	public Pose RelativePose;

	public GrabPoseScore Score;

	public HandGrabResult()
	{
		RelativePose = Pose.identity;
		HandPose = new HandPose();
	}

	public void CopyFrom(HandGrabResult other)
	{
		HasHandPose = other.HasHandPose;
		HandPose.CopyFrom(other.HandPose);
		RelativePose.CopyFrom(in other.RelativePose);
		Score = other.Score;
	}
}
