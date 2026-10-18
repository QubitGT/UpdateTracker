using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public class BodyJointsCache : SkeletonJointsCache
{
	private ReadOnlyBodyJointPoses _posesFromRootCollection;

	private ReadOnlyBodyJointPoses _worldPosesCollection;

	private ReadOnlyBodyJointPoses _localPosesCollection;

	private readonly ISkeletonMapping _mapping;

	protected override bool TryGetParent(int joint, out int parent)
	{
		if (_mapping.TryGetParentJointId((BodyJointId)joint, out var parent2))
		{
			parent = (int)parent2;
			return true;
		}
		parent = -1;
		return false;
	}

	public BodyJointsCache(ISkeletonMapping mapping)
		: base(84)
	{
		_mapping = mapping;
		_localPosesCollection = new ReadOnlyBodyJointPoses(_localPoses);
		_worldPosesCollection = new ReadOnlyBodyJointPoses(_worldPoses);
		_posesFromRootCollection = new ReadOnlyBodyJointPoses(_posesFromRoot);
	}

	public void Update(BodyDataAsset data, int dataVersion, Transform trackingSpace = null)
	{
		if (data.IsDataValid)
		{
			Update(dataVersion, data.Root, data.JointPoses, data.RootScale, trackingSpace);
		}
	}

	public Pose GetLocalJointPose(BodyJointId jointId)
	{
		return GetLocalJointPose((int)jointId);
	}

	public Pose GetJointPoseFromRoot(BodyJointId jointId)
	{
		return GetJointPoseFromRoot((int)jointId);
	}

	public Pose GetWorldJointPose(BodyJointId jointId)
	{
		return GetWorldJointPose((int)jointId);
	}

	[Obsolete]
	public bool GetAllLocalPoses(out ReadOnlyBodyJointPoses localJointPoses)
	{
		UpdateAllLocalPoses();
		localJointPoses = _localPosesCollection;
		return _localPosesCollection.Count > 0;
	}

	[Obsolete]
	public bool GetAllPosesFromRoot(out ReadOnlyBodyJointPoses posesFromRoot)
	{
		UpdateAllPosesFromRoot();
		posesFromRoot = _posesFromRootCollection;
		return _posesFromRootCollection.Count > 0;
	}

	[Obsolete]
	public bool GetAllWorldPoses(out ReadOnlyBodyJointPoses worldJointPoses)
	{
		UpdateAllWorldPoses();
		worldJointPoses = _worldPosesCollection;
		return _worldPosesCollection.Count > 0;
	}
}
