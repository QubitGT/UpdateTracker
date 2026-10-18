using System.Collections;
using System.Collections.Generic;

namespace g3;

public class DVectorArray2<T> : IEnumerable<T>, IEnumerable
{
	public DVector<T> vector;

	public int Count => vector.Length / 2;

	public DVectorArray2(int nCount = 0)
	{
		vector = new DVector<T>();
		vector.resize(nCount * 2);
	}

	public DVectorArray2(T[] data)
	{
		vector = new DVector<T>(data);
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < vector.Length)
		{
			yield return vector[i];
			int num = i + 1;
			i = num;
		}
	}

	public void Resize(int count)
	{
		vector.resize(2 * count);
	}

	public void Set(int i, T a, T b)
	{
		vector.insert(a, 2 * i);
		vector.insert(b, 2 * i + 1);
	}

	public void Append(T a, T b)
	{
		vector.push_back(a);
		vector.push_back(b);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
