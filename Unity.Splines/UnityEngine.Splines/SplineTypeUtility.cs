namespace UnityEngine.Splines;

internal static class SplineTypeUtility
{
	internal static TangentMode GetTangentMode(this SplineType splineType)
	{
		return splineType switch
		{
			SplineType.Bezier => TangentMode.Mirrored, 
			SplineType.Linear => TangentMode.Linear, 
			_ => TangentMode.AutoSmooth, 
		};
	}
}
