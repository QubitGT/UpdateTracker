using System;
using System.Collections.Generic;
using Meta.Voice;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Meta.WitAi.Events;

[Serializable]
public class SpeechEvents : EventRegistry, ISpeechEvents, ITranscriptionEvent, IAudioInputEvents
{
	protected const string EVENT_CATEGORY_ACTIVATION_SETUP = "Activation Setup Events";

	[EventCategory("Activation Setup Events")]
	[Tooltip("Called prior to initialization for WitRequestOption customization")]
	[FormerlySerializedAs("OnRequestOptionSetup")]
	[SerializeField]
	private WitRequestOptionsEvent _onRequestOptionSetup = new WitRequestOptionsEvent();

	[EventCategory("Activation Setup Events")]
	[Tooltip("Called when a request is created.  This occurs as soon as a activation is called successfully.")]
	[FormerlySerializedAs("OnRequestInitialized")]
	[SerializeField]
	private VoiceServiceRequestEvent _onRequestInitialized = new VoiceServiceRequestEvent();

	public Action<VoiceServiceRequest> OnRequestFinalize;

	[EventCategory("Activation Setup Events")]
	[Tooltip("Called when a request is sent. This occurs immediately once data is being transmitted to the endpoint.")]
	[FormerlySerializedAs("OnRequestCreated")]
	[SerializeField]
	[HideInInspector]
	private WitRequestCreatedEvent _onRequestCreated = new WitRequestCreatedEvent();

	[EventCategory("Activation Setup Events")]
	[Tooltip("Called when a request is sent. This occurs immediately once data is being transmitted to the endpoint.")]
	[SerializeField]
	private VoiceServiceRequestEvent _onSend = new VoiceServiceRequestEvent();

	protected const string EVENT_CATEGORY_ACTIVATION_INFO = "Activation Info Events";

	[EventCategory("Activation Info Events")]
	[Tooltip("Fired when the minimum wake threshold is hit after an activation.  Not called for ActivateImmediately")]
	[FormerlySerializedAs("OnMinimumWakeThresholdHit")]
	[SerializeField]
	private UnityEvent _onMinimumWakeThresholdHit = new UnityEvent();

	[EventCategory("Activation Info Events")]
	[Tooltip("Fired when recording stops, the minimum volume threshold was hit, and data is being sent to the server.")]
	[FormerlySerializedAs("OnMicDataSent")]
	[SerializeField]
	private UnityEvent _onMicDataSent = new UnityEvent();

	[EventCategory("Activation Info Events")]
	[Tooltip("The Deactivate() method has been called ending the current activation.")]
	[FormerlySerializedAs("OnStoppedListeningDueToDeactivation")]
	[SerializeField]
	private UnityEvent _onStoppedListeningDueToDeactivation = new UnityEvent();

	[EventCategory("Activation Info Events")]
	[Tooltip("Called when the microphone input volume has been below the volume threshold for the specified duration and microphone data is no longer being collected")]
	[FormerlySerializedAs("OnStoppedListeningDueToInactivity")]
	[SerializeField]
	private UnityEvent _onStoppedListeningDueToInactivity = new UnityEvent();

	[EventCategory("Activation Info Events")]
	[Tooltip("The microphone has stopped recording because maximum recording time has been hit for this activation")]
	[FormerlySerializedAs("OnStoppedListeningDueToTimeout")]
	[SerializeField]
	private UnityEvent _onStoppedListeningDueToTimeout = new UnityEvent();

	protected const string EVENT_CATEGORY_ACTIVATION_CANCELATION = "Activation Cancelation Events";

	[EventCategory("Activation Cancelation Events")]
	[Tooltip("Called when the activation is about to be aborted by a direct user interaction via DeactivateAndAbort.")]
	[FormerlySerializedAs("OnAborting")]
	[SerializeField]
	private UnityEvent _onAborting = new UnityEvent();

	[EventCategory("Activation Cancelation Events")]
	[Tooltip("Called when the activation stopped because the network request was aborted. This can be via a timeout or call to DeactivateAndAbort.")]
	[FormerlySerializedAs("OnAborted")]
	[SerializeField]
	private UnityEvent _onAborted = new UnityEvent();

	[EventCategory("Activation Cancelation Events")]
	[Tooltip("Called when a request has been canceled either prior to or after a request has begun transmission.  Returns the cancelation reason.")]
	[FormerlySerializedAs("OnCanceled")]
	[SerializeField]
	private WitTranscriptionEvent _onCanceled = new WitTranscriptionEvent();

