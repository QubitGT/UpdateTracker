using System;
using UnityEngine;

namespace Meta.WitAi.TTS.Events;

[Serializable]
public class TTSWebRequestEvents
{
	[Tooltip("Called when a web request begins transmission")]
	public TTSClipEvent OnRequestBegin = new TTSClipEvent();

	[Tooltip("Called when a web request is cancelled")]
	public TTSClipEvent OnRequestCancel = new TTSClipEvent();

	[Tooltip("Called when a web request fails")]
	public TTSClipErrorEvent OnRequestError = new TTSClipErrorEvent();

	[Tooltip("Called when a web request receives first data")]
	public TTSClipEvent OnRequestFirstResponse = new TTSClipEvent();

	[Tooltip("Called when a web request is ready for playback")]
	public TTSClipEvent OnRequestReady = new TTSClipEvent();

	[Tooltip("Called when a web request is completed via success, cancellation or failure")]
	public TTSClipEvent OnRequestComplete = new TTSClipEvent();
}
