using System.Collections.Generic;

namespace UnityEngine.Splines;

internal class DataPointComparer<T> : IComparer<T> where T : IDataPoint
{
	public int Compare(T x, T y)
	{
		return x.Index.CompareTo(y.Index);
	}
}
