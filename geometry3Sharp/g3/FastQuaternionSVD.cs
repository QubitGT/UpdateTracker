using System;

namespace g3;

public class FastQuaternionSVD
{
	private int NumJacobiIterations = 4;

	public Quaterniond U;

	public Quaterniond V;

	public Vector3d S;

	private SymmetricMatrix3d ATA;

	private double[] AV;

	private const double gamma = 5.82842712474619;

	private const double sinBackup = 0.3826834323650897;

	private const double cosBackup = 0.9238795325112867;

	public FastQuaternionSVD()
	{
	}

	public FastQuaternionSVD(Matrix3d matrix, double epsilon = 2.220446049250313E-16, int jacobiIters = 4)
	{
		Solve(matrix, epsilon, jacobiIters);
	}

	public void Solve(Matrix3d matrix, double epsilon = 2.220446049250313E-16, int jacobiIters = -1)
	{
		if (jacobiIters != -1)
		{
			NumJacobiIterations = jacobiIters;
		}
		if (ATA == null)
		{
			ATA = new SymmetricMatrix3d();
		}
		ATA.SetATA(ref matrix);
		Vector4d V = jacobiDiagonalize(ATA);
		if (AV == null)
		{
			AV = new double[9];
		}
		computeAV(ref matrix, ref V, AV);
		Vector4d U = Vector4d.Zero;
		QRFactorize(AV, ref V, epsilon, ref S, ref U);
		this.U = new Quaterniond(U[1], U[2], U[3], U[0]);
		this.V = new Quaterniond(V[1], V[2], V[3], V[0]);
	}

	public Matrix3d ReconstructMatrix()
	{
		Matrix3d matrix3d = new Matrix3d(S[0], S[1], S[2]);
		return U.ToRotationMatrix() * matrix3d * V.Conjugate().ToRotationMatrix();
	}

	private Vector4d jacobiDiagonalize(SymmetricMatrix3d ATA)
	{
		Vector4d lhs = new Vector4d(1.0, 0.0, 0.0, 0.0);
		for (int i = 0; i < NumJacobiIterations; i++)
		{
			Vector2d vector2d = givensAngles(ATA, 0, 1);
			ATA.quatConjugate01(vector2d.x, vector2d.y);
			quatTimesEqualCoordinateAxis(ref lhs, vector2d.x, vector2d.y, 2);
			vector2d = givensAngles(ATA, 1, 2);
			ATA.quatConjugate12(vector2d.x, vector2d.y);
			quatTimesEqualCoordinateAxis(ref lhs, vector2d.x, vector2d.y, 0);
			vector2d = givensAngles(ATA, 0, 2);
			ATA.quatConjugate02(vector2d.x, vector2d.y);
			quatTimesEqualCoordinateAxis(ref lhs, vector2d.x, vector2d.y, 1);
		}
		return lhs;
	}

	private Vector2d givensAngles(SymmetricMatrix3d B, int p, int q)
	{
		double num = 0.0;
		double num2 = 0.0;
		switch (p)
		{
		case 0:
			if (q == 1)
			{
				num = B.entries[p] - B.entries[q];
				num2 = 0.5 * B.entries[3];
			}
			else
			{
				num = B.entries[q] - B.entries[p];
				num2 = 0.5 * B.entries[4];
			}
			break;
		case 1:
			num = B.entries[p] - B.entries[q];
			num2 = 0.5 * B.entries[5];
			break;
		}
		double num3 = 1.0 / Math.Sqrt(num * num + num2 * num2);
		num *= num3;
		num2 *= num3;
		bool num4 = 5.82842712474619 * num2 * num2 < num * num;
		num = (num4 ? num : 0.9238795325112867);
		num2 = (num4 ? num2 : 0.3826834323650897);
		return new Vector2d(num, num2);
	}

	private void computeAV(ref Matrix3d matrix, ref Vector4d V, double[] buf)
	{
		Matrix3d matrix3d = new Quaterniond(V[1], V[2], V[3], V[0]).ToRotationMatrix();
		(matrix * matrix3d).ToBuffer(buf);
	}

	private void QRFactorize(double[] AV, ref Vector4d V, double eps, ref Vector3d S, ref Vector4d U)
	{
		permuteColumns(AV, ref V);
		U = new Vector4d(1.0, 0.0, 0.0, 0.0);
		Vector2d vector2d = computeGivensQR(AV, eps, 1, 0);
		givensQTB2(AV, vector2d.x, vector2d.y);
		quatTimesEqualCoordinateAxis(ref U, vector2d.x, vector2d.y, 2);
		Vector2d vector2d2 = computeGivensQR(AV, eps, 2, 0);
		givensQTB1(AV, vector2d2.x, 0.0 - vector2d2.y);
		quatTimesEqualCoordinateAxis(ref U, vector2d2.x, 0.0 - vector2d2.y, 1);
		Vector2d vector2d3 = computeGivensQR(AV, eps, 2, 1);
		givensQTB0(AV, vector2d3.x, vector2d3.y);
		quatTimesEqualCoordinateAxis(ref U, vector2d3.x, vector2d3.y, 0);
		S = new Vector3d(AV[0], AV[4], AV[8]);
	}

