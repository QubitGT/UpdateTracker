using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class VectorArray3<T> : IEnumerable<T>, IEnumerable
{
	public T[] array;

	public int Count => array.Length / 3;

	public VectorArray3(int nCount = 0)
	{
		array = new T[nCount * 3];
	}

	public VectorArray3(T[] data)
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
		array = new T[3 * Count];
	}

	public void Set(int i, T a, T b, T c)
	{
		array[3 * i] = a;
		array[3 * i + 1] = b;
		array[3 * i + 2] = c;
	}

	public void Set(int iStart, int iCount, VectorArray3<T> source)
	{
		Array.Copy(source.array, 0, array, 3 * iStart, 3 * iCount);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return array.GetEnumerator();
	}
}
