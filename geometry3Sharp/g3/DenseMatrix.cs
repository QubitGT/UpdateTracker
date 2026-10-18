using System;

namespace g3;

public class DenseMatrix : IMatrix
{
	private double[] d;

	private int N;

	private int M;

	public double[] Buffer => d;

	public int Rows => N;

	public int Columns => M;

	public Index2i Size => new Index2i(N, M);

	public int Length => M * N;

	public double this[int r, int c]
	{
		get
		{
			return d[r * M + c];
		}
		set
		{
			d[r * M + c] = value;
		}
	}

	public double this[int i]
	{
		get
		{
			return d[i];
		}
		set
		{
			d[i] = value;
		}
	}

	public DenseMatrix(int Nrows, int Mcols)
	{
		d = new double[Nrows * Mcols];
		Array.Clear(d, 0, d.Length);
		N = Nrows;
		M = Mcols;
	}

	public DenseMatrix(DenseMatrix copy)
	{
		N = copy.N;
		M = copy.M;
		d = new double[N * M];
		Array.Copy(copy.d, d, copy.d.Length);
	}

	public void Set(int r, int c, double value)
	{
		d[r * M + c] = value;
	}

	public void Set(double[] values)
	{
		if (values.Length != N * M)
		{
			throw new Exception("DenseMatrix.Set: incorrect length");
		}
		Array.Copy(values, d, d.Length);
	}

	public DenseVector Row(int r)
	{
		DenseVector denseVector = new DenseVector(M);
		int num = r * M;
		for (int i = 0; i < M; i++)
		{
			denseVector[i] = d[num + i];
		}
		return denseVector;
	}

	public DenseVector Column(int c)
	{
		DenseVector denseVector = new DenseVector(N);
		for (int i = 0; i < N; i++)
		{
			denseVector[i] = d[i * M + c];
		}
		return denseVector;
	}

	public DenseVector Diagonal()
	{
		if (M != N)
		{
			throw new Exception("DenseMatrix.Diagonal: matrix is not square!");
		}
		DenseVector denseVector = new DenseVector(N);
		for (int i = 0; i < N; i++)
		{
			denseVector[i] = d[i * M + i];
		}
		return denseVector;
	}

