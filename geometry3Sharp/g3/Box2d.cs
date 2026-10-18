using System;

namespace g3;

public struct Box2d
{
	public Vector2d Center;

	public Vector2d AxisX;

	public Vector2d AxisY;

	public Vector2d Extent;

	public static readonly Box2d Empty = new Box2d(Vector2d.Zero);

	public double MaxExtent => Math.Max(Extent.x, Extent.y);

	public double MinExtent => Math.Min(Extent.x, Extent.y);

	public Vector2d Diagonal => Extent.x * AxisX + Extent.y * AxisY - ((0.0 - Extent.x) * AxisX - Extent.y * AxisY);

	public double Area => 2.0 * Extent.x * 2.0 * Extent.y;

	public Box2d(Vector2d center)
	{
		Center = center;
		AxisX = Vector2d.AxisX;
		AxisY = Vector2d.AxisY;
		Extent = Vector2d.Zero;
	}

	public Box2d(Vector2d center, Vector2d x, Vector2d y, Vector2d extent)
	{
		Center = center;
		AxisX = x;
		AxisY = y;
		Extent = extent;
	}

	public Box2d(Vector2d center, Vector2d extent)
	{
		Center = center;
		Extent = extent;
		AxisX = Vector2d.AxisX;
		AxisY = Vector2d.AxisY;
	}

	public Box2d(AxisAlignedBox2d aaBox)
	{
		Extent = 0.5 * aaBox.Diagonal;
		Center = aaBox.Min + Extent;
		AxisX = Vector2d.AxisX;
		AxisY = Vector2d.AxisY;
	}

	public Box2d(Segment2d seg)
	{
		Center = seg.Center;
		AxisX = seg.Direction;
		AxisY = seg.Direction.Perp;
		Extent = new Vector2d(seg.Extent, 0.0);
	}

	public Vector2d Axis(int i)
	{
		if (i != 0)
		{
			return AxisY;
		}
		return AxisX;
	}

	public Vector2d[] ComputeVertices()
	{
		Vector2d[] array = new Vector2d[4];
		ComputeVertices(array);
		return array;
	}

	public void ComputeVertices(Vector2d[] vertex)
	{
		Vector2d vector2d = Extent.x * AxisX;
		Vector2d vector2d2 = Extent.y * AxisY;
		vertex[0] = Center - vector2d - vector2d2;
		vertex[1] = Center + vector2d - vector2d2;
		vertex[2] = Center + vector2d + vector2d2;
		vertex[3] = Center - vector2d + vector2d2;
	}

	public void ComputeVertices(ref Vector2dTuple4 vertex)
	{
		Vector2d vector2d = Extent.x * AxisX;
		Vector2d vector2d2 = Extent.y * AxisY;
		vertex[0] = Center - vector2d - vector2d2;
		vertex[1] = Center + vector2d - vector2d2;
		vertex[2] = Center + vector2d + vector2d2;
		vertex[3] = Center - vector2d + vector2d2;
	}

	public void Contain(Vector2d v)
	{
		Vector2d vector2d = v - Center;
		for (int i = 0; i < 2; i++)
		{
			double num = vector2d.Dot(Axis(i));
			if (Math.Abs(num) > Extent[i])
			{
				double num2 = 0.0 - Extent[i];
				double num3 = Extent[i];
				if (num < num2)
				{
					num2 = num;
				}
				else if (num > num3)
				{
					num3 = num;
				}
				Extent[i] = (num3 - num2) * 0.5;
				Center += (num3 + num2) * 0.5 * Axis(i);
			}
		}
	}

	public void Contain(Box2d o)
	{
		Vector2d[] array = o.ComputeVertices();
		for (int i = 0; i < 4; i++)
		{
			Contain(array[i]);
		}
	}

	public bool Contains(Vector2d v)
	{
		Vector2d vector2d = v - Center;
		if (Math.Abs(vector2d.Dot(AxisX)) <= Extent.x)
		{
			return Math.Abs(vector2d.Dot(AxisY)) <= Extent.y;
		}
		return false;
	}

	public void Expand(double f)
	{
		Extent += f;
	}

