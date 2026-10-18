using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class CylinderSurface : MonoBehaviour, ISurface, IBounds
{
	public enum NormalFacing
	{
		Any,
		In,
		Out
	}

	[Tooltip("The cylinder that will drive this surface.")]
	[SerializeField]
	private Cylinder _cylinder;

	[Tooltip("The normal facing of the surface. Hits will be registered either on the outer or inner face of the cylinder depending on this value.")]
	[SerializeField]
	private NormalFacing _facing = NormalFacing.Out;

	[Tooltip("The height of the cylinder. If zero or negative, height will be infinite.")]
	[SerializeField]
	private float _height = 1f;

	protected bool _started;

	public bool IsValid
	{
		get
		{
			if (_cylinder != null)
			{
				return Radius > 0f;
			}
			return false;
		}
	}

	public float Radius => _cylinder.Radius;

	public Cylinder Cylinder => _cylinder;

	public Transform Transform => _cylinder.transform;

	public Bounds Bounds
	{
		get
		{
			float num = Mathf.Max(Transform.lossyScale.x, Mathf.Max(Transform.lossyScale.y, Transform.lossyScale.z)) * (Height + Radius);
			return new Bounds(Transform.position, new Vector3(num, num, num));
		}
	}

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

	public float Height
	{
		get
		{
			return _height;
		}
		set
		{
			_height = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public bool ClosestSurfacePoint(in Vector3 point, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		if (!IsValid)
		{
			return false;
		}
		Vector3 vector;
		Vector3 a = (vector = _cylinder.transform.InverseTransformPoint(point));
		if (_height > 0f)
		{
			vector.y = Mathf.Clamp(vector.y, (0f - _height) / 2f, _height / 2f);
		}
		Vector3 vector2 = Vector3.Project(vector, Vector3.up);
		Vector3 vector3 = ((vector == vector2) ? Vector3.forward : (vector - vector2).normalized);
		bool flag = (vector - vector2).magnitude > Radius;
		Vector3 b = vector2 + vector3 * Radius;
		float val = Vector3.Distance(a, b);
		if (maxDistance > 0f && TransformScale(val) > maxDistance)
		{
			return false;
		}
		Vector3 direction = _facing switch
		{
			NormalFacing.In => -vector3, 
			NormalFacing.Out => vector3, 
			_ => flag ? vector3 : (-vector3), 
		};
		hit.Point = _cylinder.transform.TransformPoint(vector2 + vector3 * Radius);
		hit.Normal = _cylinder.transform.TransformDirection(direction);
		hit.Distance = TransformScale(val);
		return true;
	}

	public bool Raycast(in Ray ray, out SurfaceHit hit, float maxDistance)
	{
		hit = default(SurfaceHit);
		if (!IsValid)
		{
			return false;
		}
		Ray ray2 = new Ray(_cylinder.transform.InverseTransformPoint(ray.origin), _cylinder.transform.InverseTransformDirection(ray.direction).normalized);
		Vector3 vector = ray2.origin;
		Vector3 origin = CancelY(in vector);
		Vector3 vector2 = ray2.direction;
		Ray ray3 = new Ray(origin, CancelY(in vector2).normalized);
		Vector3 vector3 = -ray3.origin;
		Vector3 vector4 = ray3.origin + Vector3.Project(vector3, ray3.direction);
		vector = ray2.direction;
		float num = Vector3.Magnitude(CancelY(in vector));
		float num2 = Vector3.Magnitude(vector4);
		bool flag = vector3.magnitude > Radius;
		NormalFacing normalFacing = ((_facing == NormalFacing.Any && !flag) ? NormalFacing.In : _facing);
		if (num2 > Radius || Mathf.Approximately(num, 0f) || (flag && Vector3.Dot(vector3, ray3.direction) < 0f) || (!flag && normalFacing == NormalFacing.Out))
		{
			return false;
		}
		float num3 = Mathf.Sqrt(Mathf.Pow(Radius, 2f) - Mathf.Pow(num2, 2f));
		float num4 = Vector3.Distance(ray3.origin, vector4 - ray3.direction * num3) / num;
		float num5 = Vector3.Distance(ray3.origin, vector4 + ray3.direction * num3) / num;
		Vector3 vector5 = ray2.GetPoint(num4);
		Vector3 point = ray2.GetPoint(num5);
		bool flag2 = (maxDistance <= 0f || TransformScale(num4) <= maxDistance) && (_height <= 0f || Mathf.Abs(vector5.y) <= _height / 2f);
		bool flag3 = (maxDistance <= 0f || TransformScale(num5) <= maxDistance) && (_height <= 0f || Mathf.Abs(point.y) <= _height / 2f);
		if (normalFacing != NormalFacing.In && flag2)
		{
			hit.Point = _cylinder.transform.TransformPoint(vector5);
			hit.Normal = _cylinder.transform.TransformDirection(CancelY(in vector5).normalized);
			hit.Distance = TransformScale(num4);
		}
		else
		{
			if (!flag3)
			{
				return false;
			}
			hit.Point = _cylinder.transform.TransformPoint(point);
			Transform obj = _cylinder.transform;
			vector = -point;
			hit.Normal = obj.TransformDirection(CancelY(in vector).normalized);
			hit.Distance = TransformScale(num5);
		}
		return true;
	}

	private float TransformScale(float val)
	{
		return val * _cylinder.transform.lossyScale.x;
	}

	private static Vector3 CancelY(in Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	public void InjectAllCylinderSurface(NormalFacing facing, Cylinder cylinder, float height)
	{
		InjectNormalFacing(facing);
		InjectCylinder(cylinder);
		InjectHeight(height);
	}

	public void InjectNormalFacing(NormalFacing facing)
	{
		_facing = facing;
	}

	public void InjectCylinder(Cylinder cylinder)
	{
		_cylinder = cylinder;
	}

	public void InjectHeight(float height)
	{
		_height = height;
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
