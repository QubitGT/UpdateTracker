using UnityEngine;

namespace Oculus.Interaction.Input;

public class ShadowHand
{
	private readonly Pose[] _localJointMap = new Pose[26];

	private readonly Pose[] _worldJointMap = new Pose[26];

	private Pose _rootPose;

	private float _rootScale;

	private ulong _dirtyMap;

	public ShadowHand()
	{
		for (int i = 0; i < _localJointMap.Length; i++)
		{
			_localJointMap[i] = Pose.identity;
			_worldJointMap[i] = Pose.identity;
		}
		_rootPose = Pose.identity;
		_rootScale = 1f;
		_dirtyMap = 0uL;
	}

	public Pose GetLocalPose(HandJointId handJointId)
	{
		return _localJointMap[(int)handJointId];
	}

	public void SetLocalPose(HandJointId jointId, Pose pose)
	{
		_localJointMap[(int)jointId] = pose;
		MarkDirty(jointId);
	}

	public Pose GetWorldPose(HandJointId jointId)
	{
		UpdateDirty(jointId);
		return _worldJointMap[(int)jointId];
	}

	public Pose[] GetWorldPoses()
	{
		UpdateDirty(HandJointId.HandWristRoot);
		return _worldJointMap;
	}

	public Pose GetRoot()
	{
		return _rootPose;
	}

	public void SetRoot(Pose rootPose)
	{
		_rootPose = rootPose;
		MarkDirty(HandJointId.HandStart);
	}

	public float GetRootScale()
	{
		return _rootScale;
	}

	public void SetRootScale(float scale)
	{
		_rootScale = scale;
		MarkDirty(HandJointId.HandStart);
	}

	private bool CheckDirtyBit(int i)
	{
		return ((_dirtyMap >> i) & 1) == 1;
	}

	private void SetDirtyBit(int i)
	{
		_dirtyMap |= (ulong)(1L << i);
	}

	private void ClearDirtyBit(int i)
	{
		_dirtyMap &= (ulong)(~(1L << i));
	}

	private void MarkDirty(HandJointId jointId)
	{
		if (!CheckDirtyBit((int)jointId))
		{
			SetDirtyBit((int)jointId);
			HandJointId[] array = HandJointUtils.JointChildrenList[(int)jointId];
			foreach (HandJointId jointId2 in array)
			{
				MarkDirty(jointId2);
			}
		}
	}

	private void UpdateDirty(HandJointId jointId)
	{
		if (CheckDirtyBit((int)jointId))
		{
			HandJointId handJointId = HandJointUtils.JointParentList[(int)jointId];
			if (handJointId != HandJointId.Invalid)
			{
				UpdateDirty(handJointId);
			}
			ClearDirtyBit((int)jointId);
			Pose a = ((handJointId != HandJointId.Invalid) ? GetWorldPose(handJointId) : _rootPose);
			Pose b = _localJointMap[(int)jointId];
			b.position *= _rootScale;
			PoseUtils.Multiply(in a, in b, ref _worldJointMap[(int)jointId]);
		}
	}

	public void Copy(ShadowHand hand)
	{
		SetRoot(hand.GetRoot());
		SetRootScale(hand.GetRootScale());
		for (int i = 0; i < 26; i++)
		{
			HandJointId handJointId = (HandJointId)i;
			SetLocalPose(handJointId, hand.GetLocalPose(handJointId));
		}
	}
}
