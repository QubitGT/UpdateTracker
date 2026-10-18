using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Splines;

[Serializable]
internal class SplinePathRef
{
	[Serializable]
	public class SliceRef
	{
		[SerializeField]
		public int Index;

		[SerializeField]
		public SplineRange Range;

		public SliceRef(int splineIndex, SplineRange range)
		{
			Index = splineIndex;
			Range = range;
		}
	}

	[SerializeField]
	public SliceRef[] Splines;

	public SplinePathRef()
	{
	}

	public SplinePathRef(IEnumerable<SliceRef> slices)
	{
		Splines = slices.ToArray();
	}
}
