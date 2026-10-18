using System.Collections.Generic;

namespace g3;

public class OrthogonalPlaneFit3
{
	public Vector3d Origin;

	public Vector3d Normal;

	public bool ResultValid;

	public OrthogonalPlaneFit3(IEnumerable<Vector3d> points)
	{
		Origin = Vector3d.Zero;
		int num = 0;
		foreach (Vector3d point in points)
		{
			Origin += point;
			num++;
		}
		double num2 = 1.0 / (double)num;
		Origin *= num2;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		double num6 = 0.0;
		double num7 = 0.0;
		double num8 = 0.0;
		foreach (Vector3d point2 in points)
		{
			Vector3d vector3d = point2 - Origin;
			num3 += vector3d[0] * vector3d[0];
			num4 += vector3d[0] * vector3d[1];
			num5 += vector3d[0] * vector3d[2];
			num6 += vector3d[1] * vector3d[1];
			num7 += vector3d[1] * vector3d[2];
			num8 += vector3d[2] * vector3d[2];
		}
		num3 *= num2;
		num4 *= num2;
		num5 *= num2;
		num6 *= num2;
		num7 *= num2;
		num8 *= num2;
		double[] input = new double[9] { num3, num4, num5, num4, num6, num7, num5, num7, num8 };
		SymmetricEigenSolver symmetricEigenSolver = new SymmetricEigenSolver(3, 4096);
		int num9 = symmetricEigenSolver.Solve(input, SymmetricEigenSolver.SortType.Decreasing);
		ResultValid = num9 > 0 && num9 < int.MaxValue;
		Normal = new Vector3d(symmetricEigenSolver.GetEigenvector(2));
	}
}
