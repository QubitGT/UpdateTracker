using System;
using Meta.Conduit;
using Meta.WitAi.Data.Intents;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.CallbackHandlers;

public abstract class WitIntentMatcher : WitResponseHandler
{
	[Header("Intent Settings")]
	[SerializeField]
	public string intent;

	[FormerlySerializedAs("confidence")]
	[Range(0f, 1f)]
	[SerializeField]
	public float confidenceThreshold = 0.6f;

	protected override string OnValidateResponse(WitResponseNode response, bool isEarlyResponse)
	{
		if (response == null)
		{
			return "No response";
		}
		WitIntentData[] intents = response.GetIntents();
		if (intents == null || intents.Length == 0)
		{
			return "No intents found";
		}
		WitIntentData witIntentData = null;
		WitIntentData[] array = intents;
		foreach (WitIntentData witIntentData2 in array)
		{
			if (string.Equals(intent, witIntentData2.name, StringComparison.CurrentCultureIgnoreCase))
			{
				witIntentData = witIntentData2;
				break;
			}
		}
		if (witIntentData == null)
		{
			return "Missing required intent '" + intent + "'";
		}
		if (witIntentData.confidence < confidenceThreshold)
		{
			return $"Required intent '{intent}' confidence too low: {witIntentData.confidence:0.000}\nRequired: {confidenceThreshold:0.000}";
		}
		return string.Empty;
	}

	protected override void OnEnable()
	{
		Manifest.WitResponseMatcherIntents.Add(intent);
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		Manifest.WitResponseMatcherIntents.Remove(intent);
		base.OnDisable();
	}
}
