using System.Collections.Generic;
using System.Text;
using Meta.Voice;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;

namespace Meta.WitAi.Composer.Integrations;

public class WitComposerRequestHandler : IComposerRequestHandler
{
	private readonly IWitRequestConfiguration _configuration;

	public WitComposerRequestHandler(IWitRequestConfiguration configuration)
	{
		_configuration = configuration;
	}

	public void OnComposerRequestSetup(ComposerSessionData sessionData, VoiceServiceRequest request)
	{
		if (request == null || sessionData.composer == null || sessionData.composer.VoiceService == null)
		{
			return;
		}
		request.Options.QueryParams["session_id"] = sessionData.sessionID;
		if (sessionData.composer.VoiceService.UsePlatformIntegrations)
		{
			request.Options.QueryParams["useComposer"] = "True";
		}
		bool flag = false;
		string text = null;
		if (request.InputType == NLPRequestInputType.Text)
		{
			text = request.Options.Text;
			bool num = IsEventJson(text);
			flag = string.IsNullOrEmpty(text);
			if (!num || flag)
			{
				if (request is WitSocketRequest)
				{
					request.Options.QueryParams["message"] = text;
					request.Options.QueryParams["type"] = WitComposerMessageType.Message.ToString().ToLower();
				}
				else
				{
					text = JsonConvert.SerializeObject(new Dictionary<string, string>
					{
						["message"] = text,
						["type"] = WitComposerMessageType.Message.ToString().ToLower()
					});
				}
			}
			request.Options.Text = text;
			if (request.Options.QueryParams.ContainsKey("q"))
			{
				request.Options.QueryParams.Remove("q");
			}
		}
		ComposerContextMap contextMap = sessionData.contextMap;
		contextMap?.SetData(sessionData.composer.contextMapEventKey, flag.ToString().ToLower());
		bool flag2 = contextMap?.Data["debug"].AsBool ?? false;
		request.Options.QueryParams["context_map"] = contextMap?.GetJson();
		request.Options.QueryParams["debug"] = (flag2 ? "true" : "false");
		bool flag3 = contextMap?.Data[WitComposerConstants.PRELOAD].AsBool ?? false;
		request.Options.QueryParams[WitComposerConstants.PRELOAD] = (flag3 ? "true" : "false");
		if (request is WitRequest witRequest)
		{
			witRequest.Path = GetEndpointPath(request.InputType);
			if (request.InputType == NLPRequestInputType.Text)
			{
				witRequest.postContentType = "application/json";
				witRequest.postData = Encoding.UTF8.GetBytes(text);
			}
		}
		else if (request is WitUnityRequest witUnityRequest)
		{
			witUnityRequest.Endpoint = GetEndpointPath(request.InputType);
			witUnityRequest.ShouldPost = true;
		}
		else if (request is WitSocketRequest witSocketRequest)
		{
			witSocketRequest.Endpoint = GetEndpointPath(request.InputType);
		}
	}

	public bool IsEventJson(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			return true;
		}
		WitResponseNode witResponseNode = JsonConvert.DeserializeToken(json);
		if (witResponseNode != null)
		{
			bool flag = false;
			WitResponseClass asObject = witResponseNode.AsObject;
			if (asObject == null || !asObject.HasChild("type"))
			{
				flag = true;
			}
			if (!flag)
			{
				return true;
			}
		}
		return false;
	}

	private string GetEndpointPath(NLPRequestInputType inputType)
	{
		switch (inputType)
		{
		case NLPRequestInputType.Audio:
			if (_configuration != null)
			{
				return _configuration.GetEndpointInfo().Converse;
			}
			return "converse";
		case NLPRequestInputType.Text:
			if (_configuration != null)
			{
				return _configuration.GetEndpointInfo().Event;
			}
			return "event";
		default:
			VLog.E($"Unsupported input type: {inputType}");
			return null;
		}
	}
}
