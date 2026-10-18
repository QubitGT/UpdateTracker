using UnityEngine;

namespace Oculus.Interaction;

public class MoveAtSourceProvider : MonoBehaviour, IMovementProvider
{
	public IMovement CreateMovement()
	{
		return new MoveRelativeToTarget();
	}
}
