using Meta.WitAi.Events;

namespace Meta.WitAi.Interfaces;

public interface ITranscriptionEvent
{
	WitTranscriptionEvent OnPartialTranscription { get; }

	WitTranscriptionEvent OnFullTranscription { get; }
}
