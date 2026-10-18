using UnityEngine;

namespace Oculus.Interaction;

public class DistantPointDetector
{
	private DistantPointDetectorFrustums _frustums;

	public DistantPointDetector(DistantPointDetectorFrustums frustums)
	{
		_frustums = frustums;
	}

	public bool ComputeIsPointing(Collider[] colliders, bool isSelecting, out float bestScore, out Vector3 bestHitPoint)
	{
		ConicalFrustum conicalFrustum = ((isSelecting || _frustums.DeselectionFrustum == null) ? _frustums.SelectionFrustum : _frustums.DeselectionFrustum);
		bestHitPoint = Vector3.zero;
		bestScore = float.NegativeInfinity;
		bool result = false;
		foreach (Collider collider in colliders)
		{
			float score = 0f;
			if (!conicalFrustum.HitsCollider(collider, out score, out var point))
			{
				continue;
			}
			if (_frustums.AidFrustum != null)
			{
				if (!_frustums.AidFrustum.HitsCollider(collider, out var score2, out var _))
				{
					continue;
				}
				score = score * (1f - _frustums.AidBlending) + score2 * _frustums.AidBlending;
			}
			if (score > bestScore)
			{
				bestHitPoint = point;
				bestScore = score;
				result = true;
			}
		}
		return result;
	}

	public bool IsPointingWithoutAid(Collider[] colliders, out Vector3 bestHitPoint)
	{
		if (_frustums.AidFrustum == null)
		{
			bestHitPoint = Vector3.zero;
			return false;
		}
		if (!IsPointingAtColliders(colliders, _frustums.AidFrustum, out bestHitPoint))
		{
			return IsWithinDeselectionRange(colliders);
		}
		return false;
	}

	public bool IsWithinDeselectionRange(Collider[] colliders)
	{
		if (!IsPointingAtColliders(colliders, _frustums.DeselectionFrustum))
		{
			return IsPointingAtColliders(colliders, _frustums.SelectionFrustum);
		}
		return true;
	}

	private bool IsPointingAtColliders(Collider[] colliders, ConicalFrustum frustum)
	{
		if (frustum == null)
		{
			return false;
		}
		foreach (Collider collider in colliders)
		{
			if (frustum.HitsCollider(collider, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPointingAtColliders(Collider[] colliders, ConicalFrustum frustum, out Vector3 bestHitPoint)
	{
		bestHitPoint = Vector3.zero;
		float num = float.NegativeInfinity;
		bool result = false;
		if (frustum == null)
		{
			return false;
		}
		foreach (Collider collider in colliders)
		{
			if (frustum.HitsCollider(collider, out var score, out var point))
			{
				result = true;
				if (score > num)
				{
					num = score;
					bestHitPoint = point;
				}
			}
		}
		return result;
	}
}
