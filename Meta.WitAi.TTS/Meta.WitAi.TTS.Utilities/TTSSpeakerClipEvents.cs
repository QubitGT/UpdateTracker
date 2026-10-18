using System;
using Meta.WitAi.Speech;
using UnityEngine;

namespace Meta.WitAi.TTS.Utilities;

[Serializable]
public class TTSSpeakerClipEvents : VoiceSpeechEvents
{
	[Header("Speaker Lifecycle Events")]
	[SerializeField]
	[Tooltip("Initial callback as soon as the audio clip speak request is generated")]
	private TTSSpeakerClipEvent _onInit = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Final call for a 'Speak' request that is called following a load failure, load abort, playback cancellation or playback completion")]
	private TTSSpeakerClipEvent _onComplete = new TTSSpeakerClipEvent();

	[Header("Speaker Loading Events")]
	[SerializeField]
	[Tooltip("Called when TTS audio clip load begins")]
	private TTSSpeakerClipEvent _onLoadBegin = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip load is cancelled")]
	private TTSSpeakerClipEvent _onLoadAbort = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip load fails due to a network or load error")]
	private TTSSpeakerClipMessageEvent _onLoadFailed = new TTSSpeakerClipMessageEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip load successfully")]
	private TTSSpeakerClipEvent _onLoadSuccess = new TTSSpeakerClipEvent();

	[Header("Speaker Playback Events")]
	[SerializeField]
	[Tooltip("Called when TTS audio clip playback is ready")]
	private TTSSpeakerClipEvent _onPlaybackReady = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip playback has begun")]
	private TTSSpeakerClipEvent _onPlaybackStart = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip playback been cancelled")]
	private TTSSpeakerClipMessageEvent _onPlaybackCancelled = new TTSSpeakerClipMessageEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip is updated during streamed playback")]
	private TTSSpeakerClipEvent _onPlaybackClipUpdated = new TTSSpeakerClipEvent();

	[SerializeField]
	[Tooltip("Called when TTS audio clip playback completed successfully")]
	private TTSSpeakerClipEvent _onPlaybackComplete = new TTSSpeakerClipEvent();

	public TTSSpeakerClipEvent OnInit => _onInit;

	public TTSSpeakerClipEvent OnComplete => _onComplete;

	public TTSSpeakerClipEvent OnLoadBegin => _onLoadBegin;

	public TTSSpeakerClipEvent OnLoadAbort => _onLoadAbort;

	public TTSSpeakerClipMessageEvent OnLoadFailed => _onLoadFailed;

	public TTSSpeakerClipEvent OnLoadSuccess => _onLoadSuccess;

	public TTSSpeakerClipEvent OnPlaybackReady => _onPlaybackReady;

	public TTSSpeakerClipEvent OnPlaybackStart => _onPlaybackStart;

	public TTSSpeakerClipMessageEvent OnPlaybackCancelled => _onPlaybackCancelled;

	public TTSSpeakerClipEvent OnPlaybackClipUpdated => _onPlaybackClipUpdated;

	public TTSSpeakerClipEvent OnPlaybackComplete => _onPlaybackComplete;
}
