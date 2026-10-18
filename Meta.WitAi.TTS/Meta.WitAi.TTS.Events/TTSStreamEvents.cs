using System;
using UnityEngine;

namespace Meta.WitAi.TTS.Events;

[Serializable]
public class TTSStreamEvents
{
	[Tooltip("Called when a audio clip stream begins")]
	public TTSClipEvent OnStreamBegin = new TTSClipEvent();

	[Tooltip("Called when a audio clip is ready for playback")]
	public TTSClipEvent OnStreamReady = new TTSClipEvent();

	[Tooltip("Called if/when an audio clip is adjusted")]
	public TTSClipEvent OnStreamClipUpdate = new TTSClipEvent();

	[Tooltip("Called when a audio clip is completely loaded")]
	public TTSClipEvent OnStreamComplete = new TTSClipEvent();

	[Tooltip("Called when a audio clip stream has been cancelled")]
	public TTSClipEvent OnStreamCancel = new TTSClipEvent();

	[Tooltip("Called when a audio clip stream has failed")]
	public TTSClipErrorEvent OnStreamError = new TTSClipErrorEvent();
}
