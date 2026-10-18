using UnityEngine;

namespace Oculus.Interaction.Input;

public interface ITrackingToWorldTransformer
{
	Transform Transform { get; }

	Quaternion WorldToTrackingWristJointFixup { get; }

	Pose ToWorldPose(Pose poseRh);

	Pose ToTrackingPose(in Pose worldPose);
}
