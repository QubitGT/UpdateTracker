using Oculus.Interaction;
using UnityEngine;

public class MoveRelativeToTarget : IMovement
{
	private Pose _current = Pose.identity;

	private Pose _originalTarget;

	private Pose _originalSource;

	private Pose _offset = Pose.identity;

	public Pose Pose => _current;

	public bool Stopped => true;

	public void MoveTo(Pose target)
	{
		_originalTarget = target;
		_offset = PoseUtils.Delta(in _originalTarget, in _originalSource);
	}

	public void UpdateTarget(Pose target)
	{
		_current = PoseUtils.Multiply(in target, in _offset);
	}

	public void StopAndSetPose(Pose source)
	{
		_current = (_originalSource = source);
	}

	public void Tick()
	{
	}
}
