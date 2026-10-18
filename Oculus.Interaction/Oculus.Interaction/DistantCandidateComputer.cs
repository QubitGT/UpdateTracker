using System;
using UnityEngine;

namespace Oculus.Interaction;

[Serializable]
public class DistantCandidateComputer<TInteractor, TInteractable> where TInteractor : Interactor<TInteractor, TInteractable> where TInteractable : Interactable<TInteractor, TInteractable>, ICollidersRef
{
	[Tooltip("Frustum used to detect and select objects.")]
	[SerializeField]
	private DistantPointDetectorFrustums _detectionFrustums;

	[Tooltip("How long you must hover over an object before it's considered a candidate for interaction.")]
	[SerializeField]
	private float _detectionDelay;

	private float _hoverStartTime;

	private DistantPointDetector _detector;

	private TInteractable _stableCandidate;

	private TInteractable _pointedCandidate;

	public DistantPointDetectorFrustums DetectionFrustums
	{
		get
		{
			return _detectionFrustums;
		}
		set
		{
			_detectionFrustums = value;
			_detector = new DistantPointDetector(value);
		}
	}

	public float DetectionDelay
	{
		get
		{
			return _detectionDelay;
		}
		set
		{
			_detectionDelay = value;
		}
	}

	public virtual Pose Origin => new Pose(_detectionFrustums.SelectionFrustum.StartPoint, Quaternion.LookRotation(_detectionFrustums.SelectionFrustum.Direction));

	public virtual TInteractable ComputeCandidate(InteractableRegistry<TInteractor, TInteractable> registry, TInteractor interactor, out Vector3 bestHitPoint)
	{
		if (_detector == null)
		{
			_detector = new DistantPointDetector(DetectionFrustums);
		}
		if (_stableCandidate != null && _detector.IsPointingWithoutAid(_stableCandidate.Colliders, out bestHitPoint))
		{
			return _stableCandidate;
		}
		if (_stableCandidate != null && !_detector.ComputeIsPointing(_stableCandidate.Colliders, isSelecting: false, out var _, out var _))
		{
			_stableCandidate = null;
		}
		InteractableRegistry<TInteractor, TInteractable>.InteractableSet candidates = registry.List(interactor);
		TInteractable val = ComputeBestInteractable(in candidates, _stableCandidate == null, out bestHitPoint);
		if (val != _pointedCandidate)
		{
			_pointedCandidate = val;
			if (val != null)
			{
				_hoverStartTime = Time.time;
			}
		}
		if ((_stableCandidate == null && val != null) || (_stableCandidate != null && val != null && _stableCandidate != val && Time.time - _hoverStartTime >= _detectionDelay))
		{
			_pointedCandidate = null;
			_stableCandidate = val;
		}
		return _stableCandidate;
	}

	private TInteractable ComputeBestInteractable(in InteractableRegistry<TInteractor, TInteractable>.InteractableSet candidates, bool narrowSearch, out Vector3 bestHitPoint)
	{
		TInteractable result = null;
		float num = float.NegativeInfinity;
		bestHitPoint = Vector3.zero;
		foreach (TInteractable candidate in candidates)
		{
			if (_detector.ComputeIsPointing(candidate.Colliders, narrowSearch, out var bestScore, out var bestHitPoint2) && bestScore > num)
			{
				num = bestScore;
				result = candidate;
				bestHitPoint = bestHitPoint2;
			}
		}
		return result;
	}
}
