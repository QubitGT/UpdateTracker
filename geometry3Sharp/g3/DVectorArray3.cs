using System.Collections;
using System.Collections.Generic;

namespace g3;

public class DVectorArray3<T> : IEnumerable<T>, IEnumerable
{
	public DVector<T> vector;

	public int Count => vector.Length / 3;

	public DVectorArray3(int nCount = 0)
	{
		vector = new DVector<T>();
		if (nCount > 0)
		{
			vector.resize(nCount * 3);
		}
	}

	public DVectorArray3(T[] data)
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
		vector.resize(3 * count);
	}

	public void Set(int i, T a, T b, T c)
	{
		vector.insert(a, 3 * i);
		vector.insert(b, 3 * i + 1);
		vector.insert(c, 3 * i + 2);
	}

	public void Append(T a, T b, T c)
	{
		vector.push_back(a);
		vector.push_back(b);
		vector.push_back(c);
	}

	public void Clear()
	{
		vector.Clear();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
