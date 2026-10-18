using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab.Visuals;

public class HandPuppet : MonoBehaviour
{
	[SerializeField]
	private List<HandJointMap> _jointMaps = new List<HandJointMap>(FingersMetadata.HAND_JOINT_IDS.Length);

	private JointCollection _jointsCache;

	public List<HandJointMap> JointMaps => _jointMaps;

	public float Scale
	{
		get
		{
			return base.transform.localScale.x;
		}
		set
		{
			base.transform.localScale = Vector3.one * value;
		}
	}

	private JointCollection JointsCache
	{
		get
		{
			if (_jointsCache == null)
			{
				_jointsCache = new JointCollection(_jointMaps);
			}
			return _jointsCache;
		}
	}

	public void SetJointRotations(in Quaternion[] jointRotations)
	{
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length && i < jointRotations.Length; i++)
		{
			HandJointMap handJointMap = JointsCache[i];
			if (handJointMap != null)
			{
				Transform transform = handJointMap.transform;
				Quaternion localRotation = handJointMap.RotationOffset * jointRotations[i];
				transform.localRotation = localRotation;
			}
		}
	}

	public void SetRootPose(in Pose rootPose)
	{
		base.transform.SetPose(in rootPose);
	}

	public void CopyCachedJoints(ref HandPose result)
	{
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			HandJointMap handJointMap = JointsCache[i];
			if (handJointMap != null)
			{
				result.JointRotations[i] = handJointMap.TrackedRotation;
			}
		}
	}
}
