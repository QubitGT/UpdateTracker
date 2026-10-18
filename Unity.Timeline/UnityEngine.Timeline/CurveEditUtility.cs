namespace UnityEngine.Timeline;

internal static class CurveEditUtility
{
	public static AnimationCurve CreateMatchingCurve(AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		for (int i = 0; i != keys.Length; i++)
		{
			if (!float.IsPositiveInfinity(keys[i].inTangent))
			{
				keys[i].inTangent = 0f - keys[i].inTangent;
			}
			if (!float.IsPositiveInfinity(keys[i].outTangent))
			{
				keys[i].outTangent = 0f - keys[i].outTangent;
			}
			keys[i].value = 1f - keys[i].value;
		}
		return new AnimationCurve(keys);
	}
}
