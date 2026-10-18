using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class BoxGrabSurface : MonoBehaviour, IGrabSurface
{
	[SerializeField]
	protected BoxGrabSurfaceData _data = new BoxGrabSurfaceData();

	[SerializeField]
	[Tooltip("Transform used as a reference to measure the local data of the grab surface")]
	private Transform _relativeTo;

	private Pose RelativePose => PoseUtils.DeltaScaled(_relativeTo, base.transform);

	public Pose GetReferencePose(Transform relativeTo)
	{
		return PoseUtils.GlobalPoseScaled(relativeTo, RelativePose);
	}

	public float GetWidthOffset(Transform relativeTo)
	{
		return _data.widthOffset * relativeTo.lossyScale.x;
	}

	public void SetWidthOffset(float widthOffset, Transform relativeTo)
	{
		_data.widthOffset = widthOffset / relativeTo.lossyScale.x;
	}

	public Vector4 GetSnapOffset(Transform relativeTo)
	{
		return _data.snapOffset * relativeTo.lossyScale.x;
	}

	public void SetSnapOffset(Vector4 snapOffset, Transform relativeTo)
	{
		_data.snapOffset = snapOffset / relativeTo.lossyScale.x;
	}

	public Vector3 GetSize(Transform relativeTo)
	{
		return _data.size * relativeTo.lossyScale.x;
	}

	public void SetSize(Vector3 size, Transform relativeTo)
	{
		_data.size = size / relativeTo.lossyScale.x;
	}

	public Quaternion GetRotation(Transform relativeTo)
	{
		return relativeTo.rotation * Quaternion.Euler(_data.eulerAngles);
	}

	public void SetRotation(Quaternion rotation, Transform relativeTo)
	{
		_data.eulerAngles = (Quaternion.Inverse(relativeTo.rotation) * rotation).eulerAngles;
	}

	public Vector3 GetDirection(Transform relativeTo)
	{
		return GetRotation(relativeTo) * Vector3.forward;
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
		Vector3 normal = Quaternion.Euler(_data.eulerAngles) * Vector3.right;
		Quaternion rotation = HandMirroring.Reflect(in pose.rotation, normal);
		return new Pose(pose.position, rotation);
	}

	public IGrabSurface CreateMirroredSurface(GameObject gameObject)
	{
		BoxGrabSurface boxGrabSurface = gameObject.AddComponent<BoxGrabSurface>();
		boxGrabSurface._data = _data.Mirror();
		return boxGrabSurface;
	}

	public IGrabSurface CreateDuplicatedSurface(GameObject gameObject)
	{
		BoxGrabSurface boxGrabSurface = gameObject.AddComponent<BoxGrabSurface>();
		boxGrabSurface._data = _data;
		return boxGrabSurface;
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

	private void CalculateCorners(out Vector3 bottomLeft, out Vector3 bottomRight, out Vector3 topLeft, out Vector3 topRight, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Vector3 size = GetSize(relativeTo);
		float widthOffset = GetWidthOffset(relativeTo);
		Vector3 vector = GetRotation(relativeTo) * Vector3.right;
		bottomLeft = referencePose.position - vector * size.x * (1f - widthOffset);
		bottomRight = referencePose.position + vector * size.x * widthOffset;
		Vector3 vector2 = GetRotation(relativeTo) * Vector3.forward * size.z;
		topLeft = bottomLeft + vector2;
		topRight = bottomRight + vector2;
	}

	private Vector3 ProjectOnSegment(Vector3 point, (Vector3, Vector3) segment)
	{
		Vector3 vector = segment.Item2 - segment.Item1;
		Vector3 vector2 = Vector3.Project(point - segment.Item1, vector);
		if (Vector3.Dot(vector2, vector) < 0f)
		{
			return segment.Item1;
		}
		if (vector2.magnitude > vector.magnitude)
		{
			return segment.Item2;
		}
		return vector2 + segment.Item1;
	}

	public bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		new Plane(GetRotation(relativeTo) * Vector3.up, base.transform.position).Raycast(targetRay, out var enter);
		Vector3 targetPosition = targetRay.origin + targetRay.direction * enter;
		Vector3 position = NearestPointInSurface(targetPosition, relativeTo);
		Pose userPose = new Pose(position, referencePose.rotation);
		bestPose = MinimalTranslationPoseAtSurface(in userPose, relativeTo);
		return true;
	}

	protected Vector3 NearestPointInSurface(Vector3 targetPosition, Transform relativeTo)
	{
		NearestPointAndAngleInSurface(targetPosition, out var surfacePoint, out var _, relativeTo);
		return surfacePoint;
	}

	private void NearestPointAndAngleInSurface(Vector3 targetPosition, out Vector3 surfacePoint, out float angle, Transform relativeTo)
	{
		Quaternion rotation = GetRotation(relativeTo);
		Vector4 snapOffset = GetSnapOffset(relativeTo);
		Vector3 vector = rotation * Vector3.right;
		Vector3 vector2 = rotation * Vector3.forward;
		CalculateCorners(out var bottomLeft, out var bottomRight, out var topLeft, out var topRight, relativeTo);
		Vector3 vector3 = ProjectOnSegment(targetPosition, (bottomLeft + vector * snapOffset.y, bottomRight + vector * snapOffset.x));
		Vector3 vector4 = ProjectOnSegment(targetPosition, (topLeft - vector * snapOffset.x, topRight - vector * snapOffset.y));
		Vector3 vector5 = ProjectOnSegment(targetPosition, (bottomLeft - vector2 * snapOffset.z, topLeft - vector2 * snapOffset.w));
		Vector3 vector6 = ProjectOnSegment(targetPosition, (bottomRight + vector2 * snapOffset.w, topRight + vector2 * snapOffset.z));
		float sqrMagnitude = (vector3 - targetPosition).sqrMagnitude;
		float sqrMagnitude2 = (vector4 - targetPosition).sqrMagnitude;
		float sqrMagnitude3 = (vector5 - targetPosition).sqrMagnitude;
		float sqrMagnitude4 = (vector6 - targetPosition).sqrMagnitude;
		float num = Mathf.Min(sqrMagnitude, Mathf.Min(sqrMagnitude2, Mathf.Min(sqrMagnitude3, sqrMagnitude4)));
		if (sqrMagnitude == num)
		{
			surfacePoint = vector3;
			angle = 0f;
		}
		else if (sqrMagnitude2 == num)
		{
			surfacePoint = vector4;
			angle = 180f;
		}
		else if (sqrMagnitude3 == num)
		{
			surfacePoint = vector5;
			angle = 90f;
		}
		else
		{
			surfacePoint = vector6;
			angle = -90f;
		}
	}

	protected Pose MinimalRotationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Quaternion rotation = GetRotation(relativeTo);
		Pose referencePose = GetReferencePose(relativeTo);
		Vector4 snapOffset = GetSnapOffset(relativeTo);
		Vector3 position = userPose.position;
		Quaternion rotation2 = referencePose.rotation;
		Quaternion to = userPose.rotation;
		Vector3 axis = rotation * Vector3.up;
		Quaternion from = rotation2;
		Quaternion from2 = Quaternion.AngleAxis(180f, axis) * rotation2;
		Quaternion from3 = Quaternion.AngleAxis(90f, axis) * rotation2;
		Quaternion from4 = Quaternion.AngleAxis(-90f, axis) * rotation2;
		float num = RotationalScore(in from, in to);
		float num2 = RotationalScore(in from2, in to);
		float num3 = RotationalScore(in from3, in to);
		float b = RotationalScore(in from4, in to);
		Vector3 vector = rotation * Vector3.right;
		Vector3 vector2 = rotation * Vector3.forward;
		CalculateCorners(out var bottomLeft, out var bottomRight, out var topLeft, out var topRight, relativeTo);
		float num4 = Mathf.Max(num, Mathf.Max(num2, Mathf.Max(num3, b)));
		if (num == num4)
		{
			return new Pose(ProjectOnSegment(position, (bottomLeft + vector * snapOffset.y, bottomRight + vector * snapOffset.x)), from);
		}
		if (num2 == num4)
		{
			return new Pose(ProjectOnSegment(position, (topLeft - vector * snapOffset.x, topRight - vector * snapOffset.y)), from2);
		}
		if (num3 == num4)
		{
			return new Pose(ProjectOnSegment(position, (bottomLeft - vector2 * snapOffset.z, topLeft - vector2 * snapOffset.w)), from3);
		}
		return new Pose(ProjectOnSegment(position, (bottomRight + vector2 * snapOffset.w, topRight + vector2 * snapOffset.z)), from4);
	}

	protected Pose MinimalTranslationPoseAtSurface(in Pose userPose, Transform relativeTo)
	{
		Pose referencePose = GetReferencePose(relativeTo);
		Quaternion rotation = GetRotation(relativeTo);
		Vector3 position = userPose.position;
		Quaternion rotation2 = referencePose.rotation;
		NearestPointAndAngleInSurface(position, out var surfacePoint, out var angle, relativeTo);
		Quaternion rotation3 = Quaternion.AngleAxis(angle, rotation * Vector3.up) * rotation2;
		return new Pose(surfacePoint, rotation3);
	}

	private static float RotationalScore(in Quaternion from, in Quaternion to)
	{
		float num = Vector3.Dot(from * Vector3.forward, to * Vector3.forward) * 0.5f + 0.5f;
		float num2 = Vector3.Dot(from * Vector3.up, to * Vector3.up) * 0.5f + 0.5f;
		return num * num2;
	}

	public void InjectAllBoxSurface(BoxGrabSurfaceData data, Transform relativeTo)
	{
		InjectData(data);
		InjectRelativeTo(relativeTo);
	}

	public void InjectData(BoxGrabSurfaceData data)
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
