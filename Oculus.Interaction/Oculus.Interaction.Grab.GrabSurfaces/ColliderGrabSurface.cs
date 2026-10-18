using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

public class ColliderGrabSurface : MonoBehaviour, IGrabSurface
{
	[SerializeField]
	private Collider _collider;

	protected virtual void Start()
	{
	}

	private Vector3 NearestPointInSurface(Vector3 targetPosition)
	{
		if (_collider.bounds.Contains(targetPosition))
		{
			targetPosition = _collider.ClosestPointOnBounds(targetPosition);
		}
		return _collider.ClosestPoint(targetPosition);
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		Pose offset = Pose.identity;
		return CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose, in scoringModifier, relativeTo);
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		Vector3 position = NearestPointInSurface(targetPose.position);
		bestPose = new Pose(position, targetPose.rotation);
		return new GrabPoseScore(in targetPose, in bestPose, in offset, scoringModifier);
	}

	public bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo)
	{
		if (_collider.Raycast(targetRay, out var hitInfo, float.PositiveInfinity))
		{
			bestPose.position = hitInfo.point;
			bestPose.rotation = relativeTo.rotation;
			return true;
		}
		bestPose = Pose.identity;
		return false;
	}

	public Pose MirrorPose(in Pose gripPose, Transform relativeTo)
	{
		return HandMirroring.Mirror(gripPose);
	}

	public IGrabSurface CreateMirroredSurface(GameObject gameObject)
	{
		return CreateDuplicatedSurface(gameObject);
	}

	public IGrabSurface CreateDuplicatedSurface(GameObject gameObject)
	{
		ColliderGrabSurface colliderGrabSurface = gameObject.AddComponent<ColliderGrabSurface>();
		colliderGrabSurface.InjectAllColliderGrabSurface(_collider);
		return colliderGrabSurface;
	}

	public void InjectAllColliderGrabSurface(Collider collider)
	{
		InjectCollider(collider);
	}

	public void InjectCollider(Collider collider)
	{
		_collider = collider;
	}

	GrabPoseScore IGrabSurface.CalculateBestPoseAtSurface(in Pose targetPose, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		return CalculateBestPoseAtSurface(in targetPose, out bestPose, in scoringModifier, relativeTo);
	}

	GrabPoseScore IGrabSurface.CalculateBestPoseAtSurface(in Pose targetPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		return CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose, in scoringModifier, relativeTo);
	}

	Pose IGrabSurface.MirrorPose(in Pose gripPose, Transform relativeTo)
	{
		return MirrorPose(in gripPose, relativeTo);
	}
}
