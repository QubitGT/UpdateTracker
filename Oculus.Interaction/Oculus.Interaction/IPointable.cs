using System;

namespace Oculus.Interaction;

public interface IPointable
{
	event Action<PointerEvent> WhenPointerEventRaised;
}
