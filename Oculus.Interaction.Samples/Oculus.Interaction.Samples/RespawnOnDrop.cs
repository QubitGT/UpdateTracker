using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Samples;

public class RespawnOnDrop : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Respawn will happen when the transform moves below this World Y position.")]
	private float _yThresholdForRespawn;

	[SerializeField]
	[Tooltip("UnityEvent triggered when a respawn occurs.")]
	private UnityEvent _whenRespawned = new UnityEvent();

	[SerializeField]
	[Tooltip("If the transform has an associated rigidbody, make it kinematic during this number of frames after a respawn, in order to avoid ghost collisions.")]
	private int _sleepFrames;

	private Vector3 _initialPosition;

	private Quaternion _initialRotation;

	private Vector3 _initialScale;

	private TwoGrabFreeTransformer[] _freeTransformers;

	private Rigidbody _rigidBody;

	private int _sleepCountDown;

	public UnityEvent WhenRespawned => _whenRespawned;

	protected virtual void OnEnable()
	{
		_initialPosition = base.transform.position;
		_initialRotation = base.transform.rotation;
		_initialScale = base.transform.localScale;
		_freeTransformers = GetComponents<TwoGrabFreeTransformer>();
		_rigidBody = GetComponent<Rigidbody>();
	}

	protected virtual void Update()
	{
		if (base.transform.position.y < _yThresholdForRespawn)
		{
			Respawn();
		}
	}

	protected virtual void FixedUpdate()
	{
		if (_sleepCountDown > 0 && --_sleepCountDown == 0)
		{
			_rigidBody.isKinematic = false;
		}
	}

	public void Respawn()
	{
		base.transform.position = _initialPosition;
		base.transform.rotation = _initialRotation;
		base.transform.localScale = _initialScale;
		if ((bool)_rigidBody)
		{
			_rigidBody.velocity = Vector3.zero;
			_rigidBody.angularVelocity = Vector3.zero;
			if (!_rigidBody.isKinematic && _sleepFrames > 0)
			{
				_sleepCountDown = _sleepFrames;
				_rigidBody.isKinematic = true;
			}
		}
		TwoGrabFreeTransformer[] freeTransformers = _freeTransformers;
		for (int i = 0; i < freeTransformers.Length; i++)
		{
			freeTransformers[i].MarkAsBaseScale();
		}
		_whenRespawned.Invoke();
	}
}
