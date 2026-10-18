using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class ClippedPlaneSurface : MonoBehaviour, IClippedSurface<IBoundsClipper>, ISurfacePatch, ISurface
{
	private static readonly Bounds InfiniteBounds = new Bounds(Vector3.zero, Vector3.one * float.PositiveInfinity);

	private static readonly Bounds PlaneBounds = new Bounds(Vector3.zero, new Vector3(float.PositiveInfinity, float.PositiveInfinity, 1E-05f));

	[Tooltip("The Plane Surface to be clipped.")]
	[SerializeField]
	private PlaneSurface _planeSurface;

	[Tooltip("The clippers that will be used to clip the Plane Surface.")]
	[SerializeField]
	[Interface(typeof(IBoundsClipper), new Type[] { })]
	private List<UnityEngine.Object> _clippers = new List<UnityEngine.Object>();

	private List<IBoundsClipper> Clippers { get; set; }

	public ISurface BackingSurface => _planeSurface;

	public Transform Transform => _planeSurface.Transform;

	public IReadOnlyList<IBoundsClipper> GetClippers()
	{
		if (Clippers != null)
		{
			return Clippers;
		}
		return _clippers.ConvertAll((UnityEngine.Object clipper) => clipper as IBoundsClipper);
	}

	protected virtual void Awake()
	{
		Clippers = _clippers.ConvertAll((UnityEngine.Object clipper) => clipper as IBoundsClipper);
	}

	protected virtual void Start()
	{
	}

	public bool ClipBounds(in Bounds bounds, out Bounds clipped)
	{
		clipped = bounds;
		IReadOnlyList<IBoundsClipper> clippers = GetClippers();
		for (int i = 0; i < clippers.Count; i++)
		{
			IBoundsClipper boundsClipper = clippers[i];
			if (boundsClipper != null && boundsClipper.GetLocalBounds(Transform, out var bounds2) && !clipped.Clip(in bounds2, out clipped))
			{
				return false;
			}
		}
		return true;
	}

	private Vector3 ClampPoint(in Vector3 point, in Bounds bounds)
	{
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		Vector3 vector = Transform.InverseTransformPoint(point);
		Vector3 position = new Vector3(Mathf.Clamp(vector.x, min.x, max.x), Mathf.Clamp(vector.y, min.y, max.y), Mathf.Clamp(vector.z, min.z, max.z));
		return Transform.TransformPoint(position);
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (_planeSurface.ClosestSurfacePoint(in point, out hit, maxDistance) && ClipBounds(in PlaneBounds, out var clipped))
		{
			Vector3 point2 = hit.Point;
			hit.Point = ClampPoint(in point2, in clipped);
			hit.Distance = Vector3.Distance(point, hit.Point);
			if (!(maxDistance <= 0f))
			{
				return hit.Distance <= maxDistance;
			}
			return true;
		}
		return false;
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (BackingSurface.Raycast(in ray, out hit, maxDistance) && ClipBounds(in InfiniteBounds, out var clipped) && clipped.size != Vector3.zero)
		{
			return clipped.Contains(Transform.InverseTransformPoint(hit.Point));
		}
		return false;
	}

	public void InjectAllClippedPlaneSurface(PlaneSurface planeSurface, IEnumerable<IBoundsClipper> clippers)
	{
		InjectPlaneSurface(planeSurface);
		InjectClippers(clippers);
	}

	public void InjectPlaneSurface(PlaneSurface planeSurface)
	{
		_planeSurface = planeSurface;
	}

	public void InjectClippers(IEnumerable<IBoundsClipper> clippers)
	{
		_clippers = new List<UnityEngine.Object>(clippers.Select((IBoundsClipper c) => c as UnityEngine.Object));
		Clippers = clippers.ToList();
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
