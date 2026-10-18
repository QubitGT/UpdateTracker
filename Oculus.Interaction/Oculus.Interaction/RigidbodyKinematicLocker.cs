using UnityEngine;

namespace Oculus.Interaction;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyKinematicLocker : MonoBehaviour
{
	private Rigidbody _rigidbody;

	private int _counter;

	private bool _savedIsKinematicState;

	public bool IsLocked => _counter != 0;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	public void LockKinematic()
	{
		if (_counter == 0)
		{
			_savedIsKinematicState = _rigidbody.isKinematic;
		}
		_counter++;
		_rigidbody.isKinematic = true;
	}

	public void UnlockKinematic()
	{
		if (_counter == 0)
		{
			Debug.LogError("Too many calls to UnlockKinematic.Expected calls to LockKinematic to balance the kinematic state.", this);
			return;
		}
		_counter--;
		if (_counter == 0)
		{
			_rigidbody.isKinematic = _savedIsKinematicState;
		}
	}
}
