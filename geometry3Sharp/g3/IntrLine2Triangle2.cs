using System;

namespace g3;

public class IntrLine2Triangle2
{
	private Line2d line;

	private Triangle2d triangle;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d Point0;

	public Vector2d Point1;

	public double Param0;

	public double Param1;

	public Line2d Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Triangle2d Triangle
	{
		get
		{
			return triangle;
		}
		set
		{
			triangle = value;
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

	public IntrLine2Triangle2(Line2d l, Triangle2d t)
	{
		line = l;
		triangle = t;
	}

	public IntrLine2Triangle2 Compute()
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
		if (!line.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		Vector3d dist = Vector3d.Zero;
		Vector3i sign = Vector3i.Zero;
		int positive = 0;
		int negative = 0;
		int zero = 0;
		TriangleLineRelations(line.Origin, line.Direction, triangle, ref dist, ref sign, ref positive, ref negative, ref zero);
		if (positive == 3 || negative == 3)
		{
			Quantity = 0;
			Type = IntersectionType.Empty;
		}
		else
		{
			Vector2d param = Vector2d.Zero;
			GetInterval(line.Origin, line.Direction, triangle, dist, sign, ref param);
			Intersector1 intersector = new Intersector1(param[0], param[1], double.MinValue, double.MaxValue);
			intersector.Find();
			Quantity = intersector.NumIntersections;
			if (Quantity == 2)
			{
				Type = IntersectionType.Segment;
				Param0 = intersector.GetIntersection(0);
				Point0 = line.Origin + Param0 * line.Direction;
				Param1 = intersector.GetIntersection(1);
				Point1 = line.Origin + Param1 * line.Direction;
			}
			else if (Quantity == 1)
			{
				Type = IntersectionType.Point;
				Param0 = intersector.GetIntersection(0);
				Point0 = line.Origin + Param0 * line.Direction;
			}
			else
			{
				Type = IntersectionType.Empty;
			}
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public static void TriangleLineRelations(Vector2d origin, Vector2d direction, Triangle2d tri, ref Vector3d dist, ref Vector3i sign, ref int positive, ref int negative, ref int zero)
	{
		positive = 0;
		negative = 0;
		zero = 0;
		for (int i = 0; i < 3; i++)
		{
			dist[i] = (tri[i] - origin).DotPerp(direction);
			if (dist[i] > 1E-08)
			{
				sign[i] = 1;
				positive++;
			}
			else if (dist[i] < -1E-08)
			{
				sign[i] = -1;
				negative++;
			}
			else
			{
				dist[i] = 0.0;
				sign[i] = 0;
				zero++;
			}
		}
	}

	public static void GetInterval(Vector2d origin, Vector2d direction, Triangle2d tri, Vector3d dist, Vector3i sign, ref Vector2d param)
	{
		Vector3d zero = Vector3d.Zero;
		for (int i = 0; i < 3; i++)
		{
			Vector2d v = tri[i] - origin;
			zero[i] = direction.Dot(v);
		}
		int num = 0;
		int key = 2;
		int num2 = 0;
		while (num2 < 3)
		{
			if (sign[key] * sign[num2] < 0)
			{
				if (num >= 2)
				{
					throw new Exception("IntrLine2Triangle2.GetInterval: too many intersections!");
				}
				double num3 = dist[key] * zero[num2] - dist[num2] * zero[key];
				double num4 = dist[key] - dist[num2];
				param[num++] = num3 / num4;
			}
			key = num2++;
		}
		if (num < 2)
		{
			key = 1;
			num2 = 2;
			int num5 = 0;
			while (num5 < 3)
			{
				if (sign[num5] == 0)
				{
					if (num >= 2)
					{
						throw new Exception("IntrLine2Triangle2.GetInterval: too many intersections!");
					}
					param[num++] = zero[num5];
				}
				key = num2;
				num2 = num5++;
			}
		}
		if (num < 1)
		{
			throw new Exception("IntrLine2Triangle2.GetInterval: need at least one intersection");
		}
		if (num == 2)
		{
			if (param[0] > param[1])
			{
				double value = param[0];
				param[0] = param[1];
				param[1] = value;
			}
		}
		else
		{
			param[1] = param[0];
		}
	}
}
