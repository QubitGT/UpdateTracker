using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Splines;

public interface ISpline : IReadOnlyList<BezierKnot>, IEnumerable<BezierKnot>, IEnumerable, IReadOnlyCollection<BezierKnot>
{
	bool Closed { get; }

	float GetLength();

	BezierCurve GetCurve(int index);

	float GetCurveLength(int index);

	float3 GetCurveUpVector(int index, float t);

	float GetCurveInterpolation(int curveIndex, float curveDistance);
}
