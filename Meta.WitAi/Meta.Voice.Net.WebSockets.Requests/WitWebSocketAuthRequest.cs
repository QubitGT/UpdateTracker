using System.Collections.Generic;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketAuthRequest : WitWebSocketJsonRequest
{
	public WitWebSocketAuthRequest(string clientAccessToken, string versionTag, Dictionary<string, string> parameters)
		: base(GetAuthNode(clientAccessToken, versionTag, parameters))
	{
	}

	private static WitResponseNode GetAuthNode(string clientAccessToken, string versionTag, Dictionary<string, string> parameters)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		witResponseClass["wit_auth_token"] = new WitResponseData(clientAccessToken);
		witResponseClass["api_version"] = new WitResponseData("20250213");
		if (!string.IsNullOrEmpty(versionTag))
		{
			witResponseClass["tag"] = new WitResponseData(versionTag);
		}
		if (parameters != null)
		{
			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				witResponseClass[parameter.Key] = new WitResponseData(parameter.Value);
			}
		}
		return witResponseClass;
	}

	protected override void SetResponseData(WitResponseNode newResponseData)
	{
		base.SetResponseData(newResponseData);
		if (!string.Equals(newResponseData["success"], "true"))
		{
			base.Error = "Authentication denied";
		}
	}
}
