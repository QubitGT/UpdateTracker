using System.Collections.Generic;

namespace g3;

public class VectorArray3f : VectorArray3<float>
{
	public Vector3f this[int i]
	{
		get
		{
			return new Vector3f(array[3 * i], array[3 * i + 1], array[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public VectorArray3f(int nCount)
		: base(nCount)
	{
	}

	public VectorArray3f(float[] data)
		: base(data)
	{
	}

	public IEnumerable<Vector3f> AsVector3f()
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
