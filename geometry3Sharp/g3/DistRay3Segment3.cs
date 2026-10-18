using System;

namespace g3;

public class DistRay3Segment3
{
	private Ray3d ray;

	private Segment3d segment;

	public double DistanceSquared = -1.0;

	public Vector3d RayClosest;

	public double RayParameter;

	public Vector3d SegmentClosest;

	public double SegmentParameter;

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

	public Segment3d Segment
	{
		get
		{
			return segment;
		}
		set
		{
			segment = value;
			DistanceSquared = -1.0;
		}
	}

	public DistRay3Segment3(Ray3d rayIn, Segment3d segmentIn)
	{
		ray = rayIn;
		segment = segmentIn;
	}

	public static double MinDistance(Ray3d r, Segment3d s)
	{
		double rayT;
		double segT;
		return Math.Sqrt(SquaredDistance(ref r, ref s, out rayT, out segT));
	}

	public static double MinDistanceSegmentParam(Ray3d r, Segment3d s)
	{
		SquaredDistance(ref r, ref s, out var _, out var segT);
		return segT;
	}

	public DistRay3Segment3 Compute()
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
		Vector3d vector3d = ray.Origin - segment.Center;
		double num = 0.0 - ray.Direction.Dot(segment.Direction);
		double num2 = vector3d.Dot(ray.Direction);
		double num3 = 0.0 - vector3d.Dot(segment.Direction);
		double lengthSquared = vector3d.LengthSquared;
		double num4 = Math.Abs(1.0 - num * num);
		double num5;
		double num6;
		double num9;
		if (num4 >= 1E-08)
		{
			num5 = num * num3 - num2;
			num6 = num * num2 - num3;
			double num7 = segment.Extent * num4;
			if (num5 >= 0.0)
			{
				if (num6 >= 0.0 - num7)
				{
					if (num6 <= num7)
					{
						double num8 = 1.0 / num4;
						num5 *= num8;
						num6 *= num8;
						num9 = num5 * (num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num6 = segment.Extent;
						num5 = 0.0 - (num * num6 + num2);
						if (num5 > 0.0)
						{
							num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else
						{
							num5 = 0.0;
							num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
					}
				}
				else
				{
					num6 = 0.0 - segment.Extent;
					num5 = 0.0 - (num * num6 + num2);
					if (num5 > 0.0)
					{
						num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num5 = 0.0;
						num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
				}
			}
			else if (num6 <= 0.0 - num7)
			{
				num5 = 0.0 - ((0.0 - num) * segment.Extent + num2);
				if (num5 > 0.0)
				{
					num6 = 0.0 - segment.Extent;
					num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else
				{
					num5 = 0.0;
					num6 = 0.0 - num3;
					if (num6 < 0.0 - segment.Extent)
					{
						num6 = 0.0 - segment.Extent;
					}
					else if (num6 > segment.Extent)
					{
						num6 = segment.Extent;
					}
					num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
			}
			else if (num6 <= num7)
			{
				num5 = 0.0;
				num6 = 0.0 - num3;
				if (num6 < 0.0 - segment.Extent)
				{
					num6 = 0.0 - segment.Extent;
				}
				else if (num6 > segment.Extent)
				{
					num6 = segment.Extent;
				}
				num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
			else
			{
				num5 = 0.0 - (num * segment.Extent + num2);
				if (num5 > 0.0)
				{
					num6 = segment.Extent;
					num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else
				{
					num5 = 0.0;
					num6 = 0.0 - num3;
					if (num6 < 0.0 - segment.Extent)
					{
						num6 = 0.0 - segment.Extent;
					}
					else if (num6 > segment.Extent)
					{
						num6 = segment.Extent;
					}
					num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
			}
		}
		else
		{
			num6 = ((!(num > 0.0)) ? segment.Extent : (0.0 - segment.Extent));
			num5 = 0.0 - (num * num6 + num2);
			if (num5 > 0.0)
			{
				num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
			else
			{
				num5 = 0.0;
				num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
		}
		RayClosest = ray.Origin + num5 * ray.Direction;
		SegmentClosest = segment.Center + num6 * segment.Direction;
		RayParameter = num5;
		SegmentParameter = num6;
		if (num9 < 0.0)
		{
			num9 = 0.0;
		}
		DistanceSquared = num9;
		return DistanceSquared;
	}

	public static double SquaredDistance(ref Ray3d ray, ref Segment3d segment, out double rayT, out double segT)
	{
		Vector3d vector3d = ray.Origin - segment.Center;
		double num = 0.0 - ray.Direction.Dot(segment.Direction);
		double num2 = vector3d.Dot(ray.Direction);
		double num3 = 0.0 - vector3d.Dot(segment.Direction);
		double lengthSquared = vector3d.LengthSquared;
		double num4 = Math.Abs(1.0 - num * num);
		double num5;
		double num6;
		double num9;
		if (num4 >= 1E-08)
		{
			num5 = num * num3 - num2;
			num6 = num * num2 - num3;
			double num7 = segment.Extent * num4;
			if (num5 >= 0.0)
			{
				if (num6 >= 0.0 - num7)
				{
					if (num6 <= num7)
					{
						double num8 = 1.0 / num4;
						num5 *= num8;
						num6 *= num8;
						num9 = num5 * (num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num6 = segment.Extent;
						num5 = 0.0 - (num * num6 + num2);
						if (num5 > 0.0)
						{
							num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else
						{
							num5 = 0.0;
							num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
					}
				}
				else
				{
					num6 = 0.0 - segment.Extent;
					num5 = 0.0 - (num * num6 + num2);
					if (num5 > 0.0)
					{
						num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num5 = 0.0;
						num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
				}
			}
			else if (num6 <= 0.0 - num7)
			{
				num5 = 0.0 - ((0.0 - num) * segment.Extent + num2);
				if (num5 > 0.0)
				{
					num6 = 0.0 - segment.Extent;
					num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else
				{
					num5 = 0.0;
					num6 = 0.0 - num3;
					if (num6 < 0.0 - segment.Extent)
					{
						num6 = 0.0 - segment.Extent;
					}
					else if (num6 > segment.Extent)
					{
						num6 = segment.Extent;
					}
					num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
			}
			else if (num6 <= num7)
			{
				num5 = 0.0;
				num6 = 0.0 - num3;
				if (num6 < 0.0 - segment.Extent)
				{
					num6 = 0.0 - segment.Extent;
				}
				else if (num6 > segment.Extent)
				{
					num6 = segment.Extent;
				}
				num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
			else
			{
				num5 = 0.0 - (num * segment.Extent + num2);
				if (num5 > 0.0)
				{
					num6 = segment.Extent;
					num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else
				{
					num5 = 0.0;
					num6 = 0.0 - num3;
					if (num6 < 0.0 - segment.Extent)
					{
						num6 = 0.0 - segment.Extent;
					}
					else if (num6 > segment.Extent)
					{
						num6 = segment.Extent;
					}
					num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
			}
		}
		else
		{
			num6 = ((!(num > 0.0)) ? segment.Extent : (0.0 - segment.Extent));
			num5 = 0.0 - (num * num6 + num2);
			if (num5 > 0.0)
			{
				num9 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
			else
			{
				num5 = 0.0;
				num9 = num6 * (num6 + 2.0 * num3) + lengthSquared;
			}
		}
		rayT = num5;
		segT = num6;
		if (num9 < 0.0)
		{
			num9 = 0.0;
		}
		return num9;
	}
}
