using System;

namespace g3;

public class DistRay3Ray3
{
	private Ray3d ray1;

	private Ray3d ray2;

	public double DistanceSquared = -1.0;

	public Vector3d Ray1Closest;

	public double Ray1Parameter;

	public Vector3d Ray2Closest;

	public double Ray2Parameter;

	public Ray3d Ray1
	{
		get
		{
			return ray1;
		}
		set
		{
			ray1 = value;
			DistanceSquared = -1.0;
		}
	}

	public Ray3d Ray2
	{
		get
		{
			return ray2;
		}
		set
		{
			ray2 = value;
			DistanceSquared = -1.0;
		}
	}

	public DistRay3Ray3(Ray3d ray1, Ray3d ray2)
	{
		this.ray1 = ray1;
		this.ray2 = ray2;
	}

	public static double MinDistance(Ray3d r1, Ray3d r2)
	{
		return new DistRay3Ray3(r1, r2).Get();
	}

	public static double MinDistanceRay2Param(Ray3d r1, Ray3d r2)
	{
		return new DistRay3Ray3(r1, r2).Compute().Ray2Parameter;
	}

	public DistRay3Ray3 Compute()
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
		Vector3d vector3d = ray1.Origin - ray2.Origin;
		double num = 0.0 - ray1.Direction.Dot(ray2.Direction);
		double num2 = vector3d.Dot(ray1.Direction);
		double lengthSquared = vector3d.LengthSquared;
		double num3 = Math.Abs(1.0 - num * num);
		double num5;
		double num6;
		double num8;
		if (num3 >= 1E-08)
		{
			double num4 = 0.0 - vector3d.Dot(ray2.Direction);
			num5 = num * num4 - num2;
			num6 = num * num2 - num4;
			if (num5 >= 0.0)
			{
				if (num6 >= 0.0)
				{
					double num7 = 1.0 / num3;
					num5 *= num7;
					num6 *= num7;
					num8 = num5 * (num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + num6 + 2.0 * num4) + lengthSquared;
				}
				else
				{
					num6 = 0.0;
					if (num2 >= 0.0)
					{
						num5 = 0.0;
						num8 = lengthSquared;
					}
					else
					{
						num5 = 0.0 - num2;
						num8 = num2 * num5 + lengthSquared;
					}
				}
			}
			else if (num6 >= 0.0)
			{
				num5 = 0.0;
				if (num4 >= 0.0)
				{
					num6 = 0.0;
					num8 = lengthSquared;
				}
				else
				{
					num6 = 0.0 - num4;
					num8 = num4 * num6 + lengthSquared;
				}
			}
			else if (num2 < 0.0)
			{
				num5 = 0.0 - num2;
				num6 = 0.0;
				num8 = num2 * num5 + lengthSquared;
			}
			else
			{
				num5 = 0.0;
				if (num4 >= 0.0)
				{
					num6 = 0.0;
					num8 = lengthSquared;
				}
				else
				{
					num6 = 0.0 - num4;
					num8 = num4 * num6 + lengthSquared;
				}
			}
		}
		else if (num > 0.0)
		{
			num6 = 0.0;
			if (num2 >= 0.0)
			{
				num5 = 0.0;
				num8 = lengthSquared;
			}
			else
			{
				num5 = 0.0 - num2;
				num8 = num2 * num5 + lengthSquared;
			}
		}
		else if (num2 >= 0.0)
		{
			double num4 = 0.0 - vector3d.Dot(ray2.Direction);
			num5 = 0.0;
			num6 = 0.0 - num4;
			num8 = num4 * num6 + lengthSquared;
		}
		else
		{
			num5 = 0.0 - num2;
			num6 = 0.0;
			num8 = num2 * num5 + lengthSquared;
		}
		Ray1Closest = ray1.Origin + num5 * ray1.Direction;
		Ray2Closest = ray2.Origin + num6 * ray2.Direction;
		Ray1Parameter = num5;
		Ray2Parameter = num6;
		if (num8 < 0.0)
		{
			num8 = 0.0;
		}
		DistanceSquared = num8;
		return num8;
	}
}
