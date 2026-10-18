using System.Collections.Generic;

namespace g3;

public class DVectorArray3f : DVectorArray3<float>
{
	public Vector3f this[int i]
	{
		get
		{
			return new Vector3f(vector[3 * i], vector[3 * i + 1], vector[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public DVectorArray3f(int nCount = 0)
		: base(nCount)
	{
	}

	public DVectorArray3f(float[] data)
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
