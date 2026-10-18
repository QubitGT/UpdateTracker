using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TeleportCandidateComputer : MonoBehaviour
{
	[SerializeField]
	[Tooltip("(Meters, World) The threshold below which distances to a interactable are treated as equal for the purposes of ranking.")]
	private float _equalDistanceThreshold = 0.1f;

	[SerializeField]
	[Tooltip("When provided, the Interactor will perform an extra check to ensurenothing is blocking the line between this point and the teleport origin")]
	private Transform _blockCheckOrigin;

	[SerializeField]
	[Optional]
	[Tooltip("When assigned in Editor, this component will inject itself into the Interactor during Awake.")]
	private TeleportInteractor _teleportInteractor;

	public float EqualDistanceThreshold
	{
		get
		{
			return _equalDistanceThreshold;
		}
		set
		{
			_equalDistanceThreshold = value;
		}
	}

	public Transform BlockCheckOrigin
	{
		get
		{
			return _blockCheckOrigin;
		}
		set
		{
			_blockCheckOrigin = value;
		}
	}

	protected virtual void Awake()
	{
		if (_teleportInteractor != null)
		{
			_teleportInteractor.InjectOptionalCandidateComputer(ComputeCandidate);
		}
	}

	public virtual TeleportInteractable ComputeCandidate(IPolyline TeleportArc, in InteractableRegistry<TeleportInteractor, TeleportInteractable>.InteractableSet interactables, TeleportInteractor.ComputeCandidateTiebreakerDelegate tiebreaker, out TeleportHit hitPose)
	{
		TeleportInteractable bestCandidate = null;
		float bestScore = float.PositiveInfinity;
		Vector3 arcOrigin = TeleportArc.PointAtIndex(0);
		Vector3 position = TeleportArc.PointAtIndex(TeleportArc.PointsCount - 1);
		TeleportHit bestHit = new TeleportHit(null, position, Vector3.up);
		if (_blockCheckOrigin != null)
		{
			bool flag = false;
			foreach (TeleportInteractable interactable in interactables)
			{
				if (!interactable.AllowTeleport)
				{
					flag |= CheckOriginBlockers(_blockCheckOrigin.position, arcOrigin, interactable);
				}
			}
			if (flag)
			{
				hitPose = bestHit;
				return bestCandidate;
			}
		}
		foreach (TeleportInteractable interactable2 in interactables)
		{
			CheckCandidate(interactable2);
		}
		hitPose = bestHit;
		return bestCandidate;
		void CheckCandidate(TeleportInteractable candidate)
		{
			Vector3 vector = arcOrigin;
			float num = 0f;
			for (int i = 1; i < TeleportArc.PointsCount; i++)
			{
				if (num > bestScore)
				{
					break;
				}
				Vector3 vector2 = TeleportArc.PointAtIndex(i);
				if (candidate.DetectHit(vector, vector2, out var hit2))
				{
					float score2 = num + Vector3.Distance(vector, hit2.Point);
					if (TrySetScore(candidate, hit2, score2))
					{
						break;
					}
				}
				num += Vector3.Distance(vector, vector2);
				vector = vector2;
			}
		}
		bool CheckOriginBlockers(Vector3 from, Vector3 to, TeleportInteractable candidate)
		{
			if (candidate.DetectHit(from, to, out var hit3))
			{
				float score3 = 0f - Vector3.Distance(to, hit3.Point);
				return TrySetScore(candidate, hit3, score3);
			}
			return false;
		}
		int Tiebreak(TeleportInteractable a, TeleportInteractable b)
		{
			if (tiebreaker != null)
			{
				return tiebreaker(a, b);
			}
			return a.TieBreakerScore.CompareTo(b.TieBreakerScore);
		}
		bool TrySetScore(TeleportInteractable candidate, TeleportHit hit, float score)
		{
			bool flag2 = Mathf.Abs(bestScore - score) <= _equalDistanceThreshold;
			if (bestCandidate == null || (!flag2 && score < bestScore) || (flag2 && Tiebreak(candidate, bestCandidate) > 0))
			{
				bestScore = score;
				bestHit = hit;
				bestCandidate = candidate;
				return true;
			}
			return false;
		}
	}
}
