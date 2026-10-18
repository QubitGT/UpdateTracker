using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class VectorArray2<T> : IEnumerable<T>, IEnumerable
{
	public T[] array;

	public int Count => array.Length / 2;

	public VectorArray2(int nCount = 0)
	{
		array = new T[nCount * 2];
	}

	public VectorArray2(T[] data)
	{
		array = data;
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < array.Length)
		{
			yield return array[i];
			int num = i + 1;
			i = num;
		}
	}

	public void Resize(int Count)
	{
		array = new T[2 * Count];
	}

	public void Set(int i, T a, T b)
	{
		array[2 * i] = a;
		array[2 * i + 1] = b;
	}

	public void Set(int iStart, int iCount, VectorArray2<T> source)
	{
		Array.Copy(source.array, 0, array, 2 * iStart, 2 * iCount);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return array.GetEnumerator();
	}
}
