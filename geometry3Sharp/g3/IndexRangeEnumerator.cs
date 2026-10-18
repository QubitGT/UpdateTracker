using System.Collections;
using System.Collections.Generic;

namespace g3;

public class IndexRangeEnumerator : IEnumerable<int>, IEnumerable
{
	private int Start;

	private int Count;

	public IndexRangeEnumerator(int count)
	{
		Count = count;
	}

	public IndexRangeEnumerator(int start, int count)
	{
		Start = start;
		Count = count;
	}

	public IEnumerator<int> GetEnumerator()
	{
		int i = 0;
		while (i < Count)
		{
			yield return Start + i;
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
