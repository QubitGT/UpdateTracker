using System.Collections.Generic;

namespace UnityEngine.Splines;

public interface IHasEmptyCurves
{
	IReadOnlyList<int> EmptyCurves { get; }
}
