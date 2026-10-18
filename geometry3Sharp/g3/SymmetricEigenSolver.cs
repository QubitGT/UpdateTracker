using System;
using System.Collections.Generic;

namespace g3;

public class SymmetricEigenSolver
{
	public enum SortType
	{
		Decreasing = -1,
		NoSorting,
		Increasing
	}

	private struct GivensRotation
	{
		public int index;

		public double cs;

		public double sn;

		public GivensRotation(int inIndex, double inCs, double inSn)
		{
			index = inIndex;
			cs = inCs;
			sn = inSn;
		}
	}

	private struct SortItem
	{
		public double eigenvalue;

		public int index;
	}

	public const int NO_CONVERGENCE = int.MaxValue;

	private int mSize;

	private int mMaxIterations;

	private double[] mMatrix;

	private double[] mDiagonal;

	private double[] mSuperdiagonal;

	private List<GivensRotation> mGivens;

	private int[] mPermutation;

	private int[] mVisited;

	private int mIsRotation;

	private double[] mPVector;

	private double[] mVVector;

	private double[] mWVector;

	public SymmetricEigenSolver(int size, int maxIterations)
	{
		mSize = (mMaxIterations = 0);
		mIsRotation = -1;
		if (size > 1 && maxIterations > 0)
		{
			mSize = size;
			mMaxIterations = maxIterations;
			mMatrix = new double[size * size];
			mDiagonal = new double[size];
			mSuperdiagonal = new double[size - 1];
			mGivens = new List<GivensRotation>(maxIterations * (size - 1));
			mPermutation = new int[size];
			mVisited = new int[size];
			mPVector = new double[size];
			mVVector = new double[size];
			mWVector = new double[size];
		}
	}

	public int Solve(double[] input, SortType eSort)
	{
		if (mSize > 0)
		{
			Array.Copy(input, mMatrix, mSize * mSize);
			Tridiagonalize();
			mGivens.Clear();
			for (int i = 0; i < mMaxIterations; i++)
			{
				int num = -1;
				int num2 = -1;
				for (int num3 = mSize - 2; num3 >= 0; num3--)
				{
					double value = mDiagonal[num3];
					double value2 = mSuperdiagonal[num3];
					double value3 = mDiagonal[num3 + 1];
					double num4 = Math.Abs(value) + Math.Abs(value3);
					if (num4 + Math.Abs(value2) != num4)
					{
						if (num2 == -1)
						{
							num2 = num3;
						}
						num = num3;
					}
					else if (num >= 0)
					{
						break;
					}
				}
				if (num2 == -1)
				{
					ComputePermutation((int)eSort);
					return i;
				}
				DoQRImplicitShift(num, num2);
			}
			return int.MaxValue;
		}
		return 0;
	}

	public void GetEigenvalues(double[] eigenvalues)
	{
		if (eigenvalues == null || mSize <= 0)
		{
			return;
		}
		if (mPermutation[0] >= 0)
		{
			for (int i = 0; i < mSize; i++)
			{
				int num = mPermutation[i];
				eigenvalues[i] = mDiagonal[num];
			}
		}
		else
		{
			Array.Copy(mDiagonal, eigenvalues, mSize);
		}
	}

	public double[] GetEigenvalues()
	{
		double[] array = new double[mSize];
		GetEigenvalues(array);
		return array;
	}

	public double GetEigenvalue(int c)
	{
		if (mSize > 0)
		{
			if (mPermutation[0] >= 0)
			{
				return mDiagonal[mPermutation[c]];
			}
			return mDiagonal[c];
		}
		return double.MaxValue;
	}

