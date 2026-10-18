using System;

namespace g3;

public class SphericalFibonacciPointSet
{
	public int N = 64;

	private static readonly double PHI = (Math.Sqrt(5.0) + 1.0) / 2.0;

	public int Count => N;

	public Vector3d this[int i] => Point(i);

	public SphericalFibonacciPointSet(int n = 64)
	{
		N = n;
	}

	public Vector3d Point(int i)
	{
		double num = (double)i / PHI;
		double num2 = Math.PI * 2.0 * (num - Math.Floor(num));
		double num3 = Math.Cos(num2);
		double num4 = Math.Sin(num2);
		double num5 = 1.0 - (2.0 * (double)i + 1.0) / (double)N;
		double num6 = Math.Sin(Math.Acos(num5));
		return new Vector3d(num3 * num6, num4 * num6, num5);
	}

	public int NearestPoint(Vector3d p, bool bIsNormalized = false)
	{
		if (bIsNormalized)
		{
			return inverseSF(ref p);
		}
		p.Normalize();
		return inverseSF(ref p);
	}

	private double madfrac(double a, double b)
	{
		return a * b + (0.0 - Math.Floor(a * b));
	}

	private int inverseSF(ref Vector3d p)
	{
		double x = Math.Min(Math.Atan2(p.y, p.x), Math.PI);
		double z = p.z;
		double y = Math.Max(2.0, Math.Floor(Math.Log((double)N * Math.PI * Math.Sqrt(5.0) * (1.0 - z * z)) / Math.Log(PHI * PHI)));
		double num = Math.Pow(PHI, y) / Math.Sqrt(5.0);
		double num2 = Math.Round(num);
		double num3 = Math.Round(num * PHI);
		Matrix2d matrix2d = new Matrix2d(Math.PI * 2.0 * madfrac(num2 + 1.0, PHI - 1.0) - Math.PI * 2.0 * (PHI - 1.0), Math.PI * 2.0 * madfrac(num3 + 1.0, PHI - 1.0) - Math.PI * 2.0 * (PHI - 1.0), -2.0 * num2 / (double)N, -2.0 * num3 / (double)N);
		Matrix2d matrix2d2 = matrix2d.Inverse();
		Vector2d vector2d = new Vector2d(x, z - (1.0 - 1.0 / (double)N));
		vector2d = matrix2d2 * vector2d;
		vector2d.x = Math.Floor(vector2d.x);
		vector2d.y = Math.Floor(vector2d.y);
		double num4 = double.PositiveInfinity;
		double num5 = 0.0;
		for (uint num6 = 0u; num6 < 4; num6++)
		{
			Vector2d v = new Vector2d(num6 % 2, num6 / 2) + vector2d;
			z = matrix2d.Row(1).Dot(v) + (1.0 - 1.0 / (double)N);
			z = MathUtil.Clamp(z, -1.0, 1.0) * 2.0 - z;
			double num7 = Math.Floor((double)N * 0.5 - z * (double)N * 0.5);
			x = Math.PI * 2.0 * madfrac(num7, PHI - 1.0);
			z = 1.0 - (2.0 * num7 + 1.0) * (1.0 / (double)N);
			double num8 = Math.Sqrt(1.0 - z * z);
			Vector3d vector3d = new Vector3d(Math.Cos(x) * num8, Math.Sin(x) * num8, z);
			double num9 = Vector3d.Dot(vector3d - p, vector3d - p);
			if (num9 < num4)
			{
				num4 = num9;
				num5 = num7;
			}
		}
		return (int)num5;
	}
}
