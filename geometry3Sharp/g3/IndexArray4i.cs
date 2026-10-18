using System.Collections.Generic;

namespace g3;

public class IndexArray4i : VectorArray4<int>
{
	public Index4i this[int i]
	{
		get
		{
			int num = 4 * i;
			return new Index4i(array[num], array[num + 1], array[num + 2], array[num + 3]);
		}
		set
		{
			Set(i, value[0], value[1], value[2], value[4]);
		}
	}

	public IndexArray4i(int nCount)
		: base(nCount)
	{
	}

	public IndexArray4i(int[] data)
		: base(data)
	{
	}

	public IEnumerable<Index4i> AsIndex4i()
	{
		int i = 0;
		while (i < base.Count)
		{
			yield return this[i];
			int num = i + 1;
			i = num;
		}
	}
}
