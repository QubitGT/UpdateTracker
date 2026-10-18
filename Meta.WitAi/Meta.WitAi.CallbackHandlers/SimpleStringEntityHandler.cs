using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.WitAi.CallbackHandlers;

[AddComponentMenu("Wit.ai/Response Matchers/Simple String Entity Handler")]
public class SimpleStringEntityHandler : WitIntentMatcher
{
	[SerializeField]
	public string entity;

	[SerializeField]
	public string format;

	[SerializeField]
	private StringEntityMatchEvent onIntentEntityTriggered = new StringEntityMatchEvent();

	public StringEntityMatchEvent OnIntentEntityTriggered => onIntentEntityTriggered;

	protected override string OnValidateResponse(WitResponseNode response, bool isEarlyResponse)
	{
		string text = base.OnValidateResponse(response, isEarlyResponse);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (string.IsNullOrEmpty(response.GetFirstEntityValue(entity)))
		{
			return "Missing required entity: " + entity;
		}
		return string.Empty;
	}

	protected override void OnResponseInvalid(WitResponseNode response, string error)
	{
	}

	protected override void OnResponseSuccess(WitResponseNode response)
	{
		string firstEntityValue = response.GetFirstEntityValue(entity);
		if (!string.IsNullOrEmpty(format))
		{
			onIntentEntityTriggered.Invoke(format.Replace("{value}", firstEntityValue));
		}
		else
		{
			onIntentEntityTriggered.Invoke(firstEntityValue);
		}
	}
}
