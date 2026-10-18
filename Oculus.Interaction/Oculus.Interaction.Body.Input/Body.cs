using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public class Body : DataModifier<BodyDataAsset>, IBody
{
	[Tooltip("If assigned, joint pose translations into world space will be performed via this transform. If unassigned, world joint poses will be returned in tracking space.")]
	[SerializeField]
	[Optional]
	private Transform _trackingSpace;

	private BodyJointsCache _jointPosesCache;

	public bool IsConnected => GetData().IsDataValid;

	public bool IsHighConfidence => GetData().IsDataHighConfidence;

	public float Scale => GetData().RootScale;

	public ISkeletonMapping SkeletonMapping => GetData().SkeletonMapping;

	public bool IsTrackedDataValid => GetData().IsDataValid;

	public event Action WhenBodyUpdated = delegate
	{
	};

	public bool GetJointPose(BodyJointId bodyJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!IsTrackedDataValid || !SkeletonMapping.Joints.Contains(bodyJointId))
		{
			return false;
		}
		CheckJointPosesCacheUpdate();
		pose = _jointPosesCache.GetWorldJointPose(bodyJointId);
		return true;
	}

	public bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!IsTrackedDataValid || !SkeletonMapping.Joints.Contains(bodyJointId))
		{
			return false;
		}
		CheckJointPosesCacheUpdate();
		pose = _jointPosesCache.GetLocalJointPose(bodyJointId);
		return true;
	}

	public bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!IsTrackedDataValid || !SkeletonMapping.Joints.Contains(bodyJointId))
		{
			return false;
		}
		CheckJointPosesCacheUpdate();
		pose = _jointPosesCache.GetJointPoseFromRoot(bodyJointId);
		return true;
	}

	public bool GetRootPose(out Pose pose)
	{
		pose = Pose.identity;
		if (!IsTrackedDataValid)
		{
			return false;
		}
		CheckJointPosesCacheUpdate();
		pose = _jointPosesCache.GetWorldRootPose();
		return true;
	}

	private void InitializeJointPosesCache()
	{
		if (_jointPosesCache == null)
		{
			_jointPosesCache = new BodyJointsCache(SkeletonMapping);
		}
	}

	private void CheckJointPosesCacheUpdate()
	{
		if (_jointPosesCache != null && CurrentDataVersion != _jointPosesCache.LocalDataVersion)
		{
			_jointPosesCache.Update(GetData(), CurrentDataVersion, _trackingSpace);
		}
	}

	protected override void Apply(BodyDataAsset data)
	{
	}

	public override void MarkInputDataRequiresUpdate()
	{
		base.MarkInputDataRequiresUpdate();
		if (base.Started)
		{
			InitializeJointPosesCache();
			this.WhenBodyUpdated();
		}
	}
}
