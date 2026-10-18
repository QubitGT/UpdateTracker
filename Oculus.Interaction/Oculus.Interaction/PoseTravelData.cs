using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public struct PoseTravelData
{
	[Tooltip("When attracting the object, indicates the rate (in m/s, or seconds if UseFixedTravelTime is enabled) for the object to realign with the hand after a grab.")]
	[SerializeField]
	private float _travelSpeed;

	[Tooltip("Changes the units of the TravelSpeed, disabled means m/s while enabled is fixed seconds")]
	[SerializeField]
	private bool _useFixedTravelTime;

	[Tooltip("Animation to use in conjunction with TravelSpeed to define the traveling motion.")]
	[SerializeField]
	private AnimationCurve _travelCurve;

	private const float DEGREES_TO_PERCEIVED_METERS = 0.0013888889f;

	public static PoseTravelData DEFAULT
	{
		get
		{
			PoseTravelData result = default(PoseTravelData);
			result._travelSpeed = 1f;
			result._useFixedTravelTime = false;
			result._travelCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			return result;
		}
	}

	public static PoseTravelData FAST
	{
		get
		{
			PoseTravelData result = default(PoseTravelData);
			result._travelSpeed = 0.1f;
			result._useFixedTravelTime = true;
			result._travelCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			return result;
		}
	}

	public Tween CreateTween(in Pose from, in Pose to)
	{
		float num = _travelSpeed;
		if (!_useFixedTravelTime && _travelSpeed != 0f)
		{
			num = PerceivedDistance(in from, in to) / _travelSpeed;
		}
		Tween tween = new Tween(from, num, num * 0.5f, _travelCurve);
		tween.MoveTo(to);
		return tween;
	}

	private static float PerceivedDistance(in Pose from, in Pose to)
	{
		float magnitude = PoseUtils.Delta(in from, in to).position.magnitude;
		float b = 0.0013888889f * Mathf.Max(Mathf.Max(Vector3.Angle(from.forward, to.forward), Vector3.Angle(from.up, to.up), Vector3.Angle(from.right, to.right)));
		return Mathf.Max(magnitude, b);
	}
}
