using Oculus.Interaction;
using UnityEngine;

public class MoveRelativeToTargetProvider : MonoBehaviour, IMovementProvider
{
	public IMovement CreateMovement()
	{
		return new MoveRelativeToTarget();
	}
}
