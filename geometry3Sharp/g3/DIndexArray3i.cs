using System.Collections.Generic;

namespace g3;

public class DIndexArray3i : DVectorArray3<int>
{
	public Index3i this[int i]
	{
		get
		{
			return new Index3i(vector[3 * i], vector[3 * i + 1], vector[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public DIndexArray3i(int nCount = 0)
		: base(nCount)
	{
	}

	public DIndexArray3i(int[] data)
		: base(data)
	{
	}

	public void Set(int i, int a, int b, int c, bool bCycle = false)
	{
		vector[3 * i] = a;
		if (bCycle)
		{
			vector[3 * i + 1] = c;
			vector[3 * i + 2] = b;
		}
		else
		{
			vector[3 * i + 1] = b;
			vector[3 * i + 2] = c;
		}
	}

	public IEnumerable<Index3i> AsIndex3i()
	{
		int i = 0;
		while (i < base.Count)
		{
			yield return new Index3i(vector[3 * i], vector[3 * i + 1], vector[3 * i + 2]);
			int num = i + 1;
			i = num;
		}
	}
}
