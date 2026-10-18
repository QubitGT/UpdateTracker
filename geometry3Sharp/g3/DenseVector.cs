using System;

namespace g3;

public class DenseVector
{
	private double[] d;

	private int N;

	public int Size => N;

	public int Length => N;

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

	public double[] Buffer => d;

	public DenseVector(int N)
	{
		d = new double[N];
		Array.Clear(d, 0, d.Length);
		this.N = N;
	}

	public void Set(int i, double value)
	{
		d[i] = value;
	}

	public double Dot(DenseVector v2)
	{
		return Dot(v2.d);
	}

	public double Dot(double[] v2)
	{
		if (v2.Length != N)
		{
			throw new Exception("DenseVector.Dot: incompatible lengths");
		}
		double num = 0.0;
		for (int i = 0; i < v2.Length; i++)
		{
			num += d[i] * v2[i];
		}
		return num;
	}
}
