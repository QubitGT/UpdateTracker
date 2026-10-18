using System.Collections.Generic;

namespace g3;

public class DVectorArray3d : DVectorArray3<double>
{
	private const double invalid_value = -99999999.0;

	public Vector3d this[int i]
	{
		get
		{
			return new Vector3d(vector[3 * i], vector[3 * i + 1], vector[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public DVectorArray3d(int nCount = 0)
		: base(nCount)
	{
	}

	public DVectorArray3d(double[] data)
		: base(data)
	{
	}

	public IEnumerable<Vector3d> AsVector3d()
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
