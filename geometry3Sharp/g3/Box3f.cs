using System;
using System.Collections.Generic;

namespace g3;

public struct Box3f
{
	public Vector3f Center;

	public Vector3f AxisX;

	public Vector3f AxisY;

	public Vector3f AxisZ;

	public Vector3f Extent;

	public static readonly Box3f Empty = new Box3f(Vector3f.Zero);

	public double MaxExtent => Math.Max(Extent.x, Math.Max(Extent.y, Extent.z));

	public double MinExtent => Math.Min(Extent.x, Math.Max(Extent.y, Extent.z));

	public Vector3f Diagonal => Extent.x * AxisX + Extent.y * AxisY + Extent.z * AxisZ - ((0f - Extent.x) * AxisX - Extent.y * AxisY - Extent.z * AxisZ);

	public double Volume => 2f * Extent.x * 2f * Extent.y * 2f * Extent.z;

	public Box3f(Vector3f center)
	{
		Center = center;
		AxisX = Vector3f.AxisX;
		AxisY = Vector3f.AxisY;
		AxisZ = Vector3f.AxisZ;
		Extent = Vector3f.Zero;
	}

	public Box3f(Vector3f center, Vector3f x, Vector3f y, Vector3f z, Vector3f extent)
	{
		Center = center;
		AxisX = x;
		AxisY = y;
		AxisZ = z;
		Extent = extent;
	}

	public Box3f(Vector3f center, Vector3f extent)
	{
		Center = center;
		Extent = extent;
		AxisX = Vector3f.AxisX;
		AxisY = Vector3f.AxisY;
		AxisZ = Vector3f.AxisZ;
	}

	public Box3f(AxisAlignedBox3f aaBox)
	{
		Extent = new Vector3f(aaBox.Width * 0.5f, aaBox.Height * 0.5f, aaBox.Depth * 0.5f);
		Center = aaBox.Center;
		AxisX = Vector3f.AxisX;
		AxisY = Vector3f.AxisY;
		AxisZ = Vector3f.AxisZ;
	}

	public Vector3f Axis(int i)
	{
		return i switch
		{
			1 => AxisY, 
			0 => AxisX, 
			_ => AxisZ, 
		};
	}

	public Vector3f[] ComputeVertices()
	{
		Vector3f[] array = new Vector3f[8];
		ComputeVertices(array);
		return array;
	}

	public void ComputeVertices(Vector3f[] vertex)
	{
		Vector3f vector3f = Extent.x * AxisX;
		Vector3f vector3f2 = Extent.y * AxisY;
		Vector3f vector3f3 = Extent.z * AxisZ;
		vertex[0] = Center - vector3f - vector3f2 - vector3f3;
		vertex[1] = Center + vector3f - vector3f2 - vector3f3;
		vertex[2] = Center + vector3f + vector3f2 - vector3f3;
		vertex[3] = Center - vector3f + vector3f2 - vector3f3;
		vertex[4] = Center - vector3f - vector3f2 + vector3f3;
		vertex[5] = Center + vector3f - vector3f2 + vector3f3;
		vertex[6] = Center + vector3f + vector3f2 + vector3f3;
		vertex[7] = Center - vector3f + vector3f2 + vector3f3;
	}

	public IEnumerable<Vector3f> VerticesItr()
	{
		Vector3f extAxis0 = Extent.x * AxisX;
		Vector3f extAxis1 = Extent.y * AxisY;
		Vector3f extAxis2 = Extent.z * AxisZ;
		yield return Center - extAxis0 - extAxis1 - extAxis2;
		yield return Center + extAxis0 - extAxis1 - extAxis2;
		yield return Center + extAxis0 + extAxis1 - extAxis2;
		yield return Center - extAxis0 + extAxis1 - extAxis2;
		yield return Center - extAxis0 - extAxis1 + extAxis2;
		yield return Center + extAxis0 - extAxis1 + extAxis2;
		yield return Center + extAxis0 + extAxis1 + extAxis2;
		yield return Center - extAxis0 + extAxis1 + extAxis2;
	}

	public AxisAlignedBox3f ToAABB()
	{
		Vector3f vector3f = Extent.x * AxisX;
		Vector3f vector3f2 = Extent.y * AxisY;
		Vector3f vector3f3 = Extent.z * AxisZ;
		AxisAlignedBox3f result = new AxisAlignedBox3f(Center - vector3f - vector3f2 - vector3f3);
		result.Contain(Center + vector3f - vector3f2 - vector3f3);
		result.Contain(Center + vector3f + vector3f2 - vector3f3);
		result.Contain(Center - vector3f + vector3f2 - vector3f3);
		result.Contain(Center - vector3f - vector3f2 + vector3f3);
		result.Contain(Center + vector3f - vector3f2 + vector3f3);
		result.Contain(Center + vector3f + vector3f2 + vector3f3);
		result.Contain(Center - vector3f + vector3f2 + vector3f3);
		return result;
	}

	public void Contain(Vector3f v)
	{
		Vector3f vector3f = v - Center;
		for (int i = 0; i < 3; i++)
		{
			double num = vector3f.Dot(Axis(i));
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

	public void Contain(Box3f o)
	{
		Vector3f[] array = o.ComputeVertices();
		for (int i = 0; i < 8; i++)
		{
			Contain(array[i]);
		}
	}

	public bool Contains(Vector3f v)
	{
		Vector3f vector3f = v - Center;
		if (Math.Abs(vector3f.Dot(AxisX)) <= Extent.x && Math.Abs(vector3f.Dot(AxisY)) <= Extent.y)
		{
			return Math.Abs(vector3f.Dot(AxisZ)) <= Extent.z;
		}
		return false;
	}

	public void Expand(float f)
	{
		Extent += f;
	}

	public void Translate(Vector3f v)
	{
		Center += v;
	}

	public void Scale(Vector3f s)
	{
		Center *= s;
		Extent *= s;
		AxisX *= s;
		AxisX.Normalize();
		AxisY *= s;
		AxisY.Normalize();
		AxisZ *= s;
		AxisZ.Normalize();
	}

	public void ScaleExtents(Vector3f s)
	{
		Extent *= s;
	}
}
