using System;

namespace Oculus.Interaction;

public interface IInteractorView
{
	int Identifier { get; }

	object Data { get; }

	bool HasCandidate { get; }

	object CandidateProperties { get; }

	bool HasInteractable { get; }

	bool HasSelectedInteractable { get; }

	InteractorState State { get; }

	event Action<InteractorStateChangeArgs> WhenStateChanged;

	event Action WhenPreprocessed;

	event Action WhenProcessed;

	event Action WhenPostprocessed;
}
