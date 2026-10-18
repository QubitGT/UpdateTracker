using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Voice;

[Serializable]
public class TranscriptionRequestEvents<TUnityEvent> : VoiceRequestEvents<TUnityEvent> where TUnityEvent : UnityEventBase
{
	[Header("Audio Events")]
	[Tooltip("Called every time audio input changes states.")]
	[SerializeField]
	private TUnityEvent _onAudioInputStateChange = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called every time audio input changes states.")]
	[SerializeField]
	private TUnityEvent _onAudioActivation = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called when audio is being listened to for this request.")]
	[SerializeField]
	private TUnityEvent _onStartListening = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called when audio is no longer being listened to for this request.")]
	[SerializeField]
	private TUnityEvent _onAudioDeactivation = Activator.CreateInstance<TUnityEvent>();

	[Tooltip("Called when audio is no longer being listened to for this request.")]
	[SerializeField]
	private TUnityEvent _onStopListening = Activator.CreateInstance<TUnityEvent>();

	[Header("Transcription Events")]
	[Tooltip("Called on request transcription while audio is still being analyzed.")]
	[SerializeField]
	private TranscriptionRequestEvent _onPartialTranscription = Activator.CreateInstance<TranscriptionRequestEvent>();

	[Tooltip("Called on request transcription when audio has been completely transferred.")]
	[SerializeField]
	private TranscriptionRequestEvent _onFullTranscription = Activator.CreateInstance<TranscriptionRequestEvent>();

	[Tooltip("Called on request transcription while audio is still being analyzed.  Also returns client user id as first parameter")]
	[SerializeField]
	private UserTranscriptionRequestEvent _onUserPartialTranscription = Activator.CreateInstance<UserTranscriptionRequestEvent>();

	[Tooltip("Called on request transcription when audio has been completely transferred.  Also returns client user id as first parameter")]
	[SerializeField]
	private UserTranscriptionRequestEvent _onUserFullTranscription = Activator.CreateInstance<UserTranscriptionRequestEvent>();

	public TUnityEvent OnAudioInputStateChange => _onAudioInputStateChange;

	public TUnityEvent OnAudioActivation => _onAudioActivation;

	public TUnityEvent OnStartListening => _onStartListening;

	public TUnityEvent OnAudioDeactivation => _onAudioDeactivation;

	public TUnityEvent OnStopListening => _onStopListening;

	public TranscriptionRequestEvent OnPartialTranscription => _onPartialTranscription;

	public TranscriptionRequestEvent OnFullTranscription => _onFullTranscription;

	public UserTranscriptionRequestEvent OnUserPartialTranscription => _onUserPartialTranscription;

	public UserTranscriptionRequestEvent OnUserFullTranscription => _onUserFullTranscription;
}
