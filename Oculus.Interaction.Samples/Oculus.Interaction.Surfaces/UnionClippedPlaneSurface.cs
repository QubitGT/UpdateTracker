using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Oculus.Interaction.Surfaces;

public class UnionClippedPlaneSurface : MonoBehaviour, IClippedSurface<IBoundsClipper>, ISurfacePatch, ISurface
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

	[Obsolete("Use the non-alloc version instead")]
	public List<Bounds> GetLocalBounds()
	{
		List<Bounds> clipBounds = new List<Bounds>();
		GetLocalBoundsNonAlloc(ref clipBounds);
		return clipBounds;
	}

	public void GetLocalBoundsNonAlloc(ref List<Bounds> clipBounds)
	{
		IReadOnlyList<IBoundsClipper> clippers = GetClippers();
		for (int i = 0; i < clippers.Count; i++)
		{
			IBoundsClipper boundsClipper = clippers[i];
			if (boundsClipper != null && boundsClipper.GetLocalBounds(Transform, out var bounds))
			{
				clipBounds.Add(bounds);
			}
		}
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
		if (!_planeSurface.ClosestSurfacePoint(in point, out hit, maxDistance))
		{
			return false;
		}
		List<Bounds> clipBounds = CollectionPool<List<Bounds>, Bounds>.Get();
		GetLocalBoundsNonAlloc(ref clipBounds);
		List<(Vector3, float)> list = new List<(Vector3, float)>();
		foreach (Bounds item in clipBounds)
		{
			Bounds bounds = item;
			Vector3 size = bounds.size;
			size.z = 1E-05f;
			bounds.size = size;
			Vector3 point2 = hit.Point;
			Vector3 vector = ClampPoint(in point2, in bounds);
			float num = Vector3.Distance(point, vector);
			if (maxDistance <= 0f || num <= maxDistance)
			{
				list.Add((vector, num));
			}
		}
		CollectionPool<List<Bounds>, Bounds>.Release(clipBounds);
		clipBounds = null;
		if (list.Count == 0)
		{
			return false;
		}
		(Vector3, float) tuple = list[0];
		for (int i = 1; i < list.Count; i++)
		{
			(Vector3, float) tuple2 = list[i];
			if (tuple2.Item2 < tuple.Item2)
			{
				tuple = tuple2;
			}
		}
		(hit.Point, hit.Distance) = tuple;
		return true;
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (BackingSurface.Raycast(in ray, out hit, maxDistance))
		{
			List<Bounds> clipBounds = CollectionPool<List<Bounds>, Bounds>.Get();
			GetLocalBoundsNonAlloc(ref clipBounds);
			foreach (Bounds item in clipBounds)
			{
				if (item.Contains(Transform.InverseTransformPoint(hit.Point)))
				{
					CollectionPool<List<Bounds>, Bounds>.Release(clipBounds);
					return true;
				}
			}
			CollectionPool<List<Bounds>, Bounds>.Release(clipBounds);
			return false;
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
