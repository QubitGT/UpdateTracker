using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class SyntheticHand : Hand
{
	[Flags]
	public enum WristLockMode
	{
		Position = 1,
		Rotation = 2,
		Full = 3
	}

	[SerializeField]
	private ProgressCurve _wristPositionLockCurve;

	[SerializeField]
	private ProgressCurve _wristPositionUnlockCurve;

	[SerializeField]
	private ProgressCurve _wristRotationLockCurve;

	[SerializeField]
	private ProgressCurve _wristRotationUnlockCurve;

	[SerializeField]
	private ProgressCurve _jointLockCurve;

	[SerializeField]
	private ProgressCurve _jointUnlockCurve;

	[SerializeField]
	[Tooltip("Use this factor to control how much the fingers can spread when nearby a constrained pose.")]
	private float _spreadAllowance = 5f;

	public Action UpdateRequired = delegate
	{
	};

	private readonly HandDataAsset _lastStates = new HandDataAsset();

	private float _wristPositionOverrideFactor;

	private float _wristRotationOverrideFactor;

	private float[] _jointsOverrideFactor = new float[FingersMetadata.HAND_JOINT_IDS.Length];

	private ProgressCurve[] _jointLockProgressCurves = new ProgressCurve[FingersMetadata.HAND_JOINT_IDS.Length];

	private ProgressCurve[] _jointUnlockProgressCurves = new ProgressCurve[FingersMetadata.HAND_JOINT_IDS.Length];

	private Pose _desiredWristPose;

	private bool _wristPositionLocked;

	private bool _wristRotationLocked;

	private Pose _constrainedWristPose;

	private Pose _lastWristPose;

	private Quaternion[] _desiredJointsRotation = new Quaternion[FingersMetadata.HAND_JOINT_IDS.Length];

	private Quaternion[] _constrainedJointRotations = new Quaternion[FingersMetadata.HAND_JOINT_IDS.Length];

	private Quaternion[] _lastSyntheticRotation = new Quaternion[FingersMetadata.HAND_JOINT_IDS.Length];

	private JointFreedom[] _jointsFreedomLevels = new JointFreedom[FingersMetadata.HAND_JOINT_IDS.Length];

	private bool _hasConnectedData;

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			_jointLockProgressCurves[i] = new ProgressCurve(_jointLockCurve);
			_jointUnlockProgressCurves[i] = new ProgressCurve(_jointUnlockCurve);
		}
		this.EndStart(ref _started);
	}

	protected override void Apply(HandDataAsset data)
	{
		if (!base.Started || !data.IsDataValid || !data.IsTracked || !data.IsHighConfidence)
		{
			data.IsConnected = false;
			data.RootPoseOrigin = PoseOrigin.None;
			_hasConnectedData = false;
			return;
		}
		UpdateRequired();
		_lastStates.CopyFrom(data);
		if (!_hasConnectedData)
		{
			_constrainedWristPose.CopyFrom(in data.Root);
			_hasConnectedData = true;
		}
		UpdateJointsRotation(data);
		UpdateRootPose(ref data.Root);
		SyncDataPoses(data);
		data.RootPoseOrigin = PoseOrigin.SyntheticPose;
	}

	private void SyncDataPoses(HandDataAsset data)
	{
		for (int i = 0; i < 26; i++)
		{
			int num = (int)HandJointUtils.JointParentList[i];
			if (num >= 0)
			{
				Vector3 position = PoseUtils.Delta(in _lastStates.JointPoses[num], in _lastStates.JointPoses[i]).position;
				ref Pose a = ref data.JointPoses[num];
				Pose b = new Pose(position, data.Joints[i]);
				PoseUtils.Multiply(in a, in b, ref data.JointPoses[i]);
			}
		}
	}

	private void UpdateRootPose(ref Pose root)
	{
		float t = (_wristPositionLocked ? _wristPositionLockCurve.Progress() : _wristPositionUnlockCurve.Progress());
		Vector3 b = Vector3.Lerp(root.position, _desiredWristPose.position, _wristPositionOverrideFactor);
		root.position = Vector3.Lerp(_constrainedWristPose.position, b, t);
		float t2 = (_wristRotationLocked ? _wristRotationLockCurve.Progress() : _wristRotationUnlockCurve.Progress());
		Quaternion b2 = Quaternion.Lerp(root.rotation, _desiredWristPose.rotation, _wristRotationOverrideFactor);
		root.rotation = Quaternion.Lerp(_constrainedWristPose.rotation, b2, t2);
		_lastWristPose.CopyFrom(in root);
	}

	private void UpdateJointsRotation(HandDataAsset data)
	{
		float num = 0f;
		Quaternion[] sourceRotations = data.Joints;
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			JointFreedom jointFreedom = _jointsFreedomLevels[i];
			Quaternion quaternion = AmendMetacarpalRotation(i, in sourceRotations);
			float t = _jointsOverrideFactor[i];
			int num2 = (int)FingersMetadata.HAND_JOINT_IDS[i];
			switch (jointFreedom)
			{
			case JointFreedom.Locked:
				sourceRotations[num2] = Quaternion.Slerp(sourceRotations[num2], quaternion, t);
				break;
			case JointFreedom.Constrained:
			{
				bool flag = false;
				if (FingersMetadata.HAND_JOINT_CAN_SPREAD[i])
				{
					flag = true;
					num = 0f;
				}
				Vector3 rightThumbSide = Constants.RightThumbSide;
				Vector3 rightDorsal = Constants.RightDorsal;
				Quaternion maxLocalRot = quaternion * Quaternion.Euler(rightThumbSide * -90f * num);
				float num3 = OverFlex(in sourceRotations[num2], in maxLocalRot);
				num = Mathf.Max(num, num3);
				if (num3 < 0f)
				{
					sourceRotations[num2] = Quaternion.Slerp(sourceRotations[num2], maxLocalRot, t);
				}
				else if (flag)
				{
					Quaternion quaternion2 = sourceRotations[num2];
					float num4 = Vector3.SignedAngle(quaternion2 * rightThumbSide, maxLocalRot * rightThumbSide, quaternion2 * rightDorsal);
					float num5 = 1f - Mathf.Clamp01(num3 * _spreadAllowance);
					quaternion2 *= Quaternion.Euler(rightDorsal * num4 * num5);
					sourceRotations[num2] = quaternion2;
				}
				break;
			}
			}
			float t2 = ((_jointsFreedomLevels[i] == JointFreedom.Free) ? _jointUnlockProgressCurves[i].Progress() : _jointLockProgressCurves[i].Progress());
			sourceRotations[num2] = Quaternion.Slerp(_constrainedJointRotations[i], sourceRotations[num2], t2);
			_lastSyntheticRotation[i] = sourceRotations[num2];
		}
	}

	private Quaternion AmendMetacarpalRotation(int jointIndex, in Quaternion[] sourceRotations)
	{
		HandJointId handJointId = FingersMetadata.HAND_JOINT_IDS[jointIndex];
		int num = (int)handJointId;
		switch (handJointId)
		{
		case HandJointId.HandIndex0:
		case HandJointId.HandMiddle0:
		case HandJointId.HandRing0:
			return sourceRotations[num];
		case HandJointId.HandIndex1:
		case HandJointId.HandMiddle1:
		case HandJointId.HandRing1:
			return Quaternion.Inverse(sourceRotations[num - 1]) * _desiredJointsRotation[jointIndex];
		default:
			return _desiredJointsRotation[jointIndex];
		}
	}

	public void OverrideAllJoints(in Quaternion[] jointRotations, float overrideFactor)
	{
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			_desiredJointsRotation[i] = jointRotations[i];
			_jointsOverrideFactor[i] = overrideFactor;
		}
	}

	public void OverrideFingerRotations(HandFinger finger, Quaternion[] rotations, float overrideFactor)
	{
		int[] array = FingersMetadata.FINGER_TO_JOINT_INDEX[(int)finger];
		for (int i = 0; i < array.Length; i++)
		{
			OverrideJointRotationAtIndex(array[i], rotations[i], overrideFactor);
		}
	}

	public void OverrideJointRotation(HandJointId jointId, Quaternion rotation, float overrideFactor)
	{
		int jointIndex = FingersMetadata.HandJointIdToIndex(jointId);
		OverrideJointRotationAtIndex(jointIndex, rotation, overrideFactor);
	}

	private void OverrideJointRotationAtIndex(int jointIndex, Quaternion rotation, float overrideFactor)
	{
		_desiredJointsRotation[jointIndex] = rotation;
		_jointsOverrideFactor[jointIndex] = overrideFactor;
	}

	public void LockFingerAtCurrent(in HandFinger finger)
	{
		JointFreedom freedomLevel = JointFreedom.Locked;
		SetFingerFreedom(in finger, in freedomLevel);
		int num = (int)finger;
		int[] array = FingersMetadata.FINGER_TO_JOINT_INDEX[num];
		foreach (int num2 in array)
		{
			int num3 = (int)FingersMetadata.HAND_JOINT_IDS[num2];
			_desiredJointsRotation[num2] = _lastStates.Joints[num3];
			_jointsOverrideFactor[num2] = 1f;
		}
	}

	public void LockJoint(in HandJointId jointId, Quaternion rotation, float overrideFactor = 1f)
	{
		int num = FingersMetadata.HandJointIdToIndex(jointId);
		_desiredJointsRotation[num] = rotation;
		_jointsOverrideFactor[num] = 1f;
		JointFreedom freedomLevel = JointFreedom.Locked;
		SetJointFreedomAtIndex(num, in freedomLevel);
	}

	public void SetFingerFreedom(in HandFinger finger, in JointFreedom freedomLevel, bool skipAnimation = false)
	{
		int[] array = FingersMetadata.FINGER_TO_JOINT_INDEX[(int)finger];
		for (int i = 0; i < array.Length; i++)
		{
			SetJointFreedomAtIndex(array[i], in freedomLevel, skipAnimation);
		}
	}

	public void SetJointFreedom(in HandJointId jointId, in JointFreedom freedomLevel, bool skipAnimation = false)
	{
		int jointId2 = FingersMetadata.HandJointIdToIndex(jointId);
		SetJointFreedomAtIndex(jointId2, in freedomLevel, skipAnimation);
	}

	public JointFreedom GetJointFreedom(in HandJointId jointId)
	{
		int num = FingersMetadata.HandJointIdToIndex(jointId);
		return _jointsFreedomLevels[num];
	}

	public void FreeAllJoints()
	{
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			int jointId = i;
			JointFreedom freedomLevel = JointFreedom.Free;
			SetJointFreedomAtIndex(jointId, in freedomLevel);
		}
	}

	private void SetJointFreedomAtIndex(int jointId, in JointFreedom freedomLevel, bool skipAnimation = false)
	{
		if (_jointsFreedomLevels[jointId] != freedomLevel)
		{
			bool locked = freedomLevel == JointFreedom.Locked || freedomLevel == JointFreedom.Constrained;
			UpdateProgressCurve(ref _jointLockProgressCurves[jointId], ref _jointUnlockProgressCurves[jointId], locked, skipAnimation);
			_constrainedJointRotations[jointId] = _lastSyntheticRotation[jointId];
		}
		_jointsFreedomLevels[jointId] = freedomLevel;
	}

	public void LockWristPose(Pose wristPose, float overrideFactor = 1f, WristLockMode lockMode = WristLockMode.Full, bool worldPose = false, bool skipAnimation = false)
	{
		Pose pose = ((worldPose && base.TrackingToWorldTransformer != null) ? base.TrackingToWorldTransformer.ToTrackingPose(in wristPose) : wristPose);
		if ((lockMode & WristLockMode.Position) != 0)
		{
			LockWristPosition(pose.position, overrideFactor, skipAnimation);
		}
		if ((lockMode & WristLockMode.Rotation) != 0)
		{
			LockWristRotation(pose.rotation, overrideFactor, skipAnimation);
		}
	}

	public void LockWristPosition(Vector3 position, float overrideFactor = 1f, bool skipAnimation = false)
	{
		_wristPositionOverrideFactor = overrideFactor;
		_desiredWristPose.position = position;
		if (!_wristPositionLocked)
		{
			_wristPositionLocked = true;
			SyntheticWristLockChangedState(WristLockMode.Position, skipAnimation);
		}
	}

	public void LockWristRotation(Quaternion rotation, float overrideFactor = 1f, bool skipAnimation = false)
	{
		_wristRotationOverrideFactor = overrideFactor;
		_desiredWristPose.rotation = rotation;
		if (!_wristRotationLocked)
		{
			_wristRotationLocked = true;
			SyntheticWristLockChangedState(WristLockMode.Rotation, skipAnimation);
		}
	}

	public void FreeWrist(WristLockMode lockMode = WristLockMode.Full)
	{
		if ((lockMode & WristLockMode.Position) != 0 && _wristPositionLocked)
		{
			_wristPositionOverrideFactor = 0f;
			_wristPositionLocked = false;
			SyntheticWristLockChangedState(WristLockMode.Position);
		}
		if ((lockMode & WristLockMode.Rotation) != 0 && _wristRotationLocked)
		{
			_wristRotationOverrideFactor = 0f;
			_wristRotationLocked = false;
			SyntheticWristLockChangedState(WristLockMode.Rotation);
		}
	}

	private void SyntheticWristLockChangedState(WristLockMode lockMode, bool skipAnimation = false)
	{
		if ((lockMode & WristLockMode.Position) != 0)
		{
			UpdateProgressCurve(ref _wristPositionLockCurve, ref _wristPositionUnlockCurve, _wristPositionLocked, skipAnimation);
			_constrainedWristPose.position = _lastWristPose.position;
		}
		if ((lockMode & WristLockMode.Rotation) != 0)
		{
			UpdateProgressCurve(ref _wristRotationLockCurve, ref _wristRotationUnlockCurve, _wristRotationLocked, skipAnimation);
			_constrainedWristPose.rotation = _lastWristPose.rotation;
		}
	}

	private static float OverFlex(in Quaternion desiredLocalRot, in Quaternion maxLocalRot)
	{
		Vector3 lhs = desiredLocalRot * Constants.RightDistal;
		Vector3 lhs2 = desiredLocalRot * Constants.RightPinkySide;
		Vector3 rhs = maxLocalRot * Constants.RightDistal;
		Vector3 rhs2 = Vector3.Cross(lhs, rhs);
		return Vector3.Dot(lhs2, rhs2);
	}

	private static void UpdateProgressCurve(ref ProgressCurve lockProgress, ref ProgressCurve unlockProgress, bool locked, bool skipAnimation)
	{
		ProgressCurve progressCurve = (locked ? lockProgress : unlockProgress);
		if (skipAnimation)
		{
			progressCurve.End();
		}
		else
		{
			progressCurve.Start();
		}
	}

	public void InjectAllSyntheticHandModifier(UpdateModeFlags updateMode, IDataSource updateAfter, DataModifier<HandDataAsset> modifyDataFromSource, bool applyModifier, ProgressCurve wristPositionLockCurve, ProgressCurve wristPositionUnlockCurve, ProgressCurve wristRotationLockCurve, ProgressCurve wristRotationUnlockCurve, ProgressCurve jointLockCurve, ProgressCurve jointUnlockCurve, float spreadAllowance)
	{
		InjectAllHand(updateMode, updateAfter, modifyDataFromSource, applyModifier);
		InjectWristPositionLockCurve(wristPositionLockCurve);
		InjectWristPositionUnlockCurve(wristPositionUnlockCurve);
		InjectWristRotationLockCurve(wristRotationLockCurve);
		InjectWristRotationUnlockCurve(wristRotationUnlockCurve);
		InjectJointLockCurve(jointLockCurve);
		InjectJointUnlockCurve(jointUnlockCurve);
		InjectSpreadAllowance(spreadAllowance);
	}

	public void InjectWristPositionLockCurve(ProgressCurve wristPositionLockCurve)
	{
		_wristPositionLockCurve = wristPositionLockCurve;
	}

	public void InjectWristPositionUnlockCurve(ProgressCurve wristPositionUnlockCurve)
	{
		_wristPositionUnlockCurve = wristPositionUnlockCurve;
	}

	public void InjectWristRotationLockCurve(ProgressCurve wristRotationLockCurve)
	{
		_wristRotationLockCurve = wristRotationLockCurve;
	}

	public void InjectWristRotationUnlockCurve(ProgressCurve wristRotationUnlockCurve)
	{
		_wristRotationUnlockCurve = wristRotationUnlockCurve;
	}

	public void InjectJointLockCurve(ProgressCurve jointLockCurve)
	{
		_jointLockCurve = jointLockCurve;
	}

	public void InjectJointUnlockCurve(ProgressCurve jointUnlockCurve)
	{
		_jointUnlockCurve = jointUnlockCurve;
	}

	public void InjectSpreadAllowance(float spreadAllowance)
	{
		_spreadAllowance = spreadAllowance;
	}
}
