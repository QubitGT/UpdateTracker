using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meta.WitAi.Attributes;
using Meta.WitAi.Json;
using Meta.WitAi.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.CallbackHandlers;

[AddComponentMenu("Wit.ai/Response Matchers/Response Matcher")]
public class WitResponseMatcher : WitIntentMatcher
{
	[FormerlySerializedAs("valuePaths")]
	[Header("Value Matching")]
	[SerializeField]
	public ValuePathMatcher[] valueMatchers;

	[Header("Output")]
	[SerializeField]
	private FormattedValueEvents[] formattedValueEvents;

	[SerializeField]
	private MultiValueEvent onMultiValueEvent = new MultiValueEvent();

	[TooltipBox("Triggered if the matching conditions did not match. The parameter will be the transcription that was received. This will only trigger if there were values for intents or entities, but those values didn't match this matcher.")]
	[SerializeField]
	private StringEvent onDidNotMatch = new StringEvent();

	[TooltipBox("Triggered if a request was checked and no intents were found. This will still trigger if entities match and only applies to intents. The parameter will be the transcription.")]
	[SerializeField]
	private StringEvent onOutOfDomain = new StringEvent();

	private static Regex valueRegex = new Regex(Regex.Escape("{value}"), RegexOptions.Compiled);

	protected override string OnValidateResponse(WitResponseNode response, bool isEarlyResponse)
	{
		string text = base.OnValidateResponse(response, isEarlyResponse);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (isEarlyResponse && !ValueMatches(response))
		{
			return "No value matches";
		}
		return string.Empty;
	}

	protected override void OnResponseInvalid(WitResponseNode response, string error)
	{
		if (response.GetIntents().Length != 0 || response.EntityCount() > 0)
		{
			onDidNotMatch?.Invoke(response.GetTranscription());
		}
		if (response.GetIntents().Length == 0)
		{
			onOutOfDomain?.Invoke(response.GetTranscription());
		}
	}

	protected override void OnResponseSuccess(WitResponseNode response)
	{
		if (ValueMatches(response))
		{
			for (int i = 0; i < this.formattedValueEvents.Length; i++)
			{
				FormattedValueEvents formattedValueEvents = this.formattedValueEvents[i];
				string text = formattedValueEvents.format;
				for (int j = 0; j < valueMatchers.Length; j++)
				{
					string stringValue = valueMatchers[j].Reference.GetStringValue(response);
					if (!string.IsNullOrEmpty(formattedValueEvents.format))
					{
						if (!string.IsNullOrEmpty(stringValue))
						{
							text = valueRegex.Replace(text, stringValue, 1);
							text = text.Replace("{" + j + "}", stringValue);
						}
						else if (text.Contains("{" + j + "}"))
						{
							text = "";
							break;
						}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					formattedValueEvents.onFormattedValueEvent?.Invoke(text);
				}
			}
		}
		else
		{
			onDidNotMatch?.Invoke(response.GetTranscription());
		}
		List<string> list = new List<string>();
		ValuePathMatcher[] array = valueMatchers;
		foreach (ValuePathMatcher valuePathMatcher in array)
		{
			string stringValue2 = valuePathMatcher.Reference.GetStringValue(response);
			list.Add(stringValue2);
			if (valuePathMatcher.ConfidenceReference != null)
			{
				WitResponseHandler.RefreshConfidenceRange(ValueMatches(response, valuePathMatcher) ? valuePathMatcher.ConfidenceReference.GetFloatValue(response) : 0f, valuePathMatcher.confidenceRanges, valuePathMatcher.allowConfidenceOverlap);
			}
		}
		onMultiValueEvent.Invoke(list.ToArray());
	}

	private bool ValueMatches(WitResponseNode response)
	{
		bool flag = true;
		for (int i = 0; i < valueMatchers.Length && flag; i++)
		{
			flag &= ValueMatches(response, valueMatchers[i]);
		}
		return flag;
	}

	private bool ValueMatches(WitResponseNode response, ValuePathMatcher matcher)
	{
		string stringValue = matcher.Reference.GetStringValue(response);
		bool flag = !matcher.contentRequired || !string.IsNullOrEmpty(stringValue);
		switch (matcher.matchMethod)
		{
		case MatchMethod.RegularExpression:
			flag &= Regex.Match(stringValue, matcher.matchValue).Success;
			break;
		case MatchMethod.Text:
			flag &= stringValue == matcher.matchValue;
			break;
		case MatchMethod.IntegerComparison:
			flag &= CompareInt(stringValue, matcher);
			break;
		case MatchMethod.FloatComparison:
			flag &= CompareFloat(stringValue, matcher);
			break;
		case MatchMethod.DoubleComparison:
			flag &= CompareDouble(stringValue, matcher);
			break;
		}
		return flag;
	}

	private bool CompareDouble(string value, ValuePathMatcher matcher)
	{
		if (!double.TryParse(value, out var result))
		{
			return false;
		}
		double num = double.Parse(matcher.matchValue);
		return matcher.comparisonMethod switch
		{
			ComparisonMethod.Equals => Math.Abs(result - num) < matcher.floatingPointComparisonTolerance, 
			ComparisonMethod.NotEquals => Math.Abs(result - num) > matcher.floatingPointComparisonTolerance, 
			ComparisonMethod.Greater => result > num, 
			ComparisonMethod.Less => result < num, 
			ComparisonMethod.GreaterThanOrEqualTo => result >= num, 
			ComparisonMethod.LessThanOrEqualTo => result <= num, 
			_ => false, 
		};
	}

	private bool CompareFloat(string value, ValuePathMatcher matcher)
	{
		if (!float.TryParse(value, out var result))
		{
			return false;
		}
		float num = float.Parse(matcher.matchValue);
		return matcher.comparisonMethod switch
		{
			ComparisonMethod.Equals => (double)Math.Abs(result - num) < matcher.floatingPointComparisonTolerance, 
			ComparisonMethod.NotEquals => (double)Math.Abs(result - num) > matcher.floatingPointComparisonTolerance, 
			ComparisonMethod.Greater => result > num, 
			ComparisonMethod.Less => result < num, 
			ComparisonMethod.GreaterThanOrEqualTo => result >= num, 
			ComparisonMethod.LessThanOrEqualTo => result <= num, 
			_ => false, 
		};
	}

	private bool CompareInt(string value, ValuePathMatcher matcher)
	{
		if (!int.TryParse(value, out var result))
		{
			return false;
		}
		int num = int.Parse(matcher.matchValue);
		return matcher.comparisonMethod switch
		{
			ComparisonMethod.Equals => result == num, 
			ComparisonMethod.NotEquals => result != num, 
			ComparisonMethod.Greater => result > num, 
			ComparisonMethod.Less => result < num, 
			ComparisonMethod.GreaterThanOrEqualTo => result >= num, 
			ComparisonMethod.LessThanOrEqualTo => result <= num, 
			_ => false, 
		};
	}
}