	protected const string EVENT_CATEGORY_ACTIVATION_RESPONSE = "Activation Response Events";

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when raw text response is returned from Wit.ai")]
	[FormerlySerializedAs("OnRawResponse")]
	[SerializeField]
	[HideInInspector]
	private WitTranscriptionEvent _onRawResponse = new WitTranscriptionEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when response from Wit.ai has been received from partial transcription")]
	[FormerlySerializedAs("OnPartialResponse")]
	[SerializeField]
	[HideInInspector]
	private WitResponseEvent _onPartialResponse = new WitResponseEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when a response from Wit.ai has been received")]
	[FormerlySerializedAs("OnResponse")]
	[FormerlySerializedAs("onResponse")]
	[SerializeField]
	private WitResponseEvent _onResponse = new WitResponseEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when there was an error with a WitRequest  or the RuntimeConfiguration is not properly configured.")]
	[FormerlySerializedAs("OnError")]
	[FormerlySerializedAs("onError")]
	[SerializeField]
	private WitErrorEvent _onError = new WitErrorEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when a request has completed and all response and error callbacks have fired.  This is not called if the request was aborted.")]
	[FormerlySerializedAs("OnRequestCompleted")]
	[SerializeField]
	private UnityEvent _onRequestCompleted = new UnityEvent();

	[EventCategory("Activation Response Events")]
	[Tooltip("Called when a request has been canceled, failed, or successfully completed")]
	[FormerlySerializedAs("OnComplete")]
	[SerializeField]
	private VoiceServiceRequestEvent _onComplete = new VoiceServiceRequestEvent();

	protected const string EVENT_CATEGORY_AUDIO_EVENTS = "Audio Events";

	[EventCategory("Audio Events")]
	[Tooltip("Called when the microphone has started collecting data collecting data to be sent to Wit.ai. There may be some buffering before data transmission starts.")]
	[FormerlySerializedAs("OnStartListening")]
	[FormerlySerializedAs("onStart")]
	[SerializeField]
	private UnityEvent _onStartListening = new UnityEvent();

	[EventCategory("Audio Events")]
	[Tooltip("Called when the voice service is no longer collecting data from the microphone")]
	[FormerlySerializedAs("OnStoppedListening")]
	[FormerlySerializedAs("onStopped")]
	[SerializeField]
	private UnityEvent _onStoppedListening = new UnityEvent();

	[EventCategory("Audio Events")]
	[Tooltip("Called when the volume level of the mic input has changed")]
	[FormerlySerializedAs("OnMicLevelChanged")]
	[SerializeField]
	private WitMicLevelChangedEvent _onMicLevelChanged = new WitMicLevelChangedEvent();

	protected const string EVENT_CATEGORY_TRANSCRIPTION_EVENTS = "Transcription Events";

	[EventCategory("Transcription Events")]
	[Tooltip("Message fired when a partial transcription has been received.")]
	[FormerlySerializedAs("onPartialTranscription")]
	[FormerlySerializedAs("OnPartialTranscription")]
	[SerializeField]
	private WitTranscriptionEvent _onPartialTranscription = new WitTranscriptionEvent();

	[FormerlySerializedAs("OnFullTranscription")]
	[EventCategory("Transcription Events")]
	[Tooltip("Message received when a complete transcription is received.")]
	[FormerlySerializedAs("onFullTranscription")]
	[FormerlySerializedAs("OnFullTranscription")]
	[SerializeField]
	private WitTranscriptionEvent _onFullTranscription = new WitTranscriptionEvent();

	[Tooltip("Called on request transcription while audio is still being analyzed.  Also returns client user id as first parameter")]
	[SerializeField]
	private UserTranscriptionRequestEvent _onUserPartialTranscription = Activator.CreateInstance<UserTranscriptionRequestEvent>();

	[Tooltip("Called on request transcription when audio has been completely transferred.  Also returns client user id as first parameter")]
	[SerializeField]
	private UserTranscriptionRequestEvent _onUserFullTranscription = Activator.CreateInstance<UserTranscriptionRequestEvent>();

	private HashSet<SpeechEvents> _listeners = new HashSet<SpeechEvents>();

	public WitRequestOptionsEvent OnRequestOptionSetup => _onRequestOptionSetup;

	public VoiceServiceRequestEvent OnRequestInitialized => _onRequestInitialized;

	[Obsolete("Deprecated for 'OnSend' event")]
	public WitRequestCreatedEvent OnRequestCreated => _onRequestCreated;

	public VoiceServiceRequestEvent OnSend => _onSend;

	public UnityEvent OnMinimumWakeThresholdHit => _onMinimumWakeThresholdHit;

	public UnityEvent OnMicDataSent => _onMicDataSent;

	public UnityEvent OnStoppedListeningDueToDeactivation => _onStoppedListeningDueToDeactivation;

	public UnityEvent OnStoppedListeningDueToInactivity => _onStoppedListeningDueToInactivity;

	public UnityEvent OnStoppedListeningDueToTimeout => _onStoppedListeningDueToTimeout;

	public UnityEvent OnAborting => _onAborting;

	public UnityEvent OnAborted => _onAborted;

	public WitTranscriptionEvent OnCanceled => _onCanceled;

