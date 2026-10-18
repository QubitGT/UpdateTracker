using Meta.WitAi.Composer.Data;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Integrations;

namespace Meta.WitAi.Composer.Integrations;

public static class WitComposerResponseExtensions
{
	public static ComposerResponseData GetComposerResponse(this WitResponseNode response)
	{
		return new ComposerResponseData
		{
			witResponse = response,
			error = response.GetError(),
			expectsInput = response.GetExpectsInput(),
			actionID = response.GetActionId(),
			responseIsFinal = response.GetIsResponseFinal(),
			responsePhrase = response.GetResponseText(),
			responseTts = response.GetTTS(),
			responseTtsSettings = response.GetTTSSettings(),
			requestId = response.GetRequestId()
		};
	}

	public static WitResponseNode GetContextMap(this WitResponseNode response)
	{
		return response?["context_map"].AsObject ?? null;
	}

	public static bool GetExpectsInput(this WitResponseNode response)
	{
		return response?["expects_input"].AsBool ?? false;
	}

	public static string GetActionId(this WitResponseNode response)
	{
		return response?["action"].Value ?? string.Empty;
	}

	public static bool GetIsResponseFinal(this WitResponseNode response)
	{
		return response?.GetFinalResponse() != null;
	}

	public static string GetResponseText(this WitResponseNode response)
	{
		return response?.GetResponse()?.SafeGet("text")?.Value ?? string.Empty;
	}

	public static WitResponseClass GetSpeech(this WitResponseNode response)
	{
		return response?.GetResponse()?.SafeGet("speech")?.AsObject ?? null;
	}

	public static string GetTTS(this WitResponseNode response)
	{
		return response?.GetSpeech()?.SafeGet("q")?.Value ?? response.GetResponseText();
	}

	public static TTSWitVoiceSettings GetTTSSettings(this WitResponseNode response)
	{
		WitResponseClass witResponseClass = response?.GetSpeech();
		if (witResponseClass == null)
		{
			return null;
		}
		TTSWitVoiceSettings tTSWitVoiceSettings = new TTSWitVoiceSettings();
		tTSWitVoiceSettings.DeserializeObject(witResponseClass);
		return tTSWitVoiceSettings;
	}
}
