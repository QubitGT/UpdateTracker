using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Meta.Voice.Audio;
using UnityEngine;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSClipData
{
	public string textToSpeak;

	public string clipID;

	[Obsolete("Use extension directly.")]
	public AudioType audioType;

	public TTSVoiceSettings voiceSettings;

	public TTSDiskCacheSettings diskCacheSettings;

	public string queryOperationId;

	public bool queryStream;

	public Dictionary<string, string> queryParameters;

	private IAudioClipStream _clipStream;

	[NonSerialized]
	public TTSClipLoadState loadState;

	[NonSerialized]
	public float loadProgress;

	[NonSerialized]
	public float readyDuration;

	[NonSerialized]
	public float completeDuration;

	public Action<TTSClipData, TTSClipLoadState> onStateChange;

	public bool useEvents;

	public string extension;

	public Action<TTSClipData> onPlaybackReady;

	public Action<string> onDownloadComplete;

	public Action<TTSClipData> onRequestBegin;

	public Action<TTSClipData> onRequestComplete;

	public Action<TTSClipData> onPlaybackQueued;

	public Action<TTSClipData> onPlaybackBegin;

	public Action<TTSClipData> onPlaybackComplete;

	public string queryRequestId { get; } = WitConstants.GetUniqueId();


	public IAudioClipStream clipStream
	{
		get
		{
			return _clipStream;
		}
		set
		{
			if (_clipStream != null && _clipStream != value)
			{
				clipStream.OnStreamReady = null;
				clipStream.OnStreamUpdated = null;
				clipStream.OnStreamComplete = null;
				_clipStream.Unload();
			}
			_clipStream = value;
		}
	}

	public AudioClip clip
	{
		get
		{
			if (clipStream is IAudioClipProvider audioClipProvider)
			{
				return audioClipProvider.Clip;
			}
			return null;
		}
	}

	public TTSEventContainer Events { get; } = new TTSEventContainer();


	public int LoadStatusCode { get; set; }

	public string LoadError { get; set; }

	public TaskCompletionSource<bool> LoadReady { get; } = new TaskCompletionSource<bool>();


	public TaskCompletionSource<bool> LoadCompletion { get; } = new TaskCompletionSource<bool>();


	public override bool Equals(object obj)
	{
		if (obj is TTSClipData other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(TTSClipData other)
	{
		return HasClipId(other?.clipID);
	}

	public bool HasClipId(string clipId)
	{
		return string.Equals(clipID, clipId, StringComparison.CurrentCultureIgnoreCase);
	}

	public override int GetHashCode()
	{
		return 17 * 31 + clipID.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("Text: {0}\nVoice: {1}\nClip Id: {2}\nType: {3}\nStream: {4}\nEvents: {5}\nAudio Length: {6:0.00} seconds", textToSpeak, voiceSettings?.SettingsId ?? "Null", clipID, extension, queryStream, (Events?.Events?.Count()).GetValueOrDefault(), clipStream?.Length ?? 0f);
	}
}
