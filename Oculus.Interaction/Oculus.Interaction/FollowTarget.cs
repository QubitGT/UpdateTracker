using UnityEngine;

namespace Oculus.Interaction;

public class FollowTarget : IMovement
{
	private float _speed;

	private Transform _space;

	private Pose _localTarget;

	private Pose _localPose;

	private float _startTime;

	private const float ROTATION_SPEED_FACTOR = 50f;

	public Pose Pose => ToWorld(in _localPose);

	public bool Stopped => false;

	public FollowTarget(float speed, Transform space)
	{
		_speed = speed;
		_space = space;
	}

	private Pose ToLocal(in Pose pose)
	{
		Vector3 position = _space.InverseTransformPoint(pose.position);
		Quaternion rotation = Quaternion.Inverse(_space.rotation) * pose.rotation;
		return new Pose(position, rotation);
	}

	private Pose ToWorld(in Pose pose)
	{
		Vector3 position = _space.TransformPoint(pose.position);
		Quaternion rotation = _space.rotation * pose.rotation;
		return new Pose(position, rotation);
	}

	public void MoveTo(Pose target)
	{
		_startTime = Time.time;
		_localTarget = ToLocal(in target);
	}

	public void UpdateTarget(Pose target)
	{
		_localTarget = ToLocal(in target);
		Tick();
	}

	public void StopAndSetPose(Pose source)
	{
		_localPose = ToLocal(in source);
	}

	public void Tick()
	{
		float time = Time.time;
		float num = (time - _startTime) * _speed;
		_startTime = time;
		_localPose.position = Vector3.MoveTowards(_localPose.position, _localTarget.position, num);
		_localPose.rotation = Quaternion.RotateTowards(_localPose.rotation, _localTarget.rotation, num * 50f);
	}
}
