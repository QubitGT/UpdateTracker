using System;
using Meta.WitAi.Data.Intents;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.CallbackHandlers;

[AddComponentMenu("Wit.ai/Response Matchers/Simple Intent Handler")]
public class SimpleIntentHandler : WitIntentMatcher
{
	[SerializeField]
	private UnityEvent onIntentTriggered = new UnityEvent();

	[Tooltip("Confidence ranges are executed in order. If checked, all confidence values will be checked instead of stopping on the first one that matches.")]
	[SerializeField]
	public bool allowConfidenceOverlap;

	[SerializeField]
	public ConfidenceRange[] confidenceRanges;

	public UnityEvent OnIntentTriggered => onIntentTriggered;

	protected override void OnResponseSuccess(WitResponseNode response)
	{
		onIntentTriggered.Invoke();
		UpdateRanges(response);
	}

	protected override void OnResponseInvalid(WitResponseNode response, string error)
	{
		UpdateRanges(response);
	}

	private void UpdateRanges(WitResponseNode response)
	{
		WitIntentData[] array = response?.GetIntents();
		if (array == null)
		{
			return;
		}
		WitIntentData[] array2 = array;
		foreach (WitIntentData witIntentData in array2)
		{
			if (string.Equals(intent, witIntentData.name, StringComparison.CurrentCultureIgnoreCase))
			{
				WitResponseHandler.RefreshConfidenceRange(witIntentData.confidence, confidenceRanges, allowConfidenceOverlap);
				return;
			}
		}
		WitResponseHandler.RefreshConfidenceRange(0f, confidenceRanges, allowConfidenceOverlap);
	}
}
