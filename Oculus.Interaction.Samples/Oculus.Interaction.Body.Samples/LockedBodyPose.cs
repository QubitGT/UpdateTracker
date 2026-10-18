using System;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using Oculus.Interaction.Body.PoseDetection;
using UnityEngine;

namespace Oculus.Interaction.Body.Samples;

public class LockedBodyPose : MonoBehaviour, IBodyPose
{
	private static readonly Pose HIP_OFFSET = new Pose
	{
		position = new Vector3(0f, 0.923987f, 0f),
		rotation = Quaternion.Euler(0f, 270f, 270f)
	};

	[Tooltip("The body pose to be locked")]
	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _pose;

	private IBodyPose Pose;

	[Tooltip("The body pose will be locked relative to this joint at the specified offset.")]
	[SerializeField]
	private BodyJointId _referenceJoint = BodyJointId.Body_Hips;

	[Tooltip("The reference joint will be placed at this offset from the root.")]
	[SerializeField]
	private Pose _referenceOffset = HIP_OFFSET;

	protected bool _started;

	private Dictionary<BodyJointId, Pose> _lockedPoses;

	public ISkeletonMapping SkeletonMapping => Pose.SkeletonMapping;

	public event Action WhenBodyPoseUpdated = delegate
	{
	};

	public bool GetJointPoseLocal(BodyJointId bodyJointId, out Pose pose)
	{
		return Pose.GetJointPoseLocal(bodyJointId, out pose);
	}

	public bool GetJointPoseFromRoot(BodyJointId bodyJointId, out Pose pose)
	{
		return _lockedPoses.TryGetValue(bodyJointId, out pose);
	}

	private void UpdateLockedBodyPose()
	{
		_lockedPoses.Clear();
		for (int i = 0; i < 84; i++)
		{
			BodyJointId bodyJointId = (BodyJointId)i;
			if (Pose.GetJointPoseFromRoot(_referenceJoint, out var pose) && Pose.GetJointPoseFromRoot(bodyJointId, out var pose2))
			{
				pose.Invert();
				PoseUtils.Multiply(in pose, in pose2, ref pose2);
				PoseUtils.Multiply(in _referenceOffset, in pose2, ref pose2);
				_lockedPoses[bodyJointId] = pose2;
			}
		}
		this.WhenBodyPoseUpdated();
	}

	protected virtual void Awake()
	{
		_lockedPoses = new Dictionary<BodyJointId, Pose>();
		Pose = _pose as IBodyPose;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		UpdateLockedBodyPose();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Pose.WhenBodyPoseUpdated += UpdateLockedBodyPose;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Pose.WhenBodyPoseUpdated -= UpdateLockedBodyPose;
		}
	}
}
