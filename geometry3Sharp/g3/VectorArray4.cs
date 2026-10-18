using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class VectorArray4<T> : IEnumerable<T>, IEnumerable
{
	public T[] array;

	public int Count => array.Length / 4;

	public VectorArray4(int nCount = 0)
	{
		array = new T[nCount * 4];
	}

	public VectorArray4(T[] data)
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
		array = new T[4 * Count];
	}

	public void Set(int i, T a, T b, T c, T d)
	{
		int num = 4 * i;
		array[num] = a;
		array[num + 1] = b;
		array[num + 2] = c;
		array[num + 3] = d;
	}

	public void Set(int iStart, int iCount, VectorArray4<T> source)
	{
		Array.Copy(source.array, 0, array, 4 * iStart, 4 * iCount);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return array.GetEnumerator();
	}
}
