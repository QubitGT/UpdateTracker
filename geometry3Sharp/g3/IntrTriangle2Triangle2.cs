using System;

namespace g3;

public class IntrTriangle2Triangle2
{
	private Triangle2d triangle0;

	private Triangle2d triangle1;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d[] Points;

	public Triangle2d Triangle0
	{
		get
		{
			return triangle0;
		}
		set
		{
			triangle0 = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Triangle2d Triangle1
	{
		get
		{
			return triangle1;
		}
		set
		{
			triangle1 = value;
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

	public IntrTriangle2Triangle2(Triangle2d t0, Triangle2d t1)
	{
		triangle0 = t0;
		triangle1 = t1;
		Points = null;
	}

	public bool Test()
	{
		Vector2d zero = Vector2d.Zero;
		int num = 0;
		int key = 2;
		while (num < 3)
		{
			zero.x = triangle0[num].y - triangle0[key].y;
			zero.y = triangle0[key].x - triangle0[num].x;
			if (WhichSide(triangle1, triangle0[key], zero) > 0)
			{
				return false;
			}
			key = num++;
		}
		num = 0;
		key = 2;
		while (num < 3)
		{
			zero.x = triangle1[num].y - triangle1[key].y;
			zero.y = triangle1[key].x - triangle1[num].x;
			if (WhichSide(triangle0, triangle1[key], zero) > 0)
			{
				return false;
			}
			key = num++;
		}
		return true;
	}

	public IntrTriangle2Triangle2 Compute()
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
		Quantity = 3;
		Points = new Vector2d[6];
		for (int i = 0; i < 3; i++)
		{
			Points[i] = triangle1[i];
		}
		int key = 2;
		int num = 0;
		while (num < 3)
		{
			Vector2d n = new Vector2d(triangle0[key].y - triangle0[num].y, triangle0[num].x - triangle0[key].x);
			double c = n.Dot(triangle0[key]);
			ClipConvexPolygonAgainstLine(n, c, ref Quantity, ref Points);
			if (Quantity == 0)
			{
				Type = IntersectionType.Empty;
			}
			else if (Quantity == 1)
			{
				Type = IntersectionType.Point;
			}
			else if (Quantity == 2)
			{
				Type = IntersectionType.Segment;
			}
			else
			{
				Type = IntersectionType.Polygon;
			}
			key = num++;
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public static int WhichSide(Triangle2d V, Vector2d P, Vector2d D)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < 3; i++)
		{
			double num4 = D.Dot(V[i] - P);
			if (num4 > 0.0)
			{
				num++;
			}
			else if (num4 < 0.0)
			{
				num2++;
			}
			else
			{
				num3++;
			}
			if (num > 0 && num2 > 0)
			{
				return 0;
			}
		}
		if (num3 != 0)
		{
			return 0;
		}
		if (num <= 0)
		{
			return -1;
		}
		return 1;
	}

	public static void ClipConvexPolygonAgainstLine(Vector2d N, double c, ref int quantity, ref Vector2d[] V)
	{
		int num = 0;
		int num2 = 0;
		int num3 = -1;
		double[] array = new double[6];
		for (int i = 0; i < quantity; i++)
		{
			array[i] = N.Dot(V[i]) - c;
			if (array[i] > 0.0)
			{
				num++;
				if (num3 < 0)
				{
					num3 = i;
				}
			}
			else if (array[i] < 0.0)
			{
				num2++;
			}
		}
		if (num > 0)
		{
			if (num2 <= 0)
			{
				return;
			}
			Vector2d[] array2 = new Vector2d[6];
			int num4 = 0;
			if (num3 > 0)
			{
				int num5 = num3;
				int num6 = num5 - 1;
				double num7 = array[num5] / (array[num5] - array[num6]);
				array2[num4++] = V[num5] + num7 * (V[num6] - V[num5]);
				while (num5 < quantity && array[num5] > 0.0)
				{
					array2[num4++] = V[num5++];
				}
				if (num5 < quantity)
				{
					num6 = num5 - 1;
				}
				else
				{
					num5 = 0;
					num6 = quantity - 1;
				}
				num7 = array[num5] / (array[num5] - array[num6]);
				array2[num4++] = V[num5] + num7 * (V[num6] - V[num5]);
			}
			else
			{
				int num5 = 0;
				while (num5 < quantity && array[num5] > 0.0)
				{
					array2[num4++] = V[num5++];
				}
				int num6 = num5 - 1;
				double num7 = array[num5] / (array[num5] - array[num6]);
				array2[num4++] = V[num5] + num7 * (V[num6] - V[num5]);
				for (; num5 < quantity && array[num5] <= 0.0; num5++)
				{
				}
				if (num5 < quantity)
				{
					num6 = num5 - 1;
					num7 = array[num5] / (array[num5] - array[num6]);
					array2[num4++] = V[num5] + num7 * (V[num6] - V[num5]);
					while (num5 < quantity && array[num5] > 0.0)
					{
						array2[num4++] = V[num5++];
					}
				}
				else
				{
					num6 = quantity - 1;
					num7 = array[0] / (array[0] - array[num6]);
					array2[num4++] = V[0] + num7 * (V[num6] - V[0]);
				}
			}
			quantity = num4;
			Array.Copy(array2, V, num4);
		}
		else
		{
			quantity = 0;
		}
	}
}
