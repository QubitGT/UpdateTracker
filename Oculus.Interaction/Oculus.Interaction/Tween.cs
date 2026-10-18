using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class Tween : IMovement
{
	private class TweenCurve
	{
		public ProgressCurve Curve;

		public float PrevProgress;

		public Pose Current;

		public Pose Target;
	}

	private List<TweenCurve> _tweenCurves;

	private Pose _pose;

	private Pose _startPose;

	private float _maxOverlapTime;

	private float _tweenTime;

	private AnimationCurve _animationCurve;

	public Pose Pose => _pose;

	public Pose StartPose => _startPose;

	public bool Stopped => _tweenCurves.TrueForAll((TweenCurve t) => t.PrevProgress >= 1f);

	public Tween(Pose start, float tweenTime = 0.5f, float maxOverlapTime = 0.25f, AnimationCurve curve = null)
	{
		_pose = (_startPose = start);
		_tweenTime = tweenTime;
		_maxOverlapTime = maxOverlapTime;
		_tweenCurves = new List<TweenCurve>();
		_animationCurve = curve ?? AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		TweenToInTime(_pose, 0f);
	}

	private void TweenToInTime(Pose target, float time)
	{
		Pose from = _pose;
		if (_tweenCurves.Count > 0)
		{
			TweenCurve tweenCurve = _tweenCurves[_tweenCurves.Count - 1];
			float num = tweenCurve.Curve.ProgressIn(Mathf.Min(_maxOverlapTime, time));
			if (num != 1f)
			{
				float num2 = num - tweenCurve.PrevProgress;
				float num3 = 1f - tweenCurve.PrevProgress;
				float t = num2 / num3;
				from = tweenCurve.Current;
				from.Lerp(in tweenCurve.Target, t);
			}
		}
		TweenCurve tweenCurve2 = new TweenCurve
		{
			Curve = new ProgressCurve(_animationCurve, time),
			PrevProgress = 0f,
			Current = from,
			Target = target
		};
		_tweenCurves.Add(tweenCurve2);
		tweenCurve2.Curve.Start();
	}

	public void MoveTo(Pose target)
	{
		if (_pose.Equals(target))
		{
			StopAndSetPose(target);
		}
		else
		{
			TweenToInTime(target, _tweenTime);
		}
	}

	public void UpdateTarget(Pose target)
	{
		_tweenCurves[_tweenCurves.Count - 1].Target = target;
	}

	public void StopAndSetPose(Pose source)
	{
		_tweenCurves.Clear();
		_pose = source;
		TweenToInTime(source, 0f);
	}

	public void Tick()
	{
		for (int num = _tweenCurves.Count - 1; num >= 0; num--)
		{
			TweenCurve tweenCurve = _tweenCurves[num];
			float num2 = tweenCurve.Curve.Progress();
			if (num2 == 1f)
			{
				tweenCurve.Current = tweenCurve.Target;
				tweenCurve.PrevProgress = 1f;
			}
			else
			{
				float num3 = num2 - tweenCurve.PrevProgress;
				float num4 = 1f - tweenCurve.PrevProgress;
				float t = num3 / num4;
				tweenCurve.Current.Lerp(in tweenCurve.Target, t);
				tweenCurve.PrevProgress = num2;
			}
		}
		float num5 = 1f;
		float num6 = 0f;
		Pose from = _tweenCurves[_tweenCurves.Count - 1].Current;
		for (int num7 = _tweenCurves.Count - 2; num7 >= 0; num7--)
		{
			TweenCurve tweenCurve2 = _tweenCurves[num7 + 1];
			float b = tweenCurve2.Curve.ProgressTime();
			num6 = ((tweenCurve2.Curve.AnimationLength != 0f) ? (Mathf.Min(_maxOverlapTime, b) / Mathf.Min(_maxOverlapTime, tweenCurve2.Curve.AnimationLength)) : 1f);
			if (num6 == 1f)
			{
				_tweenCurves.RemoveRange(0, num7);
				break;
			}
			num5 = (1f - num6) * num5;
			Pose to = _tweenCurves[num7].Current;
			from.Lerp(in to, num5);
		}
		_pose = from;
	}
}
