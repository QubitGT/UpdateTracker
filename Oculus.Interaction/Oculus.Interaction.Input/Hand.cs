using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class Hand : DataModifier<HandDataAsset>, IHand
{
	private HandJointCache _jointPosesCache;

	private static readonly Vector3 PALM_LOCAL_OFFSET = new Vector3(0.08f, -0.01f, 0f);

	public Handedness Handedness => GetData().Config.Handedness;

	public ITrackingToWorldTransformer TrackingToWorldTransformer => GetData().Config.TrackingToWorldTransformer;

	public HandSkeleton HandSkeleton => GetData().Config.HandSkeleton;

	public bool IsConnected => GetData().IsDataValidAndConnected;

	public bool IsHighConfidence => GetData().IsHighConfidence;

	public bool IsDominantHand => GetData().IsDominantHand;

	public float Scale => ((TrackingToWorldTransformer != null) ? TrackingToWorldTransformer.Transform.lossyScale.x : 1f) * GetData().HandScale;

	public bool IsPointerPoseValid => IsPoseOriginAllowed(GetData().PointerPoseOrigin);

	public bool IsTrackedDataValid => IsPoseOriginAllowed(GetData().RootPoseOrigin);

	public event Action WhenHandUpdated = delegate
	{
	};

	protected override void Apply(HandDataAsset data)
	{
	}

	public override void MarkInputDataRequiresUpdate()
	{
		base.MarkInputDataRequiresUpdate();
		if (base.Started)
		{
			InitializeJointPosesCache();
			this.WhenHandUpdated();
		}
	}

	private void InitializeJointPosesCache()
	{
		if (_jointPosesCache == null && GetData().IsDataValidAndConnected)
		{
			_jointPosesCache = new HandJointCache();
		}
	}

	private void CheckJointPosesCacheUpdate()
	{
		if (_jointPosesCache != null && CurrentDataVersion != _jointPosesCache.LocalDataVersion)
		{
			_jointPosesCache.Update(GetData(), CurrentDataVersion, TrackingToWorldTransformer?.Transform);
		}
	}

	public bool GetFingerIsPinching(HandFinger finger)
	{
		HandDataAsset data = GetData();
		if (data.IsConnected)
		{
			return data.IsFingerPinching[(int)finger];
		}
		return false;
	}

	public bool GetIndexFingerIsPinching()
	{
		return GetFingerIsPinching(HandFinger.Index);
	}

	public bool GetPointerPose(out Pose pose)
	{
		HandDataAsset data = GetData();
		return ValidatePose(in data.PointerPose, data.PointerPoseOrigin, out pose);
	}

	public bool GetJointPose(HandJointId handJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!IsTrackedDataValid || _jointPosesCache == null || !GetRootPose(out var _))
		{
			return false;
		}
		CheckJointPosesCacheUpdate();
		pose = _jointPosesCache.GetWorldJointPose(handJointId);
		return true;
	}

	public bool GetJointPoseLocal(HandJointId handJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!GetJointPosesLocal(out var localJointPoses))
		{
			return false;
		}
		pose = localJointPoses[(int)handJointId];
		return true;
	}

	public bool GetJointPosesLocal(out ReadOnlyHandJointPoses localJointPoses)
	{
		if (!IsTrackedDataValid || _jointPosesCache == null)
		{
			localJointPoses = ReadOnlyHandJointPoses.Empty;
			return false;
		}
		CheckJointPosesCacheUpdate();
		return _jointPosesCache.GetAllLocalPoses(out localJointPoses);
	}

	public bool GetJointPoseFromWrist(HandJointId handJointId, out Pose pose)
	{
		pose = Pose.identity;
		if (!GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			return false;
		}
		pose = jointPosesFromWrist[(int)handJointId];
		return true;
	}

	public bool GetJointPosesFromWrist(out ReadOnlyHandJointPoses jointPosesFromWrist)
	{
		if (!IsTrackedDataValid || _jointPosesCache == null)
		{
			jointPosesFromWrist = ReadOnlyHandJointPoses.Empty;
			return false;
		}
		CheckJointPosesCacheUpdate();
		return _jointPosesCache.GetAllPosesFromWrist(out jointPosesFromWrist);
	}

	public bool GetPalmPoseLocal(out Pose pose)
	{
		Quaternion identity = Quaternion.identity;
		Vector3 vector = PALM_LOCAL_OFFSET;
		if (Handedness == Handedness.Left)
		{
			vector = -vector;
		}
		pose = new Pose(vector * Scale, identity);
		return true;
	}

	public bool GetFingerIsHighConfidence(HandFinger finger)
	{
		return GetData().IsFingerHighConfidence[(int)finger];
	}

	public float GetFingerPinchStrength(HandFinger finger)
	{
		return GetData().FingerPinchStrength[(int)finger];
	}

	public bool GetRootPose(out Pose pose)
	{
		HandDataAsset data = GetData();
		return ValidatePose(in data.Root, data.RootPoseOrigin, out pose);
	}

	private bool ValidatePose(in Pose sourcePose, PoseOrigin sourcePoseOrigin, out Pose pose)
	{
		if (IsPoseOriginDisallowed(sourcePoseOrigin))
		{
			pose = Pose.identity;
			return false;
		}
		pose = ((TrackingToWorldTransformer != null) ? TrackingToWorldTransformer.ToWorldPose(sourcePose) : sourcePose);
		return true;
	}

	private bool IsPoseOriginAllowed(PoseOrigin poseOrigin)
	{
		return poseOrigin != PoseOrigin.None;
	}

	private bool IsPoseOriginDisallowed(PoseOrigin poseOrigin)
	{
		return poseOrigin == PoseOrigin.None;
	}

	public void InjectAllHand(UpdateModeFlags updateMode, IDataSource updateAfter, DataModifier<HandDataAsset> modifyDataFromSource, bool applyModifier)
	{
		InjectAllDataModifier(updateMode, updateAfter, modifyDataFromSource, applyModifier);
	}
}
