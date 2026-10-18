using System;

namespace g3;

public struct QuadricError
{
	public double Axx;

	public double Axy;

	public double Axz;

	public double Ayy;

	public double Ayz;

	public double Azz;

	public double bx;

	public double by;

	public double bz;

	public double c;

	public static readonly QuadricError Zero = new QuadricError
	{
		Axx = 0.0,
		Axy = 0.0,
		Axz = 0.0,
		Ayy = 0.0,
		Ayz = 0.0,
		Azz = 0.0,
		bx = 0.0,
		by = 0.0,
		bz = 0.0,
		c = 0.0
	};

	public QuadricError(ref Vector3d n, ref Vector3d p)
	{
		Axx = n.x * n.x;
		Axy = n.x * n.y;
		Axz = n.x * n.z;
		Ayy = n.y * n.y;
		Ayz = n.y * n.z;
		Azz = n.z * n.z;
		bx = (by = (bz = (c = 0.0)));
		Vector3d v = multiplyA(ref p);
		bx = 0.0 - v.x;
		by = 0.0 - v.y;
		bz = 0.0 - v.z;
		c = p.Dot(ref v);
	}

	public QuadricError(ref QuadricError a, ref QuadricError b)
	{
		Axx = a.Axx + b.Axx;
		Axy = a.Axy + b.Axy;
		Axz = a.Axz + b.Axz;
		Ayy = a.Ayy + b.Ayy;
		Ayz = a.Ayz + b.Ayz;
		Azz = a.Azz + b.Azz;
		bx = a.bx + b.bx;
		by = a.by + b.by;
		bz = a.bz + b.bz;
		c = a.c + b.c;
	}

	public void Add(double w, ref QuadricError b)
	{
		Axx += w * b.Axx;
		Axy += w * b.Axy;
		Axz += w * b.Axz;
		Ayy += w * b.Ayy;
		Ayz += w * b.Ayz;
		Azz += w * b.Azz;
		bx += w * b.bx;
		by += w * b.by;
		bz += w * b.bz;
		c += w * b.c;
	}

	public double Evaluate(ref Vector3d pt)
	{
		double num = Axx * pt.x + Axy * pt.y + Axz * pt.z;
		double num2 = Axy * pt.x + Ayy * pt.y + Ayz * pt.z;
		double num3 = Axz * pt.x + Ayz * pt.y + Azz * pt.z;
		return pt.x * num + pt.y * num2 + pt.z * num3 + 2.0 * (pt.x * bx + pt.y * by + pt.z * bz) + c;
	}

	private Vector3d multiplyA(ref Vector3d pt)
	{
		double x = Axx * pt.x + Axy * pt.y + Axz * pt.z;
		double y = Axy * pt.x + Ayy * pt.y + Ayz * pt.z;
		double z = Axz * pt.x + Ayz * pt.y + Azz * pt.z;
		return new Vector3d(x, y, z);
	}

	public bool OptimalPoint(ref Vector3d result)
	{
		double num = Azz * Ayy - Ayz * Ayz;
		double num2 = Axz * Ayz - Azz * Axy;
		double num3 = Axy * Ayz - Axz * Ayy;
		double num4 = Azz * Axx - Axz * Axz;
		double num5 = Axy * Axz - Axx * Ayz;
		double num6 = Axx * Ayy - Axy * Axy;
		double num7 = Axx * num + Axy * num2 + Axz * num3;
		if (Math.Abs(num7) > 2.220446049250313E-13)
		{
			num7 = 1.0 / num7;
			num *= num7;
			num2 *= num7;
			num3 *= num7;
			num4 *= num7;
			num5 *= num7;
			num6 *= num7;
			double num8 = num * bx + num2 * by + num3 * bz;
			double num9 = num2 * bx + num4 * by + num5 * bz;
			double num10 = num3 * bx + num5 * by + num6 * bz;
			result = new Vector3d(0.0 - num8, 0.0 - num9, 0.0 - num10);
			return true;
		}
		return false;
	}
}
