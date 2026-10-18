using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class SphereGrabSurface : MonoBehaviour, IGrabSurface
{
	[SerializeField]
	protected SphereGrabSurfaceData _data = new SphereGrabSurfaceData();

	[SerializeField]
	[Tooltip("Transform used as a reference to measure the local data of the grab surface")]
	private Transform _relativeTo;

	private Pose RelativePose => PoseUtils.DeltaScaled(_relativeTo, base.transform);

	public Pose GetReferencePose(Transform relativeTo)
	{
		return PoseUtils.GlobalPoseScaled(relativeTo, RelativePose);
	}

	public Vector3 GetCentre(Transform relativeTo)
	{
		return relativeTo.TransformPoint(_data.centre);
	}

	public void SetCentre(Vector3 point, Transform relativeTo)
	{
		_data.centre = relativeTo.InverseTransformPoint(point);
	}

	public float GetRadius(Transform relativeTo)
	{
		return Vector3.Distance(GetCentre(relativeTo), GetReferencePose(relativeTo).position);
	}

	public Vector3 GetDirection(Transform relativeTo)
	{
		Vector3 centre = GetCentre(relativeTo);
		return (GetReferencePose(relativeTo).position - centre).normalized;
	}

	protected virtual void Reset()
	{
		_relativeTo = GetComponentInParent<IRelativeToRef>()?.RelativeTo;
	}

	protected virtual void Start()
	{
	}

	public Pose MirrorPose(in Pose pose, Transform relativeTo)
	{
		Vector3 normalized = Vector3.Cross(pose.position, Vector3.up).normalized;
		Quaternion rotation = HandMirroring.Reflect(in pose.rotation, normalized);
		return new Pose(pose.position, rotation);
	}

	public bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo)
	{
		Vector3 centre = GetCentre(relativeTo);
		Vector3 vector = Vector3.Project(centre - targetRay.origin, targetRay.direction);
		Vector3 vector2 = targetRay.origin + vector;
		float radius = GetRadius(relativeTo);
		float num = Mathf.Max(Vector3.Distance(centre, vector2) - radius);
		if (num < radius)
		{
			float num2 = Mathf.Sqrt(radius * radius - num * num);
			vector2 -= targetRay.direction * num2;
		}
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 position = NearestPointInSurface(vector2, relativeTo);
		Pose userPose = new Pose(position, referencePose.rotation);
		bestPose = MinimalTranslationPoseAtSurface(in userPose, relativeTo);
		return true;
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		Pose offset = Pose.identity;
		return CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose, in scoringModifier, relativeTo);
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		return GrabPoseHelper.CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose, in scoringModifier, relativeTo, MinimalTranslationPoseAtSurface, MinimalRotationPoseAtSurface);
	}

	public IGrabSurface CreateMirroredSurface(GameObject gameObject)
	{
		SphereGrabSurface sphereGrabSurface = gameObject.AddComponent<SphereGrabSurface>();
		sphereGrabSurface._data = _data.Mirror();
		return sphereGrabSurface;
	}

	public IGrabSurface CreateDuplicatedSurface(GameObject gameObject)
	{
		SphereGrabSurface sphereGrabSurface = gameObject.AddComponent<SphereGrabSurface>();
		sphereGrabSurface._data = _data;
		return sphereGrabSurface;
	}

	protected Vector3 NearestPointInSurface(Vector3 targetPosition, Transform relativeTo)
	{
		Vector3 centre = GetCentre(relativeTo);
		Vector3 normalized = (targetPosition - centre).normalized;
		float radius = GetRadius(relativeTo);
		return centre + normalized * radius;
	}

	protected Pose MinimalRotationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Vector3 centre = GetCentre(relativeTo);
		Pose referencePose = GetReferencePose(relativeTo);
		float radius = GetRadius(relativeTo);
		Vector3 vector = userPose.rotation * Quaternion.Inverse(referencePose.rotation) * GetDirection(relativeTo);
		Vector3 vector2 = NearestPointInSurface(centre + vector * radius, relativeTo);
		Quaternion rotation = RotationAtPoint(vector2, referencePose.rotation, userPose.rotation, relativeTo);
		return new Pose(vector2, rotation);
	}

	protected Pose MinimalTranslationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 position = userPose.position;
		Quaternion rotation = referencePose.rotation;
		Vector3 vector = NearestPointInSurface(position, relativeTo);
		Quaternion rotation2 = RotationAtPoint(vector, rotation, userPose.rotation, relativeTo);
		return new Pose(vector, rotation2);
	}

	protected Quaternion RotationAtPoint(Vector3 surfacePoint, Quaternion baseRot, Quaternion desiredRotation, Transform relativeTo)
	{
		Vector3 normalized = (surfacePoint - GetCentre(relativeTo)).normalized;
		Quaternion quaternion = Quaternion.FromToRotation(GetDirection(relativeTo), normalized) * baseRot;
		Vector3 normalized2 = Vector3.ProjectOnPlane(quaternion * Vector3.forward, normalized).normalized;
		Vector3 normalized3 = Vector3.ProjectOnPlane(desiredRotation * Vector3.forward, normalized).normalized;
		return Quaternion.FromToRotation(normalized2, normalized3) * quaternion;
	}

	public void InjectAllSphereSurface(SphereGrabSurfaceData data, Transform relativeTo)
	{
		InjectData(data);
		InjectRelativeTo(relativeTo);
	}

	public void InjectData(SphereGrabSurfaceData data)
	{
		_data = data;
	}

	public void InjectRelativeTo(Transform relativeTo)
	{
		_relativeTo = relativeTo;
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
