using System;
using UnityEngine;

namespace g3;

public struct AxisAlignedBox3d : IComparable<AxisAlignedBox3d>, IEquatable<AxisAlignedBox3d>
{
	public Vector3d Min;

	public Vector3d Max;

	public static readonly AxisAlignedBox3d Empty = new AxisAlignedBox3d(bIgnore: false);

	public static readonly AxisAlignedBox3d Zero = new AxisAlignedBox3d(0.0);

	public static readonly AxisAlignedBox3d UnitPositive = new AxisAlignedBox3d(1.0);

	public static readonly AxisAlignedBox3d Infinite = new AxisAlignedBox3d(double.MinValue, double.MinValue, double.MinValue, double.MaxValue, double.MaxValue, double.MaxValue);

	public double Width => Math.Max(Max.x - Min.x, 0.0);

	public double Height => Math.Max(Max.y - Min.y, 0.0);

	public double Depth => Math.Max(Max.z - Min.z, 0.0);

	public double Volume => Width * Height * Depth;

	public double DiagonalLength => Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y) + (Max.z - Min.z) * (Max.z - Min.z));

	public double MaxDim => Math.Max(Width, Math.Max(Height, Depth));

	public Vector3d Diagonal => new Vector3d(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);

	public Vector3d Extents => new Vector3d((Max.x - Min.x) * 0.5, (Max.y - Min.y) * 0.5, (Max.z - Min.z) * 0.5);

	public Vector3d Center => new Vector3d(0.5 * (Min.x + Max.x), 0.5 * (Min.y + Max.y), 0.5 * (Min.z + Max.z));

	public AxisAlignedBox3d(bool bIgnore)
	{
		Min = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
		Max = new Vector3d(double.MinValue, double.MinValue, double.MinValue);
	}

	public AxisAlignedBox3d(double xmin, double ymin, double zmin, double xmax, double ymax, double zmax)
	{
		Min = new Vector3d(xmin, ymin, zmin);
		Max = new Vector3d(xmax, ymax, zmax);
	}

	public AxisAlignedBox3d(double fCubeSize)
	{
		Min = new Vector3d(0.0, 0.0, 0.0);
		Max = new Vector3d(fCubeSize, fCubeSize, fCubeSize);
	}

	public AxisAlignedBox3d(double fWidth, double fHeight, double fDepth)
	{
		Min = new Vector3d(0.0, 0.0, 0.0);
		Max = new Vector3d(fWidth, fHeight, fDepth);
	}

	public AxisAlignedBox3d(Vector3d vMin, Vector3d vMax)
	{
		Min = new Vector3d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
		Max = new Vector3d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
	}

	public AxisAlignedBox3d(ref Vector3d vMin, ref Vector3d vMax)
	{
		Min = new Vector3d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
		Max = new Vector3d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
	}

	public AxisAlignedBox3d(Vector3d vCenter, double fHalfWidth, double fHalfHeight, double fHalfDepth)
	{
		Min = new Vector3d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
		Max = new Vector3d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
	}

	public AxisAlignedBox3d(ref Vector3d vCenter, double fHalfWidth, double fHalfHeight, double fHalfDepth)
	{
		Min = new Vector3d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
		Max = new Vector3d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
	}

	public AxisAlignedBox3d(Vector3d vCenter, double fHalfSize)
	{
		Min = new Vector3d(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
		Max = new Vector3d(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
	}

	public AxisAlignedBox3d(Vector3d vCenter)
	{
		Min = (Max = vCenter);
	}

	public static bool operator ==(AxisAlignedBox3d a, AxisAlignedBox3d b)
	{
		if (a.Min == b.Min)
		{
			return a.Max == b.Max;
		}
		return false;
	}

	public static bool operator !=(AxisAlignedBox3d a, AxisAlignedBox3d b)
	{
		if (!(a.Min != b.Min))
		{
			return a.Max != b.Max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (AxisAlignedBox3d)obj;
	}

	public bool Equals(AxisAlignedBox3d other)
	{
		return this == other;
	}

	public int CompareTo(AxisAlignedBox3d other)
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

	public Vector3d Corner(int i)
	{
		double x = ((((i & 1) != 0) ^ ((i & 2) != 0)) ? Max.x : Min.x);
		double y = ((i / 2 % 2 == 0) ? Min.y : Max.y);
		double z = ((i < 4) ? Min.z : Max.z);
		return new Vector3d(x, y, z);
	}

	public Vector3d Point(int xi, int yi, int zi)
	{
		double x = ((xi < 0) ? Min.x : ((xi == 0) ? (0.5 * (Min.x + Max.x)) : Max.x));
		double y = ((yi < 0) ? Min.y : ((yi == 0) ? (0.5 * (Min.y + Max.y)) : Max.y));
		double z = ((zi < 0) ? Min.z : ((zi == 0) ? (0.5 * (Min.z + Max.z)) : Max.z));
		return new Vector3d(x, y, z);
	}

	public void Expand(double fRadius)
	{
		Min.x -= fRadius;
		Min.y -= fRadius;
		Min.z -= fRadius;
		Max.x += fRadius;
		Max.y += fRadius;
		Max.z += fRadius;
	}

	public AxisAlignedBox3d Expanded(double fRadius)
	{
		return new AxisAlignedBox3d(Min.x - fRadius, Min.y - fRadius, Min.z - fRadius, Max.x + fRadius, Max.y + fRadius, Max.z + fRadius);
	}

	public void Contract(double fRadius)
	{
		double num = 2.0 * fRadius;
		if (num > Max.x - Min.x)
		{
			Min.x = (Max.x = 0.5 * (Min.x + Max.x));
		}
		else
		{
			Min.x += fRadius;
			Max.x -= fRadius;
		}
		if (num > Max.y - Min.y)
		{
			Min.y = (Max.y = 0.5 * (Min.y + Max.y));
		}
		else
		{
			Min.y += fRadius;
			Max.y -= fRadius;
		}
		if (num > Max.z - Min.z)
		{
			Min.z = (Max.z = 0.5 * (Min.z + Max.z));
			return;
		}
		Min.z += fRadius;
		Max.z -= fRadius;
	}

	public AxisAlignedBox3d Contracted(double fRadius)
	{
		AxisAlignedBox3d result = new AxisAlignedBox3d(Min.x + fRadius, Min.y + fRadius, Min.z + fRadius, Max.x - fRadius, Max.y - fRadius, Max.z - fRadius);
		if (result.Min.x > result.Max.x)
		{
			result.Min.x = (result.Max.x = 0.5 * (Min.x + Max.x));
		}
		if (result.Min.y > result.Max.y)
		{
			result.Min.y = (result.Max.y = 0.5 * (Min.y + Max.y));
		}
		if (result.Min.z > result.Max.z)
		{
			result.Min.z = (result.Max.z = 0.5 * (Min.z + Max.z));
		}
		return result;
	}

	public void Scale(double sx, double sy, double sz)
	{
		Vector3d center = Center;
		Vector3d extents = Extents;
		extents.x *= sx;
		extents.y *= sy;
		extents.z *= sz;
		Min = new Vector3d(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		Max = new Vector3d(center.x + extents.x, center.y + extents.y, center.z + extents.z);
	}

	public void Contain(Vector3d v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Min.z = Math.Min(Min.z, v.z);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
		Max.z = Math.Max(Max.z, v.z);
	}

	public void Contain(ref Vector3d v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Min.z = Math.Min(Min.z, v.z);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
		Max.z = Math.Max(Max.z, v.z);
	}

	public void Contain(AxisAlignedBox3d box)
	{
		Min.x = Math.Min(Min.x, box.Min.x);
		Min.y = Math.Min(Min.y, box.Min.y);
		Min.z = Math.Min(Min.z, box.Min.z);
		Max.x = Math.Max(Max.x, box.Max.x);
		Max.y = Math.Max(Max.y, box.Max.y);
		Max.z = Math.Max(Max.z, box.Max.z);
	}

	public void Contain(ref AxisAlignedBox3d box)
	{
		Min.x = Math.Min(Min.x, box.Min.x);
		Min.y = Math.Min(Min.y, box.Min.y);
		Min.z = Math.Min(Min.z, box.Min.z);
		Max.x = Math.Max(Max.x, box.Max.x);
		Max.y = Math.Max(Max.y, box.Max.y);
		Max.z = Math.Max(Max.z, box.Max.z);
	}

	public AxisAlignedBox3d Intersect(AxisAlignedBox3d box)
	{
		AxisAlignedBox3d result = new AxisAlignedBox3d(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Max(Min.z, box.Min.z), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y), Math.Min(Max.z, box.Max.z));
		if (result.Height <= 0.0 || result.Width <= 0.0 || result.Depth <= 0.0)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector3d v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Min.z <= v.z && Max.x >= v.x && Max.y >= v.y)
		{
			return Max.z >= v.z;
		}
		return false;
	}

	public bool Contains(ref Vector3d v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Min.z <= v.z && Max.x >= v.x && Max.y >= v.y)
		{
			return Max.z >= v.z;
		}
		return false;
	}

	public bool Contains(AxisAlignedBox3d box2)
	{
		if (Contains(ref box2.Min))
		{
			return Contains(ref box2.Max);
		}
		return false;
	}

	public bool Contains(ref AxisAlignedBox3d box2)
	{
		if (Contains(ref box2.Min))
		{
			return Contains(ref box2.Max);
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox3d box)
	{
		if (!(box.Max.x <= Min.x) && !(box.Min.x >= Max.x) && !(box.Max.y <= Min.y) && !(box.Min.y >= Max.y) && !(box.Max.z <= Min.z))
		{
			return !(box.Min.z >= Max.z);
		}
		return false;
	}

	public double DistanceSquared(Vector3d v)
	{
		double num = ((v.x < Min.x) ? (Min.x - v.x) : ((v.x > Max.x) ? (v.x - Max.x) : 0.0));
		double num2 = ((v.y < Min.y) ? (Min.y - v.y) : ((v.y > Max.y) ? (v.y - Max.y) : 0.0));
		double num3 = ((v.z < Min.z) ? (Min.z - v.z) : ((v.z > Max.z) ? (v.z - Max.z) : 0.0));
		return num * num + num2 * num2 + num3 * num3;
	}

	public double Distance(Vector3d v)
	{
		return Math.Sqrt(DistanceSquared(v));
	}

	public double SignedDistance(Vector3d v)
	{
		if (!Contains(v))
		{
			return Distance(v);
		}
		double a = Math.Min(Math.Abs(v.x - Min.x), Math.Abs(v.x - Max.x));
		double b = Math.Min(Math.Abs(v.y - Min.y), Math.Abs(v.y - Max.y));
		double c = Math.Min(Math.Abs(v.z - Min.z), Math.Abs(v.z - Max.z));
		return 0.0 - MathUtil.Min(a, b, c);
	}

	public double DistanceSquared(ref AxisAlignedBox3d box2)
	{
		double num = Math.Abs(box2.Min.x + box2.Max.x - (Min.x + Max.x)) - (Max.x - Min.x + (box2.Max.x - box2.Min.x));
		if (num < 0.0)
		{
			num = 0.0;
		}
		double num2 = Math.Abs(box2.Min.y + box2.Max.y - (Min.y + Max.y)) - (Max.y - Min.y + (box2.Max.y - box2.Min.y));
		if (num2 < 0.0)
		{
			num2 = 0.0;
		}
		double num3 = Math.Abs(box2.Min.z + box2.Max.z - (Min.z + Max.z)) - (Max.z - Min.z + (box2.Max.z - box2.Min.z));
		if (num3 < 0.0)
		{
			num3 = 0.0;
		}
		return 0.25 * (num * num + num2 * num2 + num3 * num3);
	}

	public void Translate(Vector3d vTranslate)
	{
		Min.Add(vTranslate);
		Max.Add(vTranslate);
	}

	public void MoveMin(Vector3d vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Max.z = vNewMin.z + (Max.z - Min.z);
		Min.Set(vNewMin);
	}

	public void MoveMin(double fNewX, double fNewY, double fNewZ)
	{
		Max.x = fNewX + (Max.x - Min.x);
		Max.y = fNewY + (Max.y - Min.y);
		Max.z = fNewZ + (Max.z - Min.z);
		Min.Set(fNewX, fNewY, fNewZ);
	}

	public override string ToString()
	{
		return $"x[{Min.x:F8},{Max.x:F8}] y[{Min.y:F8},{Max.y:F8}] z[{Min.z:F8},{Max.z:F8}]";
	}

	public static implicit operator AxisAlignedBox3d(AxisAlignedBox3f v)
	{
		return new AxisAlignedBox3d(v.Min, v.Max);
	}

	public static explicit operator AxisAlignedBox3f(AxisAlignedBox3d v)
	{
		return new AxisAlignedBox3f((Vector3f)v.Min, (Vector3f)v.Max);
	}

	public static implicit operator AxisAlignedBox3d(Bounds b)
	{
		return new AxisAlignedBox3f(b.min, b.max);
	}

	public static explicit operator Bounds(AxisAlignedBox3d b)
	{
		Bounds result = default(Bounds);
		result.SetMinMax((Vector3)b.Min, (Vector3)b.Max);
		return result;
	}
}
