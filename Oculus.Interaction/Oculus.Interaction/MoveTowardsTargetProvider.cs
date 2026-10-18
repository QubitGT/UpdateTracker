using UnityEngine;

namespace Oculus.Interaction;

public class MoveTowardsTargetProvider : MonoBehaviour, IMovementProvider
{
	[SerializeField]
	private PoseTravelData _travellingData = PoseTravelData.FAST;

	public IMovement CreateMovement()
	{
		return new MoveTowardsTarget(_travellingData);
	}

	public void InjectAllMoveTowardsTargetProvider(PoseTravelData travellingData)
	{
		InjectTravellingData(travellingData);
	}

	public void InjectTravellingData(PoseTravelData travellingData)
	{
		_travellingData = travellingData;
	}
}
