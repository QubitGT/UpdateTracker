using Meta.WitAi.Events;
using UnityEngine.Events;

namespace Meta.WitAi.Interfaces;

public interface ITranscriptionProvider
{
	string LastTranscription { get; }

	WitTranscriptionEvent OnPartialTranscription { get; }

	WitTranscriptionEvent OnFullTranscription { get; }

	UnityEvent OnStoppedListening { get; }

	UnityEvent OnStartListening { get; }

	WitMicLevelChangedEvent OnMicLevelChanged { get; }

	bool OverrideMicLevel { get; }

	void Activate();

	void Deactivate();
}