	public void GetEigenvectors(double[] eigenvectors)
	{
		if (eigenvectors == null || mSize <= 0)
		{
			return;
		}
		Array.Clear(eigenvectors, 0, mSize * mSize);
		for (int i = 0; i < mSize; i++)
		{
			eigenvectors[i + mSize * i] = 1.0;
		}
		int num = mSize - 3;
		int num2 = num + 1;
		while (num >= 0)
		{
			ArrayAlias<double> arrayAlias = new ArrayAlias<double>(mMatrix, num);
			double num3 = arrayAlias[mSize * (num + 1)];
			int j;
			for (j = 0; j < num + 1; j++)
			{
				mVVector[j] = 0.0;
			}
			mVVector[j] = 1.0;
			for (j++; j < mSize; j++)
			{
				mVVector[j] = arrayAlias[mSize * j];
			}
			for (j = 0; j < mSize; j++)
			{
				mWVector[j] = 0.0;
				for (int k = num2; k < mSize; k++)
				{
					mWVector[j] += mVVector[k] * eigenvectors[j + mSize * k];
				}
				mWVector[j] *= num3;
			}
			for (j = num2; j < mSize; j++)
			{
				for (int k = 0; k < mSize; k++)
				{
					eigenvectors[k + mSize * j] -= mVVector[j] * mWVector[k];
				}
			}
			num--;
			num2--;
		}
		foreach (GivensRotation mGiven in mGivens)
		{
			for (int j = 0; j < mSize; j++)
			{
				int num4 = mGiven.index + mSize * j;
				double num5 = eigenvectors[num4];
				double num6 = eigenvectors[num4 + 1];
				double num7 = mGiven.cs * num5 - mGiven.sn * num6;
				double num8 = mGiven.sn * num5 + mGiven.cs * num6;
				eigenvectors[num4] = num7;
				eigenvectors[num4 + 1] = num8;
			}
		}
		mIsRotation = 1 - (mSize & 1);
		if (mPermutation[0] < 0)
		{
			return;
		}
		Array.Clear(mVisited, 0, mVisited.Length);
		for (int l = 0; l < mSize; l++)
		{
			if (mVisited[l] != 0 || mPermutation[l] == l)
			{
				continue;
			}
			mIsRotation = 1 - mIsRotation;
			int num9 = l;
			int num10 = l;
			for (int m = 0; m < mSize; m++)
			{
				mPVector[m] = eigenvectors[l + mSize * m];
			}
			int num11;
			while ((num11 = mPermutation[num10]) != num9)
			{
				mVisited[num10] = 1;
				for (int m = 0; m < mSize; m++)
				{
					eigenvectors[num10 + mSize * m] = eigenvectors[num11 + mSize * m];
				}
				num10 = num11;
			}
			mVisited[num10] = 1;
			for (int m = 0; m < mSize; m++)
			{
				eigenvectors[num10 + mSize * m] = mPVector[m];
			}
		}
	}

	public double[] GetEigenvectors()
	{
		double[] array = new double[mSize * mSize];
		GetEigenvectors(array);
		return array;
	}

	public bool IsRotation()
	{
		if (mSize > 0)
		{
			if (mIsRotation == -1)
			{
				mIsRotation = 1 - (mSize & 1);
				if (mPermutation[0] >= 0)
				{
					Array.Clear(mVisited, 0, mVisited.Length);
					for (int i = 0; i < mSize; i++)
					{
						if (mVisited[i] == 0 && mPermutation[i] != i)
						{
							int num = i;
							int num2 = i;
							int num3;
							while ((num3 = mPermutation[num2]) != num)
							{
								mVisited[num2] = 1;
								num2 = num3;
							}
							mVisited[num2] = 1;
						}
					}
				}
			}
			return mIsRotation == 1;
		}
		return false;
	}

