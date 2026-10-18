using UnityEngine;

namespace Oculus.Interaction;

public class MoveFromTarget : IMovement
{
	public Pose Pose { get; private set; } = Pose.identity;


	public bool Stopped => true;

	public void StopMovement()
	{
	}

	public void MoveTo(Pose target)
	{
		Pose = target;
	}

	public void UpdateTarget(Pose target)
	{
		Pose = target;
	}

	public void StopAndSetPose(Pose source)
	{
		Pose = source;
	}

	public void Tick()
	{
	}
}
