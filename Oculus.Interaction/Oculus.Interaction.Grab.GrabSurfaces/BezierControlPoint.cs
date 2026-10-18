using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public struct BezierControlPoint
{
	[SerializeField]
	[FormerlySerializedAs("pose")]
	private Pose _pose;

	[SerializeField]
	[FormerlySerializedAs("tangentPoint")]
	private Vector3 _tangentPoint;

	[SerializeField]
	[FormerlySerializedAs("disconnected")]
	private bool _disconnected;

	public bool Disconnected
	{
		get
		{
			return _disconnected;
		}
		set
		{
			_disconnected = value;
		}
	}

	public Pose GetPose(Transform relativeTo)
	{
		return PoseUtils.GlobalPoseScaled(relativeTo, _pose);
	}

	public void SetPose(in Pose worldSpacePose, Transform relativeTo)
	{
		_pose = PoseUtils.DeltaScaled(relativeTo, worldSpacePose);
	}

	public Vector3 GetTangent(Transform relativeTo)
	{
		return relativeTo.TransformPoint(_tangentPoint);
	}

	public void SetTangent(in Vector3 tangent, Transform relativeTo)
	{
		_tangentPoint = relativeTo.InverseTransformPoint(tangent);
	}
}
