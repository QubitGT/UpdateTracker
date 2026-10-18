namespace UnityEngine.Splines;

internal struct SplineModificationData
{
	public readonly Spline Spline;

	public readonly SplineModification Modification;

	public readonly int KnotIndex;

	public readonly float PrevCurveLength;

	public readonly float NextCurveLength;

	public SplineModificationData(Spline spline, SplineModification modification, int knotIndex, float prevCurveLength, float nextCurveLength)
	{
		Spline = spline;
		Modification = modification;
		KnotIndex = knotIndex;
		PrevCurveLength = prevCurveLength;
		NextCurveLength = nextCurveLength;
	}
}
