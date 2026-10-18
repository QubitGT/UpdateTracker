using System;
using UnityEngine;

namespace g3;

public struct AxisAlignedBox3f : IComparable<AxisAlignedBox3f>, IEquatable<AxisAlignedBox3f>
{
	public Vector3f Min;

	public Vector3f Max;

	public static readonly AxisAlignedBox3f Empty = new AxisAlignedBox3f(bIgnore: false);

	public static readonly AxisAlignedBox3f Zero = new AxisAlignedBox3f(0f);

	public static readonly AxisAlignedBox3f UnitPositive = new AxisAlignedBox3f(1f);

	public static readonly AxisAlignedBox3f Infinite = new AxisAlignedBox3f(float.MinValue, float.MinValue, float.MinValue, float.MaxValue, float.MaxValue, float.MaxValue);

	public float Width => Math.Max(Max.x - Min.x, 0f);

	public float Height => Math.Max(Max.y - Min.y, 0f);

	public float Depth => Math.Max(Max.z - Min.z, 0f);

	public float Volume => Width * Height * Depth;

	public float DiagonalLength => (float)Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y) + (Max.z - Min.z) * (Max.z - Min.z));

	public float MaxDim => Math.Max(Width, Math.Max(Height, Depth));

	public Vector3f Diagonal => new Vector3f(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);

	public Vector3f Extents => new Vector3f((double)(Max.x - Min.x) * 0.5, (double)(Max.y - Min.y) * 0.5, (double)(Max.z - Min.z) * 0.5);

	public Vector3f Center => new Vector3f(0.5 * (double)(Min.x + Max.x), 0.5 * (double)(Min.y + Max.y), 0.5 * (double)(Min.z + Max.z));

	public AxisAlignedBox3f(bool bIgnore)
	{
		Min = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);
		Max = new Vector3f(float.MinValue, float.MinValue, float.MinValue);
	}

	public AxisAlignedBox3f(float xmin, float ymin, float zmin, float xmax, float ymax, float zmax)
	{
		Min = new Vector3f(xmin, ymin, zmin);
		Max = new Vector3f(xmax, ymax, zmax);
	}

	public AxisAlignedBox3f(float fCubeSize)
	{
		Min = new Vector3f(0f, 0f, 0f);
		Max = new Vector3f(fCubeSize, fCubeSize, fCubeSize);
	}

	public AxisAlignedBox3f(float fWidth, float fHeight, float fDepth)
	{
		Min = new Vector3f(0f, 0f, 0f);
		Max = new Vector3f(fWidth, fHeight, fDepth);
	}

	public AxisAlignedBox3f(Vector3f vMin, Vector3f vMax)
	{
		Min = new Vector3f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
		Max = new Vector3f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
	}

	public AxisAlignedBox3f(ref Vector3f vMin, ref Vector3f vMax)
	{
		Min = new Vector3f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y), Math.Min(vMin.z, vMax.z));
		Max = new Vector3f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y), Math.Max(vMin.z, vMax.z));
	}

	public AxisAlignedBox3f(Vector3f vCenter, float fHalfWidth, float fHalfHeight, float fHalfDepth)
	{
		Min = new Vector3f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
		Max = new Vector3f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
	}

	public AxisAlignedBox3f(ref Vector3f vCenter, float fHalfWidth, float fHalfHeight, float fHalfDepth)
	{
		Min = new Vector3f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight, vCenter.z - fHalfDepth);
		Max = new Vector3f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight, vCenter.z + fHalfDepth);
	}

	public AxisAlignedBox3f(Vector3f vCenter, float fHalfSize)
	{
		Min = new Vector3f(vCenter.x - fHalfSize, vCenter.y - fHalfSize, vCenter.z - fHalfSize);
		Max = new Vector3f(vCenter.x + fHalfSize, vCenter.y + fHalfSize, vCenter.z + fHalfSize);
	}

	public AxisAlignedBox3f(Vector3f vCenter)
	{
		Min = (Max = vCenter);
	}

	public static bool operator ==(AxisAlignedBox3f a, AxisAlignedBox3f b)
	{
		if (a.Min == b.Min)
		{
			return a.Max == b.Max;
		}
		return false;
	}

	public static bool operator !=(AxisAlignedBox3f a, AxisAlignedBox3f b)
	{
		if (!(a.Min != b.Min))
		{
			return a.Max != b.Max;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (AxisAlignedBox3f)obj;
	}

	public bool Equals(AxisAlignedBox3f other)
	{
		return this == other;
	}

	public int CompareTo(AxisAlignedBox3f other)
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

	public Vector3f Corner(int i)
	{
		float x = ((((i & 1) != 0) ^ ((i & 2) != 0)) ? Max.x : Min.x);
		float y = ((i / 2 % 2 == 0) ? Min.y : Max.y);
		float z = ((i < 4) ? Min.z : Max.z);
		return new Vector3f(x, y, z);
	}

	public Vector3f Point(int xi, int yi, int zi)
	{
		float x = ((xi < 0) ? Min.x : ((xi == 0) ? (0.5f * (Min.x + Max.x)) : Max.x));
		float y = ((yi < 0) ? Min.y : ((yi == 0) ? (0.5f * (Min.y + Max.y)) : Max.y));
		float z = ((zi < 0) ? Min.z : ((zi == 0) ? (0.5f * (Min.z + Max.z)) : Max.z));
		return new Vector3f(x, y, z);
	}

	public void Expand(float fRadius)
	{
		Min.x -= fRadius;
		Min.y -= fRadius;
		Min.z -= fRadius;
		Max.x += fRadius;
		Max.y += fRadius;
		Max.z += fRadius;
	}

	public void Contract(float fRadius)
	{
		Min.x += fRadius;
		Min.y += fRadius;
		Min.z += fRadius;
		Max.x -= fRadius;
		Max.y -= fRadius;
		Max.z -= fRadius;
	}

	public void Scale(float sx, float sy, float sz)
	{
		Vector3f center = Center;
		Vector3f extents = Extents;
		extents.x *= sx;
		extents.y *= sy;
		extents.z *= sz;
		Min = new Vector3f(center.x - extents.x, center.y - extents.y, center.z - extents.z);
		Max = new Vector3f(center.x + extents.x, center.y + extents.y, center.z + extents.z);
	}

	public void Contain(Vector3f v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Min.z = Math.Min(Min.z, v.z);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
		Max.z = Math.Max(Max.z, v.z);
	}

	public void Contain(AxisAlignedBox3f box)
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
		Min.x = Math.Min(Min.x, (float)v.x);
		Min.y = Math.Min(Min.y, (float)v.y);
		Min.z = Math.Min(Min.z, (float)v.z);
		Max.x = Math.Max(Max.x, (float)v.x);
		Max.y = Math.Max(Max.y, (float)v.y);
		Max.z = Math.Max(Max.z, (float)v.z);
	}

	public void Contain(AxisAlignedBox3d box)
	{
		Min.x = Math.Min(Min.x, (float)box.Min.x);
		Min.y = Math.Min(Min.y, (float)box.Min.y);
		Min.z = Math.Min(Min.z, (float)box.Min.z);
		Max.x = Math.Max(Max.x, (float)box.Max.x);
		Max.y = Math.Max(Max.y, (float)box.Max.y);
		Max.z = Math.Max(Max.z, (float)box.Max.z);
	}

	public AxisAlignedBox3f Intersect(AxisAlignedBox3f box)
	{
		AxisAlignedBox3f result = new AxisAlignedBox3f(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Max(Min.z, box.Min.z), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y), Math.Min(Max.z, box.Max.z));
		if (result.Height <= 0f || result.Width <= 0f || result.Depth <= 0f)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector3f v)
	{
		if (Min.x <= v.x && Min.y <= v.y && Min.z <= v.z && Max.x >= v.x && Max.y >= v.y)
		{
			return Max.z >= v.z;
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox3f box)
	{
		if (!(box.Max.x <= Min.x) && !(box.Min.x >= Max.x) && !(box.Max.y <= Min.y) && !(box.Min.y >= Max.y) && !(box.Max.z <= Min.z))
		{
			return !(box.Min.z >= Max.z);
		}
		return false;
	}

	public double DistanceSquared(Vector3f v)
	{
		float num = ((v.x < Min.x) ? (Min.x - v.x) : ((v.x > Max.x) ? (v.x - Max.x) : 0f));
		float num2 = ((v.y < Min.y) ? (Min.y - v.y) : ((v.y > Max.y) ? (v.y - Max.y) : 0f));
		float num3 = ((v.z < Min.z) ? (Min.z - v.z) : ((v.z > Max.z) ? (v.z - Max.z) : 0f));
		return num * num + num2 * num2 + num3 * num3;
	}

	public float Distance(Vector3f v)
	{
		return (float)Math.Sqrt(DistanceSquared(v));
	}

	public Vector3f NearestPoint(Vector3f v)
	{
		float x = ((v.x < Min.x) ? Min.x : ((v.x > Max.x) ? Max.x : v.x));
		float y = ((v.y < Min.y) ? Min.y : ((v.y > Max.y) ? Max.y : v.y));
		float z = ((v.z < Min.z) ? Min.z : ((v.z > Max.z) ? Max.z : v.z));
		return new Vector3f(x, y, z);
	}

	public void Translate(Vector3f vTranslate)
	{
		Min.Add(vTranslate);
		Max.Add(vTranslate);
	}

	public void MoveMin(Vector3f vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Max.z = vNewMin.z + (Max.z - Min.z);
		Min.Set(vNewMin);
	}

	public void MoveMin(float fNewX, float fNewY, float fNewZ)
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

	public static implicit operator AxisAlignedBox3f(Bounds b)
	{
		return new AxisAlignedBox3f(b.min, b.max);
	}

	public static implicit operator Bounds(AxisAlignedBox3f b)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(b.Min, b.Max);
		return result;
	}
}
