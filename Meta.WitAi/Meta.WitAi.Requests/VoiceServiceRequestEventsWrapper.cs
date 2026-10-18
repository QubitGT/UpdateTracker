using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

public class VoiceServiceRequestEventsWrapper
{
	public void Wrap(VoiceServiceRequestEvents events)
	{
		SetListeners(events, add: true);
	}

	public void Unwrap(VoiceServiceRequestEvents events)
	{
		SetListeners(events, add: false);
	}

	private void SetListeners(VoiceServiceRequestEvents events, bool add)
	{
		events.OnInit.SetListener(OnInit, add);
		events.OnStateChange.SetListener(OnStateChange, add);
		events.OnAudioInputStateChange.SetListener(OnAudioInputStateChange, add);
		events.OnStartListening.SetListener(OnStartListening, add);
		events.OnStopListening.SetListener(OnStopListening, add);
		events.OnAudioActivation.SetListener(OnAudioActivation, add);
		events.OnAudioDeactivation.SetListener(OnAudioDeactivation, add);
		events.OnSend.SetListener(OnSend, add);
		events.OnRawResponse.SetListener(OnRawResponse, add);
		events.OnPartialTranscription.SetListener(OnPartialTranscription, add);
		events.OnFullTranscription.SetListener(OnFullTranscription, add);
		events.OnPartialResponse.SetListener(OnPartialResponse, add);
		events.OnFullResponse.SetListener(OnFullResponse, add);
		events.OnDownloadProgressChange.SetListener(OnDownloadProgressChange, add);
		events.OnUploadProgressChange.SetListener(OnUploadProgressChange, add);
		events.OnCancel.SetListener(OnCancel, add);
		events.OnFailed.SetListener(OnFailed, add);
		events.OnSuccess.SetListener(OnSuccess, add);
		events.OnComplete.SetListener(OnComplete, add);
	}

	protected virtual void OnAudioInputStateChange(VoiceServiceRequest request)
	{
	}

	protected virtual void OnUploadProgressChange(VoiceServiceRequest request)
	{
	}

	protected virtual void OnDownloadProgressChange(VoiceServiceRequest request)
	{
	}

	protected virtual void OnStateChange(VoiceServiceRequest request)
	{
	}

	protected virtual void OnStopListening(VoiceServiceRequest request)
	{
	}

	protected virtual void OnStartListening(VoiceServiceRequest request)
	{
	}

	protected virtual void OnFullTranscription(string transcription)
	{
	}

	protected virtual void OnPartialTranscription(string transcription)
	{
	}

	protected virtual void OnRawResponse(string rawResponse)
	{
	}

	protected virtual void OnPartialResponse(WitResponseNode request)
	{
	}

	protected virtual void OnFullResponse(WitResponseNode request)
	{
	}

	protected virtual void OnAudioDeactivation(VoiceServiceRequest request)
	{
	}

	protected virtual void OnAudioActivation(VoiceServiceRequest request)
	{
	}

	protected virtual void OnSuccess(VoiceServiceRequest request)
	{
	}

	protected virtual void OnSend(VoiceServiceRequest request)
	{
	}

	protected virtual void OnInit(VoiceServiceRequest request)
	{
	}

	protected virtual void OnFailed(VoiceServiceRequest request)
	{
	}

	protected virtual void OnComplete(VoiceServiceRequest request)
	{
	}

	protected virtual void OnCancel(VoiceServiceRequest request)
	{
	}
}
