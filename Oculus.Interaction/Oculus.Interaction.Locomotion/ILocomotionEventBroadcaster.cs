using System;

namespace Oculus.Interaction.Locomotion;

public interface ILocomotionEventBroadcaster
{
	event Action<LocomotionEvent> WhenLocomotionPerformed;
}
