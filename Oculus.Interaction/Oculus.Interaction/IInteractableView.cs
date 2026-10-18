using System;
using System.Collections.Generic;

namespace Oculus.Interaction;

public interface IInteractableView
{
	object Data { get; }

	InteractableState State { get; }

	int MaxInteractors { get; }

	int MaxSelectingInteractors { get; }

	IEnumerable<IInteractorView> InteractorViews { get; }

	IEnumerable<IInteractorView> SelectingInteractorViews { get; }

	event Action<InteractableStateChangeArgs> WhenStateChanged;

	event Action<IInteractorView> WhenInteractorViewAdded;

	event Action<IInteractorView> WhenInteractorViewRemoved;

	event Action<IInteractorView> WhenSelectingInteractorViewAdded;

	event Action<IInteractorView> WhenSelectingInteractorViewRemoved;
}
