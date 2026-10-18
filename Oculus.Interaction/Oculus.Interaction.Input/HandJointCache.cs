using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HandJointCache : SkeletonJointsCache
{
	private ReadOnlyHandJointPoses _posesFromWristCollection;

	private ReadOnlyHandJointPoses _localPosesCollection;

	protected override bool TryGetParent(int joint, out int parent)
	{
		parent = (int)HandJointUtils.JointParentList[joint];
		return parent >= 0;
	}

	public HandJointCache()
		: base(26)
	{
		_posesFromWristCollection = new ReadOnlyHandJointPoses(_posesFromRoot);
		_localPosesCollection = new ReadOnlyHandJointPoses(_localPoses);
	}

	public void Update(HandDataAsset data, int dataVersion, Transform trackingSpace = null)
	{
		if (data.IsDataValidAndConnected)
		{
			Update(dataVersion, data.Root, data.JointPoses, data.HandScale, trackingSpace);
		}
	}

	public bool GetAllLocalPoses(out ReadOnlyHandJointPoses localJointPoses)
	{
		UpdateAllLocalPoses();
		localJointPoses = _localPosesCollection;
		return _posesFromWristCollection.Count > 0;
	}

	public bool GetAllPosesFromWrist(out ReadOnlyHandJointPoses jointPosesFromWrist)
	{
		UpdateAllPosesFromRoot();
		jointPosesFromWrist = _posesFromWristCollection;
		return _posesFromWristCollection.Count > 0;
	}

	public Pose GetLocalJointPose(HandJointId jointId)
	{
		return GetLocalJointPose((int)jointId);
	}

	public Pose GetJointPoseFromRoot(HandJointId jointId)
	{
		return GetJointPoseFromRoot((int)jointId);
	}

	public Pose GetWorldJointPose(HandJointId jointId)
	{
		return GetWorldJointPose((int)jointId);
	}

	[Obsolete("Use GetLocalJointPose instead")]
	public Pose LocalJointPose(HandJointId jointid)
	{
		return GetLocalJointPose((int)jointid);
	}

	[Obsolete("Use GetJointPoseFromRoot instead")]
	public Pose PoseFromWrist(HandJointId jointid)
	{
		return GetJointPoseFromRoot((int)jointid);
	}

	[Obsolete("Use GetWorldJointPose instead")]
	public Pose WorldJointPose(HandJointId jointid, Pose rootPose, float handScale)
	{
		return GetWorldJointPose((int)jointid);
	}
}
