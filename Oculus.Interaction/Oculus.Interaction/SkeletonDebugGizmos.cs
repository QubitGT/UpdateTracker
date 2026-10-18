using System;
using UnityEngine;

namespace Oculus.Interaction;

public abstract class SkeletonDebugGizmos : MonoBehaviour
{
	[Flags]
	public enum VisibilityFlags
	{
		Joints = 1,
		Axes = 2,
		Bones = 4
	}

	[Tooltip("Which components of the skeleton will be visualized.")]
	[SerializeField]
	private VisibilityFlags _visibility = VisibilityFlags.Joints | VisibilityFlags.Axes;

	[Tooltip("The joint debug spheres will be drawn with this color.")]
	[SerializeField]
	private Color _jointColor = Color.white;

	[Tooltip("The bone connecting lines will be drawn with this color.")]
	[SerializeField]
	private Color _boneColor = Color.gray;

	[Tooltip("The radius of the joint spheres and the thickness of the bone and axis lines.")]
	[SerializeField]
	private float _radius = 0.02f;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public VisibilityFlags Visibility
	{
		get
		{
			return _visibility;
		}
		set
		{
			_visibility = value;
		}
	}

	public Color JointColor
	{
		get
		{
			return _jointColor;
		}
		set
		{
			_jointColor = value;
		}
	}

	public Color BoneColor
	{
		get
		{
			return _boneColor;
		}
		set
		{
			_boneColor = value;
		}
	}

	private float LineWidth => _radius / 2f;

	protected bool HasNegativeScale
	{
		get
		{
			if (!(base.transform.lossyScale.x < 0f) && !(base.transform.lossyScale.y < 0f))
			{
				return base.transform.lossyScale.z < 0f;
			}
			return true;
		}
	}

	protected abstract bool TryGetJointPose(int jointId, out Pose pose);

	protected abstract bool TryGetParentJointId(int jointId, out int parent);

	protected void Draw(int joint, VisibilityFlags visibility)
	{
		if (TryGetJointPose(joint, out var pose))
		{
			if (visibility.HasFlag(VisibilityFlags.Axes))
			{
				DebugGizmos.LineWidth = LineWidth;
				DebugGizmos.DrawAxis(pose, _radius);
			}
			if (visibility.HasFlag(VisibilityFlags.Joints))
			{
				DebugGizmos.Color = _jointColor;
				DebugGizmos.LineWidth = _radius;
				DebugGizmos.DrawPoint(pose.position);
			}
			if (visibility.HasFlag(VisibilityFlags.Bones) && TryGetParentJointId(joint, out var parent) && TryGetJointPose(parent, out var pose2))
			{
				DebugGizmos.Color = _boneColor;
				DebugGizmos.LineWidth = LineWidth;
				DebugGizmos.DrawLine(pose.position, pose2.position);
			}
		}
	}
}
