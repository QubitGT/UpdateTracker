using System;
using System.Collections.Generic;

namespace g3;

public static class FastTriWinding
{
	public static void ComputeCoeffs(DMesh3 mesh, IEnumerable<int> triangles, ref Vector3d p, ref double r, ref Vector3d order1, ref Matrix3d order2, MeshTriInfoCache triCache = null)
	{
		p = Vector3d.Zero;
		order1 = Vector3d.Zero;
		order2 = Matrix3d.Zero;
		r = 0.0;
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		double num = 0.0;
		foreach (int triangle in triangles)
		{
			if (triCache != null)
			{
				double num2 = triCache.Areas[triangle];
				num += num2;
				p += num2 * triCache.Centroids[triangle];
			}
			else
			{
				mesh.GetTriVertices(triangle, ref v, ref v2, ref v3);
				double num3 = MathUtil.Area(ref v, ref v2, ref v3);
				num += num3;
				p += num3 * ((v + v2 + v3) / 3.0);
			}
		}
		p /= num;
		Vector3d n = Vector3d.Zero;
		Vector3d c = Vector3d.Zero;
		double area = 0.0;
		foreach (int triangle2 in triangles)
		{
			mesh.GetTriVertices(triangle2, ref v, ref v2, ref v3);
			if (triCache == null)
			{
				c = 1.0 / 3.0 * (v + v2 + v3);
				n = MathUtil.FastNormalArea(ref v, ref v2, ref v3, out area);
			}
			else
			{
				triCache.GetTriInfo(triangle2, ref n, ref area, ref c);
			}
			order1 += area * n;
			Vector3d u = c - p;
			order2 += area * new Matrix3d(ref u, ref n);
			double d = MathUtil.Max(v.DistanceSquared(ref p), v2.DistanceSquared(ref p), v3.DistanceSquared(ref p));
			r = Math.Max(r, Math.Sqrt(d));
		}
	}

	public static double EvaluateOrder1Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Vector3d q)
	{
		Vector3d vector3d = center - q;
		double length = vector3d.Length;
		return 1.0 / (4.0 * Math.PI) * order1Coeff.Dot(vector3d / (length * length * length));
	}

	public static double EvaluateOrder2Approx(ref Vector3d center, ref Vector3d order1Coeff, ref Matrix3d order2Coeff, ref Vector3d q)
	{
		Vector3d v = center - q;
		double length = v.Length;
		double num = length * length * length;
		double num2 = 1.0 / (Math.PI * 4.0 * num);
		double num3 = num2 * order1Coeff.Dot(ref v);
		double num4 = -3.0 / (Math.PI * 4.0 * num * length * length);
		Matrix3d m = new Matrix3d(num2 + num4 * v.x * v.x, num4 * v.x * v.y, num4 * v.x * v.z, num4 * v.y * v.x, num2 + num4 * v.y * v.y, num4 * v.y * v.z, num4 * v.z * v.x, num4 * v.z * v.y, num2 + num4 * v.z * v.z);
		double num5 = order2Coeff.InnerProduct(ref m);
		return num3 + num5;
	}

	public static double Order1Approx(ref Triangle3d t, ref Vector3d p, ref Vector3d xn, ref double xA, ref Vector3d q)
	{
		Vector3d vector3d = xA * xn;
		Vector3d vector3d2 = p - q;
		double length = vector3d2.Length;
		return 1.0 / (4.0 * Math.PI) * vector3d.Dot(vector3d2 / (length * length * length));
	}

	public static double Order2Approx(ref Triangle3d t, ref Vector3d p, ref Vector3d xn, ref double xA, ref Vector3d q)
	{
		Vector3d u = p - q;
		double length = u.Length;
		double num = length * length * length;
		double num2 = xA / (Math.PI * 4.0) * xn.Dot(u / num);
		Matrix3d matrix3d = new Matrix3d(ref u, ref u);
		matrix3d *= 3.0 / (Math.PI * 4.0 * num * length * length);
		double num3 = 1.0 / (Math.PI * 4.0 * num);
		Matrix3d m = new Matrix3d(num3, num3, num3) - matrix3d;
		Vector3d u2 = new Vector3d((t.V0.x + t.V1.x + t.V2.x) / 3.0, (t.V0.y + t.V1.y + t.V2.y) / 3.0, (t.V0.z + t.V1.z + t.V2.z) / 3.0) - p;
		Matrix3d matrix3d2 = new Matrix3d(ref u2, ref xn);
		double num4 = xA * matrix3d2.InnerProduct(ref m);
		return num2 + num4;
	}
}
