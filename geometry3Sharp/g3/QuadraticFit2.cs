using System;

namespace g3;

public static class QuadraticFit2
{
	public static double Fit(Vector2d[] points, double[] coefficients)
	{
		DenseMatrix denseMatrix = new DenseMatrix(6, 6);
		int num = points.Length;
		for (int i = 0; i < num; i++)
		{
			double x = points[i].x;
			double y = points[i].y;
			double num2 = x * x;
			double num3 = y * y;
			double num4 = x * y;
			double num5 = x * num2;
			double num6 = x * num3;
			double num7 = x * num4;
			double num8 = y * num3;
			double num9 = x * num5;
			double num10 = x * num6;
			double num11 = x * num7;
			double num12 = y * num8;
			double num13 = x * num8;
			denseMatrix[0, 1] += x;
			denseMatrix[0, 2] += y;
			denseMatrix[0, 3] += num2;
			denseMatrix[0, 4] += num3;
			denseMatrix[0, 5] += num4;
			denseMatrix[1, 3] += num5;
			denseMatrix[1, 4] += num6;
			denseMatrix[1, 5] += num7;
			denseMatrix[2, 4] += num8;
			denseMatrix[3, 3] += num9;
			denseMatrix[3, 4] += num10;
			denseMatrix[3, 5] += num11;
			denseMatrix[4, 4] += num12;
			denseMatrix[4, 5] += num13;
		}
		denseMatrix[0, 0] = num;
		denseMatrix[1, 1] = denseMatrix[0, 3];
		denseMatrix[1, 2] = denseMatrix[0, 5];
		denseMatrix[2, 2] = denseMatrix[0, 4];
		denseMatrix[2, 3] = denseMatrix[1, 5];
		denseMatrix[2, 5] = denseMatrix[1, 4];
		denseMatrix[5, 5] = denseMatrix[3, 4];
		for (int j = 0; j < 6; j++)
		{
			for (int k = 0; k < j; k++)
			{
				denseMatrix[j, k] = denseMatrix[k, j];
			}
		}
		double num14 = 1.0 / (double)num;
		for (int l = 0; l < 6; l++)
		{
			for (int m = 0; m < 6; m++)
			{
				denseMatrix[l, m] *= num14;
			}
		}
		SymmetricEigenSolver symmetricEigenSolver = new SymmetricEigenSolver(6, 1024);
		symmetricEigenSolver.Solve(denseMatrix.Buffer, SymmetricEigenSolver.SortType.Increasing);
		symmetricEigenSolver.GetEigenvector(0, coefficients);
		return Math.Abs(symmetricEigenSolver.GetEigenvalue(0));
	}

	public static double FitCircle2(Vector2d[] points, out Circle2d circle)
	{
		DenseMatrix denseMatrix = new DenseMatrix(4, 4);
		int num = points.Length;
		for (int i = 0; i < num; i++)
		{
			double x = points[i].x;
			double y = points[i].y;
			double num2 = x * x;
			double num3 = y * y;
			double num4 = x * y;
			double num5 = num2 + num3;
			double num6 = x * num5;
			double num7 = y * num5;
			double num8 = num5 * num5;
			denseMatrix[0, 1] += x;
			denseMatrix[0, 2] += y;
			denseMatrix[0, 3] += num5;
			denseMatrix[1, 1] += num2;
			denseMatrix[1, 2] += num4;
			denseMatrix[1, 3] += num6;
			denseMatrix[2, 2] += num3;
			denseMatrix[2, 3] += num7;
			denseMatrix[3, 3] += num8;
		}
		denseMatrix[0, 0] = num;
		for (int j = 0; j < 4; j++)
		{
			for (int k = 0; k < j; k++)
			{
				denseMatrix[j, k] = denseMatrix[k, j];
			}
		}
		double num9 = 1.0 / (double)num;
		for (int l = 0; l < 4; l++)
		{
			for (int m = 0; m < 4; m++)
			{
				denseMatrix[l, m] *= num9;
			}
		}
		SymmetricEigenSolver symmetricEigenSolver = new SymmetricEigenSolver(4, 1024);
		symmetricEigenSolver.Solve(denseMatrix.Buffer, SymmetricEigenSolver.SortType.Increasing);
		double[] array = new double[4];
		symmetricEigenSolver.GetEigenvector(0, array);
		double num10 = 1.0 / array[3];
		Vector3d zero = Vector3d.Zero;
		for (int n = 0; n < 3; n++)
		{
			zero[n] = num10 * array[n];
		}
		Vector2d center = new Vector2d(-0.5 * zero[1], -0.5 * zero[2]);
		double radius = Math.Sqrt(Math.Abs(center.LengthSquared - zero[0]));
		circle = new Circle2d(center, radius);
		return Math.Abs(symmetricEigenSolver.GetEigenvalue(0));
	}
}
