using System;
using System.Collections.Generic;

namespace g3;

public class SingularValueDecomposition
{
	private struct GivensRotation
	{
		public int index0;

		public int index1;

		public double cs;

		public double sn;

		public GivensRotation(int inIndex0, int inIndex1, double inCs, double inSn)
		{
			index0 = inIndex0;
			index1 = inIndex1;
			cs = inCs;
			sn = inSn;
		}
	}

	private int mNumRows;

	private int mNumCols;

	private int mMaxIterations;

	private double[] mMatrix;

	private double[] mDiagonal;

	private double[] mSuperdiagonal;

	private List<GivensRotation> mRGivens;

	private List<GivensRotation> mLGivens;

	private double[] mFixupDiagonal;

	private int[] mPermutation;

	private int[] mVisited;

	private double[] mTwoInvUTU;

	private double[] mTwoInvVTV;

	private double[] mUVector;

	private double[] mVVector;

	private double[] mWVector;

	public SingularValueDecomposition(int numRows, int numCols, int maxIterations)
	{
		mNumRows = (mNumCols = (mMaxIterations = 0));
		if (numCols > 1 && numRows >= numCols && maxIterations > 0)
		{
			mNumRows = numRows;
			mNumCols = numCols;
			mMaxIterations = maxIterations;
			mMatrix = new double[numRows * numCols];
			mDiagonal = new double[numCols];
			mSuperdiagonal = new double[numCols - 1];
			mRGivens = new List<GivensRotation>(maxIterations * (numCols - 1));
			mLGivens = new List<GivensRotation>(maxIterations * (numCols - 1));
			mFixupDiagonal = new double[numCols];
			mPermutation = new int[numCols];
			mVisited = new int[numCols];
			mTwoInvUTU = new double[numCols];
			mTwoInvVTV = new double[numCols - 2];
			mUVector = new double[numRows];
			mVVector = new double[numCols];
			mWVector = new double[numRows];
		}
	}

	public uint Solve(double[] input, int sortType = -1)
	{
		if (mNumRows > 0)
		{
			int num = mNumRows * mNumCols;
			Array.Copy(input, mMatrix, num);
			Bidiagonalize();
			double num2 = Math.Abs(input[0]);
			for (int i = 1; i < num; i++)
			{
				double num3 = Math.Abs(input[i]);
				if (num3 > num2)
				{
					num2 = num3;
				}
			}
			double num4 = 0.0;
			if (num2 > 0.0)
			{
				double num5 = 1.0 / num2;
				for (int j = 0; j < num; j++)
				{
					double num6 = input[j] * num5;
					num4 += num6 * num6;
				}
				num4 = num2 * Math.Sqrt(num4);
			}
			double num7 = double.Epsilon;
			double threshold = 8.0 * num7 * num4;
			mRGivens.Clear();
			mLGivens.Clear();
			for (uint num8 = 0u; num8 < mMaxIterations; num8++)
			{
				int num9 = -1;
				int num10 = -1;
				for (int num11 = mNumCols - 2; num11 >= 0; num11--)
				{
					double value = mDiagonal[num11];
					double value2 = mSuperdiagonal[num11];
					double value3 = mDiagonal[num11 + 1];
					double num12 = Math.Abs(value) + Math.Abs(value3);
					if (num12 + Math.Abs(value2) != num12)
					{
						if (num10 == -1)
						{
							num10 = num11;
						}
						num9 = num11;
					}
					else if (num9 >= 0)
					{
						break;
					}
				}
				if (num10 == -1)
				{
					EnsureNonnegativeDiagonal();
					ComputePermutation(sortType);
					return num8;
				}
				if (DiagonalEntriesNonzero(num9, num10, threshold))
				{
					DoGolubKahanStep(num9, num10);
				}
			}
			return uint.MaxValue;
		}
		return 0u;
	}

	public void GetSingularValues(double[] singularValues)
	{
		if (singularValues == null || mNumCols <= 0)
		{
			return;
		}
		if (mPermutation[0] >= 0)
		{
			for (int i = 0; i < mNumCols; i++)
			{
				int num = mPermutation[i];
				singularValues[i] = mDiagonal[num];
			}
		}
		else
		{
			for (int j = 0; j < mNumCols; j++)
			{
				singularValues[j] = mDiagonal[j];
			}
		}
	}

