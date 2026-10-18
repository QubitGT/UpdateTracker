using System.Collections.Generic;

namespace g3;

public class DVectorArray2f : DVectorArray2<float>
{
	public Vector2f this[int i]
	{
		get
		{
			return new Vector2f(vector[2 * i], vector[2 * i + 1]);
		}
		set
		{
			Set(i, value[0], value[1]);
		}
	}

	public DVectorArray2f(int nCount = 0)
		: base(nCount)
	{
	}

	public DVectorArray2f(float[] data)
		: base(data)
	{
	}

	public IEnumerable<Vector2d> AsVector2f()
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