	private Vector2d computeGivensQR(double[] B, double eps, int r, int c)
	{
		double num = B[4 * c];
		double num2 = B[3 * r + c];
		double num3 = Math.Sqrt(num * num + num2 * num2);
		double num4 = ((num3 > eps) ? num2 : 0.0);
		double num5 = Math.Abs(num) + Math.Max(num3, eps);
		if (num < 0.0)
		{
			double num6 = num4;
			num4 = num5;
			num5 = num6;
		}
		double num7 = 1.0 / Math.Sqrt(num5 * num5 + num4 * num4);
		num5 *= num7;
		num4 *= num7;
		return new Vector2d(num5, num4);
	}

	private void givensQTB2(double[] B, double ch, double sh)
	{
		double num = ch * ch - sh * sh;
		double num2 = 2.0 * sh * ch;
		double num3 = B[0] * num + B[3] * num2;
		double num4 = B[1] * num + B[4] * num2;
		double num5 = B[2] * num + B[5] * num2;
		double num6 = 0.0;
		double num7 = B[4] * num - B[1] * num2;
		double num8 = B[5] * num - B[2] * num2;
		B[0] = num3;
		B[1] = num4;
		B[2] = num5;
		B[3] = num6;
		B[4] = num7;
		B[5] = num8;
	}

	private void givensQTB1(double[] B, double ch, double sh)
	{
		double num = ch * ch - sh * sh;
		double num2 = 2.0 * sh * ch;
		double num3 = B[0] * num - B[6] * num2;
		double num4 = B[1] * num - B[7] * num2;
		double num5 = B[2] * num - B[8] * num2;
		double num6 = 0.0;
		double num7 = B[1] * num2 + B[7] * num;
		double num8 = B[2] * num2 + B[8] * num;
		B[0] = num3;
		B[1] = num4;
		B[2] = num5;
		B[6] = num6;
		B[7] = num7;
		B[8] = num8;
	}

	private void givensQTB0(double[] B, double ch, double sh)
	{
		double num = ch * ch - sh * sh;
		double num2 = 2.0 * ch * sh;
		double num3 = B[4] * num + B[7] * num2;
		double num4 = B[8] * num - B[5] * num2;
		B[4] = num3;
		B[8] = num4;
	}

	private void quatTimesEqualCoordinateAxis(ref Vector4d lhs, double c, double s, int i)
	{
		double x = lhs.x * c - lhs[i + 1] * s;
		Vector3d vector3d = new Vector3d(c * lhs.y, c * lhs.z, c * lhs.w);
		vector3d[i] += lhs.x * s;
		vector3d[(i + 1) % 3] += s * lhs[1 + (i + 2) % 3];
		vector3d[(i + 2) % 3] -= s * lhs[1 + (i + 1) % 3];
		lhs.x = x;
		lhs.y = vector3d.x;
		lhs.z = vector3d.y;
		lhs.w = vector3d.z;
	}

	private void permuteColumns(double[] B, ref Vector4d V)
	{
		double num = B[0] * B[0] + B[3] * B[3] + B[6] * B[6];
		double num2 = B[1] * B[1] + B[4] * B[4] + B[7] * B[7];
		double num3 = B[2] * B[2] + B[5] * B[5] + B[8] * B[8];
		if (num < num2)
		{
			swapColsNeg(B, 0, 1);
			quatTimesEqualCoordinateAxis(ref V, 0.7071067811865475, 0.7071067811865475, 2);
			double num4 = num;
			num = num2;
			num2 = num4;
		}
		if (num < num3)
		{
			swapColsNeg(B, 0, 2);
			quatTimesEqualCoordinateAxis(ref V, 0.7071067811865475, -0.7071067811865475, 1);
			double num5 = num;
			num = num3;
			num3 = num5;
		}
		if (num2 < num3)
		{
			swapColsNeg(B, 1, 2);
			quatTimesEqualCoordinateAxis(ref V, 0.7071067811865475, 0.7071067811865475, 0);
		}
	}

	private void swapColsNeg(double[] B, int i, int j)
	{
		double num = 0.0 - B[i];
		B[i] = B[j];
		B[j] = num;
		num = 0.0 - B[i + 3];
		B[i + 3] = B[j + 3];
		B[j + 3] = num;
		num = 0.0 - B[i + 6];
		B[i + 6] = B[j + 6];
		B[j + 6] = num;
	}
}
