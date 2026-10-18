using System;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using Oculus.Interaction.Collections;
using UnityEngine;

namespace Oculus.Interaction.Body.PoseDetection;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Detection/Body Pose")]
public class BodyPoseData : ScriptableObject, IBodyPose, ISerializationCallbackReceiver
{
	[Serializable]
	internal struct JointData
	{
		public BodyJointId JointId;

		public BodyJointId ParentId;

		public Pose PoseFromRoot;

		public Pose LocalPose;
	}

	private class Mapping : ISkeletonMapping
	{
		public EnumerableHashSet<BodyJointId> Joints = new EnumerableHashSet<BodyJointId>();

		public Dictionary<BodyJointId, BodyJointId> JointToParent = new Dictionary<BodyJointId, BodyJointId>();

		IEnumerableHashSet<BodyJointId> ISkeletonMapping.Joints => Joints;

		bool ISkeletonMapping.TryGetParentJointId(BodyJointId jointId, out BodyJointId parent)
		{
			return JointToParent.TryGetValue(jointId, out parent);
		}
	}

	internal const int DATA_VERSION = 1;

	[SerializeField]
	[HideInInspector]
	private int _serializedVersion;

	[SerializeField]
	[HideInInspector]
	private List<JointData> _jointData = new List<JointData>();

	private Dictionary<BodyJointId, Pose> _posesFromRoot = new Dictionary<BodyJointId, Pose>();

	private Dictionary<BodyJointId, Pose> _localPoses = new Dictionary<BodyJointId, Pose>();

	private Mapping _mapping = new Mapping();

	public ISkeletonMapping SkeletonMapping => _mapping;

	public event Action WhenBodyPoseUpdated = delegate
	{
	};

	public bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose)
	{
		return _posesFromRoot.TryGetValue(bodyJointId, out pose);
	}

	public bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose)
	{
		return _localPoses.TryGetValue(bodyJointId, out pose);
	}

	public void SetBodyPose(IBody body)
	{
		_jointData.Clear();
		foreach (BodyJointId joint in body.SkeletonMapping.Joints)
		{
			if (body.GetJointPoseLocal(joint, out var pose) && body.GetJointPoseFromRoot(joint, out var pose2) && body.SkeletonMapping.TryGetParentJointId(joint, out var parent))
			{
				_jointData.Add(new JointData
				{
					JointId = joint,
					ParentId = parent,
					PoseFromRoot = pose2,
					LocalPose = pose
				});
			}
		}
		_serializedVersion = 1;
		Rebuild();
		this.WhenBodyPoseUpdated();
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		Rebuild();
	}

	private void Rebuild()
	{
		_localPoses.Clear();
		_posesFromRoot.Clear();
		_mapping.Joints.Clear();
		_mapping.JointToParent.Clear();
		for (int i = 0; i < _jointData.Count; i++)
		{
			_localPoses[_jointData[i].JointId] = _jointData[i].LocalPose;
			_posesFromRoot[_jointData[i].JointId] = _jointData[i].PoseFromRoot;
			_mapping.Joints.Add(_jointData[i].JointId);
			_mapping.JointToParent.Add(_jointData[i].JointId, _jointData[i].ParentId);
		}
	}
}
