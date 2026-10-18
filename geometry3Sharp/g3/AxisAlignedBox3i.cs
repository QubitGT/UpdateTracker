using System;
using System.Collections.Generic;

namespace g3;

public struct AxisAlignedBox3i : IComparable<AxisAlignedBox3i>, IEquatable<AxisAlignedBox3i>
{
	public Vector3i Min;

	public Vector3i Max;

	public static readonly AxisAlignedBox3i Empty = new AxisAlignedBox3i(bIgnore: false);

	public static readonly AxisAlignedBox3i Zero = new AxisAlignedBox3i(0);

	public static readonly AxisAlignedBox3i UnitPositive = new AxisAlignedBox3i(1);

	public static readonly AxisAlignedBox3i Infinite = new AxisAlignedBox3i(int.MinValue, int.MinValue, int.MinValue, int.MaxValue, int.MaxValue, int.MaxValue);

	public int Width => Math.Max(Max.x - Min.x, 0);

	public int Height => Math.Max(Max.y - Min.y, 0);

	public int Depth => Math.Max(Max.z - Min.z, 0);

	public int Volume => Width * Height * Depth;

	public int DiagonalLength => (int)Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y) + (Max.z - Min.z) * (Max.z - Min.z));

	public int MaxDim => Math.Max(Width, Math.Max(Height, Depth));

	public Vector3i Diagonal => new Vector3i(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);

	public Vector3i Extents => new Vector3i((Max.x - Min.x) / 2, (Max.y - Min.y) / 2, (Max.z - Min.z) / 2);

	public Vector3i Center => new Vector3i((Min.x + Max.x) / 2, (Min.y + Max.y) / 2, (Min.z + Max.z) / 2);

	public AxisAlignedBox3i(bool bIgnore)
	{
		Min = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);
		Max = new Vector3i(int.MinValue, int.MinValue, int.MinValue);
	}

	public AxisAlignedBox3i(int xmin, int ymin, int zmin, int xmax, int ymax, int zmax)
	{
		Min = new Vector3i(xmin, ymin, zmin);
		Max = new Vector3i(xmax, ymax, zmax);
	}

	public AxisAlignedBox3i(int fCubeSize)
	{
		Min = new Vector3i(0, 0, 0);
		Max = new Vector3i(fCubeSize, fCubeSize, fCubeSize);
	}

	public AxisAlignedBox3i(int fWidth, int fHeight, int fDepth)
	{
		Min = new Vector3i(0, 0, 0);
		Max = new Vector3i(fWidth, fHeight, fDepth);
	}

	public AxisAlignedBox3i(Vector3i vMin, Vector3i vMax)
	{
		Min = new Vector3i(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
		Max = new Vector3i(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
	}

	public AxisAlignedBox3i(Vector3i vCenter, int fHalfWidth, int fHalfHeight, int fHalfDepth)
	{
		Min = new Vector3i(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
		Max = new Vector3i(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
	}

	public AxisAlignedBox3i(Vector3i vCenter, int fHalfSize)
	{
		Min = new Vector3i(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
		Max = new Vector3i(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
	}

	public AxisAlignedBox3i(Vector3i vCenter)
	{
		Min = (Max = vCenter);
	}

	public static bool operator ==(AxisAlignedBox3i a, AxisAlignedBox3i b)
	{
		if (a.Min == b.Min)
		{
			return a.Max == b.Max;
		}
		return false;
	}

	public static bool operator !=(AxisAlignedBox3i a, AxisAlignedBox3i b)
	{
		if (!(a.Min != b.Min))
		{
			return a.Max != b.Max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (AxisAlignedBox3i)obj;
	}

	public bool Equals(AxisAlignedBox3i other)
	{
		return this == other;
	}

	public int CompareTo(AxisAlignedBox3i other)
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

	public void Expand(int nRadius)
	{
		Min.x -= nRadius;
		Min.y -= nRadius;
		Min.z -= nRadius;
		Max.x += nRadius;
		Max.y += nRadius;
		Max.z += nRadius;
	}

	public void Contract(int nRadius)
	{
		Min.x += nRadius;
		Min.y += nRadius;
		Min.z += nRadius;
		Max.x -= nRadius;
		Max.y -= nRadius;
		Max.z -= nRadius;
	}

	public void Scale(int sx, int sy, int sz)
	{
		Vector3i center = Center;
		Vector3i extents = Extents;
		extents.x *= sx;
		extents.y *= sy;
		extents.z *= sz;
		Min = new Vector3i(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		Max = new Vector3i(center.x + extents.x, center.y + extents.y, center.z + extents.z);
	}

	public void Contain(Vector3i v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Min.z = Math.Min(Min.z, v.z);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
		Max.z = Math.Max(Max.z, v.z);
	}

	public void Contain(AxisAlignedBox3i box)
	{
		Min.x = Math.Min(Min.x, box.Min.x);
		Min.y = Math.Min(Min.y, box.Min.y);
		Min.z = Math.Min(Min.z, box.Min.z);
		Max.x = Math.Max(Max.x, box.Max.x);
		Max.y = Math.Max(Max.y, box.Max.y);
		Max.z = Math.Max(Max.z, box.Max.z);
	}

	public void Contain(Vector3d v)
	{
		Min.x = Math.Min(Min.x, (int)v.x);
		Min.y = Math.Min(Min.y, (int)v.y);
		Min.z = Math.Min(Min.z, (int)v.z);
		Max.x = Math.Max(Max.x, (int)v.x);
		Max.y = Math.Max(Max.y, (int)v.y);
		Max.z = Math.Max(Max.z, (int)v.z);
	}

	public void Contain(AxisAlignedBox3d box)
	{
		Min.x = Math.Min(Min.x, (int)box.Min.x);
		Min.y = Math.Min(Min.y, (int)box.Min.y);
		Min.z = Math.Min(Min.z, (int)box.Min.z);
		Max.x = Math.Max(Max.x, (int)box.Max.x);
		Max.y = Math.Max(Max.y, (int)box.Max.y);
		Max.z = Math.Max(Max.z, (int)box.Max.z);
	}

	public AxisAlignedBox3i Intersect(AxisAlignedBox3i box)
	{
		AxisAlignedBox3i result = new AxisAlignedBox3i(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Max(Min.z, box.Min.z), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y), Math.Min(Max.z, box.Max.z));
		if (result.Height <= 0 || result.Width <= 0 || result.Depth <= 0)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector3i v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Min.z <= v.z && Max.x >= v.x && Max.y >= v.y)
		{
			return Max.z >= v.z;
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox3i box)
	{
		if (box.Max.x > Min.x && box.Min.x < Max.x && box.Max.y > Min.y && box.Min.y < Max.y && box.Max.z > Min.z)
		{
			return box.Min.z < Max.z;
		}
		return false;
	}

	public double DistanceSquared(Vector3i v)
	{
		int num = ((v.x < Min.x) ? (Min.x - v.x) : ((v.x > Max.x) ? (v.x - Max.x) : 0));
		int num2 = ((v.y < Min.y) ? (Min.y - v.y) : ((v.y > Max.y) ? (v.y - Max.y) : 0));
		int num3 = ((v.z < Min.z) ? (Min.z - v.z) : ((v.z > Max.z) ? (v.z - Max.z) : 0));
		return num * num + num2 * num2 + num3 * num3;
	}

	public int Distance(Vector3i v)
	{
		return (int)Math.Sqrt(DistanceSquared(v));
	}

	public Vector3i NearestPoint(Vector3i v)
	{
		int x = ((v.x < Min.x) ? Min.x : ((v.x > Max.x) ? Max.x : v.x));
		int y = ((v.y < Min.y) ? Min.y : ((v.y > Max.y) ? Max.y : v.y));
		int z = ((v.z < Min.z) ? Min.z : ((v.z > Max.z) ? Max.z : v.z));
		return new Vector3i(x, y, z);
	}

	public Vector3i ClampInclusive(Vector3i v)
	{
		return new Vector3i(MathUtil.Clamp(v.x, Min.x, Max.x), MathUtil.Clamp(v.y, Min.y, Max.y), MathUtil.Clamp(v.z, Min.z, Max.z));
	}

	public Vector3i ClampExclusive(Vector3i v)
	{
		return new Vector3i(MathUtil.Clamp(v.x, Min.x, Max.x - 1), MathUtil.Clamp(v.y, Min.y, Max.y - 1), MathUtil.Clamp(v.z, Min.z, Max.z - 1));
	}

	public void Translate(Vector3i vTranslate)
	{
		Min.Add(vTranslate);
		Max.Add(vTranslate);
	}

	public void MoveMin(Vector3i vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Max.z = vNewMin.z + (Max.z - Min.z);
		Min.Set(vNewMin);
	}

	public void MoveMin(int fNewX, int fNewY, int fNewZ)
	{
		Max.x = fNewX + (Max.x - Min.x);
		Max.y = fNewY + (Max.y - Min.y);
		Max.z = fNewZ + (Max.z - Min.z);
		Min.Set(fNewX, fNewY, fNewZ);
	}

	public IEnumerable<Vector3i> IndicesInclusive()
	{
		int zi = Min.z;
		while (zi <= Max.z)
		{
			int num;
			for (int yi = Min.y; yi <= Max.y; yi = num)
			{
				for (int xi = Min.x; xi <= Max.x; xi = num)
				{
					yield return new Vector3i(xi, yi, zi);
					num = xi + 1;
				}
				num = yi + 1;
			}
			num = zi + 1;
			zi = num;
		}
	}

	public IEnumerable<Vector3i> IndicesExclusive()
	{
		int zi = Min.z;
		while (zi < Max.z)
		{
			int num;
			for (int yi = Min.y; yi < Max.y; yi = num)
			{
				for (int xi = Min.x; xi < Max.x; xi = num)
				{
					yield return new Vector3i(xi, yi, zi);
					num = xi + 1;
				}
				num = yi + 1;
			}
			num = zi + 1;
			zi = num;
		}
	}

	public override string ToString()
	{
		return $"x[{Min.x},{Max.x}] y[{Min.y},{Max.y}] z[{Min.z},{Max.z}]";
	}
}
