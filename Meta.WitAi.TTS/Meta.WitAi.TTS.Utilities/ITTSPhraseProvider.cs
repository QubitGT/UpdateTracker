using System.Collections.Generic;

namespace Meta.WitAi.TTS.Utilities;

public interface ITTSPhraseProvider
{
	List<string> GetVoiceIds();

	List<string> GetVoicePhrases(string voiceId);
}
