using System.Collections.Generic;

namespace g3;

public class IndexArray3i : VectorArray3<int>
{
	public Index3i this[int i]
	{
		get
		{
			return new Index3i(array[3 * i], array[3 * i + 1], array[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public IndexArray3i(int nCount)
		: base(nCount)
	{
	}

	public IndexArray3i(int[] data)
		: base(data)
	{
	}

	public void Set(int i, int a, int b, int c, bool bCycle = false)
	{
		array[3 * i] = a;
		if (bCycle)
		{
			array[3 * i + 1] = c;
			array[3 * i + 2] = b;
		}
		else
		{
			array[3 * i + 1] = b;
			array[3 * i + 2] = c;
		}
	}

	public IEnumerable<Index3i> AsIndex3i()
	{
		int i = 0;
		while (i < base.Count)
		{
			yield return new Index3i(array[3 * i], array[3 * i + 1], array[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}
}
