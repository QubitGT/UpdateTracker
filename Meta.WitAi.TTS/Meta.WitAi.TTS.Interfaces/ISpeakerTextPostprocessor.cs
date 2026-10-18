using System.Collections.Generic;
using Meta.WitAi.TTS.Utilities;

namespace Meta.WitAi.TTS.Interfaces;

public interface ISpeakerTextPostprocessor
{
	void OnPostprocessTTS(TTSSpeaker speaker, List<string> phrases);
}
