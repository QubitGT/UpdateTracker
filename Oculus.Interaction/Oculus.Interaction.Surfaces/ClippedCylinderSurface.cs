using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class ClippedCylinderSurface : MonoBehaviour, IClippedSurface<ICylinderClipper>, ISurfacePatch, ISurface
{
	private const float EPSILON = 0.0001f;

	[Tooltip("The Cylinder Surface to be clipped.")]
	[SerializeField]
	private CylinderSurface _cylinderSurface;

	[Tooltip("The clippers that will be used to clip the Cylinder Surface.")]
	[SerializeField]
	[Interface(typeof(ICylinderClipper), new Type[] { })]
	private List<UnityEngine.Object> _clippers = new List<UnityEngine.Object>();

	private List<ICylinderClipper> Clippers { get; set; }

	public Transform Transform => _cylinderSurface.Transform;

	public ISurface BackingSurface => _cylinderSurface;

	public Cylinder Cylinder => _cylinderSurface.Cylinder;

	public IReadOnlyList<ICylinderClipper> GetClippers()
	{
		if (Clippers != null)
		{
			return Clippers;
		}
		return _clippers.ConvertAll((UnityEngine.Object clipper) => clipper as ICylinderClipper);
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (BackingSurface.Raycast(in ray, out hit, maxDistance))
		{
			Vector3 point = hit.Point;
			if (ClosestSurfacePoint(in point, out var hit2))
			{
				return hit.Point.Approximately(hit2.Point, 0.0001f);
			}
		}
		return false;
	}

	protected virtual void Awake()
	{
		Clippers = _clippers.ConvertAll((UnityEngine.Object clipper) => clipper as ICylinderClipper);
	}

	protected virtual void Start()
	{
	}

	public bool GetClipped(out CylinderSegment clipped)
	{
		bool flag = false;
		bool flag2 = true;
		float num = float.MinValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MaxValue;
		IReadOnlyList<ICylinderClipper> clippers = GetClippers();
		for (int i = 0; i < clippers.Count; i++)
		{
			ICylinderClipper cylinderClipper = clippers[i];
			if (cylinderClipper != null && cylinderClipper.GetCylinderSegment(out var segment))
			{
				flag = true;
				float b = segment.Rotation - segment.ArcDegrees / 2f;
				float b2 = segment.Rotation + segment.ArcDegrees / 2f;
				num = Mathf.Max(num, b);
				num2 = Mathf.Min(num2, b2);
				if (!segment.IsInfiniteHeight)
				{
					flag2 = false;
					num3 = Mathf.Max(num3, segment.Bottom);
					num4 = Mathf.Min(num4, segment.Top);
				}
			}
		}
		if (!flag)
		{
			clipped = CylinderSegment.Infinite();
			return true;
		}
		if (num > num2 || (!flag2 && num3 > num4))
		{
			clipped = default(CylinderSegment);
			return false;
		}
		float rotation = Mathf.Lerp(num, num2, 0.5f) % 360f;
		if (flag2)
		{
			num3 = 1f;
			num4 = -1f;
		}
		clipped = new CylinderSegment(rotation, num2 - num, num3, num4);
		return true;
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance = 0f)
	{
		if (!GetClipped(out var clipped))
		{
			hit = default(SurfaceHit);
			return false;
		}
		Vector3 vector;
		Vector3 vector2 = (vector = Cylinder.transform.InverseTransformPoint(point));
		if (!clipped.IsInfiniteHeight)
		{
			vector.y = Mathf.Clamp(vector.y, clipped.Bottom, clipped.Top);
		}
		Vector3 vector3;
		if (!clipped.IsInfiniteArc)
		{
			float num = Mathf.Atan2(vector.x, vector.z) * 57.29578f % 360f;
			float num2 = clipped.Rotation % 360f;
			if (num > num2 + 180f)
			{
				num -= 360f;
			}
			else if (num < num2 - 180f)
			{
				num += 360f;
			}
			num = Mathf.Clamp(num, num2 - clipped.ArcDegrees / 2f, num2 + clipped.ArcDegrees / 2f);
			vector.x = Mathf.Sin(num * (MathF.PI / 180f)) * Cylinder.Radius;
			vector.z = Mathf.Cos(num * (MathF.PI / 180f)) * Cylinder.Radius;
			vector3 = new Vector3(0f, vector.y, 0f);
		}
		else
		{
			vector3 = new Vector3(0f, vector.y, 0f);
			vector = Vector3.MoveTowards(maxDistanceDelta: Vector3.Distance(vector, vector3) - Cylinder.Radius, current: vector, target: vector3);
		}
		bool flag = (vector2 - vector3).magnitude > Cylinder.Radius;
		Vector3 vector4 = (vector3 - vector).normalized;
		switch (_cylinderSurface.Facing)
		{
		default:
			vector4 = (flag ? (-vector4) : vector4);
			break;
		case CylinderSurface.NormalFacing.Out:
			vector4 = -vector4;
			break;
		case CylinderSurface.NormalFacing.In:
			break;
		}
		hit = default(SurfaceHit);
		hit.Point = Cylinder.transform.TransformPoint(vector);
		hit.Distance = Vector3.Distance(point, hit.Point);
		hit.Normal = Cylinder.transform.TransformDirection(vector4).normalized;
		if (!(maxDistance <= 0f))
		{
			return hit.Distance <= maxDistance;
		}
		return true;
	}

	public void InjectAllClippedCylinderSurface(CylinderSurface surface, IEnumerable<ICylinderClipper> clippers)
	{
		InjectCylinderSurface(surface);
		InjectClippers(clippers);
	}

	public void InjectCylinderSurface(CylinderSurface surface)
	{
		_cylinderSurface = surface;
	}

	public void InjectClippers(IEnumerable<ICylinderClipper> clippers)
	{
		_clippers = new List<UnityEngine.Object>(clippers.Select((ICylinderClipper c) => c as UnityEngine.Object));
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
