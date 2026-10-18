using System;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public interface IBody
{
	ISkeletonMapping SkeletonMapping { get; }

	bool IsConnected { get; }

	bool IsHighConfidence { get; }

	bool IsTrackedDataValid { get; }

	float Scale { get; }

	int CurrentDataVersion { get; }

	event Action WhenBodyUpdated;

	bool GetRootPose(out Pose pose);

	bool GetJointPose(BodyJointId bodyJointId, out Pose pose);

	bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose);

	bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose);
}
