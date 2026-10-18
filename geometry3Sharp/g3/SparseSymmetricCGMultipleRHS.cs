using System;

namespace g3;

public class SparseSymmetricCGMultipleRHS
{
	public Action<double[][], double[][]> MultiplyF;

	public Action<double[][], double[][]> PreconditionMultiplyF;

	public double[][] B;

	public double ConvergeTolerance = 1E-08;

	public double[][] X;

	public bool UseXAsInitialGuess = true;

	public int MaxIterations = 1024;

	public int Iterations;

	private double[][] R;

	private double[][] P;

	private double[][] W;

	private double[][] AP;

	private double[][] Z;

	public bool Solve()
	{
		Iterations = 0;
		if (B == null || MultiplyF == null)
		{
			throw new Exception("SparseSymmetricCGMultipleRHS.Solve(): Must set B and MultiplyF!");
		}
		int num = B.Length;
		if (num == 0)
		{
			throw new Exception("SparseSymmetricCGMultipleRHS.Solve(): Need at least one RHS vector in B");
		}
		int num2 = B[0].Length;
		R = BufferUtil.AllocNxM(num, num2);
		P = BufferUtil.AllocNxM(num, num2);
		W = BufferUtil.AllocNxM(num, num2);
		if (X == null || !UseXAsInitialGuess)
		{
			if (X == null)
			{
				X = BufferUtil.AllocNxM(num, num2);
			}
			for (int i = 0; i < num; i++)
			{
				Array.Clear(X[i], 0, num2);
				Array.Copy(B[i], R[i], num2);
			}
		}
		else
		{
			InitializeR(R);
		}
		double[] array = new double[num];
		for (int k = 0; k < num; k++)
		{
			array[k] = BufferUtil.Dot(B[k], B[k]);
		}
		double[] array2 = new double[num];
		for (int l = 0; l < num; l++)
		{
			array2[l] = Math.Sqrt(array[l]);
		}
		double[] rho0 = new double[num];
		for (int m = 0; m < num; m++)
		{
			rho0[m] = BufferUtil.Dot(R[m], R[m]);
		}
		bool[] converged = new bool[num];
		int num3 = 0;
		for (int n = 0; n < num; n++)
		{
			converged[n] = rho0[n] < ConvergeTolerance * array2[n];
			if (converged[n])
			{
				num3++;
			}
		}
		if (num3 == num)
		{
			return true;
		}
		for (int num4 = 0; num4 < num; num4++)
		{
			Array.Copy(R[num4], P[num4], num2);
		}
		MultiplyF(P, W);
		double[] alpha = new double[num];
		for (int num5 = 0; num5 < num; num5++)
		{
			alpha[num5] = rho0[num5] / BufferUtil.Dot(P[num5], W[num5]);
		}
		for (int num6 = 0; num6 < num; num6++)
		{
			BufferUtil.MultiplyAdd(X[num6], alpha[num6], P[num6]);
		}
		for (int num7 = 0; num7 < num; num7++)
		{
			BufferUtil.MultiplyAdd(R[num7], 0.0 - alpha[num7], W[num7]);
		}
		double[] rho1 = new double[num];
		for (int num8 = 0; num8 < num; num8++)
		{
			rho1[num8] = BufferUtil.Dot(R[num8], R[num8]);
		}
		double[] array3 = new double[num];
		Interval1i interval1i = Interval1i.Range(num);
		int num9;
		for (num9 = 1; num9 < MaxIterations; num9++)
		{
			bool flag = true;
			for (int num10 = 0; num10 < num; num10++)
			{
				if (!converged[num10] && Math.Sqrt(rho1[num10]) <= ConvergeTolerance * array2[num10])
				{
					converged[num10] = true;
				}
				if (!converged[num10])
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			for (int num11 = 0; num11 < num; num11++)
			{
				array3[num11] = rho1[num11] / rho0[num11];
			}
			UpdateP(P, array3, R, converged);
			MultiplyF(P, W);
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					alpha[j] = rho1[j] / BufferUtil.Dot(P[j], W[j]);
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					BufferUtil.MultiplyAdd(X[j], alpha[j], P[j]);
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					rho0[j] = rho1[j];
					rho1[j] = BufferUtil.MultiplyAdd_GetSqrSum(R[j], 0.0 - alpha[j], W[j]);
				}
			});
		}
		Iterations = num9;
		return num9 < MaxIterations;
	}

	public bool SolvePreconditioned()
	{
		Iterations = 0;
		if (B == null || MultiplyF == null || PreconditionMultiplyF == null)
		{
			throw new Exception("SparseSymmetricCGMultipleRHS.SolvePreconditioned(): Must set B and MultiplyF and PreconditionMultiplyF!");
		}
		int num = B.Length;
		if (num == 0)
		{
			throw new Exception("SparseSymmetricCGMultipleRHS.SolvePreconditioned(): Need at least one RHS vector in B");
		}
		int i = B[0].Length;
		R = BufferUtil.AllocNxM(num, i);
		P = BufferUtil.AllocNxM(num, i);
		AP = BufferUtil.AllocNxM(num, i);
		Z = BufferUtil.AllocNxM(num, i);
		if (X == null || !UseXAsInitialGuess)
		{
			if (X == null)
			{
				X = BufferUtil.AllocNxM(num, i);
			}
			for (int k = 0; k < num; k++)
			{
				Array.Clear(X[k], 0, i);
				Array.Copy(B[k], R[k], i);
			}
		}
		else
		{
			InitializeR(R);
		}
		double[] array = new double[num];
		for (int l = 0; l < num; l++)
		{
			array[l] = BufferUtil.Dot(B[l], B[l]);
		}
		double[] array2 = new double[num];
		for (int m = 0; m < num; m++)
		{
			array2[m] = Math.Sqrt(array[m]);
		}
		MultiplyF(X, R);
		for (int n = 0; n < num; n++)
		{
			for (int num2 = 0; num2 < i; num2++)
			{
				R[n][num2] = B[n][num2] - R[n][num2];
			}
		}
		PreconditionMultiplyF(R, Z);
		for (int num3 = 0; num3 < num; num3++)
		{
			Array.Copy(Z[num3], P[num3], i);
		}
		double[] RdotZ_k = new double[num];
		for (int num4 = 0; num4 < num; num4++)
		{
			RdotZ_k[num4] = BufferUtil.Dot(R[num4], Z[num4]);
		}
		double[] alpha_k = new double[num];
		double[] beta_k = new double[num];
		bool[] converged = new bool[num];
		Interval1i interval1i = Interval1i.Range(num);
		int num5 = 0;
		while (num5++ < MaxIterations)
		{
			bool flag = true;
			for (int num6 = 0; num6 < num; num6++)
			{
				if (!converged[num6] && Math.Sqrt(RdotZ_k[num6]) <= ConvergeTolerance * array2[num6])
				{
					converged[num6] = true;
				}
				if (!converged[num6])
				{
					flag = false;
				}
			}
			if (flag)
			{
				break;
			}
			MultiplyF(P, AP);
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					alpha_k[j] = RdotZ_k[j] / BufferUtil.Dot(P[j], AP[j]);
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					BufferUtil.MultiplyAdd(X[j], alpha_k[j], P[j]);
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					BufferUtil.MultiplyAdd(R[j], 0.0 - alpha_k[j], AP[j]);
				}
			});
			PreconditionMultiplyF(R, Z);
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					beta_k[j] = BufferUtil.Dot(Z[j], R[j]) / RdotZ_k[j];
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					for (int num7 = 0; num7 < i; num7++)
					{
						P[j][num7] = Z[j][num7] + beta_k[j] * P[j][num7];
					}
				}
			});
			gParallel.ForEach(interval1i, delegate(int j)
			{
				if (!converged[j])
				{
					RdotZ_k[j] = BufferUtil.Dot(R[j], Z[j]);
				}
			});
		}
		Iterations = num5;
		return num5 < MaxIterations;
	}

	private void UpdateP(double[][] P, double[] beta, double[][] R, bool[] converged)
	{
		gParallel.ForEach(Interval1i.Range(P.Length), delegate(int j)
		{
			if (!converged[j])
			{
				int num = P[j].Length;
				for (int i = 0; i < num; i++)
				{
					P[j][i] = R[j][i] + beta[j] * P[j][i];
				}
			}
		});
	}

	private void InitializeR(double[][] R)
	{
		MultiplyF(X, R);
		for (int i = 0; i < X.Length; i++)
		{
			int num = R[i].Length;
			for (int j = 0; j < num; j++)
			{
				R[i][j] = B[i][j] - R[i][j];
			}
		}
	}
}
