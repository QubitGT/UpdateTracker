using System;
using Meta.Voice;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

[Serializable]
public class VoiceServiceRequestEvents : NLPRequestEvents<VoiceServiceRequestEvent, WitResponseNode>
{
	public void AddListeners(VoiceServiceRequestEvents events)
	{
		SetListeners(events, add: true);
	}

	public void RemoveListeners(VoiceServiceRequestEvents events)
	{
		SetListeners(events, add: false);
	}

	public void SetListeners(VoiceServiceRequestEvents events, bool add)
	{
		base.OnInit.SetListener(events.OnInit.Invoke, add);
		base.OnStateChange.SetListener(events.OnStateChange.Invoke, add);
		base.OnAudioInputStateChange.SetListener(events.OnAudioInputStateChange.Invoke, add);
		base.OnStartListening.SetListener(events.OnStartListening.Invoke, add);
		base.OnStopListening.SetListener(events.OnStopListening.Invoke, add);
		base.OnAudioActivation.SetListener(events.OnAudioActivation.Invoke, add);
		base.OnAudioDeactivation.SetListener(events.OnAudioDeactivation.Invoke, add);
		base.OnSend.SetListener(events.OnSend.Invoke, add);
		base.OnRawResponse.SetListener(events.OnRawResponse.Invoke, add);
		base.OnPartialTranscription.SetListener(events.OnPartialTranscription.Invoke, add);
		base.OnFullTranscription.SetListener(events.OnFullTranscription.Invoke, add);
		base.OnPartialResponse.SetListener(events.OnPartialResponse.Invoke, add);
		base.OnFullResponse.SetListener(events.OnFullResponse.Invoke, add);
		base.OnDownloadProgressChange.SetListener(events.OnDownloadProgressChange.Invoke, add);
		base.OnUploadProgressChange.SetListener(events.OnUploadProgressChange.Invoke, add);
		base.OnCancel.SetListener(events.OnCancel.Invoke, add);
		base.OnFailed.SetListener(events.OnFailed.Invoke, add);
		base.OnSuccess.SetListener(events.OnSuccess.Invoke, add);
		base.OnComplete.SetListener(events.OnComplete.Invoke, add);
	}
}
