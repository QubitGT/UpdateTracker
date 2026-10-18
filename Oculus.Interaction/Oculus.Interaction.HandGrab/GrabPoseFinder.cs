using System;
using System.Collections.Generic;
using Oculus.Interaction.Grab;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public class GrabPoseFinder
{
	[Obsolete]
	public enum FindResult
	{
		NotFound,
		NotCompatible,
		Found
	}

	private class InterpolationCache
	{
		public HandGrabResult underResult = new HandGrabResult();

		public HandGrabResult overResult = new HandGrabResult();
	}

	private List<HandGrabPose> _handGrabPoses;

	private Transform _relativeTo;

	private InterpolationCache _interpolationCache = new InterpolationCache();

	public bool UsesHandPose
	{
		get
		{
			if (_handGrabPoses.Count > 0)
			{
				return _handGrabPoses[0].HandPose != null;
			}
			return false;
		}
	}

	public GrabPoseFinder(List<HandGrabPose> handGrabPoses, Transform relativeTo)
	{
		_handGrabPoses = handGrabPoses;
		_relativeTo = relativeTo;
	}

	public bool SupportsHandedness(Handedness handedness)
	{
		if (!UsesHandPose)
		{
			return true;
		}
		return _handGrabPoses[0].HandPose.Handedness == handedness;
	}

	public bool FindBestPose(in Pose userPose, in Pose offset, float handScale, Handedness handedness, PoseMeasureParameters scoringModifier, ref HandGrabResult result)
	{
		if (_handGrabPoses.Count == 1)
		{
			_handGrabPoses[0].CalculateBestPose(in userPose, in offset, _relativeTo, handedness, scoringModifier, ref result);
			return true;
		}
		if (_handGrabPoses.Count > 1)
		{
			CalculateBestScaleInterpolatedPose(in userPose, in offset, handedness, handScale, scoringModifier, ref result);
			return true;
		}
		return false;
	}

	private void CalculateBestScaleInterpolatedPose(in Pose userPose, in Pose offset, Handedness handedness, float handScale, PoseMeasureParameters scoringModifier, ref HandGrabResult result)
	{
		result.HasHandPose = false;
		FindInterpolationRange(handScale / _relativeTo.lossyScale.x, _handGrabPoses, out var from, out var to, out var t);
		if (t < 0f)
		{
			from.CalculateBestPose(in userPose, in offset, _relativeTo, handedness, scoringModifier, ref _interpolationCache.underResult);
			Pose userPose2 = _relativeTo.GlobalPose(in _interpolationCache.underResult.RelativePose);
			to.CalculateBestPose(in userPose2, in offset, _relativeTo, handedness, PoseMeasureParameters.DEFAULT, ref _interpolationCache.overResult);
		}
		else if (t > 1f)
		{
			to.CalculateBestPose(in userPose, in offset, _relativeTo, handedness, scoringModifier, ref _interpolationCache.overResult);
			Pose userPose3 = _relativeTo.GlobalPose(in _interpolationCache.overResult.RelativePose);
			from.CalculateBestPose(in userPose3, in offset, _relativeTo, handedness, PoseMeasureParameters.DEFAULT, ref _interpolationCache.underResult);
		}
		else
		{
			from.CalculateBestPose(in userPose, in offset, _relativeTo, handedness, scoringModifier, ref _interpolationCache.underResult);
			to.CalculateBestPose(in userPose, in offset, _relativeTo, handedness, scoringModifier, ref _interpolationCache.overResult);
		}
		if (_interpolationCache.underResult.HasHandPose && _interpolationCache.overResult.HasHandPose)
		{
			result.HasHandPose = true;
			result.HandPose.CopyFrom(_interpolationCache.underResult.HandPose);
			HandPose.Lerp(in _interpolationCache.underResult.HandPose, in _interpolationCache.overResult.HandPose, t, ref result.HandPose);
			PoseUtils.Lerp(in _interpolationCache.underResult.RelativePose, in _interpolationCache.overResult.RelativePose, t, ref result.RelativePose);
		}
		else if (_interpolationCache.underResult.HasHandPose)
		{
			result.HasHandPose = true;
			result.HandPose.CopyFrom(_interpolationCache.underResult.HandPose);
			result.RelativePose.CopyFrom(in _interpolationCache.underResult.RelativePose);
		}
		else if (_interpolationCache.overResult.HasHandPose)
		{
			result.HasHandPose = true;
			result.HandPose.CopyFrom(_interpolationCache.overResult.HandPose);
			result.RelativePose.CopyFrom(in _interpolationCache.overResult.RelativePose);
		}
		result.Score = GrabPoseScore.Lerp(in _interpolationCache.underResult.Score, in _interpolationCache.overResult.Score, t);
	}

	public static bool FindInterpolationRange(float relativeHandScale, List<HandGrabPose> grabPoses, out HandGrabPose from, out HandGrabPose to, out float t)
	{
		if (grabPoses.Count == 0)
		{
			from = (to = null);
			t = 0f;
			return false;
		}
		if (grabPoses.Count == 1)
		{
			t = 0f;
			from = (to = grabPoses[0]);
			return true;
		}
		from = FindPreviousScaledGrabPose(grabPoses, relativeHandScale);
		to = FindNextScaledGrabPose(grabPoses, relativeHandScale);
		if (from == null && to == null)
		{
			t = 0f;
			return false;
		}
		if (to == null)
		{
			to = from;
			from = FindPreviousScaledGrabPose(grabPoses, from.RelativeScale, notEqual: true);
		}
		if (from == null)
		{
			from = to;
			to = FindNextScaledGrabPose(grabPoses, to.RelativeScale, notEqual: true);
		}
		float num = to.RelativeScale - from.RelativeScale;
		if (num == 0f)
		{
			t = 0f;
		}
		else
		{
			t = (relativeHandScale - from.RelativeScale) / num;
		}
		return true;
	}

	private static HandGrabPose FindPreviousScaledGrabPose(List<HandGrabPose> grabPoses, float upLimit, bool notEqual = false)
	{
		float num = float.NegativeInfinity;
		HandGrabPose result = null;
		foreach (HandGrabPose grabPose in grabPoses)
		{
			float relativeScale = grabPose.RelativeScale;
			if (((!notEqual && relativeScale <= upLimit) || (notEqual && relativeScale < upLimit)) && relativeScale > num)
			{
				num = relativeScale;
				result = grabPose;
			}
		}
		return result;
	}

	private static HandGrabPose FindNextScaledGrabPose(List<HandGrabPose> grabPoses, float lowLimit, bool notEqual = false)
	{
		float num = float.PositiveInfinity;
		HandGrabPose result = null;
		foreach (HandGrabPose grabPose in grabPoses)
		{
			float relativeScale = grabPose.RelativeScale;
			if (((!notEqual && relativeScale >= lowLimit) || (notEqual && relativeScale > lowLimit)) && relativeScale < num)
			{
				num = relativeScale;
				result = grabPose;
			}
		}
		return result;
	}
}
