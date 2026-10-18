using System;
using System.Collections.Generic;

namespace g3;

public static class FastPointWinding
{
	public static void ComputeCoeffs(IPointSet pointSet, IEnumerable<int> points, double[] pointAreas, ref Vector3d p, ref double r, ref Vector3d order1, ref Matrix3d order2)
	{
		if (!pointSet.HasVertexNormals)
		{
			throw new Exception("FastPointWinding.ComputeCoeffs: point set does not have normals!");
		}
		p = Vector3d.Zero;
		order1 = Vector3d.Zero;
		order2 = Matrix3d.Zero;
		r = 0.0;
		double num = 0.0;
		foreach (int point in points)
		{
			num += pointAreas[point];
			p += pointAreas[point] * pointSet.GetVertex(point);
		}
		p /= num;
		foreach (int point2 in points)
		{
			Vector3d vertex = pointSet.GetVertex(point2);
			Vector3d v = pointSet.GetVertexNormal(point2);
			double num2 = pointAreas[point2];
			order1 += num2 * v;
			Vector3d u = vertex - p;
			order2 += num2 * new Matrix3d(ref u, ref v);
			r = Math.Max(r, vertex.Distance(p));
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

	public static double ExactEval(ref Vector3d x, ref Vector3d xn, double xA, ref Vector3d q)
	{
		Vector3d vector3d = x - q;
		double length = vector3d.Length;
		return xA / (Math.PI * 4.0) * xn.Dot(vector3d / (length * length * length));
	}

	public static double Order1Approx(ref Vector3d x, ref Vector3d p, ref Vector3d xn, double xA, ref Vector3d q)
	{
		Vector3d vector3d = p - q;
		double length = vector3d.Length;
		return xA / (Math.PI * 4.0) * xn.Dot(vector3d / (length * length * length));
	}

	public static double Order2Approx(ref Vector3d x, ref Vector3d p, ref Vector3d xn, double xA, ref Vector3d q)
	{
		Vector3d u = p - q;
		Vector3d u2 = x - p;
		double length = u.Length;
		double num = length * length * length;
		double num2 = xA / (Math.PI * 4.0) * xn.Dot(u / num);
		Matrix3d matrix3d = new Matrix3d(ref u, ref u);
		matrix3d *= 3.0 / (Math.PI * 4.0 * num * length * length);
		double num3 = 1.0 / (Math.PI * 4.0 * num);
		Matrix3d m = new Matrix3d(num3, num3, num3) - matrix3d;
		double num4 = xA * new Matrix3d(ref u2, ref xn).InnerProduct(ref m);
		return num2 + num4;
	}
}
