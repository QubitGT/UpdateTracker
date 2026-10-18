using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class CylinderGrabSurface : MonoBehaviour, IGrabSurface
{
	[SerializeField]
	protected CylinderSurfaceData _data = new CylinderSurfaceData();

	[SerializeField]
	[Tooltip("Transform used as a reference to measure the local data of the grab surface")]
	private Transform _relativeTo;

	private const float Epsilon = 1E-06f;

	private Pose RelativePose => PoseUtils.DeltaScaled(_relativeTo, base.transform);

	public float ArcOffset
	{
		get
		{
			return _data.arcOffset;
		}
		set
		{
			if (value != 0f && value % 360f == 0f)
			{
				_data.arcOffset = 360f;
			}
			else
			{
				_data.arcOffset = Mathf.Repeat(value, 360f);
			}
		}
	}

	public float ArcLength
	{
		get
		{
			return _data.arcLength;
		}
		set
		{
			if (value != 0f && value % 360f == 0f)
			{
				_data.arcLength = 360f;
			}
			else
			{
				_data.arcLength = Mathf.Repeat(value, 360f);
			}
		}
	}

	private Vector3 LocalPerpendicularDir => Vector3.ProjectOnPlane(RelativePose.position - _data.startPoint, LocalDirection).normalized;

	private Vector3 LocalDirection
	{
		get
		{
			Vector3 vector = _data.endPoint - _data.startPoint;
			if (vector.sqrMagnitude <= 1E-06f)
			{
				return Vector3.up;
			}
			return vector.normalized;
		}
	}

	public Pose GetReferencePose(Transform relativeTo)
	{
		return PoseUtils.GlobalPoseScaled(relativeTo, RelativePose);
	}

	public Vector3 GetPerpendicularDir(Transform relativeTo)
	{
		return relativeTo.TransformDirection(LocalPerpendicularDir);
	}

	public Vector3 GetStartArcDir(Transform relativeTo)
	{
		Vector3 direction = Quaternion.AngleAxis(ArcOffset, LocalDirection) * LocalPerpendicularDir;
		return relativeTo.TransformDirection(direction);
	}

	public Vector3 GetEndArcDir(Transform relativeTo)
	{
		Vector3 direction = Quaternion.AngleAxis(ArcLength, LocalDirection) * Quaternion.AngleAxis(ArcOffset, LocalDirection) * LocalPerpendicularDir;
		return relativeTo.TransformDirection(direction);
	}

	public Vector3 GetStartPoint(Transform relativeTo)
	{
		return relativeTo.TransformPoint(_data.startPoint);
	}

	public void SetStartPoint(Vector3 point, Transform relativeTo)
	{
		_data.startPoint = relativeTo.InverseTransformPoint(point);
	}

	public Vector3 GetEndPoint(Transform relativeTo)
	{
		return relativeTo.TransformPoint(_data.endPoint);
	}

	public void SetEndPoint(Vector3 point, Transform relativeTo)
	{
		_data.endPoint = relativeTo.InverseTransformPoint(point);
	}

	public float GetRadius(Transform relativeTo)
	{
		Vector3 startPoint = GetStartPoint(relativeTo);
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		return Vector3.Distance(startPoint + Vector3.Project(referencePose.position - startPoint, direction), referencePose.position);
	}

	public Vector3 GetDirection(Transform relativeTo)
	{
		return relativeTo.TransformDirection(LocalDirection);
	}

	private float GetHeight(Transform relativeTo)
	{
		Vector3 startPoint = GetStartPoint(relativeTo);
		Vector3 endPoint = GetEndPoint(relativeTo);
		return Vector3.Distance(startPoint, endPoint);
	}

	private Quaternion GetRotation(Transform relativeTo)
	{
		if (_data.startPoint == _data.endPoint)
		{
			return relativeTo.rotation;
		}
		return relativeTo.rotation * Quaternion.LookRotation(LocalPerpendicularDir, LocalDirection);
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
		Vector3 normalized = Vector3.Cross(LocalPerpendicularDir, LocalDirection).normalized;
		Quaternion rotation = HandMirroring.Reflect(in pose.rotation, normalized);
		return new Pose(pose.position, rotation);
	}

	private Vector3 PointAltitude(Vector3 point, Transform relativeTo)
	{
		Vector3 startPoint = GetStartPoint(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		return startPoint + Vector3.Project(point - startPoint, direction);
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
		CylinderGrabSurface cylinderGrabSurface = gameObject.AddComponent<CylinderGrabSurface>();
		cylinderGrabSurface._data = _data.Mirror();
		return cylinderGrabSurface;
	}

	public IGrabSurface CreateDuplicatedSurface(GameObject gameObject)
	{
		CylinderGrabSurface cylinderGrabSurface = gameObject.AddComponent<CylinderGrabSurface>();
		cylinderGrabSurface._data = _data.Clone() as CylinderSurfaceData;
		return cylinderGrabSurface;
	}

	protected Vector3 NearestPointInSurface(Vector3 targetPosition, Transform relativeTo)
	{
		Vector3 startPoint = GetStartPoint(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		Vector3 vector = Vector3.Project(targetPosition - startPoint, direction);
		float height = GetHeight(relativeTo);
		if (vector.magnitude > height)
		{
			vector = vector.normalized * height;
		}
		if (Vector3.Dot(vector, direction) < 0f)
		{
			vector = Vector3.zero;
		}
		Vector3 vector2 = startPoint + vector;
		Vector3 vector3 = Vector3.ProjectOnPlane(targetPosition - vector2, direction).normalized;
		Vector3 startArcDir = GetStartArcDir(relativeTo);
		float num = Mathf.Repeat(Vector3.SignedAngle(startArcDir, vector3, direction), 360f);
		if (num > ArcLength)
		{
			vector3 = ((!(Mathf.Abs(num - ArcLength) >= Mathf.Abs(360f - num))) ? GetEndArcDir(relativeTo) : startArcDir);
		}
		return vector2 + vector3 * GetRadius(relativeTo);
	}

	public bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 startPoint = GetStartPoint(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		Vector3 lhs = startPoint - targetRay.origin;
		float num = Vector3.Dot(targetRay.direction, direction);
		float num2 = Vector3.Dot(lhs, targetRay.direction);
		float num3 = Vector3.Dot(lhs, direction);
		float num4 = 1f / (num * num - 1f);
		float num5 = (num * num3 - num2) * num4;
		float num6 = (num3 - num * num2) * num4;
		float radius = GetRadius(relativeTo);
		Vector3 vector = targetRay.origin + targetRay.direction * num5;
		Vector3 a = startPoint + direction * num6;
		float num7 = Mathf.Max(Vector3.Distance(a, vector) - radius);
		if (num7 < radius)
		{
			float num8 = Mathf.Sqrt(radius * radius - num7 * num7);
			vector -= targetRay.direction * num8;
		}
		Vector3 position = NearestPointInSurface(vector, relativeTo);
		Pose userPose = new Pose(position, referencePose.rotation);
		bestPose = MinimalTranslationPoseAtSurface(in userPose, relativeTo);
		return true;
	}

	protected Pose MinimalRotationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		Quaternion rotation = GetRotation(relativeTo);
		float radius = GetRadius(relativeTo);
		Vector3 position = userPose.position;
		Quaternion rotation2 = userPose.rotation;
		Quaternion rotation3 = referencePose.rotation;
		Vector3 normalized = Vector3.ProjectOnPlane(rotation2 * Quaternion.Inverse(rotation3) * rotation * Vector3.forward, direction).normalized;
		Vector3 vector = PointAltitude(position, relativeTo);
		Vector3 vector2 = NearestPointInSurface(vector + normalized * radius, relativeTo);
		Quaternion rotation4 = CalculateRotationOffset(vector2, relativeTo) * rotation3;
		return new Pose(vector2, rotation4);
	}

	protected Pose MinimalTranslationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 position = userPose.position;
		Quaternion rotation = referencePose.rotation;
		Vector3 vector = NearestPointInSurface(position, relativeTo);
		Quaternion rotation2 = CalculateRotationOffset(vector, relativeTo) * rotation;
		return new Pose(vector, rotation2);
	}

	protected Quaternion CalculateRotationOffset(Vector3 surfacePoint, Transform relativeTo)
	{
		Vector3 startPoint = GetStartPoint(relativeTo);
		Vector3 direction = GetDirection(relativeTo);
		Vector3 fromDirection = Vector3.ProjectOnPlane(GetPerpendicularDir(relativeTo), direction);
		Vector3 toDirection = Vector3.ProjectOnPlane(surfacePoint - startPoint, direction);
		return Quaternion.FromToRotation(fromDirection, toDirection);
	}

	public void InjectAllCylinderSurface(CylinderSurfaceData data, Transform relativeTo)
	{
		InjectData(data);
		InjectRelativeTo(relativeTo);
	}

	public void InjectData(CylinderSurfaceData data)
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
