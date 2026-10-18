using UnityEngine;

namespace Oculus.Interaction;

public class MoveTowardsTarget : IMovement
{
	private PoseTravelData _travellingData;

	private Tween _tween;

	private Pose _source;

	private Pose _target;

	public Pose Pose => _tween.Pose;

	public bool Stopped
	{
		get
		{
			if (_tween != null)
			{
				return _tween.Stopped;
			}
			return false;
		}
	}

	public MoveTowardsTarget(PoseTravelData travellingData)
	{
		_travellingData = travellingData;
	}

	public void MoveTo(Pose target)
	{
		_target = target;
		_tween = _travellingData.CreateTween(in _source, in target);
	}

	public void UpdateTarget(Pose target)
	{
		if (_target != target)
		{
			_target = target;
			_tween.UpdateTarget(_target);
		}
	}

	public void StopAndSetPose(Pose pose)
	{
		_source = pose;
		_tween?.StopAndSetPose(_source);
	}

	public void Tick()
	{
		_tween.Tick();
	}
}
