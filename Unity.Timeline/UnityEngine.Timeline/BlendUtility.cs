using System;

namespace UnityEngine.Timeline;

internal static class BlendUtility
{
	private static readonly double kMinOverlapTime = TimeUtility.kTimeEpsilon * 1000.0;

	private static bool Overlaps(TimelineClip blendOut, TimelineClip blendIn)
	{
		if (blendIn == blendOut)
		{
			return false;
		}
		if (Math.Abs(blendIn.start - blendOut.start) < TimeUtility.kTimeEpsilon)
		{
			return blendIn.duration >= blendOut.duration;
		}
		if (blendIn.start >= blendOut.start)
		{
			return blendIn.start < blendOut.end;
		}
		return false;
	}

	public static void ComputeBlendsFromOverlaps(TimelineClip[] clips)
	{
		foreach (TimelineClip obj in clips)
		{
			obj.blendInDuration = -1.0;
			obj.blendOutDuration = -1.0;
		}
		Array.Sort(clips, (TimelineClip c1, TimelineClip c2) => (!(Math.Abs(c1.start - c2.start) < TimeUtility.kTimeEpsilon)) ? c1.start.CompareTo(c2.start) : c1.duration.CompareTo(c2.duration));
		for (int j = 0; j < clips.Length; j++)
		{
			TimelineClip timelineClip = clips[j];
			if (timelineClip.SupportsBlending())
			{
				TimelineClip timelineClip2 = timelineClip;
				TimelineClip timelineClip3 = null;
				TimelineClip timelineClip4 = clips[Math.Max(j - 1, 0)];
				if (Overlaps(timelineClip4, timelineClip2))
				{
					timelineClip3 = timelineClip4;
				}
				if (timelineClip3 != null)
				{
					UpdateClipIntersection(timelineClip3, timelineClip2);
				}
			}
		}
	}

	private static void UpdateClipIntersection(TimelineClip blendOutClip, TimelineClip blendInClip)
	{
		if (blendOutClip.SupportsBlending() && blendInClip.SupportsBlending() && !(blendInClip.start - blendOutClip.start < blendOutClip.duration - blendInClip.duration))
		{
			double num = Math.Max(0.0, blendOutClip.start + blendOutClip.duration - blendInClip.start);
			num = (blendOutClip.blendOutDuration = ((num <= kMinOverlapTime) ? 0.0 : num));
			blendInClip.blendInDuration = num;
			TimelineClip.BlendCurveMode blendInCurveMode = blendInClip.blendInCurveMode;
			TimelineClip.BlendCurveMode blendOutCurveMode = blendOutClip.blendOutCurveMode;
			if (blendInCurveMode == TimelineClip.BlendCurveMode.Manual && blendOutCurveMode == TimelineClip.BlendCurveMode.Auto)
			{
				blendOutClip.mixOutCurve = CurveEditUtility.CreateMatchingCurve(blendInClip.mixInCurve);
			}
			else if (blendInCurveMode == TimelineClip.BlendCurveMode.Auto && blendOutCurveMode == TimelineClip.BlendCurveMode.Manual)
			{
				blendInClip.mixInCurve = CurveEditUtility.CreateMatchingCurve(blendOutClip.mixOutCurve);
			}
			else if (blendInCurveMode == TimelineClip.BlendCurveMode.Auto && blendOutCurveMode == TimelineClip.BlendCurveMode.Auto)
			{
				blendInClip.mixInCurve = null;
				blendOutClip.mixOutCurve = null;
			}
		}
	}
}
