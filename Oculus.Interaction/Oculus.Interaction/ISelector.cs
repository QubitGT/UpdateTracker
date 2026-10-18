using System;

namespace Oculus.Interaction;

public interface ISelector
{
	event Action WhenSelected;

	event Action WhenUnselected;
}
