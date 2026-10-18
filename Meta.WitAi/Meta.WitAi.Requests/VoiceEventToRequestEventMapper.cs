using Meta.WitAi.Events;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

public class VoiceEventToRequestEventMapper : VoiceServiceRequestEventsWrapper
{
	private VoiceEvents _voiceEvents;

	public VoiceEvents VoiceEvents
	{
		get
		{
			return _voiceEvents;
		}
		set
		{
			_voiceEvents = value;
		}
	}

	public VoiceEventToRequestEventMapper()
	{
	}

	public VoiceEventToRequestEventMapper(VoiceEvents voiceEvents)
	{
		_voiceEvents = voiceEvents;
	}

	protected override void OnStateChange(VoiceServiceRequest request)
	{
	}

	protected override void OnStopListening(VoiceServiceRequest request)
	{
		_voiceEvents.OnStoppedListening.Invoke();
	}

	protected override void OnStartListening(VoiceServiceRequest request)
	{
		_voiceEvents.OnStartListening.Invoke();
	}

	protected override void OnFullTranscription(string transcription)
	{
		_voiceEvents.OnFullTranscription.Invoke(transcription);
	}

	protected override void OnPartialTranscription(string transcription)
	{
		_voiceEvents.OnPartialTranscription.Invoke(transcription);
	}

	protected override void OnPartialResponse(WitResponseNode response)
	{
		_voiceEvents.OnPartialResponse.Invoke(response);
	}

	protected override void OnFullResponse(WitResponseNode response)
	{
		_voiceEvents.OnResponse.Invoke(response);
	}

	protected override void OnSuccess(VoiceServiceRequest request)
	{
	}

	protected override void OnSend(VoiceServiceRequest request)
	{
	}

	protected override void OnInit(VoiceServiceRequest request)
	{
	}

	protected override void OnFailed(VoiceServiceRequest request)
	{
		_voiceEvents.OnError.Invoke(request.Results.Message, "Error: " + request.Results.StatusCode);
	}

	protected override void OnComplete(VoiceServiceRequest request)
	{
		_voiceEvents.OnComplete.Invoke(request);
	}

	protected override void OnCancel(VoiceServiceRequest request)
	{
		_voiceEvents.OnCanceled.Invoke(request.Results?.Message ?? "");
	}
}
