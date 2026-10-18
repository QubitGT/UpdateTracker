using System;
using System.Collections.Generic;

namespace g3;

public struct Box3d
{
	public Vector3d Center;

	public Vector3d AxisX;

	public Vector3d AxisY;

	public Vector3d AxisZ;

	public Vector3d Extent;

	public static readonly Box3d Empty = new Box3d(Vector3d.Zero);

	public static readonly Box3d UnitZeroCentered = new Box3d(Vector3d.Zero, 0.5 * Vector3d.One);

	public static readonly Box3d UnitPositive = new Box3d(0.5 * Vector3d.One, 0.5 * Vector3d.One);

	public double MaxExtent => Math.Max(Extent.x, Math.Max(Extent.y, Extent.z));

	public double MinExtent => Math.Min(Extent.x, Math.Max(Extent.y, Extent.z));

	public Vector3d Diagonal => Extent.x * AxisX + Extent.y * AxisY + Extent.z * AxisZ - ((0.0 - Extent.x) * AxisX - Extent.y * AxisY - Extent.z * AxisZ);

	public double Volume => 2.0 * Extent.x * 2.0 * Extent.y * 2.0 * Extent.z;

	public Box3d(Vector3d center)
	{
		Center = center;
		AxisX = Vector3d.AxisX;
		AxisY = Vector3d.AxisY;
		AxisZ = Vector3d.AxisZ;
		Extent = Vector3d.Zero;
	}

	public Box3d(Vector3d center, Vector3d x, Vector3d y, Vector3d z, Vector3d extent)
	{
		Center = center;
		AxisX = x;
		AxisY = y;
		AxisZ = z;
		Extent = extent;
	}

	public Box3d(Vector3d center, Vector3d extent)
	{
		Center = center;
		Extent = extent;
		AxisX = Vector3d.AxisX;
		AxisY = Vector3d.AxisY;
		AxisZ = Vector3d.AxisZ;
	}

	public Box3d(AxisAlignedBox3d aaBox)
	{
		Extent = new Vector3f(aaBox.Width * 0.5, aaBox.Height * 0.5, aaBox.Depth * 0.5);
		Center = aaBox.Center;
		AxisX = Vector3d.AxisX;
		AxisY = Vector3d.AxisY;
		AxisZ = Vector3d.AxisZ;
	}

	public Box3d(Frame3f frame, Vector3d extent)
	{
		Center = frame.Origin;
		AxisX = frame.X;
		AxisY = frame.Y;
		AxisZ = frame.Z;
		Extent = extent;
	}

	public Box3d(Segment3d seg)
	{
		Center = seg.Center;
		AxisZ = seg.Direction;
		Vector3d.MakePerpVectors(ref AxisZ, out AxisX, out AxisY);
		Extent = new Vector3d(0.0, 0.0, seg.Extent);
	}

	public Vector3d Axis(int i)
	{
		return i switch
		{
			1 => AxisY, 
			0 => AxisX, 
			_ => AxisZ, 
		};
	}

	public Vector3d[] ComputeVertices()
	{
		Vector3d[] array = new Vector3d[8];
		ComputeVertices(array);
		return array;
	}

	public void ComputeVertices(Vector3d[] vertex)
	{
		Vector3d vector3d = Extent.x * AxisX;
		Vector3d vector3d2 = Extent.y * AxisY;
		Vector3d vector3d3 = Extent.z * AxisZ;
		vertex[0] = Center - vector3d - vector3d2 - vector3d3;
		vertex[1] = Center + vector3d - vector3d2 - vector3d3;
		vertex[2] = Center + vector3d + vector3d2 - vector3d3;
		vertex[3] = Center - vector3d + vector3d2 - vector3d3;
		vertex[4] = Center - vector3d - vector3d2 + vector3d3;
		vertex[5] = Center + vector3d - vector3d2 + vector3d3;
		vertex[6] = Center + vector3d + vector3d2 + vector3d3;
		vertex[7] = Center - vector3d + vector3d2 + vector3d3;
	}

