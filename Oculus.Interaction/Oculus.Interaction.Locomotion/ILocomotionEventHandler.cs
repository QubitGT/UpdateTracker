using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public interface ILocomotionEventHandler
{
	event Action<LocomotionEvent, Pose> WhenLocomotionEventHandled;

	void HandleLocomotionEvent(LocomotionEvent locomotionEvent);
}
