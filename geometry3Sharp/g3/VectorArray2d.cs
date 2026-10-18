using System.Collections.Generic;

namespace g3;

public class VectorArray2d : VectorArray2<double>
{
	public Vector2d this[int i]
	{
		get
		{
			return new Vector2d(array[2 * i], array[2 * i + 1]);
		}
		set
		{
			Set(i, value[0], value[1]);
		}
	}

	public VectorArray2d(int nCount)
		: base(nCount)
	{
	}

	public VectorArray2d(double[] data)
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
