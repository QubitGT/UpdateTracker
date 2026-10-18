using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class JoystickPoseMovement : IMovement
{
	private Pose _currentPose;

	private Pose _targetPose;

	private Vector3 _localDirection;

	private IController _controller;

	private float _moveSpeed;

	private float _rotationSpeed;

	private float _minDistance;

	private float _maxDistance;

	public Pose Pose => _currentPose;

	public bool Stopped => false;

	public JoystickPoseMovement(IController controller, float moveSpeed, float rotationSpeed, float minDistance, float maxDistance)
	{
		_controller = controller;
		_moveSpeed = moveSpeed;
		_rotationSpeed = rotationSpeed;
		_minDistance = minDistance;
		_maxDistance = maxDistance;
	}

	public void MoveTo(Pose target)
	{
		_targetPose = target;
		_localDirection = Quaternion.Inverse(_targetPose.rotation) * (_currentPose.position - _targetPose.position).normalized;
	}

	public void UpdateTarget(Pose target)
	{
		_targetPose = target;
	}

	public void StopAndSetPose(Pose pose)
	{
		_currentPose = pose;
	}

	public void Tick()
	{
		AdjustPoseWithJoystickInput();
	}

	public void AdjustPoseWithJoystickInput()
	{
		if (_controller != null)
		{
			Vector2 primary2DAxis = _controller.ControllerInput.Primary2DAxis;
			float num = primary2DAxis.y * _moveSpeed;
			float angle = (0f - primary2DAxis.x) * _rotationSpeed;
			Vector3 vector = _targetPose.rotation * _localDirection;
			Vector3 position = _targetPose.position;
			float num2 = Mathf.Clamp(Vector3.Project(_currentPose.position - position, vector).magnitude + num, _minDistance, _maxDistance);
			Vector3 position2 = position + vector * num2;
			Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up) * _currentPose.rotation;
			_currentPose = new Pose(position2, rotation);
			UpdateTarget(_currentPose);
		}
	}

	public void InjectController(IController controller)
	{
		_controller = controller;
	}
}
