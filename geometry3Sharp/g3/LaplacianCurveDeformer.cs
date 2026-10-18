using System;
using System.Collections.Generic;

namespace g3;

public class LaplacianCurveDeformer
{
	public struct SoftConstraintV
	{
		public Vector3d Position;

		public double Weight;

		public bool PostFix;
	}

	public DCurve3 Curve;

	public bool SolveX = true;

	public bool SolveY = true;

	public bool SolveZ = true;

	public bool ConvergeFailed;

	private PackedSparseMatrix PackedM;

	private int N;

	private int[] ToCurveV;

	private int[] ToIndex;

	private double[] Px;

	private double[] Py;

	private double[] Pz;

	private int[] nbr_counts;

	private double[] MLx;

	private double[] MLy;

	private double[] MLz;

	private Dictionary<int, SoftConstraintV> SoftConstraints = new Dictionary<int, SoftConstraintV>();

	private bool HavePostFixedConstraints;

	private bool need_solve_update;

	private DiagonalMatrix WeightsM;

	private double[] Cx;

	private double[] Cy;

	private double[] Cz;

	private double[] Bx;

	private double[] By;

	private double[] Bz;

	private DiagonalMatrix Preconditioner;

	public bool UseSoftConstraintNormalEquations = true;

	private double[] Sx;

	private double[] Sy;

	private double[] Sz;

	public LaplacianCurveDeformer(DCurve3 curve)
	{
		Curve = curve;
	}

	public void SetConstraint(int vID, Vector3d targetPos, double weight, bool bForceToFixedPos = false)
	{
		SoftConstraints[vID] = new SoftConstraintV
		{
			Position = targetPos,
			Weight = weight,
			PostFix = bForceToFixedPos
		};
		HavePostFixedConstraints |= bForceToFixedPos;
		need_solve_update = true;
	}

	public bool IsConstrained(int vID)
	{
		return SoftConstraints.ContainsKey(vID);
	}

	public void ClearConstraints()
	{
		SoftConstraints.Clear();
		HavePostFixedConstraints = false;
		need_solve_update = true;
	}

	public void Initialize()
	{
		int vertexCount = Curve.VertexCount;
		ToCurveV = new int[vertexCount];
		ToIndex = new int[vertexCount];
		N = 0;
		for (int i = 0; i < vertexCount; i++)
		{
			int num = i;
			ToCurveV[N] = num;
			ToIndex[num] = N;
			N++;
		}
		Px = new double[N];
		Py = new double[N];
		Pz = new double[N];
		nbr_counts = new int[N];
		SymmetricSparseMatrix symmetricSparseMatrix = new SymmetricSparseMatrix();
		for (int j = 0; j < N; j++)
		{
			int i2 = ToCurveV[j];
			Vector3d vertex = Curve.GetVertex(i2);
			Px[j] = vertex.x;
			Py[j] = vertex.y;
			Pz[j] = vertex.z;
			nbr_counts[j] = ((j == 0 || j == N - 1) ? 1 : 2);
		}
		for (int k = 0; k < N; k++)
		{
			int num2 = ToCurveV[k];
			_ = nbr_counts[k];
			Index2i index2i = Curve.Neighbours(num2);
			double num3 = 0.0;
			for (int l = 0; l < 2; l++)
			{
				int num4 = index2i[l];
				if (num4 != -1)
				{
					int num5 = ToIndex[num4];
					_ = nbr_counts[num5];
					double num6 = -1.0;
					symmetricSparseMatrix.Set(k, num5, num6);
					num3 += num6;
				}
			}
			num3 = 0.0 - num3;
			symmetricSparseMatrix.Set(num2, num2, num3);
		}
		if (UseSoftConstraintNormalEquations)
		{
			PackedM = symmetricSparseMatrix.SquarePackedParallel();
		}
		else
		{
			PackedM = new PackedSparseMatrix(symmetricSparseMatrix);
		}
		MLx = new double[N];
		MLy = new double[N];
		MLz = new double[N];
		PackedM.Multiply(Px, MLx);
		PackedM.Multiply(Py, MLy);
		PackedM.Multiply(Pz, MLz);
		Preconditioner = new DiagonalMatrix(N);
		WeightsM = new DiagonalMatrix(N);
		Cx = new double[N];
		Cy = new double[N];
		Cz = new double[N];
		Bx = new double[N];
		By = new double[N];
		Bz = new double[N];
		Sx = new double[N];
		Sy = new double[N];
		Sz = new double[N];
		need_solve_update = true;
		UpdateForSolve();
	}

