using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public interface IHand
{
	Handedness Handedness { get; }

	bool IsConnected { get; }

	bool IsHighConfidence { get; }

	bool IsDominantHand { get; }

	float Scale { get; }

	bool IsPointerPoseValid { get; }

	bool IsTrackedDataValid { get; }

	int CurrentDataVersion { get; }

	event Action WhenHandUpdated;

	bool GetFingerIsPinching(HandFinger finger);

	bool GetIndexFingerIsPinching();

	bool GetPointerPose(out Pose pose);

	bool GetJointPose(HandJointId handJointId, out Pose pose);

	bool GetJointPoseLocal(HandJointId handJointId, out Pose pose);

	bool GetJointPosesLocal(out ReadOnlyHandJointPoses localJointPoses);

	bool GetJointPoseFromWrist(HandJointId handJointId, out Pose pose);

	bool GetJointPosesFromWrist(out ReadOnlyHandJointPoses jointPosesFromWrist);

	bool GetPalmPoseLocal(out Pose pose);

	bool GetFingerIsHighConfidence(HandFinger finger);

	float GetFingerPinchStrength(HandFinger finger);

	bool GetRootPose(out Pose pose);
}
