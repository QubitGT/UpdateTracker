using Meta.WitAi.Attributes;
using Meta.WitAi.Json;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.CallbackHandlers;

[AddComponentMenu("Wit.ai/Response Matchers/Out Of Domain")]
public class OutOfScopeUtteranceHandler : WitResponseHandler
{
	[Tooltip("If set to a value greater than zero, any intent that returns with a confidence lower than this value will be treated as out of domain/scope.")]
	[Range(0f, 1f)]
	[SerializeField]
	private float confidenceThreshold;

	[Space(8f)]
	[TooltipBox("Triggered when a activation on the associated AppVoiceExperience does not return any intents.")]
	[SerializeField]
	private StringEvent onOutOfDomain = new StringEvent();

	protected override string OnValidateResponse(WitResponseNode response, bool isEarlyResponse)
	{
		if (response == null)
		{
			return "Response is null";
		}
		if (response["intents"].Count > 0)
		{
			if (response.GetFirstIntent()["confidence"].AsFloat < confidenceThreshold)
			{
				return string.Empty;
			}
			return "Intents found";
		}
		return string.Empty;
	}

	protected override void OnResponseInvalid(WitResponseNode response, string error)
	{
	}

	protected override void OnResponseSuccess(WitResponseNode response)
	{
		onOutOfDomain?.Invoke(response.GetTranscription());
	}
}
