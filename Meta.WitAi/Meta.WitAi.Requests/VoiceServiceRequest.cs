using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

[Serializable]
public abstract class VoiceServiceRequest : NLPRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults, WitResponseNode>
{
	private static WitResponseDecoder _responseDecoder = new WitResponseDecoder();

	public bool IsLocalRequest => string.Equals(base.Options.ClientUserId, WitRequestSettings.LocalClientUserId);

	public int StatusCode => base.Results.StatusCode;

	protected override INLPRequestResponseDecoder<WitResponseNode> ResponseDecoder => _responseDecoder;

	protected VoiceServiceRequest(NLPRequestInputType newInputType, WitRequestOptions newOptions, VoiceServiceRequestEvents newEvents)
		: base(newInputType, newOptions, newEvents)
	{
	}

	protected override bool ShouldIgnoreError(int errorStatusCode, string errorMessage)
	{
		if (base.ShouldIgnoreError(errorStatusCode, errorMessage))
		{
			return true;
		}
		if (string.Equals(errorMessage, "Empty transcription."))
		{
			return true;
		}
		return false;
	}

	protected override bool OnSimulateResponse()
	{
		if (VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse == null)
		{
			return false;
		}
		SimulateResponse();
		return true;
	}

	private async void SimulateResponse()
	{
		new StackTrace();
		_ = VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.responseDescription;
		for (int i = 0; i < VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.messages.Count - 1; i++)
		{
			SimulatedResponseMessage message = VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.messages[i];
			await Task.Delay((int)(message.delay * 1000f));
			WitResponseNode witResponseNode = WitResponseNode.Parse(message.responseBody);
			witResponseNode["code"] = new WitResponseData(VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.code);
			ApplyResponseData(witResponseNode, isFinal: false);
		}
		SimulatedResponseMessage lastMessage = VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.messages.Last();
		await Task.Delay((int)(lastMessage.delay * 1000f));
		WitResponseNode witResponseNode2 = WitResponseNode.Parse(lastMessage.responseBody);
		witResponseNode2["code"] = new WitResponseData(VoiceRequest<VoiceServiceRequestEvent, WitRequestOptions, VoiceServiceRequestEvents, VoiceServiceRequestResults>.simulatedResponse.code);
		ApplyResponseData(witResponseNode2, isFinal: true);
	}

	internal virtual void SimulateError(VoiceErrorSimulationType errorType)
	{
		throw new NotImplementedException();
	}

	protected override void ApplyResponseData(WitResponseNode responseData, bool isFinal)
	{
		if (responseData != null)
		{
			responseData["client_request_id"] = base.Options?.RequestId;
			responseData["client_user_id"] = base.Options?.ClientUserId;
			responseData["operation_id"] = base.Options?.OperationId;
		}
		base.ApplyResponseData(responseData, isFinal);
	}

	protected override void SetEventListeners(VoiceServiceRequestEvents newEvents, bool add)
	{
		base.Events.SetListeners(newEvents, add);
	}

	protected override void RaiseEvent(VoiceServiceRequestEvent eventCallback)
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			eventCallback?.Invoke(this);
		});
	}
}