	private void UpdateForSolve()
	{
		if (!need_solve_update)
		{
			return;
		}
		WeightsM.Clear();
		Array.Clear(Cx, 0, N);
		Array.Clear(Cy, 0, N);
		Array.Clear(Cz, 0, N);
		foreach (KeyValuePair<int, SoftConstraintV> softConstraint in SoftConstraints)
		{
			int key = softConstraint.Key;
			int num = ToIndex[key];
			double num2 = softConstraint.Value.Weight;
			if (UseSoftConstraintNormalEquations)
			{
				num2 *= num2;
			}
			WeightsM.Set(num, num, num2);
			Vector3d position = softConstraint.Value.Position;
			Cx[num] = num2 * position.x;
			Cy[num] = num2 * position.y;
			Cz[num] = num2 * position.z;
		}
		for (int i = 0; i < N; i++)
		{
			Bx[i] = MLx[i] + Cx[i];
			By[i] = MLy[i] + Cy[i];
			Bz[i] = MLz[i] + Cz[i];
		}
		for (int j = 0; j < N; j++)
		{
			double num3 = PackedM[j, j] + WeightsM[j, j];
			Preconditioner.Set(j, j, 1.0 / num3);
		}
		need_solve_update = false;
	}

	public bool SolveMultipleCG(Vector3d[] Result)
	{
		if (WeightsM == null)
		{
			Initialize();
		}
		UpdateForSolve();
		Array.Copy(Px, Sx, N);
		Array.Copy(Py, Sy, N);
		Array.Copy(Pz, Sz, N);
		Action<double[], double[]> multiplyF = delegate(double[] X, double[] B)
		{
			PackedM.Multiply_Parallel(X, B);
			for (int l = 0; l < N; l++)
			{
				B[l] += WeightsM[l, l] * X[l];
			}
		};
		List<SparseSymmetricCG> Solvers = new List<SparseSymmetricCG>();
		if (SolveX)
		{
			Solvers.Add(new SparseSymmetricCG
			{
				B = Bx,
				X = Sx,
				MultiplyF = multiplyF,
				PreconditionMultiplyF = Preconditioner.Multiply,
				UseXAsInitialGuess = true
			});
		}
		if (SolveY)
		{
			Solvers.Add(new SparseSymmetricCG
			{
				B = By,
				X = Sy,
				MultiplyF = multiplyF,
				PreconditionMultiplyF = Preconditioner.Multiply,
				UseXAsInitialGuess = true
			});
		}
		if (SolveZ)
		{
			Solvers.Add(new SparseSymmetricCG
			{
				B = Bz,
				X = Sz,
				MultiplyF = multiplyF,
				PreconditionMultiplyF = Preconditioner.Multiply,
				UseXAsInitialGuess = true
			});
		}
		bool[] ok = new bool[Solvers.Count];
		gParallel.ForEach(Interval1i.Range(Solvers.Count), delegate(int i)
		{
			ok[i] = Solvers[i].Solve();
		});
		ConvergeFailed = false;
		bool[] array = ok;
		for (int j = 0; j < array.Length; j++)
		{
			if (!array[j])
			{
				ConvergeFailed = true;
			}
		}
		for (int k = 0; k < N; k++)
		{
			int num = ToCurveV[k];
			Result[num] = new Vector3d(Sx[k], Sy[k], Sz[k]);
		}
		if (HavePostFixedConstraints)
		{
			foreach (KeyValuePair<int, SoftConstraintV> softConstraint in SoftConstraints)
			{
				if (softConstraint.Value.PostFix)
				{
					int key = softConstraint.Key;
					Result[key] = softConstraint.Value.Position;
				}
			}
		}
		return true;
	}

	public bool SolveMultipleRHS(Vector3d[] Result)
	{
		if (WeightsM == null)
		{
			Initialize();
		}
		UpdateForSolve();
		double[][] b = BufferUtil.InitNxM(3, N, new double[3][] { Bx, By, Bz });
		double[][] array = BufferUtil.InitNxM(3, N, new double[3][] { Px, Py, Pz });
		Action<double[][], double[][]> multiplyF = delegate(double[][] Xt, double[][] Bt)
		{
			PackedM.Multiply_Parallel_3(Xt, Bt);
			gParallel.ForEach(Interval1i.Range(3), delegate(int j)
			{
				BufferUtil.MultiplyAdd(Bt[j], WeightsM.D, Xt[j]);
			});
		};
		if (!new SparseSymmetricCGMultipleRHS
		{
			B = b,
			X = array,
			MultiplyF = multiplyF,
			PreconditionMultiplyF = null,
			UseXAsInitialGuess = true
		}.Solve())
		{
			return false;
		}
		for (int i = 0; i < N; i++)
		{
			int num = ToCurveV[i];
			Result[num] = new Vector3d(array[0][i], array[1][i], array[2][i]);
		}
		if (HavePostFixedConstraints)
		{
			foreach (KeyValuePair<int, SoftConstraintV> softConstraint in SoftConstraints)
			{
				if (softConstraint.Value.PostFix)
				{
					int key = softConstraint.Key;
					Result[key] = softConstraint.Value.Position;
				}
			}
		}
		return true;
	}

	public bool Solve(Vector3d[] Result)
	{
		if (Curve.VertexCount < 10000)
		{
			return SolveMultipleCG(Result);
		}
		return SolveMultipleRHS(Result);
	}

	public bool SolveAndUpdateCurve()
	{
		int vertexCount = Curve.VertexCount;
		Vector3d[] array = new Vector3d[vertexCount];
		if (!Solve(array))
		{
			return false;
		}
		for (int i = 0; i < vertexCount; i++)
		{
			Curve[i] = array[i];
		}
		return true;
	}
}
