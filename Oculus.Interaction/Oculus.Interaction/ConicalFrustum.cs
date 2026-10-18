using System;
using UnityEngine;

namespace Oculus.Interaction;

public class ConicalFrustum : MonoBehaviour
{
	[SerializeField]
	[Min(0f)]
	private float _minLength;

	[SerializeField]
	[Min(0f)]
	private float _maxLength = 5f;

	[SerializeField]
	[Min(0f)]
	private float _radiusStart = 0.03f;

	[SerializeField]
	[Range(0f, 90f)]
	private float _apertureDegrees = 20f;

	public Pose Pose => base.transform.GetPose();

	public float MinLength
	{
		get
		{
			return _minLength;
		}
		set
		{
			_minLength = value;
		}
	}

	public float MaxLength
	{
		get
		{
			return _maxLength;
		}
		set
		{
			_maxLength = value;
		}
	}

	public float RadiusStart
	{
		get
		{
			return _radiusStart;
		}
		set
		{
			_radiusStart = value;
		}
	}

	public float ApertureDegrees
	{
		get
		{
			return _apertureDegrees;
		}
		set
		{
			_apertureDegrees = value;
		}
	}

	public Vector3 StartPoint => base.transform.position + Direction * MinLength;

	public Vector3 EndPoint => base.transform.position + Direction * MaxLength;

	public Vector3 Direction => base.transform.forward;

	public bool IsPointInConeFrustum(Vector3 point)
	{
		Vector3 vector = Vector3.Project(point - base.transform.position, Direction);
		if (Vector3.Dot(vector, Direction) < 0f)
		{
			return false;
		}
		float magnitude = vector.magnitude;
		if (magnitude < _minLength || magnitude > _maxLength)
		{
			return false;
		}
		return Vector3.Distance(Pose.position + vector, point) <= ConeFrustumRadiusAtLength(magnitude);
	}

	public float ConeFrustumRadiusAtLength(float length)
	{
		float b = _maxLength * Mathf.Tan(_apertureDegrees * (MathF.PI / 180f));
		float t = length / _maxLength;
		return Mathf.Lerp(_radiusStart, b, t);
	}

	public bool HitsCollider(Collider collider, out float score, out Vector3 point)
	{
		Vector3 center = collider.bounds.center;
		Vector3 position = Pose.position + Vector3.Project(center - Pose.position, Pose.forward);
		point = collider.ClosestPointOnBounds(position);
		if (!IsPointInConeFrustum(point))
		{
			score = 0f;
			return false;
		}
		float num = Vector3.Angle((point - Pose.position).normalized, Pose.forward);
		score = 1f - Mathf.Clamp01(num / _apertureDegrees);
		return true;
	}

	public Vector3 NearestColliderHit(Collider collider, out float score)
	{
		Vector3 center = collider.bounds.center;
		Vector3 position = Pose.position + Vector3.Project(center - Pose.position, Pose.forward);
		Vector3 vector = collider.ClosestPointOnBounds(position);
		float num = Vector3.Angle((vector - Pose.position).normalized, Pose.forward);
		score = 1f - Mathf.Clamp01(num / _apertureDegrees);
		return vector;
	}
}
