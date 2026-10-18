using System;
using UnityEngine;

namespace Meta.WitAi.TTS.Events;

[Serializable]
public class TTSDownloadEvents
{
	[Tooltip("Called when a audio clip download begins")]
	public TTSClipDownloadEvent OnDownloadBegin = new TTSClipDownloadEvent();

	[Tooltip("Called when a audio clip is downloaded successfully")]
	public TTSClipDownloadEvent OnDownloadSuccess = new TTSClipDownloadEvent();

	[Tooltip("Called when a audio clip downloaded has been cancelled")]
	public TTSClipDownloadEvent OnDownloadCancel = new TTSClipDownloadEvent();

	[Tooltip("Called when a audio clip downloaded has failed")]
	public TTSClipDownloadErrorEvent OnDownloadError = new TTSClipDownloadErrorEvent();
}
