using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class PackedSparseMatrix
{
	public struct nonzero
	{
		public int j;

		public double d;
	}

	public enum StorageModes
	{
		Full
	}

	public nonzero[][] Rows;

	public int Columns;

	public bool Sorted;

	public int NumNonZeros;

	public StorageModes StorageMode;

	public bool IsSymmetric;

	public double this[int r, int c]
	{
		get
		{
			nonzero[] array = Rows[r];
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (array[i].j == c)
				{
					return array[i].d;
				}
			}
			return 0.0;
		}
		set
		{
			nonzero[] array = Rows[r];
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				if (array[i].j == c)
				{
					array[i].d = value;
					return;
				}
			}
			throw new Exception("PackedSparseMatrix[r,c]: value at index " + r + "," + c + " does not exist!");
		}
	}

	public double FrobeniusNorm
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < Rows.Length; i++)
			{
				nonzero[] array = Rows[i];
				for (int j = 0; j < array.Length; j++)
				{
					num += array[j].d * array[j].d;
				}
			}
			return Math.Sqrt(num);
		}
	}

	public double MaxNorm
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < Rows.Length; i++)
			{
				nonzero[] array = Rows[i];
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].d > num)
					{
						num = array[j].d;
					}
				}
			}
			return num;
		}
	}

	public double Trace
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < Rows.Length; i++)
			{
				nonzero[] array = Rows[i];
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].j == i)
					{
						num += array[j].d;
					}
				}
			}
			return num;
		}
	}

	public PackedSparseMatrix(PackedSparseMatrix copy)
	{
		int num = copy.Rows.Length;
		Rows = new nonzero[num][];
		for (int i = 0; i < num; i++)
		{
			Rows[i] = new nonzero[copy.Rows[i].Length];
			Array.Copy(copy.Rows[i], Rows[i], Rows[i].Length);
		}
		Columns = copy.Columns;
		Sorted = copy.Sorted;
		NumNonZeros = copy.NumNonZeros;
		StorageMode = copy.StorageMode;
		IsSymmetric = copy.IsSymmetric;
	}

	public PackedSparseMatrix(SymmetricSparseMatrix m, bool bTranspose = false)
	{
		int num = (bTranspose ? m.Columns : m.Rows);
		Columns = (bTranspose ? m.Columns : m.Rows);
		Rows = new nonzero[num][];
		int[] array = new int[num];
		foreach (Index2i item in m.NonZeroIndices())
		{
			array[item.a]++;
			if (item.a != item.b)
			{
				array[item.b]++;
			}
		}
		NumNonZeros = 0;
		for (int i = 0; i < num; i++)
		{
			Rows[i] = new nonzero[array[i]];
			NumNonZeros += array[i];
		}
		int[] array2 = new int[num];
		foreach (KeyValuePair<Index2i, double> item2 in m.NonZeros())
		{
			int num2 = item2.Key.a;
			int num3 = item2.Key.b;
			if (bTranspose)
			{
				int num4 = num2;
				num2 = num3;
				num3 = num4;
			}
			int num5 = array2[num2]++;
			Rows[num2][num5].j = num3;
			Rows[num2][num5].d = item2.Value;
			if (num2 != num3)
			{
				num5 = array2[num3]++;
				Rows[num3][num5].j = num2;
				Rows[num3][num5].d = item2.Value;
			}
		}
		Sorted = false;
		IsSymmetric = true;
		StorageMode = StorageModes.Full;
	}

	public PackedSparseMatrix(DVector<matrix_entry> entries, int numRows, int numCols, bool bSymmetric = true)
	{
		Columns = numCols;
		Rows = new nonzero[numRows][];
		int size = entries.size;
		int[] array = new int[numRows];
		for (int i = 0; i < size; i++)
		{
			array[entries[i].r]++;
			if (bSymmetric && entries[i].r != entries[i].c)
			{
				array[entries[i].c]++;
			}
		}
		NumNonZeros = 0;
		for (int j = 0; j < numRows; j++)
		{
			Rows[j] = new nonzero[array[j]];
			NumNonZeros += array[j];
		}
		int[] array2 = new int[numRows];
		for (int k = 0; k < size; k++)
		{
			matrix_entry matrix_entry2 = entries[k];
			int num = array2[matrix_entry2.r]++;
			Rows[matrix_entry2.r][num].j = matrix_entry2.c;
			Rows[matrix_entry2.r][num].d = matrix_entry2.value;
			if (bSymmetric && matrix_entry2.c != matrix_entry2.r)
			{
				num = array2[matrix_entry2.c]++;
				Rows[matrix_entry2.c][num].j = matrix_entry2.r;
				Rows[matrix_entry2.c][num].d = matrix_entry2.value;
			}
		}
		Sorted = false;
		IsSymmetric = bSymmetric;
		StorageMode = StorageModes.Full;
	}

	public static PackedSparseMatrix FromDense(DenseMatrix m, bool bSymmetric)
	{
		DVector<matrix_entry> dVector = new DVector<matrix_entry>();
		for (int i = 0; i < m.Rows; i++)
		{
			int num = (bSymmetric ? (i + 1) : m.Columns);
			for (int j = 0; j < num; j++)
			{
				if (m[i, j] != 0.0)
				{
					dVector.Add(new matrix_entry
					{
						r = i,
						c = j,
						value = m[i, j]
					});
				}
			}
		}
		return new PackedSparseMatrix(dVector, m.Rows, m.Columns, bSymmetric);
	}

	public void Sort(bool bParallel = true)
	{
		if (bParallel)
		{
			gParallel.BlockStartEnd(0, Rows.Length - 1, delegate(int a, int b)
			{
				for (int j = a; j <= b; j++)
				{
					Array.Sort(Rows[j], (nonzero x, nonzero y) => x.j.CompareTo(y.j));
				}
			});
		}
		else
		{
			for (int i = 0; i < Rows.Length; i++)
			{
				Array.Sort(Rows[i], (nonzero x, nonzero y) => x.j.CompareTo(y.j));
			}
		}
		Sorted = true;
	}

	public Interval1i NonZerosRange(int r)
	{
		nonzero[] array = Rows[r];
		if (array.Length == 0)
		{
			return Interval1i.Empty;
		}
		if (!Sorted)
		{
			Interval1i empty = Interval1i.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				empty.Contain(array[i].j);
			}
			return empty;
		}
		return new Interval1i(array[0].j, array[^1].j);
	}

	public IEnumerable<Vector2i> NonZeroIndicesByRow(bool bWantSorted = true)
	{
		if (bWantSorted && !Sorted)
		{
			throw new Exception("PackedSparseMatrix.NonZeroIndicesByRow: sorting requested but not available");
		}
		int N = Rows.Length;
		int r = 0;
		while (r < N)
		{
			nonzero[] Row = Rows[r];
			int num;
			for (int i = 0; i < Row.Length; i = num)
			{
				yield return new Vector2i(r, Row[i].j);
				num = i + 1;
			}
			num = r + 1;
			r = num;
		}
	}

	public IEnumerable<Vector2i> NonZeroIndicesForRow(int r, bool bWantSorted = true)
	{
		if (bWantSorted && !Sorted)
		{
			throw new Exception("PackedSparseMatrix.NonZeroIndicesByRow: sorting requested but not available");
		}
		nonzero[] Row = Rows[r];
		int i = 0;
		while (i < Row.Length)
		{
			yield return new Vector2i(r, Row[i].j);
			int num = i + 1;
			i = num;
		}
	}

	public void Multiply(double[] X, double[] Result)
	{
		Array.Clear(Result, 0, Result.Length);
		for (int i = 0; i < Rows.Length; i++)
		{
			int num = Rows[i].Length;
			for (int j = 0; j < num; j++)
			{
				int j2 = Rows[i][j].j;
				Result[i] += Rows[i][j].d * X[j2];
			}
		}
	}

	public void Multiply_Parallel(double[] X, double[] Result)
	{
		gParallel.BlockStartEnd(0, Rows.Length - 1, delegate(int i_start, int i_end)
		{
			for (int i = i_start; i <= i_end; i++)
			{
				Result[i] = 0.0;
				nonzero[] array = Rows[i];
				int num = array.Length;
				for (int j = 0; j < num; j++)
				{
					Result[i] += array[j].d * X[array[j].j];
				}
			}
		});
	}

	public void Multiply_Parallel_3(double[][] X, double[][] Result)
	{
		_ = X.Length;
		gParallel.BlockStartEnd(0, Rows.Length - 1, delegate(int i_start, int i_end)
		{
			for (int i = i_start; i <= i_end; i++)
			{
				Result[0][i] = (Result[1][i] = (Result[2][i] = 0.0));
				nonzero[] array = Rows[i];
				int num = array.Length;
				for (int j = 0; j < num; j++)
				{
					int j2 = array[j].j;
					double d = array[j].d;
					Result[0][i] += d * X[0][j2];
					Result[1][i] += d * X[1][j2];
					Result[2][i] += d * X[2][j2];
				}
			}
		});
	}

	public double DotRowColumn(int r, int c, PackedSparseMatrix MTranspose)
	{
		if (!Sorted || !MTranspose.Sorted)
		{
			throw new Exception("PackedSparseMatrix.DotRowColumn: matrices must be sorted!");
		}
		if (Rows.Length != MTranspose.Rows.Length)
		{
			throw new Exception("PackedSparseMatrix.DotRowColumn: matrices are not the same size!");
		}
		int num = 0;
		int num2 = 0;
		nonzero[] array = Rows[r];
		nonzero[] array2 = MTranspose.Rows[c];
		int num3 = array.Length;
		int num4 = array2.Length;
		int j = array2[num4 - 1].j;
		int j2 = array[num3 - 1].j;
		double num5 = 0.0;
		while (num < num3 && num2 < num4 && array[num].j <= j && array2[num2].j <= j2)
		{
			if (array[num].j == array2[num2].j)
			{
				num5 += array[num].d * array2[num2].d;
				num++;
				num2++;
			}
			else if (array[num].j < array2[num2].j)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		return num5;
	}

	public double DotRowSelf(int r)
	{
		nonzero[] array = Rows[r];
		double num = 0.0;
		for (int i = 0; i < array.Length; i++)
		{
			num += array[i].d * array[i].d;
		}
		return num;
	}

	public void DotRowAllColumns(int r, double[] sums, int[] col_indices, PackedSparseMatrix MTranspose)
	{
		int num = Rows.Length;
		int i = 0;
		nonzero[] array = Rows[r];
		int num2 = array.Length;
		Array.Clear(sums, 0, num);
		Array.Clear(col_indices, 0, num);
		for (; i < num2; i++)
		{
			int j = array[i].j;
			for (int k = 0; k < num; k++)
			{
				nonzero[] array2 = MTranspose.Rows[k];
				int l = col_indices[k];
				if (l < array2.Length)
				{
					for (; l < array2.Length && array2[l].j < j; l++)
					{
					}
					if (l < array2.Length && j == array2[l].j)
					{
						sums[k] += array[i].d * array2[l].d;
						l++;
					}
					col_indices[k] = l;
				}
			}
		}
	}

	public double DotRows(int r1, int r2, int MaxCol = int.MaxValue)
	{
		if (!Sorted)
		{
			throw new Exception("PackedSparseMatrix.DotRows: matrices must be sorted!");
		}
		MaxCol = Math.Min(MaxCol, Columns);
		int num = 0;
		int num2 = 0;
		nonzero[] array = Rows[r1];
		nonzero[] array2 = Rows[r2];
		int num3 = array.Length;
		int num4 = array2.Length;
		double num5 = 0.0;
		while (num < num3 && num2 < num4 && array[num].j <= MaxCol && array2[num2].j <= MaxCol)
		{
			if (array[num].j == array2[num2].j)
			{
				num5 += array[num].d * array2[num2].d;
				num++;
				num2++;
			}
			else if (array[num].j < array2[num2].j)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		return num5;
	}

	public double DotRowVector(int r, double[] vec, int MaxCol = int.MaxValue)
	{
		if (!Sorted && MaxCol < int.MaxValue)
		{
			throw new Exception("PackedSparseMatrix.DotRows: matrices must be sorted if MaxCol is specified!");
		}
		MaxCol = Math.Min(MaxCol, Columns);
		nonzero[] array = Rows[r];
		double num = 0.0;
		for (int i = 0; i < array.Length && array[i].j <= MaxCol; i++)
		{
			num += array[i].d * vec[array[i].j];
		}
		return num;
	}

	public double DotColumnVector(int c, double[] vec, int start_row = 0, int end_row = int.MaxValue)
	{
		_ = Rows.Length;
		double num = 0.0;
		if (Sorted)
		{
			for (int i = start_row; i <= end_row; i++)
			{
				nonzero[] array = Rows[i];
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].j == c)
					{
						num += array[j].d * vec[i];
						break;
					}
					if (array[j].j > c)
					{
						break;
					}
				}
			}
		}
		else
		{
			for (int k = start_row; k <= end_row; k++)
			{
				nonzero[] array2 = Rows[k];
				for (int l = 0; l < array2.Length; l++)
				{
					if (array2[l].j == c)
					{
						num += array2[l].d * vec[k];
						break;
					}
				}
			}
		}
		return num;
	}

	public PackedSparseMatrix Square()
	{
		if (Rows.Length != Columns)
		{
			throw new Exception("PackedSparseMatrix.Square: matrix is not square!");
		}
		int columns = Columns;
		DVector<matrix_entry> entries = new DVector<matrix_entry>();
		SpinLock entries_lock = default(SpinLock);
		gParallel.BlockStartEnd(0, columns - 1, delegate(int r_start, int r_end)
		{
			for (int i = r_start; i <= r_end; i++)
			{
				HashSet<int> hashSet = new HashSet<int> { i };
				nonzero[] array = Rows[i];
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].j > i)
					{
						hashSet.Add(array[j].j);
					}
					nonzero[] array2 = Rows[array[j].j];
					for (int k = 0; k < array2.Length; k++)
					{
						if (array2[k].j > i)
						{
							hashSet.Add(array2[k].j);
						}
					}
				}
				foreach (int item in hashSet)
				{
					double value = DotRowColumn(i, item, this);
					if (Math.Abs(value) > 1E-08)
					{
						bool lockTaken = false;
						entries_lock.Enter(ref lockTaken);
						entries.Add(new matrix_entry
						{
							r = i,
							c = item,
							value = value
						});
						entries_lock.Exit();
					}
				}
			}
		});
		return new PackedSparseMatrix(entries, columns, columns);
	}

	public string MatrixInfo(bool bExtended = false)
	{
		string text = $"Rows {Rows.Length}  Cols {Columns}   NonZeros {NumNonZeros}  Sorted {Sorted}";
		if (bExtended)
		{
			double num = 0.0;
			nonzero[][] rows = Rows;
			foreach (nonzero[] array in rows)
			{
				for (int j = 0; j < array.Length; j++)
				{
					nonzero nonzero = array[j];
					num += nonzero.d;
				}
			}
			text += $"  Sum {num}  Frobenius {FrobeniusNorm}  Max {MaxNorm}  Trace {Trace}";
		}
		return text;
	}
}
