using System;
using System.Collections.Generic;

namespace g3;

public class SparseObjectList<T> where T : class
{
	private T[] dense;

	private Dictionary<int, T> sparse;

	public T this[int idx]
	{
		get
		{
			if (dense != null)
			{
				return dense[idx];
			}
			if (sparse.TryGetValue(idx, out var value))
			{
				return value;
			}
			return null;
		}
		set
		{
			if (dense != null)
			{
				dense[idx] = value;
			}
			else
			{
				sparse[idx] = value;
			}
		}
	}

	public SparseObjectList(int MaxIndex, int SubsetCountEst)
	{
		bool num = MaxIndex < 1024;
		float num2 = (float)SubsetCountEst / (float)MaxIndex;
		float num3 = 0.1f;
		if (num || num2 > num3)
		{
			dense = new T[MaxIndex];
			for (int i = 0; i < MaxIndex; i++)
			{
				dense[i] = null;
			}
		}
		else
		{
			sparse = new Dictionary<int, T>();
		}
	}

	public int Count(Func<T, bool> CountF)
	{
		int num = 0;
		if (dense != null)
		{
			for (int i = 0; i < dense.Length; i++)
			{
				if (CountF(dense[i]))
				{
					num++;
				}
			}
		}
		else
		{
			foreach (KeyValuePair<int, T> item in sparse)
			{
				if (CountF(item.Value))
				{
					num++;
				}
			}
		}
		return num;
	}

	public IEnumerable<KeyValuePair<int, T>> Values()
	{
		if (dense != null)
		{
			int i = 0;
			while (i < dense.Length)
			{
				yield return new KeyValuePair<int, T>(i, dense[i]);
				int num = i + 1;
				i = num;
			}
			yield break;
		}
		foreach (KeyValuePair<int, T> item in sparse)
		{
			yield return item;
		}
	}

	public IEnumerable<KeyValuePair<int, T>> NonZeroValues()
	{
		if (dense != null)
		{
			int i = 0;
			while (i < dense.Length)
			{
				if (dense[i] != null)
				{
					yield return new KeyValuePair<int, T>(i, dense[i]);
				}
				int num = i + 1;
				i = num;
			}
			yield break;
		}
		foreach (KeyValuePair<int, T> item in sparse)
		{
			yield return item;
		}
	}

	public void Clear()
	{
		if (dense != null)
		{
			Array.Clear(dense, 0, dense.Length);
		}
		else
		{
			sparse.Clear();
		}
	}
}
