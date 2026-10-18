using System;

namespace g3;

public class BSplineBasis
{
	protected int mNumCtrlPoints;

	protected int mDegree;

	protected double[] mKnot;

	protected bool mOpen;

	protected bool mUniform;

	protected double[,] mBD0;

	protected double[,] mBD1;

	protected double[,] mBD2;

	protected double[,] mBD3;

	public int KnotCount => mNumCtrlPoints + mDegree + 1;

	public int InteriorKnotCount => mNumCtrlPoints - mDegree - 1;

	protected BSplineBasis()
	{
	}

	public BSplineBasis(int numCtrlPoints, int degree, bool open)
	{
		mUniform = true;
		int num = Initialize(numCtrlPoints, degree, open);
		double num2 = 1.0 / (double)(mNumCtrlPoints - mDegree);
		if (mOpen)
		{
			int i;
			for (i = 0; i <= mDegree; i++)
			{
				mKnot[i] = 0.0;
			}
			for (; i < mNumCtrlPoints; i++)
			{
				mKnot[i] = (double)(i - mDegree) * num2;
			}
			for (; i < num; i++)
			{
				mKnot[i] = 1.0;
			}
		}
		else
		{
			for (int i = 0; i < num; i++)
			{
				mKnot[i] = (double)(i - mDegree) * num2;
			}
		}
	}

	public BSplineBasis(int numCtrlPoints, int degree, double[] knots, bool bIsInteriorKnots)
	{
		mUniform = false;
		int num = Initialize(numCtrlPoints, degree, open: true);
		if (bIsInteriorKnots)
		{
			if (knots.Length != mNumCtrlPoints - mDegree - 1)
			{
				throw new Exception("BSplineBasis nonuniform constructor: invalid interior knot vector");
			}
			int i;
			for (i = 0; i <= mDegree; i++)
			{
				mKnot[i] = 0.0;
			}
			int num2 = 0;
			while (i < mNumCtrlPoints)
			{
				mKnot[i] = knots[num2];
				i++;
				num2++;
			}
			for (; i < num; i++)
			{
				mKnot[i] = 1.0;
			}
		}
		else
		{
			if (mKnot.Length != knots.Length)
			{
				throw new Exception("BSplineBasis nonuniform constructor: invalid knot vector");
			}
			Array.Copy(knots, mKnot, knots.Length);
		}
	}

	public BSplineBasis Clone()
	{
		return new BSplineBasis
		{
			mNumCtrlPoints = mNumCtrlPoints,
			mDegree = mDegree,
			mKnot = (double[])mKnot.Clone(),
			mOpen = mOpen,
			mUniform = mUniform
		};
	}

	public int GetNumCtrlPoints()
	{
		return mNumCtrlPoints;
	}

	public int GetDegree()
	{
		return mDegree;
	}

	public bool IsOpen()
	{
		return mOpen;
	}

	public bool IsUniform()
	{
		return mUniform;
	}

	public void SetInteriorKnot(int j, double value)
	{
		if (!mUniform)
		{
			int num = j + mDegree + 1;
			if (mDegree + 1 <= num && num <= mNumCtrlPoints)
			{
				mKnot[num] = value;
				return;
			}
			throw new Exception("BSplineBasis.SetKnot: index out of range: " + j);
		}
		throw new Exception("BSplineBasis.SetKnot: knots cannot be set for uniform splines");
	}

	public double GetInteriorKnot(int j)
	{
		int num = j + mDegree + 1;
		if (mDegree + 1 <= num && num <= mNumCtrlPoints)
		{
			return mKnot[num];
		}
		throw new Exception("BSplineBasis.GetKnot: index out of range: " + j);
	}

	public void SetKnot(int j, double value)
	{
		mKnot[j] = value;
	}

	public double GetKnot(int j)
	{
		return mKnot[j];
	}

	public double GetD0(int i)
	{
		return mBD0[mDegree, i];
	}

	public double GetD1(int i)
	{
		return mBD1[mDegree, i];
	}

	public double GetD2(int i)
	{
		return mBD2[mDegree, i];
	}

	public double GetD3(int i)
	{
		return mBD3[mDegree, i];
	}

