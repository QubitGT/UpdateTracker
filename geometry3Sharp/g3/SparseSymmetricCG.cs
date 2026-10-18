using System;

namespace g3;

public class SparseSymmetricCG
{
	public Action<double[], double[]> MultiplyF;

	public Action<double[], double[]> PreconditionMultiplyF;

	public double[] B;

	public double[] X;

	public bool UseXAsInitialGuess = true;

	public int MaxIterations = 1024;

	public int Iterations;

	private double[] R;

	private double[] P;

	private double[] AP;

	private double[] Z;

	public bool Solve()
	{
		Iterations = 0;
		int num = B.Length;
		R = new double[num];
		P = new double[num];
		AP = new double[num];
		if (X == null || !UseXAsInitialGuess)
		{
			if (X == null)
			{
				X = new double[num];
			}
			Array.Clear(X, 0, X.Length);
			Array.Copy(B, R, B.Length);
		}
		else
		{
			InitializeR(R);
		}
		double num2 = Math.Sqrt(BufferUtil.Dot(B, B));
		double num3 = BufferUtil.Dot(R, R);
		if (num3 < 1E-08 * num2)
		{
			return true;
		}
		Array.Copy(R, P, R.Length);
		MultiplyF(P, AP);
		double alpha = num3 / BufferUtil.Dot(P, AP);
		BufferUtil.MultiplyAdd(X, alpha, P);
		BufferUtil.MultiplyAdd(R, 0.0 - alpha, AP);
		double num4 = BufferUtil.Dot(R, R);
		int i;
		for (i = 1; i < MaxIterations; i++)
		{
			if (Math.Sqrt(num4) <= 1E-08 * num2)
			{
				break;
			}
			double beta = num4 / num3;
			UpdateP(P, beta, R);
			MultiplyF(P, AP);
			alpha = num4 / BufferUtil.Dot(P, AP);
			double RdotR = 0.0;
			gParallel.Evaluate(delegate
			{
				BufferUtil.MultiplyAdd(X, alpha, P);
			}, delegate
			{
				RdotR = BufferUtil.MultiplyAdd_GetSqrSum(R, 0.0 - alpha, AP);
			});
			num3 = num4;
			num4 = RdotR;
		}
		Iterations = i;
		return i < MaxIterations;
	}

	private void UpdateP(double[] P, double beta, double[] R)
	{
		for (int i = 0; i < P.Length; i++)
		{
			P[i] = R[i] + beta * P[i];
		}
	}

	private void InitializeR(double[] R)
	{
		MultiplyF(X, R);
		for (int i = 0; i < X.Length; i++)
		{
			R[i] = B[i] - R[i];
		}
	}

	public bool SolvePreconditioned()
	{
		Iterations = 0;
		int i = B.Length;
		R = new double[i];
		P = new double[i];
		AP = new double[i];
		Z = new double[i];
		if (X == null || !UseXAsInitialGuess)
		{
			if (X == null)
			{
				X = new double[i];
			}
			Array.Clear(X, 0, X.Length);
			Array.Copy(B, R, B.Length);
		}
		else
		{
			InitializeR(R);
		}
		double num = Math.Sqrt(BufferUtil.Dot(B, B));
		MultiplyF(X, R);
		for (int j = 0; j < i; j++)
		{
			R[j] = B[j] - R[j];
		}
		PreconditionMultiplyF(R, Z);
		Array.Copy(Z, P, i);
		double RdotZ_k = BufferUtil.Dot(R, Z);
		int num2 = 0;
		while (num2++ < MaxIterations)
		{
			if (Math.Sqrt(RdotZ_k) <= 1E-08 * num)
			{
				break;
			}
			MultiplyF(P, AP);
			double alpha_k = RdotZ_k / BufferUtil.Dot(P, AP);
			gParallel.Evaluate(delegate
			{
				BufferUtil.MultiplyAdd(X, alpha_k, P);
			}, delegate
			{
				BufferUtil.MultiplyAdd(R, 0.0 - alpha_k, AP);
			});
			PreconditionMultiplyF(R, Z);
			double beta_k = BufferUtil.Dot(Z, R) / RdotZ_k;
			gParallel.Evaluate(delegate
			{
				for (int k = 0; k < i; k++)
				{
					P[k] = Z[k] + beta_k * P[k];
				}
			}, delegate
			{
				RdotZ_k = BufferUtil.Dot(R, Z);
			});
		}
		Iterations = num2;
		return num2 < MaxIterations;
	}
}
