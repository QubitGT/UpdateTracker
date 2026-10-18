using UnityEngine;

namespace Oculus.Interaction;

public class CandidatePositionComparer : CandidateComparer<ICandidatePosition>
{
	[SerializeField]
	private Transform _compareOrigin;

	public override int Compare(ICandidatePosition a, ICandidatePosition b)
	{
		float sqrMagnitude = (a.CandidatePosition - _compareOrigin.position).sqrMagnitude;
		float sqrMagnitude2 = (b.CandidatePosition - _compareOrigin.position).sqrMagnitude;
		if (sqrMagnitude == sqrMagnitude2)
		{
			return 0;
		}
		if (!(sqrMagnitude < sqrMagnitude2))
		{
			return 1;
		}
		return -1;
	}
}