	public IEnumerable<Vector3d> VerticesItr()
	{
		Vector3d extAxis0 = Extent.x * AxisX;
		Vector3d extAxis1 = Extent.y * AxisY;
		Vector3d extAxis2 = Extent.z * AxisZ;
		yield return Center - extAxis0 - extAxis1 - extAxis2;
		yield return Center + extAxis0 - extAxis1 - extAxis2;
		yield return Center + extAxis0 + extAxis1 - extAxis2;
		yield return Center - extAxis0 + extAxis1 - extAxis2;
		yield return Center - extAxis0 - extAxis1 + extAxis2;
		yield return Center + extAxis0 - extAxis1 + extAxis2;
		yield return Center + extAxis0 + extAxis1 + extAxis2;
		yield return Center - extAxis0 + extAxis1 + extAxis2;
	}

	public AxisAlignedBox3d ToAABB()
	{
		Vector3d vector3d = Extent.x * AxisX;
		Vector3d vector3d2 = Extent.y * AxisY;
		Vector3d vector3d3 = Extent.z * AxisZ;
		AxisAlignedBox3d result = new AxisAlignedBox3d(Center - vector3d - vector3d2 - vector3d3);
		result.Contain(Center + vector3d - vector3d2 - vector3d3);
		result.Contain(Center + vector3d + vector3d2 - vector3d3);
		result.Contain(Center - vector3d + vector3d2 - vector3d3);
		result.Contain(Center - vector3d - vector3d2 + vector3d3);
		result.Contain(Center + vector3d - vector3d2 + vector3d3);
		result.Contain(Center + vector3d + vector3d2 + vector3d3);
		result.Contain(Center - vector3d + vector3d2 + vector3d3);
		return result;
	}

	public Vector3d Corner(int i)
	{
		return Center + ((((i & 1) != 0) ^ ((i & 2) != 0)) ? (Extent.x * AxisX) : ((0.0 - Extent.x) * AxisX)) + ((i / 2 % 2 == 0) ? ((0.0 - Extent.y) * AxisY) : (Extent.y * AxisY)) + ((i < 4) ? ((0.0 - Extent.z) * AxisZ) : (Extent.z * AxisZ));
	}

