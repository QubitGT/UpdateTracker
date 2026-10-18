using System;
using System.Collections.Generic;

namespace UnityEngine.Splines;

[Obsolete("Use ISplineContainer instead.")]
public interface ISplineProvider
{
	IEnumerable<Spline> Splines { get; }
}