	public void Translate(Vector2d v)
	{
		Center += v;
	}

	public void RotateAxes(Matrix2d m)
	{
		AxisX = m * AxisX;
		AxisY = m * AxisY;
	}

	public double DistanceSquared(Vector2d v)
	{
		v -= Center;
		double num = 0.0;
		for (int i = 0; i < 2; i++)
		{
			double num2;
			double num3;
			if (i == 0)
			{
				num2 = v.Dot(AxisX);
				num3 = Extent.x;
			}
			else
			{
				num2 = v.Dot(AxisY);
				num3 = Extent.y;
			}
			if (num2 < 0.0 - num3)
			{
				double num4 = num2 + num3;
				num += num4 * num4;
			}
			else if (num2 > num3)
			{
				double num4 = num2 - num3;
				num += num4 * num4;
			}
		}
		return num;
	}

	public Vector2d ClosestPoint(Vector2d v)
	{
		Vector2d vector2d = v - Center;
		double num = 0.0;
		Vector2d vector2d2 = default(Vector2d);
		for (int i = 0; i < 2; i++)
		{
			vector2d2[i] = vector2d.Dot((i == 0) ? AxisX : AxisY);
			double num2 = ((i == 0) ? Extent.x : Extent.y);
			if (vector2d2[i] < 0.0 - num2)
			{
				double num3 = vector2d2[i] + num2;
				num += num3 * num3;
				vector2d2[i] = 0.0 - num2;
			}
			else if (vector2d2[i] > num2)
			{
				double num3 = vector2d2[i] - num2;
				num += num3 * num3;
				vector2d2[i] = num2;
			}
		}
		return Center + vector2d2.x * AxisX + vector2d2.y * AxisY;
	}

	public static Box2d Merge(ref Box2d box0, ref Box2d box1)
	{
		Box2d result = default(Box2d);
		result.Center = 0.5 * (box0.Center + box1.Center);
		if (box0.AxisX.Dot(box1.AxisX) >= 0.0)
		{
			result.AxisX = 0.5 * (box0.AxisX + box1.AxisX);
			result.AxisX.Normalize();
		}
		else
		{
			result.AxisX = 0.5 * (box0.AxisX - box1.AxisX);
			result.AxisX.Normalize();
		}
		result.AxisY = -result.AxisX.Perp;
		Vector2d vector2d = default(Vector2d);
		Vector2d zero = Vector2d.Zero;
		Vector2d zero2 = Vector2d.Zero;
		Vector2dTuple4 vertex = default(Vector2dTuple4);
		box0.ComputeVertices(ref vertex);
		for (int i = 0; i < 4; i++)
		{
			vector2d = vertex[i] - result.Center;
			for (int j = 0; j < 2; j++)
			{
				double num = vector2d.Dot(result.Axis(j));
				if (num > zero2[j])
				{
					zero2[j] = num;
				}
				else if (num < zero[j])
				{
					zero[j] = num;
				}
			}
		}
		box1.ComputeVertices(ref vertex);
		for (int i = 0; i < 4; i++)
		{
			vector2d = vertex[i] - result.Center;
			for (int j = 0; j < 2; j++)
			{
				double num = vector2d.Dot(result.Axis(j));
				if (num > zero2[j])
				{
					zero2[j] = num;
				}
				else if (num < zero[j])
				{
					zero[j] = num;
				}
			}
		}
		result.Extent[0] = 0.5 * (zero2[0] - zero[0]);
		result.Extent[1] = 0.5 * (zero2[1] - zero[1]);
		result.Center += result.AxisX * (0.5 * (zero2[0] + zero[0]));
		result.Center += result.AxisY * (0.5 * (zero2[1] + zero[1]));
		return result;
	}

	public static implicit operator Box2d(Box2f v)
	{
		return new Box2d(v.Center, v.AxisX, v.AxisY, v.Extent);
	}

	public static explicit operator Box2f(Box2d v)
	{
		return new Box2f((Vector2f)v.Center, (Vector2f)v.AxisX, (Vector2f)v.AxisY, (Vector2f)v.Extent);
	}
}