	public void GetEigenvector(int c, double[] eigenvector)
	{
		if (0 > c || c >= mSize)
		{
			return;
		}
		double[] array = eigenvector;
		double[] array2 = mPVector;
		Array.Clear(array, 0, mSize);
		if (mPermutation[c] >= 0)
		{
			array[mPermutation[c]] = 1.0;
		}
		else
		{
			array[c] = 1.0;
		}
		for (int num = mGivens.Count - 1; num >= 0; num--)
		{
			GivensRotation givensRotation = mGivens[num];
			double num2 = array[givensRotation.index];
			double num3 = array[givensRotation.index + 1];
			double num4 = givensRotation.cs * num2 + givensRotation.sn * num3;
			double num5 = (0.0 - givensRotation.sn) * num2 + givensRotation.cs * num3;
			array[givensRotation.index] = num4;
			array[givensRotation.index + 1] = num5;
		}
		for (int num6 = mSize - 3; num6 >= 0; num6--)
		{
			ArrayAlias<double> arrayAlias = new ArrayAlias<double>(mMatrix, num6);
			double num7 = arrayAlias[mSize * (num6 + 1)];
			int i;
			for (i = 0; i < num6 + 1; i++)
			{
				array2[i] = array[i];
			}
			double num8 = array[i];
			for (int j = i + 1; j < mSize; j++)
			{
				num8 += array[j] * arrayAlias[mSize * j];
			}
			num8 *= num7;
			array2[i] = array[i] - num8;
			for (i++; i < mSize; i++)
			{
				array2[i] = array[i] - num8 * arrayAlias[mSize * i];
			}
			double[] array3 = array;
			array = array2;
			array2 = array3;
		}
		if (array != eigenvector)
		{
			Array.Copy(array, eigenvector, mSize);
		}
	}

	public double[] GetEigenvector(int c)
	{
		double[] array = new double[mSize];
		GetEigenvector(c, array);
		return array;
	}

	private void Tridiagonalize()
	{
		int num = 0;
		int num2 = 1;
		while (num < mSize - 2)
		{
			double num3 = 0.0;
			for (int i = 0; i < num2; i++)
			{
				mVVector[i] = 0.0;
			}
			for (int i = num2; i < mSize; i++)
			{
				double num4 = mMatrix[i + mSize * num];
				mVVector[i] = num4;
				num3 += num4 * num4;
			}
			double num5 = 1.0;
			num3 = Math.Sqrt(num3);
			if (num3 > 0.0)
			{
				double num6 = mVVector[num2];
				double num7 = ((num6 >= 0.0) ? 1 : (-1));
				double num8 = 1.0 / (num6 + num7 * num3);
				mVVector[num2] = 1.0;
				for (int i = num2 + 1; i < mSize; i++)
				{
					mVVector[i] *= num8;
					num5 += mVVector[i] * mVVector[i];
				}
			}
			double num9 = 1.0 / num5;
			double num10 = num9 * 2.0;
			double num11 = 0.0;
			for (int i = num; i < mSize; i++)
			{
				mPVector[i] = 0.0;
				int j;
				for (j = num; j < i; j++)
				{
					mPVector[i] += mMatrix[i + mSize * j] * mVVector[j];
				}
				for (; j < mSize; j++)
				{
					mPVector[i] += mMatrix[j + mSize * i] * mVVector[j];
				}
				mPVector[i] *= num10;
				num11 += mPVector[i] * mVVector[i];
			}
			num11 *= num9;
			for (int i = num; i < mSize; i++)
			{
				mWVector[i] = mPVector[i] - num11 * mVVector[i];
			}
			for (int i = num; i < mSize; i++)
			{
				double num12 = mVVector[i];
				double num13 = mWVector[i];
				double num14 = num12 * num13 * 2.0;
				mMatrix[i + mSize * i] -= num14;
				for (int j = i + 1; j < mSize; j++)
				{
					num14 = num12 * mWVector[j] + num13 * mVVector[j];
					mMatrix[j + mSize * i] -= num14;
				}
			}
			mMatrix[num + mSize * num2] = num10;
			for (int i = num2 + 1; i < mSize; i++)
			{
				mMatrix[num + mSize * i] = mVVector[i];
			}
			num++;
			num2++;
		}
		int num15 = mSize - 1;
		int num16 = 0;
		int num17 = mSize + 1;
		int num18 = 0;
		while (num18 < num15)
		{
			mDiagonal[num18] = mMatrix[num16];
			mSuperdiagonal[num18] = mMatrix[num16 + 1];
			num18++;
			num16 += num17;
		}
		mDiagonal[num18] = mMatrix[num16];
	}

