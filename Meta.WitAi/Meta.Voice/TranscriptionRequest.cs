using Meta.Voice.Logging;
using Meta.WitAi;
using UnityEngine.Events;

namespace Meta.Voice;

public abstract class TranscriptionRequest<TUnityEvent, TOptions, TEvents, TResults> : VoiceRequest<TUnityEvent, TOptions, TEvents, TResults> where TUnityEvent : UnityEventBase where TOptions : ITranscriptionRequestOptions where TEvents : TranscriptionRequestEvents<TUnityEvent> where TResults : ITranscriptionRequestResults
{
	public VoiceAudioInputState AudioInputState { get; private set; }

	public bool IsAudioInputActivated
	{
		get
		{
			if (AudioInputState != VoiceAudioInputState.Activating)
			{
				return AudioInputState == VoiceAudioInputState.On;
			}
			return true;
		}
	}

	public bool IsListening => AudioInputState == VoiceAudioInputState.On;

	public bool CanActivateAudio => string.IsNullOrEmpty(GetActivateAudioError());

	public bool CanDeactivateAudio => IsAudioInputActivated;

	public string Transcription
	{
		get
		{
			TResults results = base.Results;
			if (results == null)
			{
				return null;
			}
			return results.Transcription;
		}
	}

	public string[] FinalTranscriptions
	{
		get
		{
			TResults results = base.Results;
			if (results == null)
			{
				return null;
			}
			return results.FinalTranscriptions;
		}
	}

	protected TranscriptionRequest(TOptions newOptions, TEvents newEvents)
		: base(newOptions, newEvents)
	{
	}

	protected virtual void SetAudioInputState(VoiceAudioInputState newAudioInputState)
	{
		if (AudioInputState != newAudioInputState)
		{
			AudioInputState = newAudioInputState;
			TEvents events = base.Events;
			RaiseEvent((events != null) ? events.OnAudioInputStateChange : null);
			switch (AudioInputState)
			{
			case VoiceAudioInputState.Activating:
				WaitForHold(OnCanActivate);
				break;
			case VoiceAudioInputState.On:
				OnStartListening();
				break;
			case VoiceAudioInputState.Deactivating:
				OnAudioDeactivation();
				HandleAudioDeactivation();
				break;
			case VoiceAudioInputState.Off:
				OnStopListening();
				break;
			}
		}
	}

	protected virtual void OnCanActivate()
	{
		if (AudioInputState == VoiceAudioInputState.Activating)
		{
			OnAudioActivation();
			HandleAudioActivation();
		}
	}

	protected override void Log(string log, VLoggerVerbosity logLevel = VLoggerVerbosity.Info)
	{
		IVLogger logger = Logger;
		CorrelationID correlationID = Logger.CorrelationID;
		object[] obj = new object[5] { log, null, null, null, null };
		TOptions options = base.Options;
		obj[1] = ((options != null) ? options.RequestId : null);
		obj[2] = base.State;
		obj[3] = AudioInputState;
		TResults results = base.Results;
		obj[4] = ((results != null) ? results.Transcription : null);
		logger.Log(correlationID, logLevel, "{0}\nRequest Id: {1}\nRequest State: {2}\nAudio Input State: {3}\nTranscription: {4}", obj);
	}

	protected virtual void ApplyTranscription(string transcription, bool full)
	{
		base.Results.SetTranscription(transcription, full);
		if (!full)
		{
			OnPartialTranscription();
		}
		else
		{
			OnFullTranscription();
		}
	}

	protected virtual void OnPartialTranscription()
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			base.Events?.OnPartialTranscription?.Invoke(Transcription);
			base.Events?.OnUserPartialTranscription?.Invoke(base.Options.ClientUserId, Transcription);
		});
	}

	protected virtual void OnFullTranscription()
	{
		ThreadUtility.CallOnMainThread(delegate
		{
			base.Events?.OnFullTranscription?.Invoke(Transcription);
			base.Events?.OnUserFullTranscription?.Invoke(base.Options.ClientUserId, Transcription);
		});
	}

	protected abstract string GetActivateAudioError();

	public virtual void ActivateAudio()
	{
		if (IsAudioInputActivated)
		{
			LogW("Activate Audio Ignored\nReason: Already activated");
			return;
		}
		string activateAudioError = GetActivateAudioError();
		if (!string.IsNullOrEmpty(activateAudioError))
		{
			LogW("Activate Audio Failed\nReason: " + activateAudioError);
			HandleFailure(activateAudioError);
		}
		else
		{
			SetAudioInputState(VoiceAudioInputState.Activating);
		}
	}

	protected virtual void OnAudioActivation()
	{
		Log("Activate Audio Begin");
		TEvents events = base.Events;
		RaiseEvent((events != null) ? events.OnAudioActivation : null);
	}

	protected abstract void HandleAudioActivation();

	protected virtual void OnStartListening()
	{
		Log("Activate Audio Complete");
		TEvents events = base.Events;
		RaiseEvent((events != null) ? events.OnStartListening : null);
	}

	public virtual void DeactivateAudio()
	{
		if (!IsAudioInputActivated)
		{
			LogW("Deactivate Audio Ignored\nReason: Not currently activated");
		}
		else
		{
			SetAudioInputState(VoiceAudioInputState.Deactivating);
		}
	}

	protected virtual void OnAudioDeactivation()
	{
		Log("Deactivate Audio Begin");
		TEvents events = base.Events;
		RaiseEvent((events != null) ? events.OnAudioDeactivation : null);
	}

	protected abstract void HandleAudioDeactivation();

	protected virtual bool HasSentAudio()
	{
		return true;
	}

	protected virtual void OnStopListening()
	{
		Log("Deactivate Audio Complete");
		TEvents events = base.Events;
		RaiseEvent((events != null) ? events.OnStopListening : null);
		if (base.State == VoiceRequestState.Initialized)
		{
			Cancel("Request cancelled prior to transmission begin");
		}
		else if (base.State == VoiceRequestState.Transmitting && !HasSentAudio())
		{
			Cancel("Request cancelled prior to audio transmission");
		}
	}

	public override void Send()
	{
		if (!IsAudioInputActivated && CanActivateAudio)
		{
			ActivateAudio();
		}
		base.Send();
	}

	public override void Cancel(string reason = "Request was cancelled.")
	{
		if (IsAudioInputActivated)
		{
			DeactivateAudio();
		}
		base.Cancel(reason);
	}
}
