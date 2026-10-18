using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class AxisAlignedBox : MonoBehaviour, ISurface
{
	private enum BoxSurface
	{
		XMin,
		YMin,
		ZMin,
		XMax,
		YMax,
		ZMax
	}

	[SerializeField]
	[Tooltip("Size of the axis-aligned box, default to mesh size")]
	private Vector3 _size = new Vector3(0f, 0f, 0f);

	private readonly Dictionary<BoxSurface, float> _distances = new Dictionary<BoxSurface, float>
	{
		{
			BoxSurface.XMin,
			0f
		},
		{
			BoxSurface.YMin,
			0f
		},
		{
			BoxSurface.ZMin,
			0f
		},
		{
			BoxSurface.XMax,
			0f
		},
		{
			BoxSurface.YMax,
			0f
		},
		{
			BoxSurface.ZMax,
			0f
		}
	};

	public Vector3 Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	public Transform Transform => base.transform;

	public Bounds Bounds => new Bounds(base.transform.position, _size);

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		Vector3 vector = Vector3.Min(Vector3.Max(point, Bounds.min), Bounds.max);
		BoxSurface boxSurface = FindClosestBoxSide(point);
		hit.Normal = ClosestSurfaceNormal(point, boxSurface);
		if (!IsWithinVolume(point))
		{
			hit.Point = vector;
			hit.Distance = (point - vector).magnitude;
			if (!(maxDistance <= 0f))
			{
				return hit.Distance <= maxDistance;
			}
			return true;
		}
		switch (boxSurface)
		{
		case BoxSurface.XMin:
			vector.x = Bounds.min.x;
			break;
		case BoxSurface.YMin:
			vector.y = Bounds.min.y;
			break;
		case BoxSurface.ZMin:
			vector.z = Bounds.min.z;
			break;
		case BoxSurface.XMax:
			vector.x = Bounds.max.x;
			break;
		case BoxSurface.YMax:
			vector.y = Bounds.max.y;
			break;
		case BoxSurface.ZMax:
			vector.z = Bounds.max.z;
			break;
		}
		hit.Point = vector;
		hit.Distance = Vector3.Distance(hit.Point, point);
		if (maxDistance > 0f && hit.Distance > maxDistance)
		{
			return false;
		}
		return true;
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		Vector3 b = new Vector3(1f / ray.direction.x, 1f / ray.direction.y, 1f / ray.direction.z);
		Vector3 vector = Vector3.Scale(Bounds.min - ray.origin, b);
		Vector3 vector2 = Vector3.Scale(Bounds.max - ray.origin, b);
		float num = Mathf.Max(Mathf.Max(Mathf.Min(vector.x, vector2.x), Mathf.Min(vector.y, vector2.y)), Mathf.Min(vector.z, vector2.z));
		float num2 = Mathf.Min(Mathf.Min(Mathf.Max(vector.x, vector2.x), Mathf.Max(vector.y, vector2.y)), Mathf.Max(vector.z, vector2.z));
		if (num2 < 0f)
		{
			hit.Distance = num2;
			return false;
		}
		if (num > num2)
		{
			hit.Distance = num2;
			return false;
		}
		hit.Distance = num;
		if (maxDistance > 0f && hit.Distance > maxDistance)
		{
			return false;
		}
		if (Mathf.Sign(num) != Mathf.Sign(num2))
		{
			hit.Distance = Mathf.Max(num2, num);
		}
		hit.Point = ray.origin + ray.direction * hit.Distance;
		hit.Normal = ClosestSurfaceNormal(hit.Point);
		return true;
	}

	protected void Start()
	{
		if ((bool)GetComponent<MeshFilter>())
		{
			_size = Vector3.Scale(base.transform.localScale, GetComponent<MeshFilter>().mesh.bounds.size);
		}
		if (_size.magnitude == 0f)
		{
			_size = new Vector3(0.1f, 0.1f, 0.1f);
		}
		Size = _size;
	}

	private bool IsWithinVolume(Vector3 point)
	{
		return Bounds.Contains(point);
	}

	private BoxSurface FindClosestBoxSide(Vector3 point)
	{
		Vector3 vector = base.transform.position - point;
		Vector3 extents = Bounds.extents;
		_distances[BoxSurface.XMin] = extents.x - vector.x;
		_distances[BoxSurface.YMin] = extents.y - vector.y;
		_distances[BoxSurface.ZMin] = extents.z - vector.z;
		_distances[BoxSurface.XMax] = extents.x + vector.x;
		_distances[BoxSurface.YMax] = extents.y + vector.y;
		_distances[BoxSurface.ZMax] = extents.z + vector.z;
		BoxSurface boxSurface = BoxSurface.XMin;
		foreach (BoxSurface key in _distances.Keys)
		{
			if (_distances[key] < _distances[boxSurface])
			{
				boxSurface = key;
			}
		}
		return boxSurface;
	}

	private Vector3 ClosestSurfaceNormal(Vector3 point, BoxSurface? side = null)
	{
		return (side ?? FindClosestBoxSide(point)) switch
		{
			BoxSurface.XMin => new Vector3(-1f, 0f, 0f), 
			BoxSurface.YMin => new Vector3(0f, -1f, 0f), 
			BoxSurface.ZMin => new Vector3(0f, 0f, -1f), 
			BoxSurface.XMax => new Vector3(1f, 0f, 0f), 
			BoxSurface.YMax => new Vector3(0f, 1f, 0f), 
			BoxSurface.ZMax => new Vector3(0f, 0f, 1f), 
			_ => throw new NotImplementedException(), 
		};
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
