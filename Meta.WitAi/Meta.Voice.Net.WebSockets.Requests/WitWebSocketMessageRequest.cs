using System;
using System.Collections.Generic;
using Meta.WitAi;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketMessageRequest : WitWebSocketJsonRequest
{
	public string Endpoint { get; }

	public bool EndWithFullTranscription { get; }

	public event Action<WitResponseNode> OnDecodedResponse;

	public WitWebSocketMessageRequest(WitResponseNode externalPostData, string requestId, string clientUserId, string operationId, bool endWithFullTranscription = false)
		: base(externalPostData, requestId, clientUserId, operationId)
	{
		Endpoint = "external";
		EndWithFullTranscription = endWithFullTranscription;
		SetResponseData(externalPostData);
		WaitForTimeout().WrapErrors();
	}

	public WitWebSocketMessageRequest(string endpoint, Dictionary<string, string> parameters, string requestId = null, string clientUserId = null, string operationId = null, bool endWithFullTranscription = false)
		: base(GetPostData(endpoint, parameters), requestId, clientUserId, operationId)
	{
		Endpoint = endpoint;
		EndWithFullTranscription = endWithFullTranscription;
	}

	public override string ToString()
	{
		return base.ToString() + "\nEndpoint: " + Endpoint;
	}

	public static WitResponseClass GetPostData(string endpoint, Dictionary<string, string> parameters)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		WitResponseClass witResponseClass2 = new WitResponseClass();
		WitResponseClass witResponseClass3 = new WitResponseClass();
		if (parameters != null)
		{
			foreach (string key in parameters.Keys)
			{
				if (string.Equals(key, "tag"))
				{
					continue;
				}
				string text = parameters[key];
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (string.Equals(key, "context"))
				{
					witResponseClass[key] = JsonConvert.DeserializeToken(text);
					continue;
				}
				if (text[0].Equals('['))
				{
					string text2 = text;
					if (text2[text2.Length - 1].Equals(']'))
					{
						text = text.Substring(1, text.Length - 2);
						WitResponseArray witResponseArray = new WitResponseArray();
						string[] array = text.Split(',');
						for (int i = 0; i < array.Length; i++)
						{
							witResponseArray[i] = new WitResponseData(array[i]);
						}
						witResponseClass3[key] = witResponseArray;
						continue;
					}
				}
				witResponseClass3[key] = new WitResponseData(text);
			}
		}
		witResponseClass2[endpoint] = witResponseClass3;
		witResponseClass["data"] = witResponseClass2;
		return witResponseClass;
	}

	public override void HandleDownload(string jsonString, WitResponseNode jsonData, byte[] binaryData)
	{
		if (!base.IsComplete)
		{
			if (!base.IsDownloading)
			{
				HandleDownloadBegin();
			}
			ReturnRawResponse(jsonString);
			SetResponseData(jsonData);
			if (!string.IsNullOrEmpty(base.Error))
			{
				HandleComplete();
			}
			else if (IsEndOfStream(base.ResponseData))
			{
				HandleComplete();
			}
		}
	}

	protected virtual bool IsEndOfStream(WitResponseNode responseData)
	{
		if ((object)responseData != null && responseData["is_final"].AsBool)
		{
			return true;
		}
		if (EndWithFullTranscription)
		{
			return responseData?["end_transcription"].AsBool ?? false;
		}
		return false;
	}

	protected override void SetResponseData(WitResponseNode newResponseData)
	{
		base.SetResponseData(newResponseData);
		this.OnDecodedResponse?.Invoke(base.ResponseData);
	}
}
