using UnityEngine;

namespace Oculus.Interaction;

public class ObjectPullProvider : MonoBehaviour, IMovementProvider
{
	[SerializeField]
	[Min(0f)]
	private float _speed = 1f;

	[SerializeField]
	[Min(0f)]
	private float _deadZone = 0.02f;

	public float Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = value;
		}
	}

	public float DeadZone
	{
		get
		{
			return _deadZone;
		}
		set
		{
			_deadZone = value;
		}
	}

	public IMovement CreateMovement()
	{
		return new ObjectPull(_speed, _deadZone);
	}
}
