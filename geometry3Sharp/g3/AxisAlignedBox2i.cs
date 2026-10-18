using System;
using System.Collections.Generic;

namespace g3;

public struct AxisAlignedBox2i : IComparable<AxisAlignedBox2i>, IEquatable<AxisAlignedBox2i>
{
	public Vector2i Min;

	public Vector2i Max;

	public static readonly AxisAlignedBox2i Empty = new AxisAlignedBox2i(bIgnore: false);

	public static readonly AxisAlignedBox2i Zero = new AxisAlignedBox2i(0);

	public static readonly AxisAlignedBox2i UnitPositive = new AxisAlignedBox2i(1);

	public static readonly AxisAlignedBox2i Infinite = new AxisAlignedBox2i(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);

	public int Width => Math.Max(Max.x - Min.x, 0);

	public int Height => Math.Max(Max.y - Min.y, 0);

	public int Area => Width * Height;

	public int DiagonalLength => (int)Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y));

	public int MaxDim => Math.Max(Width, Height);

	public Vector2i Diagonal => new Vector2i(Max.x - Min.x, Max.y - Min.y);

	public Vector2i Extents => new Vector2i((Max.x - Min.x) / 2, (Max.y - Min.y) / 2);

	public Vector2i Center => new Vector2i((Min.x + Max.x) / 2, (Min.y + Max.y) / 2);

	public AxisAlignedBox2i(bool bIgnore)
	{
		Min = new Vector2i(int.MaxValue, int.MaxValue);
		Max = new Vector2i(int.MinValue, int.MinValue);
	}

	public AxisAlignedBox2i(int xmin, int ymin, int xmax, int ymax)
	{
		Min = new Vector2i(xmin, ymin);
		Max = new Vector2i(xmax, ymax);
	}

	public AxisAlignedBox2i(int fCubeSize)
	{
		Min = new Vector2i(0, 0);
		Max = new Vector2i(fCubeSize, fCubeSize);
	}

	public AxisAlignedBox2i(int fWidth, int fHeight)
	{
		Min = new Vector2i(0, 0);
		Max = new Vector2i(fWidth, fHeight);
	}

	public AxisAlignedBox2i(Vector2i vMin, Vector2i vMax)
	{
		Min = new Vector2i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
		Max = new Vector2i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
	}

	public AxisAlignedBox2i(Vector2i vCenter, int fHalfWidth, int fHalfHeight, int fHalfDepth)
	{
		Min = new Vector2i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
		Max = new Vector2i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
	}

	public AxisAlignedBox2i(Vector2i vCenter, int fHalfSize)
	{
		Min = new Vector2i(vCenter.x - fHalfSize, vCenter.y - fHalfSize);
		Max = new Vector2i(vCenter.x + fHalfSize, vCenter.y + fHalfSize);
	}

	public AxisAlignedBox2i(Vector2i vCenter)
	{
		Min = (Max = vCenter);
	}

	public static bool operator ==(AxisAlignedBox2i a, AxisAlignedBox2i b)
	{
		if (a.Min == b.Min)
		{
			return a.Max == b.Max;
		}
		return false;
	}

	public static bool operator !=(AxisAlignedBox2i a, AxisAlignedBox2i b)
	{
		if (!(a.Min != b.Min))
		{
			return a.Max != b.Max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (AxisAlignedBox2i)obj;
	}

	public bool Equals(AxisAlignedBox2i other)
	{
		return this == other;
	}

	public int CompareTo(AxisAlignedBox2i other)
	{
		int num = Min.CompareTo(other.Min);
		if (num == 0)
		{
			return Max.CompareTo(other.Max);
		}
		return num;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ Min.GetHashCode()) * 16777619) ^ Max.GetHashCode();
	}

	public Vector2i GetCorner(int i)
	{
		return new Vector2i((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
	}

	public void Expand(int nRadius)
	{
		Min.x -= nRadius;
		Min.y -= nRadius;
		Max.x += nRadius;
		Max.y += nRadius;
	}

	public void Contract(int nRadius)
	{
		Min.x += nRadius;
		Min.y += nRadius;
		Max.x -= nRadius;
		Max.y -= nRadius;
	}

	public void Scale(int sx, int sy, int sz)
	{
		Vector2i center = Center;
		Vector2i extents = Extents;
		extents.x *= sx;
		extents.y *= sy;
		Min = new Vector2i(center.x - extents.x, center.y - extents.y);
		Max = new Vector2i(center.x + extents.x, center.y + extents.y);
	}

	public void Contain(Vector2i v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
	}

	public void Contain(AxisAlignedBox2i box)
	{
		Min.x = Math.Min(Min.x, box.Min.x);
		Min.y = Math.Min(Min.y, box.Min.y);
		Max.x = Math.Max(Max.x, box.Max.x);
		Max.y = Math.Max(Max.y, box.Max.y);
	}

	public void Contain(Vector3d v)
	{
		Min.x = Math.Min(Min.x, (int)v.x);
		Min.y = Math.Min(Min.y, (int)v.y);
		Max.x = Math.Max(Max.x, (int)v.x);
		Max.y = Math.Max(Max.y, (int)v.y);
	}

	public void Contain(AxisAlignedBox3d box)
	{
		Min.x = Math.Min(Min.x, (int)box.Min.x);
		Min.y = Math.Min(Min.y, (int)box.Min.y);
		Max.x = Math.Max(Max.x, (int)box.Max.x);
		Max.y = Math.Max(Max.y, (int)box.Max.y);
	}

	public AxisAlignedBox2i Intersect(AxisAlignedBox2i box)
	{
		AxisAlignedBox2i result = new AxisAlignedBox2i(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
		if (result.Height <= 0 || result.Width <= 0)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector2i v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Max.x >= v.x)
		{
			return Max.y >= v.y;
		}
		return false;
	}

	public bool Contains(ref Vector2i v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Max.x >= v.x)
		{
			return Max.y >= v.y;
		}
		return false;
	}

	public bool Contains(AxisAlignedBox2i box)
	{
		if (Contains(ref box.Min))
		{
			return Contains(ref box.Max);
		}
		return false;
	}

	public bool Contains(ref AxisAlignedBox2i box)
	{
		if (Contains(ref box.Min))
		{
			return Contains(ref box.Max);
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox2i box)
	{
		if (box.Max.x > Min.x && box.Min.x < Max.x && box.Max.y > Min.y)
		{
			return box.Min.y < Max.y;
		}
		return false;
	}

	public double DistanceSquared(Vector2i v)
	{
		int num = ((v.x < Min.x) ? (Min.x - v.x) : ((v.x > Max.x) ? (v.x - Max.x) : 0));
		int num2 = ((v.y < Min.y) ? (Min.y - v.y) : ((v.y > Max.y) ? (v.y - Max.y) : 0));
		return num * num + num2 * num2;
	}

	public int Distance(Vector2i v)
	{
		return (int)Math.Sqrt(DistanceSquared(v));
	}

	public Vector2i NearestPoint(Vector2i v)
	{
		int x = ((v.x < Min.x) ? Min.x : ((v.x > Max.x) ? Max.x : v.x));
		int y = ((v.y < Min.y) ? Min.y : ((v.y > Max.y) ? Max.y : v.y));
		return new Vector2i(x, y);
	}

	public void Translate(Vector2i vTranslate)
	{
		Min += vTranslate;
		Max += vTranslate;
	}

	public void MoveMin(Vector2i vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Min = vNewMin;
	}

	public void MoveMin(int fNewX, int fNewY)
	{
		Max.x = fNewX + (Max.x - Min.x);
		Max.y = fNewY + (Max.y - Min.y);
		Min = new Vector2i(fNewX, fNewY);
	}

	public IEnumerable<Vector2i> IndicesInclusive()
	{
		int yi = Min.y;
		while (yi <= Max.y)
		{
			int num;
			for (int xi = Min.x; xi <= Max.x; xi = num)
			{
				yield return new Vector2i(xi, yi);
				num = xi + 1;
			}
			num = yi + 1;
			yi = num;
		}
	}

	public IEnumerable<Vector2i> IndicesExclusive()
	{
		int yi = Min.y;
		while (yi < Max.y)
		{
			int num;
			for (int xi = Min.x; xi < Max.x; xi = num)
			{
				yield return new Vector2i(xi, yi);
				num = xi + 1;
			}
			num = yi + 1;
			yi = num;
		}
	}

	public override string ToString()
	{
		return $"x[{Min.x},{Max.x}] y[{Min.y},{Max.y}]";
	}
}
