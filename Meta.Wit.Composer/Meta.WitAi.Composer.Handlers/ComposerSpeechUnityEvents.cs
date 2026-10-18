using Meta.WitAi.Attributes;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.Events;
using Meta.WitAi.Json;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.Composer.Handlers;

public class ComposerSpeechUnityEvents : MonoBehaviour, IComposerSpeechHandler
{
	[TooltipBox("Events for receipt of partial transcriptions")]
	[SerializeField]
	private StringEvent onPartialText;

	[SerializeField]
	private WitObjectEvent onPartialTextResponse;

	[SerializeField]
	private ComposerResponseDataEvent onPartialComposerResponse;

	[TooltipBox("Events for receipt of full transcriptions")]
	[SerializeField]
	private StringEvent onFullText;

	[SerializeField]
	private WitObjectEvent onFullTextResponse;

	[SerializeField]
	private ComposerResponseDataEvent onFullComposerResponse;

	public void SpeakPhrase(ComposerSessionData sessionData)
	{
		ComposerResponseData responseData = sessionData.responseData;
		string responsePhrase = responseData.responsePhrase;
		if (!responseData.responseIsFinal)
		{
			onPartialComposerResponse?.Invoke(responseData);
			onPartialTextResponse?.Invoke(HandleResponse(sessionData, responsePhrase));
			onPartialText.Invoke(responsePhrase);
		}
		else
		{
			onFullComposerResponse?.Invoke(responseData);
			onFullTextResponse?.Invoke(HandleResponse(sessionData, responsePhrase));
			onFullText.Invoke(responsePhrase);
		}
	}

	private WitResponseClass HandleResponse(ComposerSessionData sessionData, string text)
	{
		WitResponseClass witResponseClass = sessionData.responseData.witResponse?.AsObject;
		if (null == witResponseClass)
		{
			witResponseClass = new WitResponseClass();
			witResponseClass["q"] = text;
		}
		return witResponseClass;
	}

	public bool IsSpeaking(ComposerSessionData sessionData)
	{
		return false;
	}
}
