using System;

namespace g3;

public class DistLine3Ray3
{
	private Line3d line;

	private Ray3d ray;

	public double DistanceSquared = -1.0;

	public Vector3d LineClosest;

	public double LineParameter;

	public Vector3d RayClosest;

	public double RayParameter;

	public Line3d Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
			DistanceSquared = -1.0;
		}
	}

	public Ray3d Ray
	{
		get
		{
			return ray;
		}
		set
		{
			ray = value;
			DistanceSquared = -1.0;
		}
	}

	public DistLine3Ray3(Ray3d rayIn, Line3d LineIn)
	{
		ray = rayIn;
		line = LineIn;
	}

	public static double MinDistance(Ray3d r, Line3d s)
	{
		return new DistLine3Ray3(r, s).Get();
	}

	public static double MinDistanceLineParam(Ray3d r, Line3d s)
	{
		return new DistLine3Ray3(r, s).Compute().LineParameter;
	}

	public DistLine3Ray3 Compute()
	{
		GetSquared();
		return this;
	}

	public double Get()
	{
		return Math.Sqrt(GetSquared());
	}

	public double GetSquared()
	{
		if (DistanceSquared >= 0.0)
		{
			return DistanceSquared;
		}
		Vector3d vector3d = line.Origin - ray.Origin;
		double num = 0.0 - line.Direction.Dot(ray.Direction);
		double num2 = vector3d.Dot(line.Direction);
		double lengthSquared = vector3d.LengthSquared;
		double num3 = Math.Abs(1.0 - num * num);
		double num7;
		double num5;
		double num8;
		if (num3 >= 1E-08)
		{
			double num4 = 0.0 - vector3d.Dot(ray.Direction);
			num5 = num * num2 - num4;
			if (num5 >= 0.0)
			{
				double num6 = 1.0 / num3;
				num7 = (num * num4 - num2) * num6;
				num5 *= num6;
				num8 = num7 * (num7 + num * num5 + 2.0 * num2) + num5 * (num * num7 + num5 + 2.0 * num4) + lengthSquared;
			}
			else
			{
				num7 = 0.0 - num2;
				num5 = 0.0;
				num8 = num2 * num7 + lengthSquared;
			}
		}
		else
		{
			num7 = 0.0 - num2;
			num5 = 0.0;
			num8 = num2 * num7 + lengthSquared;
		}
		LineClosest = line.Origin + num7 * line.Direction;
		RayClosest = ray.Origin + num5 * ray.Direction;
		LineParameter = num7;
		RayParameter = num5;
		if (num8 < 0.0)
		{
			num8 = 0.0;
		}
		DistanceSquared = num8;
		return num8;
	}
}
