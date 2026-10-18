using UnityEngine;

namespace Oculus.Interaction;

public class MoveFromTargetProvider : MonoBehaviour, IMovementProvider
{
	public IMovement CreateMovement()
	{
		return new MoveFromTarget();
	}
}