	public void Contain(Vector3d v)
	{
		Vector3d vector3d = v - Center;
		for (int i = 0; i < 3; i++)
		{
			double num = vector3d.Dot(Axis(i));
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

	public void Contain(IEnumerable<Vector3d> points)
	{
		IEnumerator<Vector3d> enumerator = points.GetEnumerator();
		enumerator.MoveNext();
		Vector3d vector3d = enumerator.Current - Center;
		Vector3d vector3d2 = new Vector3d(vector3d.Dot(AxisX), vector3d.Dot(AxisY), vector3d.Dot(AxisZ));
		Vector3d vector3d3 = vector3d2;
		while (enumerator.MoveNext())
		{
			vector3d = enumerator.Current - Center;
			double num = vector3d.Dot(AxisX);
			if (num < vector3d2[0])
			{
				vector3d2[0] = num;
			}
			else if (num > vector3d3[0])
			{
				vector3d3[0] = num;
			}
			double num2 = vector3d.Dot(AxisY);
			if (num2 < vector3d2[1])
			{
				vector3d2[1] = num2;
			}
			else if (num2 > vector3d3[1])
			{
				vector3d3[1] = num2;
			}
			double num3 = vector3d.Dot(AxisZ);
			if (num3 < vector3d2[2])
			{
				vector3d2[2] = num3;
			}
			else if (num3 > vector3d3[2])
			{
				vector3d3[2] = num3;
			}
		}
		for (int i = 0; i < 3; i++)
		{
			Center += 0.5 * (vector3d2[i] + vector3d3[i]) * Axis(i);
			Extent[i] = 0.5 * (vector3d3[i] - vector3d2[i]);
		}
	}

	public void Contain(Box3d o)
	{
		Vector3d[] array = o.ComputeVertices();
		for (int i = 0; i < 8; i++)
		{
			Contain(array[i]);
		}
	}

	public bool Contains(Vector3d v)
	{
		Vector3d vector3d = v - Center;
		if (Math.Abs(vector3d.Dot(AxisX)) <= Extent.x && Math.Abs(vector3d.Dot(AxisY)) <= Extent.y)
		{
			return Math.Abs(vector3d.Dot(AxisZ)) <= Extent.z;
		}
		return false;
	}

	public void Expand(double f)
	{
		Extent += f;
	}

	public void Translate(Vector3d v)
	{
		Center += v;
	}

	public void Scale(Vector3d s)
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

	public void ScaleExtents(Vector3d s)
	{
		Extent *= s;
	}

	public double DistanceSquared(Vector3d v)
	{
		v -= Center;
		double num = 0.0;
		Vector3d vector3d = default(Vector3d);
		for (int i = 0; i < 3; i++)
		{
			vector3d[i] = Axis(i).Dot(ref v);
			if (vector3d[i] < 0.0 - Extent[i])
			{
				double num2 = vector3d[i] + Extent[i];
				num += num2 * num2;
				vector3d[i] = 0.0 - Extent[i];
			}
			else if (vector3d[i] > Extent[i])
			{
				double num2 = vector3d[i] - Extent[i];
				num += num2 * num2;
				vector3d[i] = Extent[i];
			}
		}
		return num;
	}

	public Vector3d ClosestPoint(Vector3d v)
	{
		v -= Center;
		double num = 0.0;
		Vector3d vector3d = default(Vector3d);
		for (int i = 0; i < 3; i++)
		{
			vector3d[i] = Axis(i).Dot(ref v);
			double num2 = Extent[i];
			if (vector3d[i] < 0.0 - num2)
			{
				double num3 = vector3d[i] + num2;
				num += num3 * num3;
				vector3d[i] = 0.0 - num2;
			}
			else if (vector3d[i] > num2)
			{
				double num3 = vector3d[i] - num2;
				num += num3 * num3;
				vector3d[i] = num2;
			}
		}
		return Center + vector3d.x * AxisX + vector3d.y * AxisY + vector3d.z * AxisZ;
	}

	public static Box3d Merge(ref Box3d box0, ref Box3d box1)
	{
		Box3d result = default(Box3d);
		result.Center = 0.5 * (box0.Center + box1.Center);
		Quaterniond quaterniond = default(Quaterniond);
		Quaterniond quaterniond2 = default(Quaterniond);
		Matrix3d rot = new Matrix3d(ref box0.AxisX, ref box0.AxisY, ref box0.AxisZ, bRows: false);
		quaterniond.SetFromRotationMatrix(ref rot);
		Matrix3d rot2 = new Matrix3d(ref box1.AxisX, ref box1.AxisY, ref box1.AxisZ, bRows: false);
		quaterniond2.SetFromRotationMatrix(ref rot2);
		if (quaterniond.Dot(quaterniond2) < 0.0)
		{
			quaterniond2 = -quaterniond2;
		}
		Quaterniond quaterniond3 = quaterniond + quaterniond2;
		double num = 1.0 / Math.Sqrt(quaterniond3.Dot(quaterniond3));
		Matrix3d matrix3d = (quaterniond3 * num).ToRotationMatrix();
		result.AxisX = matrix3d.Column(0);
		result.AxisY = matrix3d.Column(1);
		result.AxisZ = matrix3d.Column(2);
		Vector3d[] array = new Vector3d[8];
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		box0.ComputeVertices(array);
		for (int i = 0; i < 8; i++)
		{
			Vector3d v = array[i] - result.Center;
			for (int j = 0; j < 3; j++)
			{
				double num2 = result.Axis(j).Dot(ref v);
				if (num2 > zero2[j])
				{
					zero2[j] = num2;
				}
				else if (num2 < zero[j])
				{
					zero[j] = num2;
				}
			}
		}
		box1.ComputeVertices(array);
		for (int i = 0; i < 8; i++)
		{
			Vector3d v2 = array[i] - result.Center;
			for (int j = 0; j < 3; j++)
			{
				double num2 = result.Axis(j).Dot(ref v2);
				if (num2 > zero2[j])
				{
					zero2[j] = num2;
				}
				else if (num2 < zero[j])
				{
					zero[j] = num2;
				}
			}
		}
		for (int j = 0; j < 3; j++)
		{
			result.Center += 0.5 * (zero2[j] + zero[j]) * result.Axis(j);
			result.Extent[j] = 0.5 * (zero2[j] - zero[j]);
		}
		return result;
	}

	public static implicit operator Box3d(Box3f v)
	{
		return new Box3d(v.Center, v.AxisX, v.AxisY, v.AxisZ, v.Extent);
	}

	public static explicit operator Box3f(Box3d v)
	{
		return new Box3f((Vector3f)v.Center, (Vector3f)v.AxisX, (Vector3f)v.AxisY, (Vector3f)v.AxisZ, (Vector3f)v.Extent);
	}
}
