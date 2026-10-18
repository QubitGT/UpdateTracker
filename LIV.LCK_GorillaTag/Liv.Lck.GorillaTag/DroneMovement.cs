using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneMovement
{
	private float _moveSpeed = 1f;

	private float _rotationSpeed = 90f;

	private float _tiltUpAngle = -45f;

	private float _tiltDownAngle = 90f;

	private bool _smoothMovement = true;

	private bool _smoothRotation = true;

	private float _moveSmoothness = 1f;

	private float _rotationSmoothness = 1f;

	private bool _snapAxis;

	private bool _useTiltAsDirection;

	private bool _isMouseInverted;

	private Transform _gimbalTransform;

	private Transform _droneTransform;

	private Vector3 _localTargetPosition;

	private Quaternion _localTargetRotation;

	private Quaternion _tiltTargetRotation = Quaternion.identity;

	private Quaternion _rollTargetRotation = Quaternion.identity;

	private float _tiltAngle;

	private float _rollAngle;

	private float PositionSpeed => _moveSpeed * Time.deltaTime;

	private float RotationSpeed => _rotationSpeed * Time.deltaTime;

	private float SmoothPositionSpeed => Time.deltaTime / _moveSmoothness;

	private float SmoothRotationSpeed => Time.deltaTime / _rotationSmoothness;

	public DroneMovement(Transform droneTransform, Transform gimbalTransform)
	{
		_droneTransform = droneTransform;
		_gimbalTransform = gimbalTransform;
		_localTargetPosition = _droneTransform.localPosition;
		_localTargetRotation = _droneTransform.localRotation;
		_tiltTargetRotation = _gimbalTransform.localRotation;
	}

	public void Run()
	{
		if (_smoothMovement)
		{
			_droneTransform.localPosition = Vector3.Lerp(_droneTransform.localPosition, _localTargetPosition, SmoothPositionSpeed);
			_droneTransform.localRotation = Quaternion.Slerp(_droneTransform.localRotation, _localTargetRotation * (_useTiltAsDirection ? _tiltTargetRotation : Quaternion.identity), SmoothRotationSpeed);
		}
		else
		{
			_droneTransform.localPosition = _localTargetPosition;
			_droneTransform.localRotation = _localTargetRotation;
		}
		if (_smoothRotation)
		{
			_gimbalTransform.localRotation = Quaternion.Slerp(_gimbalTransform.localRotation, ((!_useTiltAsDirection) ? _tiltTargetRotation : Quaternion.identity) * _rollTargetRotation, SmoothRotationSpeed);
		}
		else
		{
			_gimbalTransform.localRotation = _tiltTargetRotation * _rollTargetRotation;
		}
	}

	public void MoveForward()
	{
		_localTargetPosition += _droneTransform.forward * PositionSpeed;
	}

	public void MoveBackward()
	{
		_localTargetPosition -= _droneTransform.forward * PositionSpeed;
	}

	public void MoveLeft()
	{
		_localTargetPosition -= _droneTransform.right * PositionSpeed;
	}

	public void MoveRight()
	{
		_localTargetPosition += _droneTransform.right * PositionSpeed;
	}

	public void MoveUp()
	{
		_localTargetPosition += _droneTransform.up * PositionSpeed;
	}

	public void MoveDown()
	{
		_localTargetPosition -= _droneTransform.up * PositionSpeed;
	}

	public void RotateLeft()
	{
		_localTargetRotation *= Quaternion.Euler(0f, 0f - RotationSpeed, 0f);
	}

	public void RotateRight()
	{
		_localTargetRotation *= Quaternion.Euler(0f, RotationSpeed, 0f);
	}

	public void TiltUp()
	{
		_tiltAngle -= RotationSpeed;
		ProcessTilt();
	}

	public void TiltDown()
	{
		_tiltAngle += RotationSpeed;
		ProcessTilt();
	}

	private void ProcessTilt()
	{
		_tiltAngle = Mathf.Clamp(_tiltAngle, _tiltUpAngle, _tiltDownAngle);
		_tiltTargetRotation = Quaternion.AngleAxis(_tiltAngle, Vector3.right);
	}

	private void ProcessRoll()
	{
		_rollTargetRotation = Quaternion.AngleAxis(_rollAngle, Vector3.forward);
	}

	private void ProcessMovement(Vector2 stick, float trigger)
	{
		Vector3 vector = _droneTransform.forward * stick.y + _droneTransform.right * stick.x + _droneTransform.up * trigger;
		_localTargetPosition += vector * PositionSpeed;
	}

	public void ResetTillAndRoll()
	{
		_tiltAngle = 0f;
		_rollAngle = 0f;
		ProcessTilt();
		ProcessRoll();
	}

	public void MoveForwardBackwardLeftRight(Vector2 v)
	{
		if (_snapAxis)
		{
			if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
			{
				v.y = 0f;
			}
			else
			{
				v.x = 0f;
			}
		}
		ProcessMovement(v, 0f);
	}

	public void MoveUpAndDown(float f)
	{
		ProcessMovement(Vector2.zero, f);
	}

	public void TiltAndRotateGamePad(Vector2 v)
	{
		v.y *= -1f;
		TiltAndRotate(v);
	}

	public void TiltAndRotateMouse(Vector2 v)
	{
		v /= 2f;
		v.y *= -1f;
		if (_isMouseInverted)
		{
			v *= -1f;
		}
		TiltAndRotate(v);
	}

	private void TiltAndRotate(Vector2 v)
	{
		if (_snapAxis)
		{
			if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
			{
				v.y = 0f;
			}
			else
			{
				v.x = 0f;
			}
		}
		_localTargetRotation *= Quaternion.Euler(0f, RotationSpeed * v.x, 0f);
		_tiltAngle += RotationSpeed * v.y;
		ProcessTilt();
	}

	public void Roll(Vector2 v)
	{
		v.x = (_isMouseInverted ? v.x : (0f - v.x));
		_rollAngle += RotationSpeed * v.x;
		ProcessRoll();
	}

	public void SetMoveSpeedChanged(float speed)
	{
		_moveSpeed = speed;
	}

	public void SetMoveSmoothness(float smoothness)
	{
		_moveSmoothness = smoothness;
	}

	public void SetRotationSpeed(float speed)
	{
		_rotationSpeed = speed;
	}

	public void SetRotationSmoothness(float smoothness)
	{
		_rotationSmoothness = smoothness;
	}

	public void SetSnapAxis(bool snap)
	{
		_snapAxis = snap;
	}

	public void SetIsSmoothMovement(bool smooth)
	{
		_smoothMovement = smooth;
	}

	public void SetIsSmoothRotation(bool smooth)
	{
		_smoothRotation = smooth;
	}

	public void SetUseTiltAsDirection(bool use)
	{
		_useTiltAsDirection = use;
	}

	public void SetIsMouseInverted(bool inverted)
	{
		_isMouseInverted = inverted;
	}

	public void MoveAndRotateDroneInstantly(Vector3 position, Quaternion rotation)
	{
		_droneTransform.localPosition = position;
		_droneTransform.localRotation = rotation;
		_localTargetPosition = position;
		_localTargetRotation = rotation;
	}
}
