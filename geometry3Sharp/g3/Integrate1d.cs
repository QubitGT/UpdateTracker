using System;

namespace g3;

public static class Integrate1d
{
	private static readonly double[] root = new double[5] { -0.9061798459, -0.5384693101, 0.0, 0.5384693101, 0.9061798459 };

	private static readonly double[] coeff = new double[5] { 0.236926885, 0.4786286705, 0.5688888889, 0.4786286705, 0.236926885 };

	public static double RombergIntegral(int order, double a, double b, Func<double, object, double> function, object userData)
	{
		if (order <= 0)
		{
			throw new Exception("Integrate1d.RombergIntegral: Integration order must be positive\n");
		}
		double[,] array = new double[2, order];
		double num = b - a;
		array[0, 0] = 0.5 * num * (function(a, userData) + function(b, userData));
		int num2 = 2;
		int num3 = 1;
		while (num2 <= order)
		{
			double num4 = 0.0;
			for (int i = 1; i <= num3; i++)
			{
				num4 += function(a + num * ((double)i - 0.5), userData);
			}
			array[1, 0] = 0.5 * (array[0, 0] + num * num4);
			int num5 = 1;
			int num6 = 4;
			while (num5 < num2)
			{
				array[1, num5] = ((double)num6 * array[1, num5 - 1] - array[0, num5 - 1]) / (double)(num6 - 1);
				num5++;
				num6 *= 4;
			}
			for (int i = 0; i < num2; i++)
			{
				array[0, i] = array[1, i];
			}
			num2++;
			num3 *= 2;
			num *= 0.5;
		}
		return array[0, order - 1];
	}

	public static double GaussianQuadrature(double a, double b, Func<double, object, double> function, object userData)
	{
		double num = 0.5 * (b - a);
		double num2 = 0.5 * (b + a);
		double num3 = 0.0;
		for (int i = 0; i < 5; i++)
		{
			num3 += coeff[i] * function(num * root[i] + num2, userData);
		}
		return num3 * num;
	}

	public static double TrapezoidRule(int numSamples, double a, double b, Func<double, object, double> function, object userData)
	{
		if (numSamples < 2)
		{
			throw new Exception("Integrate1d.TrapezoidRule: Must have more than two samples\n");
		}
		double num = (b - a) / (double)(numSamples - 1);
		double num2 = 0.5 * (function(a, userData) + function(b, userData));
		for (int i = 1; i <= numSamples - 2; i++)
		{
			num2 += function(a + (double)i * num, userData);
		}
		return num2 * num;
	}
}