	public void Compute(double t, int order, ref int minIndex, ref int maxIndex)
	{
		if (order > 3)
		{
			throw new Exception("BSplineBasis.Compute: cannot compute order " + order);
		}
		if (order >= 1)
		{
			if (mBD1 == null)
			{
				mBD1 = Allocate();
			}
			if (order >= 2)
			{
				if (mBD2 == null)
				{
					mBD2 = Allocate();
				}
				if (order >= 3 && mBD3 == null)
				{
					mBD3 = Allocate();
				}
			}
		}
		int key = GetKey(ref t);
		mBD0[0, key] = 1.0;
		if (order >= 1)
		{
			mBD1[0, key] = 0.0;
			if (order >= 2)
			{
				mBD2[0, key] = 0.0;
				if (order >= 3)
				{
					mBD3[0, key] = 0.0;
				}
			}
		}
		double num = t - mKnot[key];
		double num2 = mKnot[key + 1] - t;
		for (int i = 1; i <= mDegree; i++)
		{
			double num3 = 1.0 / (mKnot[key + i] - mKnot[key]);
			double num4 = 1.0 / (mKnot[key + 1] - mKnot[key - i + 1]);
			if (mKnot[key + i] == mKnot[key])
			{
				num3 = 0.0;
			}
			if (mKnot[key + 1] == mKnot[key - i + 1])
			{
				num4 = 0.0;
			}
			mBD0[i, key] = num * mBD0[i - 1, key] * num3;
			mBD0[i, key - i] = num2 * mBD0[i - 1, key - i + 1] * num4;
			if (order < 1)
			{
				continue;
			}
			mBD1[i, key] = (num * mBD1[i - 1, key] + mBD0[i - 1, key]) * num3;
			mBD1[i, key - i] = (num2 * mBD1[i - 1, key - i + 1] - mBD0[i - 1, key - i + 1]) * num4;
			if (order >= 2)
			{
				mBD2[i, key] = (num * mBD2[i - 1, key] + 2.0 * mBD1[i - 1, key]) * num3;
				mBD2[i, key - i] = (num2 * mBD2[i - 1, key - i + 1] - 2.0 * mBD1[i - 1, key - i + 1]) * num4;
				if (order >= 3)
				{
					mBD3[i, key] = (num * mBD3[i - 1, key] + 3.0 * mBD2[i - 1, key]) * num3;
					mBD3[i, key - i] = (num2 * mBD3[i - 1, key - i + 1] - 3.0 * mBD2[i - 1, key - i + 1]) * num4;
				}
			}
		}
		for (int i = 2; i <= mDegree; i++)
		{
			for (int j = key - i + 1; j < key; j++)
			{
				num = t - mKnot[j];
				num2 = mKnot[j + i + 1] - t;
				double num3 = 1.0 / (mKnot[j + i] - mKnot[j]);
				double num4 = 1.0 / (mKnot[j + i + 1] - mKnot[j + 1]);
				if (mKnot[j + i] == mKnot[j])
				{
					num3 = 0.0;
				}
				if (mKnot[j + i + 1] == mKnot[j + 1])
				{
					num4 = 0.0;
				}
				mBD0[i, j] = num * mBD0[i - 1, j] * num3 + num2 * mBD0[i - 1, j + 1] * num4;
				if (order < 1)
				{
					continue;
				}
				mBD1[i, j] = (num * mBD1[i - 1, j] + mBD0[i - 1, j]) * num3 + (num2 * mBD1[i - 1, j + 1] - mBD0[i - 1, j + 1]) * num4;
				if (order >= 2)
				{
					mBD2[i, j] = (num * mBD2[i - 1, j] + 2.0 * mBD1[i - 1, j]) * num3 + (num2 * mBD2[i - 1, j + 1] - 2.0 * mBD1[i - 1, j + 1]) * num4;
					if (order >= 3)
					{
						mBD3[i, j] = (num * mBD3[i - 1, j] + 3.0 * mBD2[i - 1, j]) * num3 + (num2 * mBD3[i - 1, j + 1] - 3.0 * mBD2[i - 1, j + 1]) * num4;
					}
				}
			}
		}
		minIndex = key - mDegree;
		maxIndex = key;
	}

	protected int Initialize(int numCtrlPoints, int degree, bool open)
	{
		if (numCtrlPoints < 2)
		{
			throw new Exception("BSplineBasis.Initialize: only received " + numCtrlPoints + " control points!");
		}
		if (degree < 1 || degree > numCtrlPoints - 1)
		{
			throw new Exception("BSplineBasis.Initialize: invalid degree " + degree);
		}
		mNumCtrlPoints = numCtrlPoints;
		mDegree = degree;
		mOpen = open;
		int num = mNumCtrlPoints + mDegree + 1;
		mKnot = new double[num];
		mBD0 = Allocate();
		mBD1 = null;
		mBD2 = null;
		mBD3 = null;
		return num;
	}

	protected double[,] Allocate()
	{
		int num = mDegree + 1;
		int num2 = mNumCtrlPoints + mDegree;
		double[,] array = new double[num, num2];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				array[i, j] = 0.0;
			}
		}
		return array;
	}

	protected int GetKey(ref double t)
	{
		if (mOpen)
		{
			if (t <= 0.0)
			{
				t = 0.0;
				return mDegree;
			}
			if (t >= 1.0)
			{
				t = 1.0;
				return mNumCtrlPoints - 1;
			}
		}
		else if (t < 0.0 || t >= 1.0)
		{
			t -= Math.Floor(t);
		}
		if (mUniform)
		{
			return mDegree + (int)((double)(mNumCtrlPoints - mDegree) * t);
		}
		int i;
		for (i = mDegree + 1; i <= mNumCtrlPoints && !(t < mKnot[i]); i++)
		{
		}
		return i - 1;
	}
}
