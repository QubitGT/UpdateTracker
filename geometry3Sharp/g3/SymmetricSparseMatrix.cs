using System;
using System.Collections.Generic;

namespace g3;

public class SymmetricSparseMatrix : IMatrix
{
	private struct mval
	{
		public int k;

		public double v;
	}

	private Dictionary<Index2i, double> d = new Dictionary<Index2i, double>();

	private int N;

	public int Rows => N;

	public int Columns => N;

	public Index2i Size => new Index2i(N, N);

	public double this[int r, int c]
	{
		get
		{
			Index2i key = new Index2i(Math.Min(r, c), Math.Max(r, c));
			if (d.TryGetValue(key, out var value))
			{
				return value;
			}
			return 0.0;
		}
		set
		{
			Set(r, c, value);
		}
	}

	public SymmetricSparseMatrix(int setN = 0)
	{
		N = setN;
	}

	public SymmetricSparseMatrix(DenseMatrix m)
	{
		if (m.Rows != m.Columns)
		{
			throw new Exception("SymmetricSparseMatrix(DenseMatrix): Matrix is not square!");
		}
		if (!m.IsSymmetric())
		{
			throw new Exception("SymmetricSparseMatrix(DenseMatrix): Matrix is not symmetric!");
		}
		N = m.Rows;
		for (int i = 0; i < N; i++)
		{
			for (int j = i; j < N; j++)
			{
				Set(i, j, m[i, j]);
			}
		}
	}

	public SymmetricSparseMatrix(SymmetricSparseMatrix m)
	{
		N = m.N;
		d = new Dictionary<Index2i, double>(m.d);
	}

	public void Set(int r, int c, double value)
	{
		Index2i key = new Index2i(Math.Min(r, c), Math.Max(r, c));
		d[key] = value;
		if (r >= N)
		{
			N = r + 1;
		}
		if (c >= N)
		{
			N = c + 1;
		}
	}

	public void Multiply(double[] X, double[] Result)
	{
		Array.Clear(Result, 0, Result.Length);
		foreach (KeyValuePair<Index2i, double> item in d)
		{
			int a = item.Key.a;
			int b = item.Key.b;
			Result[a] += item.Value * X[b];
			if (a != b)
			{
				Result[b] += item.Value * X[a];
			}
		}
	}

	public SymmetricSparseMatrix Square(bool bParallel = true)
	{
		SymmetricSparseMatrix R = new SymmetricSparseMatrix();
		PackedSparseMatrix M = new PackedSparseMatrix(this);
		M.Sort();
		if (bParallel)
		{
			gParallel.ForEach(Interval1i.Range(N), delegate(int r1i)
			{
				for (int k = r1i; k < N; k++)
				{
					double value2 = M.DotRowColumn(r1i, k, M);
					if (Math.Abs(value2) > 1E-08)
					{
						lock (R)
						{
							R[r1i, k] = value2;
						}
					}
				}
			});
		}
		else
		{
			for (int i = 0; i < N; i++)
			{
				for (int j = i; j < N; j++)
				{
					double value = M.DotRowColumn(i, j, M);
					if (Math.Abs(value) > 1E-08)
					{
						R[i, j] = value;
					}
				}
			}
		}
		return R;
	}

	public PackedSparseMatrix SquarePackedParallel()
	{
		PackedSparseMatrix packedSparseMatrix = new PackedSparseMatrix(this);
		packedSparseMatrix.Sort();
		return packedSparseMatrix.Square();
	}

	public SymmetricSparseMatrix Multiply(SymmetricSparseMatrix M2)
	{
		SymmetricSparseMatrix R = new SymmetricSparseMatrix();
		Multiply(M2, ref R);
		return R;
	}

	public void Multiply(SymmetricSparseMatrix M2, ref SymmetricSparseMatrix R, bool bParallel = true)
	{
		multiply_fast(M2, ref R, bParallel);
	}

	private void multiply_fast(SymmetricSparseMatrix M2in, ref SymmetricSparseMatrix Rin, bool bParallel)
	{
		int N = Rows;
		if (M2in.Rows != N)
		{
			throw new Exception("SymmetricSparseMatrix.Multiply: matrices have incompatible dimensions");
		}
		if (Rin == null)
		{
			Rin = new SymmetricSparseMatrix();
		}
		SymmetricSparseMatrix R = Rin;
		PackedSparseMatrix M = new PackedSparseMatrix(this);
		M.Sort();
		PackedSparseMatrix M2 = new PackedSparseMatrix(M2in, bTranspose: true);
		M2.Sort();
		if (bParallel)
		{
			gParallel.ForEach(Interval1i.Range(N), delegate(int r1i)
			{
				for (int k = r1i; k < N; k++)
				{
					double value2 = M.DotRowColumn(r1i, k, M2);
					if (Math.Abs(value2) > 1E-08)
					{
						lock (R)
						{
							R[r1i, k] = value2;
						}
					}
				}
			});
			return;
		}
		for (int i = 0; i < N; i++)
		{
			for (int j = i; j < N; j++)
			{
				double value = M.DotRowColumn(i, j, M2);
				if (Math.Abs(value) > 1E-08)
				{
					R[i, j] = value;
				}
			}
		}
	}

	private void multiply_slow(SymmetricSparseMatrix M2, ref SymmetricSparseMatrix R)
	{
		int rows = Rows;
		if (M2.Rows != rows)
		{
			throw new Exception("SymmetricSparseMatrix.Multiply: matrices have incompatible dimensions");
		}
		if (R == null)
		{
			R = new SymmetricSparseMatrix();
		}
		List<mval> list = new List<mval>(128);
		for (int i = 0; i < rows; i++)
		{
			list.Clear();
			get_row_nonzeros(i, list);
			int count = list.Count;
			for (int j = i; j < rows; j++)
			{
				double num = 0.0;
				for (int k = 0; k < count; k++)
				{
					int k2 = list[k].k;
					num += list[k].v * M2[k2, j];
				}
				if (Math.Abs(num) > 1E-08)
				{
					R[i, j] = num;
				}
			}
		}
	}

	public IEnumerable<KeyValuePair<Index2i, double>> NonZeros()
	{
		return d;
	}

	public IEnumerable<Index2i> NonZeroIndices()
	{
		return d.Keys;
	}

	public bool EpsilonEqual(SymmetricSparseMatrix B, double eps = 2.220446049250313E-16)
	{
		foreach (KeyValuePair<Index2i, double> item in d)
		{
			if (Math.Abs(B[item.Key.a, item.Key.b] - item.Value) > eps)
			{
				return false;
			}
		}
		foreach (KeyValuePair<Index2i, double> item2 in B.d)
		{
			if (Math.Abs(this[item2.Key.a, item2.Key.b] - item2.Value) > eps)
			{
				return false;
			}
		}
		return true;
	}

	private void get_row_nonzeros(int r, List<mval> buf)
	{
		int rows = Rows;
		for (int i = 0; i < rows; i++)
		{
			double num = this[r, i];
			if (num != 0.0)
			{
				buf.Add(new mval
				{
					k = i,
					v = num
				});
			}
		}
	}
}
