using System;

namespace Technie.PhysicsCreator.QHull;

public class Vector3d
{
	private const double DOUBLE_PREC = 2.220446049250313E-16;

	public double x;

	public double y;

	public double z;

	public Vector3d()
	{
	}

	public Vector3d(Vector3d v)
	{
		set(v);
	}

	public Vector3d(double x, double y, double z)
	{
		set(x, y, z);
	}

	public double get(int i)
	{
		return i switch
		{
			0 => x, 
			1 => y, 
			2 => z, 
			_ => throw new IndexOutOfRangeException(i.ToString() ?? ""), 
		};
	}

	public void set(int i, double value)
	{
		switch (i)
		{
		case 0:
			x = value;
			break;
		case 1:
			y = value;
			break;
		case 2:
			z = value;
			break;
		default:
			throw new IndexOutOfRangeException(i.ToString() ?? "");
		}
	}

	public void set(Vector3d v1)
	{
		x = v1.x;
		y = v1.y;
		z = v1.z;
	}

	public void add(Vector3d v1, Vector3d v2)
	{
		x = v1.x + v2.x;
		y = v1.y + v2.y;
		z = v1.z + v2.z;
	}

	public void add(Vector3d v1)
	{
		x += v1.x;
		y += v1.y;
		z += v1.z;
	}

	public void sub(Vector3d v1, Vector3d v2)
	{
		x = v1.x - v2.x;
		y = v1.y - v2.y;
		z = v1.z - v2.z;
	}

	public void sub(Vector3d v1)
	{
		x -= v1.x;
		y -= v1.y;
		z -= v1.z;
	}

	public void scale(double s)
	{
		x = s * x;
		y = s * y;
		z = s * z;
	}

	public void scale(double s, Vector3d v1)
	{
		x = s * v1.x;
		y = s * v1.y;
		z = s * v1.z;
	}

	public double norm()
	{
		return Math.Sqrt(x * x + y * y + z * z);
	}

	public double normSquared()
	{
		return x * x + y * y + z * z;
	}

	public double distance(Vector3d v)
	{
		double num = x - v.x;
		double num2 = y - v.y;
		double num3 = z - v.z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public double distanceSquared(Vector3d v)
	{
		double num = x - v.x;
		double num2 = y - v.y;
		double num3 = z - v.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public double dot(Vector3d v1)
	{
		return x * v1.x + y * v1.y + z * v1.z;
	}

	public void normalize()
	{
		double num = x * x + y * y + z * z;
		double num2 = num - 1.0;
		if (num2 > 4.440892098500626E-16 || num2 < -4.440892098500626E-16)
		{
			double num3 = Math.Sqrt(num);
			x /= num3;
			y /= num3;
			z /= num3;
		}
	}

	public void setZero()
	{
		x = 0.0;
		y = 0.0;
		z = 0.0;
	}

	public void set(double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public void cross(Vector3d v1, Vector3d v2)
	{
		double num = v1.y * v2.z - v1.z * v2.y;
		double num2 = v1.z * v2.x - v1.x * v2.z;
		double num3 = v1.x * v2.y - v1.y * v2.x;
		x = num;
		y = num2;
		z = num3;
	}

	protected void setRandom(double lower, double upper, Random generator)
	{
		double num = upper - lower;
		x = generator.NextDouble() * num + lower;
		y = generator.NextDouble() * num + lower;
		z = generator.NextDouble() * num + lower;
	}

	public string toString()
	{
		return x + " " + y + " " + z;
	}
}
