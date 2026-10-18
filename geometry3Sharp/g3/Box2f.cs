using System;

namespace g3;

public struct Box2f
{
	public Vector2f Center;

	public Vector2f AxisX;

	public Vector2f AxisY;

	public Vector2f Extent;

	public static readonly Box2f Empty = new Box2f(Vector2f.Zero);

	public double MaxExtent => Math.Max(Extent.x, Extent.y);

	public double MinExtent => Math.Min(Extent.x, Extent.y);

	public Vector2f Diagonal => Extent.x * AxisX + Extent.y * AxisY - ((0f - Extent.x) * AxisX - Extent.y * AxisY);

	public double Area => 2f * Extent.x * 2f * Extent.y;

	public Box2f(Vector2f center)
	{
		Center = center;
		AxisX = Vector2f.AxisX;
		AxisY = Vector2f.AxisY;
		Extent = Vector2f.Zero;
	}

	public Box2f(Vector2f center, Vector2f x, Vector2f y, Vector2f extent)
	{
		Center = center;
		AxisX = x;
		AxisY = y;
		Extent = extent;
	}

	public Box2f(Vector2f center, Vector2f extent)
	{
		Center = center;
		Extent = extent;
		AxisX = Vector2f.AxisX;
		AxisY = Vector2f.AxisY;
	}

	public Box2f(AxisAlignedBox2f aaBox)
	{
		Extent = 0.5f * aaBox.Diagonal;
		Center = aaBox.Min + Extent;
		AxisX = Vector2f.AxisX;
		AxisY = Vector2f.AxisY;
	}

	public Vector2f Axis(int i)
	{
		if (i != 0)
		{
			return AxisY;
		}
		return AxisX;
	}

	public Vector2f[] ComputeVertices()
	{
		Vector2f[] array = new Vector2f[4];
		ComputeVertices(array);
		return array;
	}

	public void ComputeVertices(Vector2f[] vertex)
	{
		Vector2f vector2f = Extent.x * AxisX;
		Vector2f vector2f2 = Extent.y * AxisY;
		vertex[0] = Center - vector2f - vector2f2;
		vertex[1] = Center + vector2f - vector2f2;
		vertex[2] = Center + vector2f + vector2f2;
		vertex[3] = Center - vector2f + vector2f2;
	}

	public void Contain(Vector2f v)
	{
		Vector2f vector2f = v - Center;
		for (int i = 0; i < 2; i++)
		{
			double num = vector2f.Dot(Axis(i));
			if (Math.Abs(num) > (double)Extent[i])
			{
				double num2 = 0f - Extent[i];
				double num3 = Extent[i];
				if (num < num2)
				{
					num2 = num;
				}
				else if (num > num3)
				{
					num3 = num;
				}
				Extent[i] = (float)(num3 - num2) * 0.5f;
				Center += (float)(num3 + num2) * 0.5f * Axis(i);
			}
		}
	}

	public void Contain(Box2f o)
	{
		Vector2f[] array = o.ComputeVertices();
		for (int i = 0; i < 4; i++)
		{
			Contain(array[i]);
		}
	}

	public bool Contains(Vector2f v)
	{
		Vector2f vector2f = v - Center;
		if (Math.Abs(vector2f.Dot(AxisX)) <= Extent.x)
		{
			return Math.Abs(vector2f.Dot(AxisY)) <= Extent.y;
		}
		return false;
	}

	public void Expand(float f)
	{
		Extent += f;
	}

	public void Translate(Vector2f v)
	{
		Center += v;
	}
}
