using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.Interfaces;

public interface ITTSVoiceProvider
{
	TTSVoiceSettings VoiceDefaultSettings { get; }

	TTSVoiceSettings[] PresetVoiceSettings { get; }
}
