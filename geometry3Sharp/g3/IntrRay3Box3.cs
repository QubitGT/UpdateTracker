using System;

namespace g3;

public class IntrRay3Box3
{
	private Ray3d ray;

	private Box3d box;

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

	public Box3d Box
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

	public IntrRay3Box3(Ray3d r, Box3d b)
	{
		ray = r;
		box = b;
	}

	public IntrRay3Box3 Compute()
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
		IntrLine3Box3.DoClipping(ref RayParam0, ref RayParam1, ray.Origin, ray.Direction, box, solid: true, ref Quantity, ref Point0, ref Point1, ref Type);
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public bool Test()
	{
		return Intersects(ref ray, ref box);
	}

	public static bool Intersects(ref Ray3d ray, ref Box3d box, double expandExtents = 0.0)
	{
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		Vector3d zero3 = Vector3d.Zero;
		Vector3d zero4 = Vector3d.Zero;
		Vector3d zero5 = Vector3d.Zero;
		Vector3d v = ray.Origin - box.Center;
		Vector3d vector3d = box.Extent + expandExtents;
		zero[0] = ray.Direction.Dot(ref box.AxisX);
		zero2[0] = Math.Abs(zero[0]);
		zero3[0] = v.Dot(ref box.AxisX);
		zero4[0] = Math.Abs(zero3[0]);
		if (zero4[0] > vector3d.x && zero3[0] * zero[0] >= 0.0)
		{
			return false;
		}
		zero[1] = ray.Direction.Dot(ref box.AxisY);
		zero2[1] = Math.Abs(zero[1]);
		zero3[1] = v.Dot(ref box.AxisY);
		zero4[1] = Math.Abs(zero3[1]);
		if (zero4[1] > vector3d.y && zero3[1] * zero[1] >= 0.0)
		{
			return false;
		}
		zero[2] = ray.Direction.Dot(ref box.AxisZ);
		zero2[2] = Math.Abs(zero[2]);
		zero3[2] = v.Dot(ref box.AxisZ);
		zero4[2] = Math.Abs(zero3[2]);
		if (zero4[2] > vector3d.z && zero3[2] * zero[2] >= 0.0)
		{
			return false;
		}
		Vector3d vector3d2 = ray.Direction.Cross(v);
		zero5[0] = Math.Abs(vector3d2.Dot(ref box.AxisX));
		double num = vector3d.y * zero2[2] + vector3d.z * zero2[1];
		if (zero5[0] > num)
		{
			return false;
		}
		zero5[1] = Math.Abs(vector3d2.Dot(ref box.AxisY));
		num = vector3d.x * zero2[2] + vector3d.z * zero2[0];
		if (zero5[1] > num)
		{
			return false;
		}
		zero5[2] = Math.Abs(vector3d2.Dot(ref box.AxisZ));
		num = vector3d.x * zero2[1] + vector3d.y * zero2[0];
		if (zero5[2] > num)
		{
			return false;
		}
		return true;
	}
}
