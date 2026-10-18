using System.Collections.Generic;

namespace g3;

public class VectorArray3i : VectorArray3<int>
{
	public Vector3i this[int i]
	{
		get
		{
			return new Vector3i(array[3 * i], array[3 * i + 1], array[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public VectorArray3i(int nCount)
		: base(nCount)
	{
	}

	public VectorArray3i(int[] data)
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
