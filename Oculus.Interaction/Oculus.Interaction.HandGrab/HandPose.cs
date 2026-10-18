using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

[Serializable]
public class HandPose
{
	[SerializeField]
	private Handedness _handedness;

	[SerializeField]
	private JointFreedom[] _fingersFreedom = FingersMetadata.DefaultFingersFreedom();

	[SerializeField]
	private Quaternion[] _jointRotations = new Quaternion[FingersMetadata.HAND_JOINT_IDS.Length];

	public Handedness Handedness
	{
		get
		{
			return _handedness;
		}
		set
		{
			_handedness = value;
		}
	}

	public Quaternion[] JointRotations
	{
		get
		{
			if (_jointRotations == null || _jointRotations.Length == 0)
			{
				_jointRotations = new Quaternion[FingersMetadata.HAND_JOINT_IDS.Length];
			}
			return _jointRotations;
		}
		set
		{
			_jointRotations = value;
		}
	}

	public JointFreedom[] FingersFreedom
	{
		get
		{
			if (_fingersFreedom == null || _fingersFreedom.Length == 0)
			{
				_fingersFreedom = FingersMetadata.DefaultFingersFreedom();
			}
			return _fingersFreedom;
		}
	}

	public HandPose()
	{
	}

	public HandPose(Handedness handedness)
	{
		_handedness = handedness;
	}

	public HandPose(HandPose other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(HandPose from, bool mirrorHandedness = false)
	{
		if (!mirrorHandedness)
		{
			_handedness = from.Handedness;
		}
		Array.Copy(from.FingersFreedom, FingersFreedom, 5);
		Array.Copy(from.JointRotations, JointRotations, FingersMetadata.HAND_JOINT_IDS.Length);
	}

	public static void Lerp(in HandPose from, in HandPose to, float t, ref HandPose result)
	{
		t = Mathf.Clamp01(t);
		for (int i = 0; i < from.JointRotations.Length && i < to.JointRotations.Length; i++)
		{
			result.JointRotations[i] = Quaternion.SlerpUnclamped(from.JointRotations[i], to.JointRotations[i], t);
		}
		HandPose handPose = ((t <= 0.5f) ? from : to);
		result._handedness = handPose.Handedness;
		for (int j = 0; j < 5; j++)
		{
			result.FingersFreedom[j] = handPose.FingersFreedom[j];
		}
	}
}
