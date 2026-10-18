using System;

namespace g3;

public class DistPoint3Triangle3
{
	private Vector3d point;

	private Triangle3d triangle;

	public double DistanceSquared = -1.0;

	public Vector3d TriangleClosest;

	public Vector3d TriangleBaryCoords;

	public Vector3d Point
	{
		get
		{
			return point;
		}
		set
		{
			point = value;
			DistanceSquared = -1.0;
		}
	}

	public Triangle3d Triangle
	{
		get
		{
			return triangle;
		}
		set
		{
			triangle = value;
			DistanceSquared = -1.0;
		}
	}

	public DistPoint3Triangle3(Vector3d PointIn, Triangle3d TriangleIn)
	{
		point = PointIn;
		triangle = TriangleIn;
	}

	public DistPoint3Triangle3 Compute()
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
		DistanceSquared = DistanceSqr(ref point, ref triangle, out TriangleClosest, out TriangleBaryCoords);
		return DistanceSquared;
	}

	public static double DistanceSqr(ref Vector3d point, ref Triangle3d triangle, out Vector3d closestPoint, out Vector3d baryCoords)
	{
		Vector3d vector3d = triangle.V0 - point;
		Vector3d v = triangle.V1 - triangle.V0;
		Vector3d v2 = triangle.V2 - triangle.V0;
		double lengthSquared = v.LengthSquared;
		double num = v.Dot(ref v2);
		double lengthSquared2 = v2.LengthSquared;
		double num2 = vector3d.Dot(ref v);
		double num3 = vector3d.Dot(ref v2);
		double lengthSquared3 = vector3d.LengthSquared;
		double num4 = Math.Abs(lengthSquared * lengthSquared2 - num * num);
		double num5 = num * num3 - lengthSquared2 * num2;
		double num6 = num * num2 - lengthSquared * num3;
		double val;
		if (num5 + num6 <= num4)
		{
			if (num5 < 0.0)
			{
				if (num6 < 0.0)
				{
					if (num2 < 0.0)
					{
						num6 = 0.0;
						if (0.0 - num2 >= lengthSquared)
						{
							num5 = 1.0;
							val = lengthSquared + 2.0 * num2 + lengthSquared3;
						}
						else
						{
							num5 = (0.0 - num2) / lengthSquared;
							val = num2 * num5 + lengthSquared3;
						}
					}
					else
					{
						num5 = 0.0;
						if (num3 >= 0.0)
						{
							num6 = 0.0;
							val = lengthSquared3;
						}
						else if (0.0 - num3 >= lengthSquared2)
						{
							num6 = 1.0;
							val = lengthSquared2 + 2.0 * num3 + lengthSquared3;
						}
						else
						{
							num6 = (0.0 - num3) / lengthSquared2;
							val = num3 * num6 + lengthSquared3;
						}
					}
				}
				else
				{
					num5 = 0.0;
					if (num3 >= 0.0)
					{
						num6 = 0.0;
						val = lengthSquared3;
					}
					else if (0.0 - num3 >= lengthSquared2)
					{
						num6 = 1.0;
						val = lengthSquared2 + 2.0 * num3 + lengthSquared3;
					}
					else
					{
						num6 = (0.0 - num3) / lengthSquared2;
						val = num3 * num6 + lengthSquared3;
					}
				}
			}
			else if (num6 < 0.0)
			{
				num6 = 0.0;
				if (num2 >= 0.0)
				{
					num5 = 0.0;
					val = lengthSquared3;
				}
				else if (0.0 - num2 >= lengthSquared)
				{
					num5 = 1.0;
					val = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = (0.0 - num2) / lengthSquared;
					val = num2 * num5 + lengthSquared3;
				}
			}
			else
			{
				double num7 = 1.0 / num4;
				num5 *= num7;
				num6 *= num7;
				val = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
			}
		}
		else if (num5 < 0.0)
		{
			double num8 = num + num2;
			double num9 = lengthSquared2 + num3;
			if (num9 > num8)
			{
				double num10 = num9 - num8;
				double num11 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num10 >= num11)
				{
					num5 = 1.0;
					num6 = 0.0;
					val = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = num10 / num11;
					num6 = 1.0 - num5;
					val = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
			else
			{
				num5 = 0.0;
				if (num9 <= 0.0)
				{
					num6 = 1.0;
					val = lengthSquared2 + 2.0 * num3 + lengthSquared3;
				}
				else if (num3 >= 0.0)
				{
					num6 = 0.0;
					val = lengthSquared3;
				}
				else
				{
					num6 = (0.0 - num3) / lengthSquared2;
					val = num3 * num6 + lengthSquared3;
				}
			}
		}
		else if (num6 < 0.0)
		{
			double num8 = num + num3;
			double num9 = lengthSquared + num2;
			if (num9 > num8)
			{
				double num10 = num9 - num8;
				double num11 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num10 >= num11)
				{
					num6 = 1.0;
					num5 = 0.0;
					val = lengthSquared2 + 2.0 * num3 + lengthSquared3;
				}
				else
				{
					num6 = num10 / num11;
					num5 = 1.0 - num6;
					val = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
			else
			{
				num6 = 0.0;
				if (num9 <= 0.0)
				{
					num5 = 1.0;
					val = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else if (num2 >= 0.0)
				{
					num5 = 0.0;
					val = lengthSquared3;
				}
				else
				{
					num5 = (0.0 - num2) / lengthSquared;
					val = num2 * num5 + lengthSquared3;
				}
			}
		}
		else
		{
			double num10 = lengthSquared2 + num3 - num - num2;
			if (num10 <= 0.0)
			{
				num5 = 0.0;
				num6 = 1.0;
				val = lengthSquared2 + 2.0 * num3 + lengthSquared3;
			}
			else
			{
				double num11 = lengthSquared - 2.0 * num + lengthSquared2;
				if (num10 >= num11)
				{
					num5 = 1.0;
					num6 = 0.0;
					val = lengthSquared + 2.0 * num2 + lengthSquared3;
				}
				else
				{
					num5 = num10 / num11;
					num6 = 1.0 - num5;
					val = num5 * (lengthSquared * num5 + num * num6 + 2.0 * num2) + num6 * (num * num5 + lengthSquared2 * num6 + 2.0 * num3) + lengthSquared3;
				}
			}
		}
		closestPoint = triangle.V0 + num5 * v + num6 * v2;
		baryCoords = new Vector3d(1.0 - num5 - num6, num5, num6);
		return Math.Max(val, 0.0);
	}
}
