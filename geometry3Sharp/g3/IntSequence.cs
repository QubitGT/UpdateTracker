using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public struct IntSequence : IList<int>, ICollection<int>, IEnumerable<int>, IEnumerable
{
	private Interval1i range;

	public int this[int index]
	{
		get
		{
			return range.a + index;
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public int Count => range.Length + 1;

	public bool IsReadOnly => true;

	public IntSequence(Interval1i ival)
	{
		range = ival;
	}

	public IntSequence(int iStart, int iEnd)
	{
		range = new Interval1i(iStart, iEnd);
	}

	public static IntSequence Range(int N)
	{
		return new IntSequence(0, N - 1);
	}

	public static IntSequence RangeInclusive(int N)
	{
		return new IntSequence(0, N);
	}

	public static IntSequence Range(int start, int N)
	{
		return new IntSequence(start, start + N - 1);
	}

	public static IntSequence FromToInclusive(int a, int b)
	{
		return new IntSequence(a, b);
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
		return range.Contains(item);
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
		return range.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
