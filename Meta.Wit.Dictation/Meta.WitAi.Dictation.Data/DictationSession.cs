using System;
using Meta.WitAi.Data;

namespace Meta.WitAi.Dictation.Data;

[Serializable]
public class DictationSession : VoiceSession
{
	public IDictationService dictationService;

	public string[] clientRequestId;

	public string sessionId = WitConstants.GetUniqueId();
}
