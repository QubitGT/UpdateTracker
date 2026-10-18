using Meta.WitAi.Events;
using UnityEngine.Events;

namespace Meta.WitAi.Interfaces;

public interface IAudioInputEvents
{
	WitMicLevelChangedEvent OnMicAudioLevelChanged { get; }

	UnityEvent OnMicStartedListening { get; }

	UnityEvent OnMicStoppedListening { get; }
}
