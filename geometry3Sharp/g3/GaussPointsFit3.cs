using System.Collections.Generic;

namespace g3;

public class GaussPointsFit3
{
	public Box3d Box;

	public bool ResultValid;

	public GaussPointsFit3(IEnumerable<Vector3d> points)
	{
		Box = new Box3d(Vector3d.Zero, Vector3d.One);
		int num = 0;
		foreach (Vector3d point in points)
		{
			Box.Center += point;
			num++;
		}
		double num2 = 1.0 / (double)num;
		Box.Center *= num2;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		foreach (Vector3d point2 in points)
		{
			Vector3d vector3d = point2 - Box.Center;
			num3 += vector3d[0] * vector3d[0];
			num4 += vector3d[0] * vector3d[1];
			num5 += vector3d[0] * vector3d[2];
			num6 += vector3d[1] * vector3d[1];
			num7 += vector3d[1] * vector3d[2];
			num8 += vector3d[2] * vector3d[2];
		}
		do_solve(num3, num4, num5, num6, num7, num8, num2);
	}

	public GaussPointsFit3(IEnumerable<Vector3d> points, IEnumerable<double> weights)
	{
		Box = new Box3d(Vector3d.Zero, Vector3d.One);
		int num = 0;
		double num2 = 0.0;
		IEnumerator<double> enumerator = weights.GetEnumerator();
		foreach (Vector3d point in points)
		{
			enumerator.MoveNext();
			double current2 = enumerator.Current;
			Box.Center += current2 * point;
			num2 += current2;
			num++;
		}
		double num3 = 1.0 / num2;
		Box.Center *= num3;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		double num9 = 0.0;
		enumerator = weights.GetEnumerator();
		foreach (Vector3d point2 in points)
		{
			enumerator.MoveNext();
			double current4 = enumerator.Current;
			current4 *= current4;
			Vector3d vector3d = point2 - Box.Center;
			num4 += current4 * vector3d[0] * vector3d[0];
			num5 += current4 * vector3d[0] * vector3d[1];
			num6 += current4 * vector3d[0] * vector3d[2];
			num7 += current4 * vector3d[1] * vector3d[1];
			num8 += current4 * vector3d[1] * vector3d[2];
			num9 += current4 * vector3d[2] * vector3d[2];
		}
		do_solve(num4, num5, num6, num7, num8, num9, num3 * num3);
	}

	private void do_solve(double sumXX, double sumXY, double sumXZ, double sumYY, double sumYZ, double sumZZ, double invSumMultiplier)
	{
		sumXX *= invSumMultiplier;
		sumXY *= invSumMultiplier;
		sumXZ *= invSumMultiplier;
		sumYY *= invSumMultiplier;
		sumYZ *= invSumMultiplier;
		sumZZ *= invSumMultiplier;
		double[] input = new double[9] { sumXX, sumXY, sumXZ, sumXY, sumYY, sumYZ, sumXZ, sumYZ, sumZZ };
		SymmetricEigenSolver symmetricEigenSolver = new SymmetricEigenSolver(3, 4096);
		int num = symmetricEigenSolver.Solve(input, SymmetricEigenSolver.SortType.Increasing);
		ResultValid = num > 0 && num < int.MaxValue;
		if (ResultValid)
		{
			Box.Extent = new Vector3d(symmetricEigenSolver.GetEigenvalues());
			double[] eigenvectors = symmetricEigenSolver.GetEigenvectors();
			Box.AxisX = new Vector3d(eigenvectors[0], eigenvectors[1], eigenvectors[2]);
			Box.AxisY = new Vector3d(eigenvectors[3], eigenvectors[4], eigenvectors[5]);
			Box.AxisZ = new Vector3d(eigenvectors[6], eigenvectors[7], eigenvectors[8]);
		}
	}
}
