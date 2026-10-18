using System.Collections.Generic;

namespace g3;

public class VectorArray2f : VectorArray2<float>
{
	public Vector2f this[int i]
	{
		get
		{
			return new Vector2f(array[2 * i], array[2 * i + 1]);
		}
		set
		{
			Set(i, value[0], value[1]);
		}
	}

	public VectorArray2f(int nCount)
		: base(nCount)
	{
	}

	public VectorArray2f(float[] data)
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