	public void GetU(double[] uMatrix)
	{
		if (uMatrix == null || mNumCols == 0)
		{
			return;
		}
		Array.Clear(uMatrix, 0, uMatrix.Length);
		for (int i = 0; i < mNumRows; i++)
		{
			uMatrix[i + mNumRows * i] = 1.0;
		}
		int num = mNumCols - 1;
		int num2 = num + 1;
		while (num >= 0)
		{
			double num3 = mTwoInvUTU[num];
			mUVector[num] = 1.0;
			for (int j = num2; j < mNumRows; j++)
			{
				mUVector[j] = mMatrix[num + mNumCols * j];
			}
			mWVector[num] = num3;
			for (int j = num2; j < mNumRows; j++)
			{
				mWVector[j] = 0.0;
				for (int k = num2; k < mNumRows; k++)
				{
					mWVector[j] += mUVector[k] * uMatrix[j + mNumRows * k];
				}
				mWVector[j] *= num3;
			}
			for (int j = num; j < mNumRows; j++)
			{
				for (int k = num; k < mNumRows; k++)
				{
					uMatrix[k + mNumRows * j] -= mUVector[j] * mWVector[k];
				}
			}
			num--;
			num2--;
		}
		foreach (GivensRotation mLGiven in mLGivens)
		{
			int num4 = mLGiven.index0;
			int num5 = mLGiven.index1;
			int j = 0;
			while (j < mNumRows)
			{
				double num6 = uMatrix[num4];
				double num7 = uMatrix[num5];
				double num8 = mLGiven.cs * num6 - mLGiven.sn * num7;
				double num9 = mLGiven.sn * num6 + mLGiven.cs * num7;
				uMatrix[num4] = num8;
				uMatrix[num5] = num9;
				j++;
				num4 += mNumRows;
				num5 += mNumRows;
			}
		}
		if (mPermutation[0] < 0)
		{
			return;
		}
		Array.Clear(mVisited, 0, mVisited.Length);
		for (int k = 0; k < mNumCols; k++)
		{
			if (mVisited[k] != 0 || mPermutation[k] == k)
			{
				continue;
			}
			int num10 = k;
			int num11 = k;
			for (int j = 0; j < mNumRows; j++)
			{
				mWVector[j] = uMatrix[k + mNumRows * j];
			}
			int num12;
			while ((num12 = mPermutation[num11]) != num10)
			{
				mVisited[num11] = 1;
				for (int j = 0; j < mNumRows; j++)
				{
					uMatrix[num11 + mNumRows * j] = uMatrix[num12 + mNumRows * j];
				}
				num11 = num12;
			}
			mVisited[num11] = 1;
			for (int j = 0; j < mNumRows; j++)
			{
				uMatrix[num11 + mNumRows * j] = mWVector[j];
			}
		}
	}

	public void GetV(double[] vMatrix)
	{
		if (vMatrix == null || mNumCols == 0)
		{
			return;
		}
		Array.Clear(vMatrix, 0, vMatrix.Length);
		for (int i = 0; i < mNumCols; i++)
		{
			vMatrix[i + mNumCols * i] = 1.0;
		}
		int num = mNumCols - 3;
		int num2 = num + 1;
		int num3 = num + 2;
		while (num >= 0)
		{
			double num4 = mTwoInvVTV[num];
			mVVector[num2] = 1.0;
			for (int j = num3; j < mNumCols; j++)
			{
				mVVector[j] = mMatrix[mNumCols * num + j];
			}
			mWVector[num2] = num4;
			for (int j = num3; j < mNumCols; j++)
			{
				mWVector[j] = 0.0;
				for (int k = num3; k < mNumCols; k++)
				{
					mWVector[j] += mVVector[k] * vMatrix[j + mNumCols * k];
				}
				mWVector[j] *= num4;
			}
			for (int j = num2; j < mNumCols; j++)
			{
				for (int k = num2; k < mNumCols; k++)
				{
					vMatrix[k + mNumCols * j] -= mVVector[j] * mWVector[k];
				}
			}
			num--;
			num2--;
			num3--;
		}
		foreach (GivensRotation mRGiven in mRGivens)
		{
			int num5 = mRGiven.index0;
			int num6 = mRGiven.index1;
			int k = 0;
			while (k < mNumCols)
			{
				double num7 = vMatrix[num5];
				double num8 = vMatrix[num6];
				double num9 = mRGiven.cs * num7 - mRGiven.sn * num8;
				double num10 = mRGiven.sn * num7 + mRGiven.cs * num8;
				vMatrix[num5] = num9;
				vMatrix[num6] = num10;
				k++;
				num5 += mNumCols;
				num6 += mNumCols;
			}
		}
		for (int j = 0; j < mNumCols; j++)
		{
			for (int k = 0; k < mNumCols; k++)
			{
				vMatrix[k + mNumCols * j] *= mFixupDiagonal[k];
			}
		}
		if (mPermutation[0] < 0)
		{
			return;
		}
		Array.Clear(mVisited, 0, mVisited.Length);
		for (int k = 0; k < mNumCols; k++)
		{
			if (mVisited[k] != 0 || mPermutation[k] == k)
			{
				continue;
			}
			int num11 = k;
			int num12 = k;
			for (int j = 0; j < mNumCols; j++)
			{
				mWVector[j] = vMatrix[k + mNumCols * j];
			}
			int num13;
			while ((num13 = mPermutation[num12]) != num11)
			{
				mVisited[num12] = 1;
				for (int j = 0; j < mNumCols; j++)
				{
					vMatrix[num12 + mNumCols * j] = vMatrix[num13 + mNumCols * j];
				}
				num12 = num13;
			}
			mVisited[num12] = 1;
			for (int j = 0; j < mNumCols; j++)
			{
				vMatrix[num12 + mNumCols * j] = mWVector[j];
			}
		}
	}

