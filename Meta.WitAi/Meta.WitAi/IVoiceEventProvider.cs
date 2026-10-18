using Meta.WitAi.Events;

namespace Meta.WitAi;

public interface IVoiceEventProvider
{
	VoiceEvents VoiceEvents { get; }
}
