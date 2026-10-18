using System.Collections.Generic;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;

namespace Meta.WitAi;

public interface IVoiceService : IVoiceEventProvider, ITelemetryEventsProvider, IVoiceActivationHandler
{
	bool IsRequestActive { get; }

	bool UsePlatformIntegrations { get; set; }

	HashSet<VoiceServiceRequest> Requests { get; }

	bool MicActive { get; }

	new VoiceEvents VoiceEvents { get; set; }

	new TelemetryEvents TelemetryEvents { get; set; }

	ITranscriptionProvider TranscriptionProvider { get; set; }

	bool CanActivateAudio();

	bool CanSend();
}
