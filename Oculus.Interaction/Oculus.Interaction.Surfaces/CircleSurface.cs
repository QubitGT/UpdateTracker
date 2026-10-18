using System;
using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class CircleSurface : MonoBehaviour, ISurfacePatch, ISurface
{
	[Tooltip("The circle will lay upon this plane, with the circle's center at the plane surface's origin.")]
	[SerializeField]
	private PlaneSurface _planeSurface;

	[Tooltip("The radius of the circle.")]
	[SerializeField]
	private float _radius = 0.1f;

	public Transform Transform => _planeSurface.Transform;

	public ISurface BackingSurface => _planeSurface;

	protected virtual void Start()
	{
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (!_planeSurface.Raycast(in ray, out hit, maxDistance))
		{
			return false;
		}
		return Vector3.SqrMagnitude(Transform.InverseTransformPoint(hit.Point)) <= _radius * _radius;
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (!_planeSurface.ClosestSurfacePoint(in point, out hit, maxDistance))
		{
			return false;
		}
		Vector3 position = Vector3.ClampMagnitude(Transform.InverseTransformPoint(hit.Point), _radius);
		hit.Distance = Vector3.Distance(b: hit.Point = Transform.TransformPoint(position), a: point);
		if (!(maxDistance <= 0f))
		{
			return hit.Distance <= maxDistance;
		}
		return true;
	}

	[Obsolete("Use InjectAllCircleSurface instead.")]
	public void InjectAllCircleProximityField(PlaneSurface planeSurface)
	{
		InjectAllCircleSurface(planeSurface);
	}

	public void InjectAllCircleSurface(PlaneSurface planeSurface)
	{
		InjectPlaneSurface(planeSurface);
	}

	public void InjectPlaneSurface(PlaneSurface planeSurface)
	{
		_planeSurface = planeSurface;
	}

	public void InjectOptionalRadius(float radius)
	{
		_radius = radius;
	}

	bool ISurface.Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		return Raycast(in ray, out hit, maxDistance);
	}

	bool ISurface.ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		return ClosestSurfacePoint(in point, out hit, maxDistance);
	}
}