	private void Bidiagonalize()
	{
		int num = 0;
		int num2 = 1;
		while (num < mNumCols)
		{
			double num3 = 0.0;
			for (int i = num; i < mNumRows; i++)
			{
				double num4 = mMatrix[num + mNumCols * i];
				mUVector[i] = num4;
				num3 += num4 * num4;
			}
			double num5 = 1.0;
			num3 = Math.Sqrt(num3);
			if (num3 > 0.0)
			{
				double num6 = mUVector[num];
				double num7 = ((num6 >= 0.0) ? 1.0 : (-1.0));
				double num8 = 1.0 / (num6 + num7 * num3);
				mUVector[num] = 1.0;
				for (int i = num2; i < mNumRows; i++)
				{
					mUVector[i] *= num8;
					num5 += mUVector[i] * mUVector[i];
				}
			}
			double num9 = 1.0 / num5 * 2.0;
			for (int j = num; j < mNumCols; j++)
			{
				mWVector[j] = 0.0;
				for (int i = num; i < mNumRows; i++)
				{
					mWVector[j] += mMatrix[j + mNumCols * i] * mUVector[i];
				}
				mWVector[j] *= num9;
			}
			for (int i = num; i < mNumRows; i++)
			{
				for (int j = num; j < mNumCols; j++)
				{
					mMatrix[j + mNumCols * i] -= mUVector[i] * mWVector[j];
				}
			}
			if (num < mNumCols - 2)
			{
				num3 = 0.0;
				for (int j = num2; j < mNumCols; j++)
				{
					double num10 = mMatrix[j + mNumCols * num];
					mVVector[j] = num10;
					num3 += num10 * num10;
				}
				double num11 = 1.0;
				num3 = Math.Sqrt(num3);
				if (num3 > 0.0)
				{
					double num12 = mVVector[num2];
					double num13 = ((num12 >= 0.0) ? 1.0 : (-1.0));
					double num14 = 1.0 / (num12 + num13 * num3);
					mVVector[num2] = 1.0;
					for (int j = num2 + 1; j < mNumCols; j++)
					{
						mVVector[j] *= num14;
						num11 += mVVector[j] * mVVector[j];
					}
				}
				double num15 = 1.0 / num11 * 2.0;
				for (int i = num; i < mNumRows; i++)
				{
					mWVector[i] = 0.0;
					for (int j = num2; j < mNumCols; j++)
					{
						mWVector[i] += mMatrix[j + mNumCols * i] * mVVector[j];
					}
					mWVector[i] *= num15;
				}
				for (int i = num; i < mNumRows; i++)
				{
					for (int j = num2; j < mNumCols; j++)
					{
						mMatrix[j + mNumCols * i] -= mWVector[i] * mVVector[j];
					}
				}
				mTwoInvVTV[num] = num15;
				for (int j = num + 2; j < mNumCols; j++)
				{
					mMatrix[j + mNumCols * num] = mVVector[j];
				}
			}
			mTwoInvUTU[num] = num9;
			for (int i = num2; i < mNumRows; i++)
			{
				mMatrix[num + mNumCols * i] = mUVector[i];
			}
			num++;
			num2++;
		}
		int num16 = mNumCols - 1;
		int num17 = 0;
		int num18 = mNumCols + 1;
		int num19 = 0;
		while (num19 < num16)
		{
			mDiagonal[num19] = mMatrix[num17];
			mSuperdiagonal[num19] = mMatrix[num17 + 1];
			num19++;
			num17 += num18;
		}
		mDiagonal[num19] = mMatrix[num17];
	}

