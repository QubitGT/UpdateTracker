using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class FlyingLocomotor : MonoBehaviour, ILocomotionEventHandler, IDeltaTimeConsumer
{
	[Header("Character")]
	[SerializeField]
	private CharacterController _characterController;

	[Header("VR Player")]
	[SerializeField]
	[Tooltip("Root of the actual VR player so it can be sync with with capsule. If you provided a _playerEyes you must also provide a _playerOrigin.")]
	private Transform _playerOrigin;

	[SerializeField]
	[Tooltip("Eyes of the actual VR player so it can be sync with the capsule. If you provided a _playerOrigin you must also provide a _playerEyes.")]
	private Transform _playerEyes;

	[Header("Parameters")]
	[SerializeField]
	[Tooltip("The rate of acceleration during movement.")]
	private float _acceleration = 150f;

	[SerializeField]
	[Tooltip("The rate of damping on the horizontal movement while in the air.")]
	private float _airDamping = 30f;

	private Func<float> _deltaTimeProvider = () => Time.deltaTime;

	protected Action<LocomotionEvent, Pose> _whenLocomotionEventHandled = delegate
	{
	};

	private Pose _accumulatedDeltaFrame;

	private Vector3 _velocity;

	private const float _sellionToTopOfHead = 0.1085f;

	private const float _sellionToBackOfHeadHalf = 0.0965f;

	private Queue<LocomotionEvent> _deferredLocomotionEvent = new Queue<LocomotionEvent>();

	private YieldInstruction _endOfFrame = new WaitForEndOfFrame();

	private Coroutine _endOfFrameRoutine;

	protected bool _started;

	public float Acceleration
	{
		get
		{
			return _acceleration;
		}
		set
		{
			_acceleration = value;
		}
	}

	public float AirDamping
	{
		get
		{
			return _airDamping;
		}
		set
		{
			_airDamping = value;
		}
	}

	public bool IsGrounded => _characterController.IsGrounded;

	public event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled
	{
		add
		{
			_whenLocomotionEventHandled = (Action<LocomotionEvent, Pose>)Delegate.Combine(_whenLocomotionEventHandled, value);
		}
		remove
		{
			_whenLocomotionEventHandled = (Action<LocomotionEvent, Pose>)Delegate.Remove(_whenLocomotionEventHandled, value);
		}
	}

	public void SetDeltaTimeProvider(Func<float> deltaTimeProvider)
	{
		_deltaTimeProvider = deltaTimeProvider;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_endOfFrameRoutine = StartCoroutine(EndOfFrameCoroutine());
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_accumulatedDeltaFrame = Pose.identity;
			StopCoroutine(_endOfFrameRoutine);
			_endOfFrameRoutine = null;
		}
	}

	protected virtual void Update()
	{
		CatchUpCharacterToPlayer();
		UpdateVelocity();
		Pose from = _characterController.Pose;
		Vector3 delta = _velocity * _deltaTimeProvider();
		_characterController.Move(delta);
		Pose to = _characterController.Pose;
		AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
	}

	protected virtual void LateUpdate()
	{
		ConsumeDeferredLocomotionEvents();
	}

	protected virtual void LastUpdate()
	{
		CatchUpPlayerToCharacter(_accumulatedDeltaFrame, GetCharacterFeet().y);
		_accumulatedDeltaFrame = Pose.identity;
	}

	public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Velocity)
		{
			AddVelocity(locomotionEvent.Pose.position);
			_whenLocomotionEventHandled(locomotionEvent, locomotionEvent.Pose);
			return;
		}
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute || locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel || locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
		{
			_velocity = Vector3.zero;
		}
		_deferredLocomotionEvent.Enqueue(locomotionEvent);
	}

	private void ConsumeDeferredLocomotionEvents()
	{
		if (_deferredLocomotionEvent.Count != 0)
		{
			Pose from = _characterController.Pose;
			while (_deferredLocomotionEvent.Count > 0)
			{
				LocomotionEvent locomotionEvent = _deferredLocomotionEvent.Dequeue();
				HandleDeferredLocomotionEvent(locomotionEvent);
			}
			Pose to = _characterController.Pose;
			AccumulateDelta(ref _accumulatedDeltaFrame, in from, in to);
		}
	}

	private void HandleDeferredLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		Pose from = _characterController.Pose;
		if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Absolute)
		{
			MoveAbsoluteFeet(locomotionEvent.Pose.position);
		}
		else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.AbsoluteEyeLevel)
		{
			MoveAbsoluteHead(locomotionEvent.Pose.position);
		}
		else if (locomotionEvent.Translation == LocomotionEvent.TranslationType.Relative)
		{
			MoveRelative(locomotionEvent.Pose.position);
		}
		if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Absolute)
		{
			RotateAbsolute(locomotionEvent.Pose.rotation);
		}
		else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Relative)
		{
			RotateRelative(locomotionEvent.Pose.rotation);
		}
		else if (locomotionEvent.Rotation == LocomotionEvent.RotationType.Velocity)
		{
			RotateVelocity(locomotionEvent.Pose.rotation);
		}
		Pose to = _characterController.Pose;
		Pose accumulator = Pose.identity;
		AccumulateDelta(ref accumulator, in from, in to);
		_whenLocomotionEventHandled(locomotionEvent, accumulator);
	}

	private void AccumulateDelta(ref Pose accumulator, in Pose from, in Pose to)
	{
		accumulator.position = accumulator.position + to.position - from.position;
		accumulator.rotation = Quaternion.Inverse(from.rotation) * to.rotation * accumulator.rotation;
	}

	private void AddVelocity(Vector3 velocity)
	{
		_velocity += velocity * GetModifiedSpeedFactor();
	}

	private void MoveAbsoluteFeet(Vector3 target)
	{
		Vector3 characterFeet = GetCharacterFeet();
		Vector3 vector = target - characterFeet;
		Vector3 position = _characterController.Pose.position + vector;
		_characterController.SetPosition(position);
	}

	private void MoveAbsoluteHead(Vector3 target)
	{
		Vector3 characterHead = GetCharacterHead();
		Vector3 vector = target - characterHead;
		Vector3 position = _characterController.Pose.position + vector;
		_characterController.SetPosition(position);
	}

	private void MoveRelative(Vector3 offset)
	{
		_velocity = Vector3.zero;
		_characterController.Move(offset);
	}

	private void RotateAbsolute(Quaternion target)
	{
		_characterController.SetRotation(target);
	}

	private void RotateRelative(Quaternion target)
	{
		target *= _characterController.Pose.rotation;
		_characterController.SetRotation(target);
	}

	private void RotateVelocity(Quaternion target)
	{
		target.ToAngleAxis(out var angle, out var axis);
		angle *= _deltaTimeProvider();
		target = Quaternion.AngleAxis(angle, axis) * _characterController.Pose.rotation;
		_characterController.SetRotation(target);
	}

	private void CatchUpCharacterToPlayer()
	{
		Vector3 delta = GetPlayerHead() - _characterController.Pose.position;
		Vector3 forward = Vector3.ProjectOnPlane(_playerEyes.forward, Vector3.up);
		_characterController.Move(delta);
		_characterController.SetRotation(Quaternion.LookRotation(forward, Vector3.up));
	}

	private void CatchUpPlayerToCharacter(Pose delta, float feetHeight)
	{
		Pose pose = _characterController.Pose;
		Vector3 playerHead = GetPlayerHead();
		_playerOrigin.rotation = delta.rotation * _playerOrigin.rotation;
		_playerOrigin.position = _playerOrigin.position + playerHead - GetPlayerHead();
		Vector3 position = _playerOrigin.position + delta.position;
		_playerOrigin.position = position;
		_characterController.SetPosition(pose.position);
		_characterController.SetRotation(pose.rotation);
	}

	public void ResetPlayerToCharacter()
	{
		Pose pose = _characterController.Pose;
		Vector3 characterFeet = GetCharacterFeet();
		Vector3 vector = _playerOrigin.position - GetPlayerHead();
		_playerOrigin.position = characterFeet + vector;
		_accumulatedDeltaFrame = Pose.identity;
		_characterController.SetPosition(pose.position);
		_characterController.SetRotation(pose.rotation);
	}

	private void UpdateVelocity()
	{
		float num = _deltaTimeProvider();
		float num2 = 1f / (1f + _airDamping * num);
		_velocity.x *= num2;
		_velocity.y *= num2;
		_velocity.z *= num2;
	}

	private float GetModifiedSpeedFactor()
	{
		return _acceleration * _deltaTimeProvider();
	}

	private Vector3 GetCharacterFeet()
	{
		return _characterController.Pose.position + Vector3.down * (_characterController.Height * 0.5f + _characterController.SkinWidth);
	}

	private Vector3 GetCharacterHead()
	{
		return _characterController.Pose.position + Vector3.up * (_characterController.Height * 0.5f - 0.1085f + _characterController.SkinWidth);
	}

	private Vector3 GetPlayerHead()
	{
		return _playerEyes.position - _playerEyes.forward * 0.0965f;
	}

	private IEnumerator EndOfFrameCoroutine()
	{
		while (true)
		{
			yield return _endOfFrame;
			LastUpdate();
		}
	}

	public void InjectAllFlyingLocomotor(CharacterController characterController, Transform playerEyes, Transform playerOrigin)
	{
		InjectCharacterController(characterController);
		InjectPlayerEyes(playerEyes);
		InjectPlayerOrigin(playerOrigin);
	}

	public void InjectCharacterController(CharacterController characterController)
	{
		_characterController = characterController;
	}

	public void InjectPlayerEyes(Transform playerEyes)
	{
		_playerEyes = playerEyes;
	}

	public void InjectPlayerOrigin(Transform playerOrigin)
	{
		_playerOrigin = playerOrigin;
	}
}
