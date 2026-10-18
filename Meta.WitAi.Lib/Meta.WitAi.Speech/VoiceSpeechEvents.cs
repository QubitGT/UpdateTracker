using System;
using UnityEngine;

namespace Meta.WitAi.Speech;

[Serializable]
public class VoiceSpeechEvents
{
	[Header("Text Events")]
	[Tooltip("Called when speech begins with the provided phrase")]
	public VoiceTextEvent OnTextPlaybackStart = new VoiceTextEvent();

	[Tooltip("Called when speech playback is cancelled")]
	public VoiceTextEvent OnTextPlaybackCancelled = new VoiceTextEvent();

	[Tooltip("Called when speech playback completes successfully")]
	public VoiceTextEvent OnTextPlaybackFinished = new VoiceTextEvent();

	[Header("Audio Clip Events")]
	[Tooltip("Called when a clip is ready for playback")]
	public VoiceAudioEvent OnAudioClipPlaybackReady = new VoiceAudioEvent();

	[Tooltip("Called when a clip playback has begun")]
	public VoiceAudioEvent OnAudioClipPlaybackStart = new VoiceAudioEvent();

	[Tooltip("Called when a clip playback has been cancelled")]
	public VoiceAudioEvent OnAudioClipPlaybackCancelled = new VoiceAudioEvent();

	[Tooltip("Called when a clip playback has completed successfully")]
	public VoiceAudioEvent OnAudioClipPlaybackFinished = new VoiceAudioEvent();
}
