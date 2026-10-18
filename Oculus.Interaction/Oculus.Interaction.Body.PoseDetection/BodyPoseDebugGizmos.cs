using System;
using Oculus.Interaction.Body.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.PoseDetection;

public class BodyPoseDebugGizmos : SkeletonDebugGizmos
{
	[Tooltip("The IBodyPose that will drive the visuals.")]
	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _bodyPose;

	private IBodyPose BodyPose;

	protected virtual void Awake()
	{
		BodyPose = _bodyPose as IBodyPose;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		foreach (BodyJointId joint in BodyPose.SkeletonMapping.Joints)
		{
			Draw((int)joint, GetVisibilityFlags());
		}
	}

	private VisibilityFlags GetVisibilityFlags()
	{
		VisibilityFlags visibilityFlags = base.Visibility;
		if (base.HasNegativeScale)
		{
			visibilityFlags &= ~VisibilityFlags.Axes;
		}
		return visibilityFlags;
	}

	protected override bool TryGetJointPose(int jointId, out Pose pose)
	{
		if (BodyPose.GetJointPoseFromRoot((BodyJointId)jointId, out pose))
		{
			pose.position = base.transform.TransformPoint(pose.position);
			pose.rotation = base.transform.rotation * pose.rotation;
			return true;
		}
		return false;
	}

	protected override bool TryGetParentJointId(int jointId, out int parent)
	{
		if (BodyPose.SkeletonMapping.TryGetParentJointId((BodyJointId)jointId, out var parent2))
		{
			parent = (int)parent2;
			return true;
		}
		parent = 0;
		return false;
	}

	public void InjectAllBodyJointDebugGizmos(IBodyPose bodyPose)
	{
		InjectBodyPose(bodyPose);
	}

	public void InjectBodyPose(IBodyPose bodyPose)
	{
		_bodyPose = bodyPose as UnityEngine.Object;
		BodyPose = bodyPose;
	}
}
