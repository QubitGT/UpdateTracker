using Meta.Voice;
using Meta.WitAi.Configuration;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

public class VoiceServiceMockRequest : VoiceServiceRequest
{
	protected override bool DecodeRawResponses => true;

	public VoiceServiceMockRequest(NLPRequestInputType newInputType, WitRequestOptions newOptions, VoiceServiceRequestEvents newEvents)
		: base(newInputType, newOptions, newEvents)
	{
	}

	protected override bool HasSentAudio()
	{
		return false;
	}

	protected override void HandleAudioActivation()
	{
		SetAudioInputState(VoiceAudioInputState.On);
	}

	protected override void HandleAudioDeactivation()
	{
		SetAudioInputState(VoiceAudioInputState.Off);
	}

	protected override void HandleSend()
	{
	}

	protected override void HandleCancel()
	{
	}

	public void SetRawResponse(string jsonText, bool final = false)
	{
		if (base.State != VoiceRequestState.Transmitting)
		{
			VLog.W("Cannot apply a raw response unless transmitting");
		}
		else
		{
			HandleRawResponse(jsonText, final);
		}
	}

	public void SetTranscription(string newTranscription, bool full = false)
	{
		if (base.State != VoiceRequestState.Transmitting)
		{
			VLog.W("Cannot set transcription unless transmitting");
		}
		else
		{
			ApplyTranscription(newTranscription, full);
		}
	}

	public void SetResponseData(WitResponseNode responseData, bool final = false)
	{
		if (base.State != VoiceRequestState.Transmitting)
		{
			VLog.W("Cannot set decoded response data unless transmitting");
		}
		else
		{
			ApplyResponseData(responseData, final);
		}
	}

	public void Fail(string error)
	{
		Fail(-1, error);
	}

	public void Fail(int statusCode, string error)
	{
		if (base.State != VoiceRequestState.Transmitting)
		{
			VLog.W("Cannot make a request fail unless transmitting");
		}
		else
		{
			HandleFailure(statusCode, error);
		}
	}
}
