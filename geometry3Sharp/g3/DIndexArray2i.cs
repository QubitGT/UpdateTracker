using System.Collections.Generic;

namespace g3;

public class DIndexArray2i : DVectorArray2<int>
{
	public Index2i this[int i]
	{
		get
		{
			return new Index2i(vector[2 * i], vector[2 * i + 1]);
		}
		set
		{
			Set(i, value[0], value[1]);
		}
	}

	public DIndexArray2i(int nCount = 0)
		: base(nCount)
	{
	}

	public DIndexArray2i(int[] data)
		: base(data)
	{
	}

	public IEnumerable<Index2i> AsIndex2i()
	{
		int i = 0;
		while (i < base.Count)
		{
			yield return new Index2i(vector[2 * i], vector[2 * i + 1]);
			int num = i + 1;
			i = num;
		}
	}
}
