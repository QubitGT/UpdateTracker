using System;
using UnityEngine;

namespace Meta.WitAi.TTS.Events;

[Serializable]
public class TTSServiceEvents
{
	[Tooltip("Called when a audio clip has been added to the runtime cache")]
	public TTSClipEvent OnClipCreated = new TTSClipEvent();

	[Tooltip("Called when a audio clip has been removed from the runtime cache")]
	public TTSClipEvent OnClipUnloaded = new TTSClipEvent();

	public TTSWebRequestEvents WebRequest = new TTSWebRequestEvents();

	public TTSStreamEvents Stream = new TTSStreamEvents();

	public TTSDownloadEvents Download = new TTSDownloadEvents();
}
