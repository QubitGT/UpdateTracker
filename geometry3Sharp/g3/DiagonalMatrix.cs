using System;

namespace g3;

public class DiagonalMatrix
{
	public double[] D;

	public int Rows => D.Length;

	public int Columns => D.Length;

	public Index2i Size => new Index2i(D.Length, D.Length);

	public double this[int r, int c]
	{
		get
		{
			return D[r];
		}
		set
		{
			Set(r, c, value);
		}
	}

	public DiagonalMatrix(int N)
	{
		D = new double[N];
	}

	public void Clear()
	{
		Array.Clear(D, 0, D.Length);
	}

	public void Set(int r, int c, double value)
	{
		if (r == c)
		{
			D[r] = value;
			return;
		}
		throw new Exception("DiagonalMatrix.Set: tried to set off-diagonal entry!");
	}

	public void Multiply(double[] X, double[] Result)
	{
		for (int i = 0; i < X.Length; i++)
		{
			Result[i] = D[i] * X[i];
		}
	}
}
