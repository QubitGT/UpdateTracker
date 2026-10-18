using UnityEngine;

namespace Oculus.Interaction;

public class ObjectPull : IMovement
{
	private float _speed = 1f;

	private float _deadZone;

	private Pose _current = Pose.identity;

	private Pose _grabberStartPose;

	private Pose _grabbableStartPose;

	private Pose _target;

	private Plane _pullingPlane;

	private Vector3 _translationDelta = Vector3.zero;

	private float _lastTime;

	private float _originalDistance;

	private bool _reachedGrabber;

	public Pose Pose => _current;

	public bool Stopped => _reachedGrabber;

	public ObjectPull(float speed, float deadZone)
	{
		_speed = speed;
		_deadZone = deadZone;
	}

	public void MoveTo(Pose target)
	{
		_target = (_grabberStartPose = target);
		_current = _grabbableStartPose;
		_lastTime = Time.time;
		_reachedGrabber = false;
		Vector3 vector = _grabbableStartPose.position - _grabberStartPose.position;
		_originalDistance = vector.magnitude;
		_pullingPlane = new Plane(vector.normalized, _grabberStartPose.position);
	}

	public void UpdateTarget(Pose target)
	{
		_target = target;
	}

	public void StopAndSetPose(Pose source)
	{
		_grabbableStartPose = source;
	}

	public void Tick()
	{
		if (_reachedGrabber)
		{
			_current = _target;
			return;
		}
		float num = Time.time - _lastTime;
		_lastTime = Time.time;
		float distanceToPoint = _pullingPlane.GetDistanceToPoint(_target.position);
		if (!(Mathf.Abs(distanceToPoint) < _deadZone))
		{
			Vector3 normalized = (_current.position - _target.position).normalized;
			_translationDelta = normalized * distanceToPoint * _speed * num;
			if (Vector3.Distance(_current.position, _target.position) < _translationDelta.magnitude)
			{
				_reachedGrabber = true;
				_current = _target;
				return;
			}
			_current.position += _translationDelta;
			float num2 = Vector3.Distance(_current.position, _target.position);
			float t = 1f - Mathf.Clamp01(num2 / _originalDistance);
			_current.rotation = Quaternion.Slerp(_grabbableStartPose.rotation, _target.rotation, t);
		}
	}
}
