using System;

namespace g3;

public class IntrRay3AxisAlignedBox3
{
	private Ray3d ray;

	private AxisAlignedBox3d box;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public double RayParam0;

	public double RayParam1;

	public Vector3d Point0 = Vector3d.Zero;

	public Vector3d Point1 = Vector3d.Zero;

	public Ray3d Ray
	{
		get
		{
			return ray;
		}
		set
		{
			ray = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public AxisAlignedBox3d Box
	{
		get
		{
			return box;
		}
		set
		{
			box = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public bool IsSimpleIntersection
	{
		get
		{
			if (Result == IntersectionResult.Intersects)
			{
				return Type == IntersectionType.Point;
			}
			return false;
		}
	}

	public IntrRay3AxisAlignedBox3(Ray3d r, AxisAlignedBox3d b)
	{
		ray = r;
		box = b;
	}

	public IntrRay3AxisAlignedBox3 Compute()
	{
		Find();
		return this;
	}

	public bool Find()
	{
		if (Result != 0)
		{
			return Result == IntersectionResult.Intersects;
		}
		if (!ray.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		RayParam0 = 0.0;
		RayParam1 = double.MaxValue;
		IntrLine3AxisAlignedBox3.DoClipping(ref RayParam0, ref RayParam1, ref ray.Origin, ref ray.Direction, ref box, solid: true, ref Quantity, ref Point0, ref Point1, ref Type);
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public bool Test()
	{
		return Intersects(ref ray, ref box);
	}

	public static bool Intersects(ref Ray3d ray, ref AxisAlignedBox3d box, double expandExtents = 0.0)
	{
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		Vector3d zero3 = Vector3d.Zero;
		Vector3d zero4 = Vector3d.Zero;
		Vector3d v = ray.Origin - box.Center;
		Vector3d vector3d = box.Extents + expandExtents;
		zero.x = ray.Direction.x;
		zero2.x = Math.Abs(zero.x);
		zero3.x = v.x;
		zero4.x = Math.Abs(zero3.x);
		if (zero4.x > vector3d.x && zero3.x * zero.x >= 0.0)
		{
			return false;
		}
		zero.y = ray.Direction.y;
		zero2.y = Math.Abs(zero.y);
		zero3.y = v.y;
		zero4.y = Math.Abs(zero3.y);
		if (zero4.y > vector3d.y && zero3.y * zero.y >= 0.0)
		{
			return false;
		}
		zero.z = ray.Direction.z;
		zero2.z = Math.Abs(zero.z);
		zero3.z = v.z;
		zero4.z = Math.Abs(zero3.z);
		if (zero4.z > vector3d.z && zero3.z * zero.z >= 0.0)
		{
			return false;
		}
		Vector3d vector3d2 = ray.Direction.Cross(v);
		Vector3d zero5 = Vector3d.Zero;
		zero5.x = Math.Abs(vector3d2.x);
		double num = vector3d.y * zero2.z + vector3d.z * zero2.y;
		if (zero5.x > num)
		{
			return false;
		}
		zero5.y = Math.Abs(vector3d2.y);
		num = vector3d.x * zero2.z + vector3d.z * zero2.x;
		if (zero5.y > num)
		{
			return false;
		}
		zero5.z = Math.Abs(vector3d2.z);
		num = vector3d.x * zero2.y + vector3d.y * zero2.x;
		if (zero5.z > num)
		{
			return false;
		}
		return true;
	}

	public static bool FindRayIntersectT(ref Ray3d ray, ref AxisAlignedBox3d box, out double RayParam)
	{
		double t = 0.0;
		double t2 = double.MaxValue;
		int quantity = 0;
		Vector3d point = Vector3d.Zero;
		Vector3d point2 = Vector3d.Zero;
		IntersectionType intrType = IntersectionType.Empty;
		IntrLine3AxisAlignedBox3.DoClipping(ref t, ref t2, ref ray.Origin, ref ray.Direction, ref box, solid: true, ref quantity, ref point, ref point2, ref intrType);
		if (intrType != 0)
		{
			RayParam = t;
			return true;
		}
		RayParam = double.MaxValue;
		return false;
	}
}
