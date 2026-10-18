using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[Serializable]
public struct SplineAutoDolly
{
	public interface ISplineAutoDolly
	{
		bool RequiresTrackingTarget { get; }

		void Validate();

		void Reset();

		float GetSplinePosition(MonoBehaviour sender, Transform target, SplineContainer spline, float currentPosition, PathIndexUnit positionUnits, float deltaTime);
	}

	[Serializable]
	public class FixedSpeed : ISplineAutoDolly
	{
		[Tooltip("Speed of travel, in current position units per second.")]
		public float Speed;

		bool ISplineAutoDolly.RequiresTrackingTarget => false;

		void ISplineAutoDolly.Validate()
		{
		}

		void ISplineAutoDolly.Reset()
		{
		}

		float ISplineAutoDolly.GetSplinePosition(MonoBehaviour sender, Transform target, SplineContainer spline, float currentPosition, PathIndexUnit positionUnits, float deltaTime)
		{
			if (Application.isPlaying && spline.IsValid() && deltaTime > 0f)
			{
				return currentPosition + Speed * deltaTime;
			}
			return currentPosition;
		}
	}

	[Serializable]
	public class NearestPointToTarget : ISplineAutoDolly
	{
		[Tooltip("Offset, in current position units, from the closest point on the spline to the follow target")]
		public float PositionOffset;

		[Tooltip("Affects how many segments to split a spline into when calculating the nearest point.  Higher values mean smaller and more segments, which increases accuracy at the cost of processing time.  In most cases, the default value (4) is appropriate. Use with SearchIteration to fine-tune point accuracy.")]
		public int SearchResolution = 4;

		[Tooltip("The nearest point is calculated by finding the nearest point on the entire length of the spline using SearchResolution to divide into equally spaced line segments. Successive iterations will then subdivide further the nearest segment, producing more accurate results. In most cases, the default value (2) is sufficient.")]
		public int SearchIteration = 2;

		bool ISplineAutoDolly.RequiresTrackingTarget => true;

		void ISplineAutoDolly.Validate()
		{
			SearchResolution = Mathf.Max(SearchResolution, 1);
			SearchIteration = Mathf.Max(SearchIteration, 1);
		}

		void ISplineAutoDolly.Reset()
		{
		}

		float ISplineAutoDolly.GetSplinePosition(MonoBehaviour sender, Transform target, SplineContainer spline, float currentPosition, PathIndexUnit positionUnits, float deltaTime)
		{
			if (target == null || !spline.IsValid())
			{
				return currentPosition;
			}
			SplineUtility.GetNearestPoint(spline.Spline, spline.transform.InverseTransformPoint(target.position), out var _, out var t, SearchResolution, SearchIteration);
			t = Mathf.Clamp01(t);
			return spline.Spline.ConvertIndexUnit(t, PathIndexUnit.Normalized, positionUnits) + PositionOffset;
		}
	}

	[Tooltip("If set, will enable the selected automatic dolly along the spline")]
	public bool Enabled;

	[SerializeReference]
	public ISplineAutoDolly Method;
}
