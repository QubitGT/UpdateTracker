using UnityEngine;

namespace Oculus.Interaction;

public class FollowTargetProvider : MonoBehaviour, IMovementProvider
{
	[SerializeField]
	private float _speed = 5f;

	private Transform _space;

	private void Awake()
	{
		_space = base.transform;
	}

	public IMovement CreateMovement()
	{
		return new FollowTarget(_speed, _space);
	}
}
