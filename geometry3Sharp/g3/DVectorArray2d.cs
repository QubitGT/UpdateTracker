using System.Collections.Generic;

namespace g3;

public class DVectorArray2d : DVectorArray2<double>
{
	public Vector2d this[int i]
	{
		get
		{
			return new Vector2d(vector[2 * i], vector[2 * i + 1]);
		}
		set
		{
			Set(i, value[0], value[1]);
		}
	}

	public DVectorArray2d(int nCount = 0)
		: base(nCount)
	{
	}

	public DVectorArray2d(double[] data)
		: base(data)
	{
	}

	public IEnumerable<Vector2d> AsVector2d()
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
