using Meta.Voice;
using Meta.WitAi.Configuration;
using Meta.WitAi.Requests;

namespace Oculus.Voice.Bindings.Android;

public class VoiceSDKImplRequest : VoiceServiceRequest
{
	public VoiceSDKBinding Service { get; private set; }

	public bool Immediately { get; private set; }

	protected override bool DecodeRawResponses => true;

	public VoiceSDKImplRequest(VoiceSDKBinding newService, NLPRequestInputType newInputType, bool newImmediately, WitRequestOptions newOptions, VoiceServiceRequestEvents newEvents)
		: base(newInputType, newOptions, newEvents)
	{
		Service = newService;
		Immediately = newImmediately;
	}

	protected override void HandleAudioActivation()
	{
		if (Immediately)
		{
			Service.ActivateImmediately(base.Options);
		}
		else
		{
			Service.Activate(base.Options);
		}
		SetAudioInputState(VoiceAudioInputState.On);
	}

	protected override void HandleAudioDeactivation()
	{
		Service.Deactivate(base.Options.RequestId);
		SetAudioInputState(VoiceAudioInputState.Off);
	}

	protected override void HandleSend()
	{
		if (base.InputType == NLPRequestInputType.Text)
		{
			Service.Activate(base.Options.Text, base.Options);
		}
	}

	protected override void HandleCancel()
	{
		Service.DeactivateAndAbortRequest(base.Options.RequestId);
	}

	public void HandlePartialResponse(string responseJson)
	{
		HandleRawResponse(responseJson, final: false);
	}

	public void HandlePartialTranscription(string transcription)
	{
		ApplyTranscription(transcription, full: false);
	}

	public void HandleFullTranscription(string transcription)
	{
		ApplyTranscription(transcription, full: true);
	}

	public void HandleTransmissionBegan()
	{
		if (base.InputType == NLPRequestInputType.Audio)
		{
			Send();
		}
	}

	public void HandleCanceled()
	{
		HandleCancel();
	}

	public void HandleError(string error, string message, string errorBody)
	{
		HandleFailure(error + " - " + message);
	}

	public void HandleResponse(string responseJson)
	{
		HandleRawResponse(responseJson, final: true);
	}
}