	public DenseMatrix Transpose()
	{
		DenseMatrix denseMatrix = new DenseMatrix(M, N);
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < M; j++)
			{
				denseMatrix.d[j * M + i] = d[i * M + j];
			}
		}
		return denseMatrix;
	}

	public void TransposeInPlace()
	{
		if (N != M)
		{
			double[] array = new double[M * N];
			for (int i = 0; i < N; i++)
			{
				for (int j = 0; j < M; j++)
				{
					array[j * M + i] = d[i * M + j];
				}
			}
			d = array;
			int m = M;
			M = N;
			N = m;
			return;
		}
		for (int k = 0; k < N; k++)
		{
			for (int l = 0; l < M; l++)
			{
				if (l != k)
				{
					int num = k * M + l;
					int num2 = l * M + k;
					double num3 = d[num];
					d[num] = d[num2];
					d[num2] = num3;
				}
			}
		}
	}

	public bool IsSymmetric(double dTolerance = 2.220446049250313E-16)
	{
		if (M != N)
		{
			throw new Exception("DenseMatrix.IsSymmetric: matrix is not square!");
		}
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < i; j++)
			{
				if (Math.Abs(d[i * M + j] - d[j * M + i]) > dTolerance)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsPositiveDefinite()
	{
		if (M != N)
		{
			throw new Exception("DenseMatrix.IsPositiveDefinite: matrix is not square!");
		}
		if (!IsSymmetric())
		{
			throw new Exception("DenseMatrix.IsPositiveDefinite: matrix is not symmetric!");
		}
		for (int i = 0; i < N; i++)
		{
			double num = d[i * M + i];
			double num2 = 0.0;
			for (int j = 0; j < N; j++)
			{
				if (j != i)
				{
					num2 += Math.Abs(d[i * M + j]);
				}
			}
			if (num < 0.0 || num < num2)
			{
				return false;
			}
		}
		return true;
	}

	public bool EpsilonEquals(DenseMatrix m2, double epsilon = 1E-08)
	{
		if (N != m2.N || M != m2.M)
		{
			throw new Exception("DenseMatrix.Equals: matrices are not the same size!");
		}
		for (int i = 0; i < d.Length; i++)
		{
			if (Math.Abs(d[i] - m2.d[i]) > epsilon)
			{
				return false;
			}
		}
		return true;
	}

	public DenseVector Multiply(DenseVector X)
	{
		DenseVector denseVector = new DenseVector(X.Length);
		Multiply(X.Buffer, denseVector.Buffer);
		return denseVector;
	}

	public void Multiply(DenseVector X, DenseVector R)
	{
		Multiply(X.Buffer, R.Buffer);
	}

	public void Multiply(double[] X, double[] Result)
	{
		for (int i = 0; i < N; i++)
		{
			Result[i] = 0.0;
			int num = i * M;
			for (int j = 0; j < M; j++)
			{
				Result[i] += d[num + j] * X[j];
			}
		}
	}

	public void Add(DenseMatrix M2)
	{
		if (N != M2.N || M != M2.M)
		{
			throw new Exception("DenseMatrix.Add: matrices have incompatible dimensions");
		}
		for (int i = 0; i < d.Length; i++)
		{
			d[i] += M2.d[i];
		}
	}

	public void Add(IMatrix M2)
	{
		if (N != M2.Rows || M != M2.Columns)
		{
			throw new Exception("DenseMatrix.Add: matrices have incompatible dimensions");
		}
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < M; j++)
			{
				d[i * M + j] += M2[i, j];
			}
		}
	}

	public void MulAdd(DenseMatrix M2, double s)
	{
		if (N != M2.N || M != M2.M)
		{
			throw new Exception("DenseMatrix.MulAdd: matrices have incompatible dimensions");
		}
		for (int i = 0; i < d.Length; i++)
		{
			d[i] += s * M2.d[i];
		}
	}

	public void MulAdd(IMatrix M2, double s)
	{
		if (N != M2.Rows || M != M2.Columns)
		{
			throw new Exception("DenseMatrix.MulAdd: matrices have incompatible dimensions");
		}
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < M; j++)
			{
				d[i * M + j] += s * M2[i, j];
			}
		}
	}

	public DenseMatrix Multiply(DenseMatrix M2, bool bParallel = true)
	{
		DenseMatrix R = new DenseMatrix(Rows, M2.Columns);
		Multiply(M2, ref R, bParallel);
		return R;
	}

	public void Multiply(DenseMatrix M2, ref DenseMatrix R, bool bParallel = true)
	{
		int n = N;
		int cols1 = M;
		int n2 = M2.N;
		int cols2 = M2.M;
		if (cols1 != n2)
		{
			throw new Exception("DenseMatrix.Multiply: matrices have incompatible dimensions");
		}
		if (R == null)
		{
			R = new DenseMatrix(Rows, M2.Columns);
		}
		if (R.Rows != n || R.Columns != cols2)
		{
			throw new Exception("DenseMatrix.Multiply: Result matrix has incorrect dimensions");
		}
		if (bParallel)
		{
			DenseMatrix Rt = R;
			gParallel.ForEach(Interval1i.Range(0, n), delegate(int r1i)
			{
				int num3 = r1i * M;
				for (int l = 0; l < cols2; l++)
				{
					double num4 = 0.0;
					for (int m = 0; m < cols1; m++)
					{
						num4 += d[num3 + m] * M2.d[m * M + l];
					}
					Rt[num3 + l] = num4;
				}
			});
			return;
		}
		for (int i = 0; i < n; i++)
		{
			int num = i * M;
			for (int j = 0; j < cols2; j++)
			{
				double num2 = 0.0;
				for (int k = 0; k < cols1; k++)
				{
					num2 += d[num + k] * M2.d[k * M + j];
				}
				R[num + j] = num2;
			}
		}
	}
}
