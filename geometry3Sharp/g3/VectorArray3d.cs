using System.Collections.Generic;

namespace g3;

public class VectorArray3d : VectorArray3<double>
{
	private const double invalid_value = -99999999.0;

	public Vector3d this[int i]
	{
		get
		{
			return new Vector3d(array[3 * i], array[3 * i + 1], array[3 * i + 2]);
		}
		set
		{
			Set(i, value[0], value[1], value[2]);
		}
	}

	public VectorArray3d(int nCount, bool debug = false)
		: base(nCount)
	{
	}

	public VectorArray3d(double[] data)
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
