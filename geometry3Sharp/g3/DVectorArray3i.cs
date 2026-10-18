using System.Collections.Generic;

namespace g3;

public class DVectorArray3i : DVectorArray3<int>
{
	public Vector3i this[int i]
	{
		get
		{
			return new Vector3i(vector[3 * i], vector[3 * i + 1], vector[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public DVectorArray3i(int nCount = 0)
		: base(nCount)
	{
	}

	public DVectorArray3i(int[] data)
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

	public IEnumerable<Vector3i> AsVector3i()
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
