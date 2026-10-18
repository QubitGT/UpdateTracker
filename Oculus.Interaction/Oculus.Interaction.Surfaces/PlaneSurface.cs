using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class PlaneSurface : MonoBehaviour, ISurface, IBounds
{
	public enum NormalFacing
	{
		Backward,
		Forward
	}

	[Tooltip("The normal facing of the surface. Hits will be registered either on the front or back of the plane depending on this value.")]
	[SerializeField]
	private NormalFacing _facing;

	[SerializeField]
	[Tooltip("Raycasts hit either side of plane, but hit normal will still respect plane facing.")]
	private bool _doubleSided;

	private static Vector3 _forward = Vector3.forward;

	private static Vector3 _back = Vector3.back;

	public NormalFacing Facing
	{
		get
		{
			return _facing;
		}
		set
		{
			_facing = value;
		}
	}

	public bool DoubleSided
	{
		get
		{
			return _doubleSided;
		}
		set
		{
			_doubleSided = value;
		}
	}

	public Vector3 Normal
	{
		get
		{
			if (_facing != NormalFacing.Forward)
			{
				return -base.transform.forward;
			}
			return base.transform.forward;
		}
	}

	public Transform Transform => base.transform;

	public Bounds Bounds
	{
		get
		{
			Vector3 normal = Normal;
			Vector3 size = new Vector3((Mathf.Abs(normal.x) == 1f) ? float.Epsilon : float.PositiveInfinity, (Mathf.Abs(normal.y) == 1f) ? float.Epsilon : float.PositiveInfinity, (Mathf.Abs(normal.z) == 1f) ? float.Epsilon : float.PositiveInfinity);
			return new Bounds(base.transform.position, size);
		}
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		GetPlaneParameters(out var planeNormal, out var planeDistance);
		float num = Vector3.Dot(planeNormal, point) + planeDistance;
		float num2 = Mathf.Abs(num);
		if (maxDistance > 0f && num2 > maxDistance)
		{
			return false;
		}
		hit.Point = point - planeNormal * num;
		hit.Distance = num2;
		hit.Normal = planeNormal.normalized;
		return true;
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		GetPlaneParameters(out var planeNormal, out var planeDistance);
		float originDistance = Vector3.Dot(planeNormal, ray.origin) + planeDistance;
		if (!_doubleSided && originDistance <= 0f)
		{
			return false;
		}
		if (Raycast(in ray, out var enter2))
		{
			if (maxDistance > 0f && enter2 > maxDistance)
			{
				return false;
			}
			hit.Point = ray.GetPoint(enter2);
			hit.Distance = enter2;
			hit.Normal = planeNormal.normalized;
			return true;
		}
		return false;
		bool Raycast(in Ray ray, out float enter)
		{
			float num = Vector3.Dot(ray.direction, planeNormal);
			if (Mathf.Approximately(num, 0f))
			{
				enter = 0f;
				return false;
			}
			enter = (0f - originDistance) / num;
			return enter > 0f;
		}
	}

	private void GetPlaneParameters(out Vector3 planeNormal, out float planeDistance)
	{
		base.transform.GetPositionAndRotation(out var position, out var rotation);
		planeNormal = rotation * ((_facing == NormalFacing.Forward) ? _forward : _back);
		planeDistance = 0f - Vector3.Dot(planeNormal, position);
	}

	public Plane GetPlane()
	{
		return new Plane(Normal, base.transform.position);
	}

	public void InjectAllPlaneSurface(NormalFacing facing, bool doubleSided)
	{
		InjectNormalFacing(facing);
		InjectDoubleSided(doubleSided);
	}

	public void InjectNormalFacing(NormalFacing facing)
	{
		_facing = facing;
	}

	public void InjectDoubleSided(bool doubleSided)
	{
		_doubleSided = doubleSided;
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
