using System.Collections;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class PingPongPaddle : MonoBehaviour, ITransformer
{
	[SerializeField]
	private HandGrabInteractable _leftHandGrabInteractable;

	[SerializeField]
	private HandGrabInteractable _rightHandGrabInteractable;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private AnimationCurve _collisionStrength;

	private const float _timeBetweenCollisions = 0.1f;

	private WaitForSeconds _hapticsWait = new WaitForSeconds(0.1f);

	private AudioPhysics.CollisionEvents _collisionEvents;

	private float _timeAtLastCollision;

	protected bool _started;

	private OVRInput.Controller _activeController;

	private IGrabbable _grabbable;

	private Pose _grabDeltaInLocalSpace;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_collisionEvents = _rigidbody.gameObject.AddComponent<AudioPhysics.CollisionEvents>();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_collisionEvents.WhenCollisionEnter += HandleCollisionEnter;
			_leftHandGrabInteractable.WhenStateChanged += HandleLeftHandGrabInteractableStateChanged;
			_rightHandGrabInteractable.WhenStateChanged += HandleRightHandGrabInteractableStateChanged;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_collisionEvents.WhenCollisionEnter -= HandleCollisionEnter;
			_leftHandGrabInteractable.WhenStateChanged -= HandleLeftHandGrabInteractableStateChanged;
			_rightHandGrabInteractable.WhenStateChanged -= HandleRightHandGrabInteractableStateChanged;
		}
	}

	private void HandleLeftHandGrabInteractableStateChanged(InteractableStateChangeArgs stateChange)
	{
		if (stateChange.NewState == InteractableState.Select)
		{
			_activeController |= OVRInput.Controller.LTouch;
		}
		else if (stateChange.PreviousState == InteractableState.Select)
		{
			_activeController &= ~OVRInput.Controller.LTouch;
		}
	}

	private void HandleRightHandGrabInteractableStateChanged(InteractableStateChangeArgs stateChange)
	{
		if (stateChange.NewState == InteractableState.Select)
		{
			_activeController |= OVRInput.Controller.RTouch;
		}
		else if (stateChange.PreviousState == InteractableState.Select)
		{
			_activeController &= ~OVRInput.Controller.RTouch;
		}
	}

	private void HandleCollisionEnter(Collision collision)
	{
		TryPlayCollisionAudio(collision);
	}

	private void TryPlayCollisionAudio(Collision collision)
	{
		float sqrMagnitude = collision.relativeVelocity.sqrMagnitude;
		if (!(collision.collider.gameObject == null))
		{
			float num = Time.time - _timeAtLastCollision;
			if (!(0.1f > num))
			{
				_timeAtLastCollision = Time.time;
				PlayCollisionHaptics(sqrMagnitude);
			}
		}
	}

	private void PlayCollisionHaptics(float strength)
	{
		float pitch = _collisionStrength.Evaluate(strength);
		StartCoroutine(HapticsRoutine(pitch, _activeController));
	}

	private IEnumerator HapticsRoutine(float pitch, OVRInput.Controller controller)
	{
		OVRInput.SetControllerVibration(pitch * 0.5f, pitch * 0.2f, controller);
		yield return _hapticsWait;
		OVRInput.SetControllerVibration(0f, 0f, controller);
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public void BeginTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _rigidbody.transform;
		_grabDeltaInLocalSpace = new Pose(transform.InverseTransformVector(pose.position - transform.position), Quaternion.Inverse(pose.rotation) * transform.rotation);
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		_rigidbody.MoveRotation(pose.rotation * _grabDeltaInLocalSpace.rotation);
		_rigidbody.MovePosition(pose.position - _rigidbody.transform.TransformVector(_grabDeltaInLocalSpace.position));
	}

	public void EndTransform()
	{
	}
}
