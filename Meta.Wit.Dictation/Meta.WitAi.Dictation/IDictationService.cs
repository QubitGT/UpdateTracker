using Meta.WitAi.Configuration;
using Meta.WitAi.Dictation.Events;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;

namespace Meta.WitAi.Dictation;

public interface IDictationService : ITelemetryEventsProvider
{
	bool Active { get; }

	bool IsRequestActive { get; }

	bool MicActive { get; }

	ITranscriptionProvider TranscriptionProvider { get; set; }

	DictationEvents DictationEvents { get; set; }

	new TelemetryEvents TelemetryEvents { get; set; }

	VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents);

	void Deactivate();

	void Cancel();
}
