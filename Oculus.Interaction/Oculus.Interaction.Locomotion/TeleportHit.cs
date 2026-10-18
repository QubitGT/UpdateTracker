using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public struct TeleportHit
{
	public Transform relativeTo;

	private Pose _localPose;

	public static readonly TeleportHit DEFAULT = new TeleportHit
	{
		relativeTo = null,
		_localPose = Pose.identity
	};

	public Vector3 Point
	{
		get
		{
			if (relativeTo == null)
			{
				return _localPose.position;
			}
			Pose a = relativeTo.GetPose();
			return PoseUtils.Multiply(in a, in _localPose).position;
		}
	}

	public Vector3 Normal
	{
		get
		{
			if (relativeTo == null)
			{
				return _localPose.rotation * Vector3.forward;
			}
			Pose a = relativeTo.GetPose();
			return PoseUtils.Multiply(in a, in _localPose).rotation * Vector3.forward;
		}
	}

	public TeleportHit(Transform relativeTo, Vector3 position, Vector3 normal)
	{
		this.relativeTo = relativeTo;
		Pose to = new Pose(position, Quaternion.LookRotation(normal));
		if (relativeTo == null)
		{
			_localPose = to;
			return;
		}
		Pose from = relativeTo.GetPose();
		_localPose = PoseUtils.Delta(in from, in to);
	}
}