	private void GetSinCos(double x, double y, ref double cs, ref double sn)
	{
		if (y != 0.0)
		{
			if (Math.Abs(y) > Math.Abs(x))
			{
				double num = (0.0 - x) / y;
				sn = 1.0 / Math.Sqrt(1.0 + num * num);
				cs = sn * num;
			}
			else
			{
				double num = (0.0 - y) / x;
				cs = 1.0 / Math.Sqrt(1.0 + num * num);
				sn = cs * num;
			}
		}
		else
		{
			cs = 1.0;
			sn = 0.0;
		}
	}

	private void DoQRImplicitShift(int imin, int imax)
	{
		double num = mDiagonal[imax];
		double num2 = mSuperdiagonal[imax];
		double num3 = mDiagonal[imax + 1];
		double num4 = (num - num3) * 0.5;
		double num5 = ((num4 >= 0.0) ? 1 : (-1));
		double num6 = num2 * num2;
		double num7 = num3 - num6 / (num4 + num5 * Math.Sqrt(num4 * num4 + num6));
		double x = mDiagonal[imin] - num7;
		double y = mSuperdiagonal[imin];
		double cs = 0.0;
		double sn = 0.0;
		double num8 = 0.0;
		int num9 = imin - 1;
		int num10 = imin;
		int num11 = imin + 1;
		while (num10 <= imax)
		{
			GetSinCos(x, y, ref cs, ref sn);
			mGivens.Add(new GivensRotation(num10, cs, sn));
			if (num10 > imin)
			{
				mSuperdiagonal[num9] = cs * mSuperdiagonal[num9] - sn * num8;
			}
			num3 = mDiagonal[num10];
			double num12 = mSuperdiagonal[num10];
			double num13 = mDiagonal[num11];
			double num14 = cs * num3 - sn * num12;
			double num15 = cs * num12 - sn * num13;
			double num16 = sn * num3 + cs * num12;
			double num17 = sn * num12 + cs * num13;
			mDiagonal[num10] = cs * num14 - sn * num15;
			mSuperdiagonal[num10] = sn * num14 + cs * num15;
			mDiagonal[num11] = sn * num16 + cs * num17;
			if (num10 < imax)
			{
				double num18 = mSuperdiagonal[num11];
				num8 = (0.0 - sn) * num18;
				mSuperdiagonal[num11] = cs * num18;
				x = mSuperdiagonal[num10];
				y = num8;
			}
			num9++;
			num10++;
			num11++;
		}
	}

	private void ComputePermutation(int sortType)
	{
		mIsRotation = -1;
		if (sortType == 0)
		{
			mPermutation[0] = -1;
			return;
		}
		SortItem[] array = new SortItem[mSize];
		for (int i = 0; i < mSize; i++)
		{
			array[i].eigenvalue = mDiagonal[i];
			array[i].index = i;
		}
		if (sortType > 0)
		{
			Array.Sort(array, (SortItem a, SortItem b) => (a.eigenvalue != b.eigenvalue) ? ((!(a.eigenvalue < b.eigenvalue)) ? 1 : (-1)) : 0);
		}
		else
		{
			Array.Sort(array, (SortItem a, SortItem b) => (a.eigenvalue != b.eigenvalue) ? ((!(a.eigenvalue > b.eigenvalue)) ? 1 : (-1)) : 0);
		}
		for (int j = 0; j < mSize; j++)
		{
			mPermutation[j] = array[j].index;
		}
	}
}
