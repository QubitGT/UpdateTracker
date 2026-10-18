using System.Text.RegularExpressions;
using Meta.WitAi.Json;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.CallbackHandlers;

[AddComponentMenu("Wit.ai/Response Matchers/Utterance Matcher")]
public class WitUtteranceMatcher : WitResponseHandler
{
	[SerializeField]
	private string searchText;

	[SerializeField]
	private bool exactMatch = true;

	[SerializeField]
	private bool useRegex;

	[SerializeField]
	private StringEvent onUtteranceMatched = new StringEvent();

	private Regex regex;

	protected override string OnValidateResponse(WitResponseNode response, bool isEarlyResponse)
	{
		string value = response["text"].Value;
		if (!IsMatch(value))
		{
			return "Required utterance does not match";
		}
		return "";
	}

	protected override void OnResponseInvalid(WitResponseNode response, string error)
	{
	}

	protected override void OnResponseSuccess(WitResponseNode response)
	{
		string value = response["text"].Value;
		onUtteranceMatched?.Invoke(value);
	}

	private bool IsMatch(string text)
	{
		if (useRegex)
		{
			if (regex == null)
			{
				regex = new Regex(searchText, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			Match match = regex.Match(text);
			if (match.Success)
			{
				if (exactMatch)
				{
					_ = match.Value == text;
					return true;
				}
				return true;
			}
		}
		else
		{
			if (exactMatch && text.ToLower() == searchText.ToLower())
			{
				return true;
			}
			if (text.ToLower().Contains(searchText.ToLower()))
			{
				return true;
			}
		}
		return false;
	}
}
