using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;

namespace Meta.WitAi.TTS.Interfaces;

public interface ISpeakerTextPreprocessor
{
	void OnPreprocessTTS(TTSSpeaker speaker, List<string> phrases);
}
