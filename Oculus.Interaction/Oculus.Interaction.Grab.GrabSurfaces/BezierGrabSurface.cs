using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

[Serializable]
public class BezierGrabSurface : MonoBehaviour, IGrabSurface
{
	[SerializeField]
	private List<BezierControlPoint> _controlPoints = new List<BezierControlPoint>();

	[SerializeField]
	[Tooltip("Transform used as a reference to measure the local data of the grab surface")]
	private Transform _relativeTo;

	private const float MAX_PLANE_DOT = 0.95f;

	public List<BezierControlPoint> ControlPoints => _controlPoints;

	protected virtual void Reset()
	{
		_relativeTo = GetComponentInParent<IRelativeToRef>()?.RelativeTo;
	}

	protected virtual void Start()
	{
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		Pose offset = Pose.identity;
		return CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose, in scoringModifier, relativeTo);
	}

	public GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo)
	{
		Pose bestPose2 = Pose.identity;
		_ = Pose.identity;
		bestPose = targetPose;
		GrabPoseScore grabPoseScore = GrabPoseScore.Max;
		for (int i = 0; i < _controlPoints.Count; i++)
		{
			BezierControlPoint bezierControlPoint = _controlPoints[i];
			BezierControlPoint bezierControlPoint2 = _controlPoints[(i + 1) % _controlPoints.Count];
			if (!bezierControlPoint.Disconnected && bezierControlPoint2.Disconnected)
			{
				continue;
			}
			GrabPoseScore grabPoseScore2;
			if ((bezierControlPoint.Disconnected && bezierControlPoint2.Disconnected) || _controlPoints.Count == 1)
			{
				Pose from = bezierControlPoint.GetPose(relativeTo);
				bestPose2.CopyFrom(in from);
				grabPoseScore2 = new GrabPoseScore(in targetPose, in bestPose2, in offset, scoringModifier);
			}
			else
			{
				Pose start = bezierControlPoint.GetPose(relativeTo);
				Pose end = bezierControlPoint2.GetPose(relativeTo);
				Vector3 tangent = bezierControlPoint.GetTangent(relativeTo);
				NearestPointInTriangle(targetPose.position, start.position, tangent, end.position, out var positionT);
				float rotationT = ProgressForRotation(targetPose.rotation, start.rotation, end.rotation);
				grabPoseScore2 = GrabPoseHelper.CalculateBestPoseAtSurface(in targetPose, in offset, out bestPose2, in scoringModifier, relativeTo, delegate
				{
					Pose result2 = default(Pose);
					result2.position = EvaluateBezier(start.position, tangent, end.position, positionT);
					result2.rotation = Quaternion.Slerp(start.rotation, end.rotation, positionT);
					return result2;
				}, delegate
				{
					Pose result = default(Pose);
					result.position = EvaluateBezier(start.position, tangent, end.position, rotationT);
					result.rotation = Quaternion.Slerp(start.rotation, end.rotation, rotationT);
					return result;
				});
			}
			if (grabPoseScore2.IsBetterThan(grabPoseScore))
			{
				grabPoseScore = grabPoseScore2;
				bestPose.CopyFrom(in bestPose2);
			}
		}
		return grabPoseScore;
	}

	public bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo)
	{
		Pose to = Pose.identity;
		Pose identity = Pose.identity;
		bestPose = Pose.identity;
		bool result = false;
		GrabPoseScore referenceScore = GrabPoseScore.Max;
		for (int i = 0; i < _controlPoints.Count; i++)
		{
			BezierControlPoint bezierControlPoint = _controlPoints[i];
			BezierControlPoint bezierControlPoint2 = _controlPoints[(i + 1) % _controlPoints.Count];
			if (!bezierControlPoint.Disconnected && bezierControlPoint2.Disconnected)
			{
				continue;
			}
			if ((bezierControlPoint.Disconnected && bezierControlPoint2.Disconnected) || _controlPoints.Count == 1)
			{
				Pose from = bezierControlPoint.GetPose(relativeTo);
				if (!new Plane(-targetRay.direction, from.position).Raycast(targetRay, out var enter))
				{
					continue;
				}
				identity.position = targetRay.GetPoint(enter);
				to.CopyFrom(in from);
			}
			else
			{
				Pose pose = bezierControlPoint.GetPose(relativeTo);
				Pose pose2 = bezierControlPoint2.GetPose(relativeTo);
				Vector3 tangent = bezierControlPoint.GetTangent(relativeTo);
				if (!GenerateRaycastPlane(pose.position, tangent, pose2.position, -targetRay.direction).Raycast(targetRay, out var enter2))
				{
					continue;
				}
				identity.position = targetRay.GetPoint(enter2);
				NearestPointInTriangle(identity.position, pose.position, tangent, pose2.position, out var t);
				to.position = EvaluateBezier(pose.position, tangent, pose2.position, t);
				to.rotation = Quaternion.Slerp(pose.rotation, pose2.rotation, t);
			}
			GrabPoseScore grabPoseScore = new GrabPoseScore(identity.position, to.position);
			if (grabPoseScore.IsBetterThan(referenceScore))
			{
				referenceScore = grabPoseScore;
				bestPose.CopyFrom(in to);
				result = true;
			}
		}
		return result;
	}

	private Plane GenerateRaycastPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 fallbackDir)
	{
		Vector3 normalized = (p1 - p0).normalized;
		Vector3 normalized2 = (p2 - p0).normalized;
		return (!(Mathf.Abs(Vector3.Dot(normalized, normalized2)) > 0.95f)) ? new Plane(p0, p1, p2) : new Plane(fallbackDir, (p0 + p2 + p1) / 3f);
	}

	private float ProgressForRotation(Quaternion targetRotation, Quaternion from, Quaternion to)
	{
		Vector3 from2 = targetRotation * Vector3.forward;
		Vector3 vector = from * Vector3.forward;
		Vector3 vector2 = to * Vector3.forward;
		Vector3 normalized = Vector3.Cross(vector, vector2).normalized;
		float num = Vector3.SignedAngle(from2, vector, normalized);
		float num2 = Vector3.SignedAngle(from2, vector2, normalized);
		if (num < 0f && num2 < 0f)
		{
			return 1f;
		}
		if (num > 0f && num2 > 0f)
		{
			return 0f;
		}
		return Mathf.Abs(num) / Vector3.Angle(vector, vector2);
	}

	private Vector3 NearestPointInTriangle(Vector3 point, Vector3 p0, Vector3 p1, Vector3 p2, out float t)
	{
		Vector3 vector = (p0 + p1 + p2) / 3f;
		float progress;
		Vector3 vector2 = NearestPointToSegment(point, p0, vector, out progress);
		float progress2;
		Vector3 vector3 = NearestPointToSegment(point, vector, p2, out progress2);
		float num = Vector3.Distance(p0, vector);
		float num2 = Vector3.Distance(p2, vector);
		float num3 = num2 / (num + num2);
		float sqrMagnitude = (vector2 - point).sqrMagnitude;
		float sqrMagnitude2 = (vector3 - point).sqrMagnitude;
		if (sqrMagnitude < sqrMagnitude2)
		{
			t = progress * num3;
			return vector2;
		}
		t = num3 + progress2 * (1f - num3);
		return vector3;
	}

	private Vector3 NearestPointToSegment(Vector3 point, Vector3 start, Vector3 end, out float progress)
	{
		Vector3 lhs = end - start;
		Vector3 vector = Vector3.Project(point - start, lhs.normalized);
		Vector3 result;
		if (Vector3.Dot(lhs, vector) <= 0f)
		{
			result = start;
			progress = 0f;
		}
		else if (vector.sqrMagnitude >= lhs.sqrMagnitude)
		{
			result = end;
			progress = 1f;
		}
		else
		{
			result = start + vector;
			progress = vector.magnitude / lhs.magnitude;
		}
		return result;
	}

	public IGrabSurface CreateDuplicatedSurface(GameObject gameObject)
	{
		BezierGrabSurface bezierGrabSurface = gameObject.AddComponent<BezierGrabSurface>();
		bezierGrabSurface._controlPoints = new List<BezierControlPoint>(_controlPoints);
		return bezierGrabSurface;
	}

	public IGrabSurface CreateMirroredSurface(GameObject gameObject)
	{
		BezierGrabSurface bezierGrabSurface = gameObject.AddComponent<BezierGrabSurface>();
		bezierGrabSurface._controlPoints = new List<BezierControlPoint>();
		foreach (BezierControlPoint controlPoint in _controlPoints)
		{
			Pose worldSpacePose = controlPoint.GetPose(_relativeTo);
			worldSpacePose.rotation *= Quaternion.Euler(180f, 180f, 0f);
			controlPoint.SetPose(in worldSpacePose, _relativeTo);
			bezierGrabSurface._controlPoints.Add(controlPoint);
		}
		return bezierGrabSurface;
	}

	public Pose MirrorPose(in Pose gripPose, Transform relativeTo)
	{
		return gripPose;
	}

	public static Vector3 EvaluateBezier(Vector3 start, Vector3 middle, Vector3 end, float t)
	{
		t = Mathf.Clamp01(t);
		float num = 1f - t;
		return num * num * start + 2f * num * t * middle + t * t * end;
	}

	public void InjectAllBezierSurface(List<BezierControlPoint> controlPoints, Transform relativeTo)
	{
		InjectControlPoints(controlPoints);
		InjectRelativeTo(relativeTo);
	}

	public void InjectControlPoints(List<BezierControlPoint> controlPoints)
	{
		_controlPoints = controlPoints;
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