	private void GetSinCos(double x, double y, out double cs, out double sn)
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

	private bool DiagonalEntriesNonzero(int imin, int imax, double threshold)
	{
		for (int i = imin; i <= imax; i++)
		{
			if (!(Math.Abs(mDiagonal[i]) <= threshold))
			{
				continue;
			}
			double num = mSuperdiagonal[i];
			mSuperdiagonal[i] = 0.0;
			for (int j = i + 1; j <= imax + 1; j++)
			{
				double num2 = mDiagonal[j];
				GetSinCos(num2, num, out var cs, out var sn);
				mLGivens.Add(new GivensRotation(i, j, cs, sn));
				mDiagonal[j] = cs * num2 - sn * num;
				if (j <= imax)
				{
					double num3 = mSuperdiagonal[j];
					mSuperdiagonal[j] = cs * num3;
					num = sn * num3;
				}
			}
			return false;
		}
		return true;
	}

	private void DoGolubKahanStep(int imin, int imax)
	{
		double num = (((double)imax >= 1.0) ? mSuperdiagonal[imax - 1] : 0.0);
		double num2 = mDiagonal[imax];
		double num3 = mSuperdiagonal[imax];
		double num4 = mDiagonal[imax + 1];
		double num5 = num2 * num2 + num * num;
		double num6 = num2 * num3;
		double num7 = num4 * num4 + num3 * num3;
		double num8 = (num5 - num7) * 0.5;
		double num9 = ((num8 >= 0.0) ? 1.0 : (-1.0));
		double num10 = num6 * num6;
		double num11 = num7 - num10 / (num8 + num9 * Math.Sqrt(num8 * num8 + num10));
		double x = mDiagonal[imin] * mDiagonal[imin] - num11;
		double y = mDiagonal[imin] * mSuperdiagonal[imin];
		double num12 = 0.0;
		int num13 = imin - 1;
		int num14 = imin;
		int num15 = imin + 1;
		while (num14 <= imax)
		{
			GetSinCos(x, y, out var cs, out var sn);
			mRGivens.Add(new GivensRotation(num14, num15, cs, sn));
			if (num14 > imin)
			{
				mSuperdiagonal[num13] = cs * mSuperdiagonal[num13] - sn * num12;
			}
			num7 = mDiagonal[num14];
			double num16 = mSuperdiagonal[num14];
			double num17 = mDiagonal[num15];
			mDiagonal[num14] = cs * num7 - sn * num16;
			mSuperdiagonal[num14] = sn * num7 + cs * num16;
			mDiagonal[num15] = cs * num17;
			double num18 = (0.0 - sn) * num17;
			x = mDiagonal[num14];
			y = num18;
			GetSinCos(x, y, out cs, out sn);
			mLGivens.Add(new GivensRotation(num14, num15, cs, sn));
			num7 = mDiagonal[num14];
			num16 = mSuperdiagonal[num14];
			num17 = mDiagonal[num15];
			mDiagonal[num14] = cs * num7 - sn * num18;
			mSuperdiagonal[num14] = cs * num16 - sn * num17;
			mDiagonal[num15] = sn * num16 + cs * num17;
			if (num14 < imax)
			{
				double num19 = mSuperdiagonal[num15];
				num12 = (0.0 - sn) * num19;
				mSuperdiagonal[num15] = cs * num19;
				x = mSuperdiagonal[num14];
				y = num12;
			}
			num13++;
			num14++;
			num15++;
		}
	}

	private void EnsureNonnegativeDiagonal()
	{
		for (int i = 0; i < mNumCols; i++)
		{
			if (mDiagonal[i] >= 0.0)
			{
				mFixupDiagonal[i] = 1.0;
				continue;
			}
			mDiagonal[i] = 0.0 - mDiagonal[i];
			mFixupDiagonal[i] = -1.0;
		}
	}

	private void ComputePermutation(int sortType)
	{
		if (sortType == 0)
		{
			mPermutation[0] = -1;
			return;
		}
		double[] array = new double[mNumCols];
		int[] array2 = new int[mNumCols];
		for (int i = 0; i < mNumCols; i++)
		{
			array[i] = mDiagonal[i];
			array2[i] = i;
		}
		Array.Sort(array, array2);
		if (sortType < 0)
		{
			Array.Reverse(array2);
		}
		mPermutation = array2;
	}
}
