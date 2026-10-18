using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class PlayerLocomotor : MonoBehaviour, ILocomotionEventHandler
{
	[SerializeField]
	private Transform _playerOrigin;

	[SerializeField]
	private Transform _playerHead;

	private Action<LocomotionEvent, Pose> _whenLocomotionEventHandled = delegate
	{
	};

	protected bool _started;

	private Queue<LocomotionEvent> _deferredEvent = new Queue<LocomotionEvent>();

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

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	private void OnEnable()
	{
		if (_started)
		{
			this.RegisterEndOfFrameCallback(MovePlayer);
		}
	}

	private void OnDisable()
	{
		if (_started)
		{
			_deferredEvent.Clear();
			this.UnregisterEndOfFrameCallback();
		}
	}

	public void HandleLocomotionEvent(LocomotionEvent locomotionEvent)
	{
		_deferredEvent.Enqueue(locomotionEvent);
	}

	private void MovePlayer()
	{
		while (_deferredEvent.Count > 0)
		{
			LocomotionEvent arg = _deferredEvent.Dequeue();
			Pose from = _playerOrigin.GetPose();
			MovePlayer(arg.Pose.position, arg.Translation);
			RotatePlayer(arg.Pose.rotation, arg.Rotation);
			Pose to = _playerOrigin.GetPose();
			Pose arg2 = PoseUtils.Delta(in from, in to);
			_whenLocomotionEventHandled(arg, arg2);
		}
	}

	private void MovePlayer(Vector3 targetPosition, LocomotionEvent.TranslationType translationMode)
	{
		switch (translationMode)
		{
		case LocomotionEvent.TranslationType.Absolute:
		{
			Vector3 vector2 = _playerOrigin.position - _playerHead.position;
			vector2.y = 0f;
			_playerOrigin.position = targetPosition + vector2;
			break;
		}
		case LocomotionEvent.TranslationType.AbsoluteEyeLevel:
		{
			Vector3 vector = _playerOrigin.position - _playerHead.position;
			_playerOrigin.position = targetPosition + vector;
			break;
		}
		case LocomotionEvent.TranslationType.Relative:
			_playerOrigin.position += targetPosition;
			break;
		case LocomotionEvent.TranslationType.Velocity:
			_playerOrigin.position += targetPosition * Time.deltaTime;
			break;
		}
	}

	private void RotatePlayer(Quaternion targetRotation, LocomotionEvent.RotationType rotationMode)
	{
		if (rotationMode != 0)
		{
			Vector3 position = _playerHead.position;
			switch (rotationMode)
			{
			case LocomotionEvent.RotationType.Absolute:
			{
				Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(_playerHead.forward, _playerOrigin.up).normalized, _playerOrigin.up);
				Quaternion rotation = Quaternion.Inverse(_playerOrigin.rotation) * quaternion;
				_playerOrigin.rotation = Quaternion.Inverse(rotation) * targetRotation;
				break;
			}
			case LocomotionEvent.RotationType.Relative:
				_playerOrigin.rotation = targetRotation * _playerOrigin.rotation;
				break;
			case LocomotionEvent.RotationType.Velocity:
			{
				targetRotation.ToAngleAxis(out var angle, out var axis);
				angle *= Time.deltaTime;
				_playerOrigin.rotation = Quaternion.AngleAxis(angle, axis) * _playerOrigin.rotation;
				break;
			}
			}
			_playerOrigin.position = _playerOrigin.position + position - _playerHead.position;
		}
	}

	public void InjectAllPlayerLocomotor(Transform playerOrigin, Transform playerHead)
	{
		InjectPlayerOrigin(playerOrigin);
		InjectPlayerHead(playerHead);
	}

	public void InjectPlayerOrigin(Transform playerOrigin)
	{
		_playerOrigin = playerOrigin;
	}

	public void InjectPlayerHead(Transform playerHead)
	{
		_playerHead = playerHead;
	}
}
