using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class MappedList : IList<int>, ICollection<int>, IEnumerable<int>, IEnumerable
{
	public IList<int> BaseList;

	public Func<int, int> MapF = (int i) => i;

	public int this[int index]
	{
		get
		{
			return MapF(BaseList[index]);
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int Count => BaseList.Count;

	public bool IsReadOnly => true;

	public MappedList(IList<int> list, int[] map)
	{
		BaseList = list;
		MapF = (int v) => map[v];
	}

	public void Add(int item)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		throw new NotImplementedException();
	}

	public void Insert(int index, int item)
	{
		throw new NotImplementedException();
	}

	public bool Remove(int item)
	{
		throw new NotImplementedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	public bool Contains(int item)
	{
		throw new NotImplementedException();
	}

	public int IndexOf(int item)
	{
		throw new NotImplementedException();
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public IEnumerator<int> GetEnumerator()
	{
		int N = BaseList.Count;
		int i = 0;
		while (i < N)
		{
			yield return MapF(BaseList[i]);
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