	public WitTranscriptionEvent OnRawResponse => _onRawResponse;

	public WitResponseEvent OnPartialResponse => _onPartialResponse;

	public WitResponseEvent OnResponse => _onResponse;

	public WitErrorEvent OnError => _onError;

	public UnityEvent OnRequestCompleted => _onRequestCompleted;

	public VoiceServiceRequestEvent OnComplete => _onComplete;

	public UnityEvent OnStartListening => _onStartListening;

	public UnityEvent OnMicStartedListening => OnStartListening;

	public UnityEvent OnStoppedListening => _onStoppedListening;

	public UnityEvent OnMicStoppedListening => OnStoppedListening;

	public WitMicLevelChangedEvent OnMicLevelChanged => _onMicLevelChanged;

	public WitMicLevelChangedEvent OnMicAudioLevelChanged => OnMicLevelChanged;

	public WitTranscriptionEvent OnPartialTranscription => _onPartialTranscription;

	[Obsolete("Deprecated for 'OnPartialTranscription' event")]
	public WitTranscriptionEvent onPartialTranscription => OnPartialTranscription;

	public WitTranscriptionEvent OnFullTranscription => _onFullTranscription;

	[Obsolete("Deprecated for 'OnPartialTranscription' event")]
	public WitTranscriptionEvent onFullTranscription => OnFullTranscription;

	public UserTranscriptionRequestEvent OnUserPartialTranscription => _onUserPartialTranscription;

	public UserTranscriptionRequestEvent OnUserFullTranscription => _onUserFullTranscription;

	public void AddListener(SpeechEvents listener)
	{
		SetListener(listener, add: true);
	}

	public void RemoveListener(SpeechEvents listener)
	{
		SetListener(listener, add: false);
	}

	public virtual void SetListener(SpeechEvents listener, bool add)
	{
		if (listener == null || listener.Equals(this))
		{
			return;
		}
		if (add)
		{
			if (!_listeners.Add(listener))
			{
				return;
			}
		}
		else if (!_listeners.Remove(listener))
		{
			return;
		}
		OnRequestOptionSetup.SetListener(listener.OnRequestOptionSetup.Invoke, add);
		OnRequestInitialized.SetListener(listener.OnRequestInitialized.Invoke, add);
		OnSend.SetListener(listener.OnSend.Invoke, add);
		OnMinimumWakeThresholdHit.SetListener(listener.OnMinimumWakeThresholdHit.Invoke, add);
		OnMicDataSent.SetListener(listener.OnMicDataSent.Invoke, add);
		OnStoppedListeningDueToDeactivation.SetListener(listener.OnStoppedListeningDueToDeactivation.Invoke, add);
		OnStoppedListeningDueToInactivity.SetListener(listener.OnStoppedListeningDueToInactivity.Invoke, add);
		OnStoppedListeningDueToTimeout.SetListener(listener.OnStoppedListeningDueToTimeout.Invoke, add);
		OnAborting.SetListener(listener.OnAborting.Invoke, add);
		OnAborted.SetListener(listener.OnAborted.Invoke, add);
		OnCanceled.SetListener(listener.OnCanceled.Invoke, add);
		OnRawResponse.SetListener(listener.OnRawResponse.Invoke, add);
		OnPartialResponse.SetListener(listener.OnPartialResponse.Invoke, add);
		OnResponse.SetListener(listener.OnResponse.Invoke, add);
		OnError.SetListener(listener.OnError.Invoke, add);
		OnRequestCompleted.SetListener(listener.OnRequestCompleted.Invoke, add);
		OnComplete.SetListener(listener.OnComplete.Invoke, add);
		OnStartListening.SetListener(listener.OnStartListening.Invoke, add);
		OnMicStartedListening.SetListener(listener.OnMicStartedListening.Invoke, add);
		OnStoppedListening.SetListener(listener.OnStoppedListening.Invoke, add);
		OnMicStoppedListening.SetListener(listener.OnMicStoppedListening.Invoke, add);
		OnMicLevelChanged.SetListener(listener.OnMicLevelChanged.Invoke, add);
		OnMicAudioLevelChanged.SetListener(listener.OnMicAudioLevelChanged.Invoke, add);
		OnPartialTranscription.SetListener(listener.OnPartialTranscription.Invoke, add);
		OnFullTranscription.SetListener(listener.OnFullTranscription.Invoke, add);
		OnUserPartialTranscription.SetListener(listener.OnUserPartialTranscription.Invoke, add);
		OnUserFullTranscription.SetListener(listener.OnUserFullTranscription.Invoke, add);
		OnRequestCreated.SetListener(listener.OnRequestCreated.Invoke, add);
		onPartialTranscription.SetListener(listener.onPartialTranscription.Invoke, add);
		onFullTranscription.SetListener(listener.onFullTranscription.Invoke, add);
	}
}
