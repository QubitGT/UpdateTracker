using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Audio;
using Meta.Voice.Logging;
using Meta.WitAi.Json;
using Meta.WitAi.Speech;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Utilities;

[LogCategory(LogCategory.TextToSpeech)]
public class TTSSpeaker : MonoBehaviour, ISpeechEventProvider, ISpeaker, ITTSEventPlayer, ILogSource
{
	private class TTSSpeakerRequestData
	{
		public TTSClipData ClipData;

		public Action<TTSClipData> OnReady;

		public bool IsReady;

		public string Error;

		public DateTime StartTime;

		public bool StopPlaybackOnLoad;

		public TTSSpeakerClipEvents PlaybackEvents;

		public TaskCompletionSource<bool> PlaybackCompletion;

		public WitResponseNode SpeechNode;
	}

	[Header("Event Settings")]
	[Tooltip("All speaker load and playback events")]
	[SerializeField]
	private TTSSpeakerEvents _events = new TTSSpeakerEvents();

	[Header("Text Settings")]
	[Tooltip("Text that is added to the front of any Speech() request")]
	[TextArea]
	[FormerlySerializedAs("prependedText")]
	public string PrependedText;

	[Tooltip("Text that is added to the end of any Speech() text")]
	[TextArea]
	[FormerlySerializedAs("appendedText")]
	public string AppendedText;

	[Header("Load Settings")]
	[Tooltip("Optional TTSService reference to be used for text-to-speech loading.  If missing, it will check the component.  If that is also missing then it will use the current singleton")]
	[SerializeField]
	private TTSService _ttsService;

	[Tooltip("Preset voice setting id of TTSService voice settings")]
	[HideInInspector]
	[SerializeField]
	private string presetVoiceID;

	[Tooltip("Custom wit specific voice settings used if the preset is null or empty")]
	[HideInInspector]
	[SerializeField]
	public TTSWitVoiceSettings customWitVoiceSettings;

	[SerializeField]
	private bool verboseLogging;

	private TTSVoiceSettings _overrideVoiceSettings;

	private float _elapsedPlayTime;

	private TTSSpeakerRequestData _speakingRequest;

	private List<TTSSpeakerRequestData> _queuedRequests = new List<TTSSpeakerRequestData>();

	private bool _hasQueue;

	private bool _queueNotYetComplete;

	private ISpeakerTextPreprocessor[] _textPreprocessors;

	private ISpeakerTextPostprocessor[] _textPostprocessors;

	private IAudioPlayer _audioPlayer;

	private Coroutine _waitForCompletion;

	private bool _isPlaying;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.TextToSpeech);


	public TTSSpeakerEvents Events => _events;

	public VoiceSpeechEvents SpeechEvents => _events;

	public TTSService TTSService
	{
		get
		{
			if (!_ttsService)
			{
				_ttsService = GetComponent<TTSService>();
				if (!_ttsService)
				{
					_ttsService = TTSService.Instance;
				}
			}
			return _ttsService;
		}
	}

	public string VoiceID
	{
		get
		{
			return presetVoiceID;
		}
		set
		{
			presetVoiceID = value;
		}
	}

	public TTSVoiceSettings VoiceSettings
	{
		get
		{
			if (_isPlaying && _overrideVoiceSettings != null)
			{
				return _overrideVoiceSettings;
			}
			TTSVoiceSettings tTSVoiceSettings = (string.IsNullOrEmpty(presetVoiceID) ? null : TTSService.GetPresetVoiceSettings(presetVoiceID));
			if (tTSVoiceSettings != null)
			{
				return tTSVoiceSettings;
			}
			return customWitVoiceSettings;
		}
	}

	public bool IsSpeaking => SpeakingClip != null;

	public TTSClipData SpeakingClip => _speakingRequest?.ClipData;

	public bool IsLoading => _queuedRequests.Count > 0;

	public bool IsPreparing
	{
		get
		{
			foreach (TTSSpeakerRequestData queuedRequest in _queuedRequests)
			{
				if (queuedRequest.ClipData != null && queuedRequest.ClipData.loadState == TTSClipLoadState.Preparing)
				{
					return true;
				}
			}
			return false;
		}
	}

	public List<TTSClipData> QueuedClips
	{
		get
		{
			List<TTSClipData> list = new List<TTSClipData>();
			foreach (TTSSpeakerRequestData queuedRequest in _queuedRequests)
			{
				list.Add(queuedRequest.ClipData);
			}
			return list;
		}
	}

	public bool IsActive
	{
		get
		{
			if (!IsSpeaking)
			{
				return IsLoading;
			}
			return true;
		}
	}

	public IAudioPlayer AudioPlayer
	{
		get
		{
			if (_audioPlayer == null)
			{
				_audioPlayer = base.gameObject.GetComponent<IAudioPlayer>();
				if (_audioPlayer == null)
				{
					_audioPlayer = TTSService?.AudioSystem?.GetAudioPlayer(base.gameObject);
					if (_audioPlayer == null)
					{
						_audioPlayer = base.gameObject.AddComponent<UnityAudioPlayer>();
					}
				}
			}
			return _audioPlayer;
		}
	}

	public AudioSource AudioSource
	{
		get
		{
			if (AudioPlayer is IAudioSourceProvider audioSourceProvider)
			{
				return audioSourceProvider.AudioSource;
			}
			return null;
		}
	}

	public bool IsPaused { get; private set; }

	public int ElapsedSamples
	{
		get
		{
			if (!IsSpeaking || _audioPlayer?.ClipStream == null)
			{
				return 0;
			}
			if (_audioPlayer.CanSetElapsedSamples)
			{
				return _audioPlayer.ElapsedSamples;
			}
			return Mathf.FloorToInt(_elapsedPlayTime * (float)_audioPlayer.ClipStream.Channels * (float)_audioPlayer.ClipStream.SampleRate);
		}
	}

	public int TotalSamples
	{
		get
		{
			if (!IsSpeaking || SpeakingClip?.clipStream == null)
			{
				return 0;
			}
			return SpeakingClip.clipStream.TotalSamples;
		}
	}

	public TTSEventSampleDelegate OnSampleUpdated { get; set; }

	public TTSEventContainer CurrentEvents => SpeakingClip?.Events;

	protected virtual void Start()
	{
		AudioPlayer.Init();
	}

	protected virtual void OnDestroy()
	{
		Stop();
		_speakingRequest = null;
		lock (_queuedRequests)
		{
			_queuedRequests.Clear();
		}
	}

	protected virtual void OnEnable()
	{
		_isPlaying = Application.isPlaying;
		if (_textPreprocessors == null)
		{
			_textPreprocessors = GetComponents<ISpeakerTextPreprocessor>();
		}
		if (_textPostprocessors == null)
		{
			_textPostprocessors = GetComponents<ISpeakerTextPostprocessor>();
		}
		if (!string.IsNullOrEmpty(PrependedText) && PrependedText.Length > 0 && !PrependedText.EndsWith(" "))
		{
			PrependedText += " ";
		}
		if (!string.IsNullOrEmpty(AppendedText) && AppendedText.Length > 0 && !AppendedText.StartsWith(" "))
		{
			AppendedText = " " + AppendedText;
		}
		if ((bool)TTSService)
		{
			TTSService.Events.OnClipUnloaded.AddListener(StopAndUnloadClip);
		}
	}

	protected virtual void OnDisable()
	{
		Stop();
		if ((bool)TTSService)
		{
			TTSService.Events.OnClipUnloaded.RemoveListener(StopAndUnloadClip);
		}
	}

	protected virtual void StopAndUnloadClip(TTSClipData clipData)
	{
		Stop(clipData, allInstances: true);
	}

	private TTSSpeakerRequestData GetFirstQueuedRequest(TTSClipData clipData)
	{
		if (_queuedRequests != null)
		{
			foreach (TTSSpeakerRequestData queuedRequest in _queuedRequests)
			{
				if (string.Equals(clipData?.clipID, queuedRequest?.ClipData?.clipID))
				{
					return queuedRequest;
				}
			}
		}
		return null;
	}

	private TTSSpeakerRequestData GetFirstQueuedRequest(string textToSpeak)
	{
		if (_queuedRequests != null)
		{
			foreach (TTSSpeakerRequestData queuedRequest in _queuedRequests)
			{
				if (string.Equals(textToSpeak, queuedRequest?.ClipData?.textToSpeak))
				{
					return queuedRequest;
				}
			}
		}
		return null;
	}

	private static bool RequestEquals(TTSSpeakerRequestData requestData1, TTSSpeakerRequestData requestData2)
	{
		if (requestData1 != null && requestData2 != null)
		{
			return requestData1.Equals(requestData2);
		}
		return false;
	}

	private static bool RequestHasClipData(TTSSpeakerRequestData requestData, TTSClipData clipData)
	{
		string a = requestData?.ClipData?.clipID;
		string b = clipData?.clipID;
		return string.Equals(a, b);
	}

	private static bool RequestHasClipText(TTSSpeakerRequestData requestData, string textToSpeak)
	{
		return string.Equals(requestData?.ClipData?.textToSpeak, textToSpeak);
	}

	private void RefreshQueueEvents()
	{
		bool flag = IsActive || _queueNotYetComplete;
		if (_hasQueue != flag)
		{
			_hasQueue = flag;
			if (_hasQueue)
			{
				RaiseEvents(RaiseOnPlaybackQueueBegin);
			}
			else
			{
				RaiseEvents(RaiseOnPlaybackQueueComplete);
			}
		}
	}

	private bool IsClipRequestActive(TTSSpeakerRequestData requestData)
	{
		if (!IsClipRequestLoading(requestData))
		{
			return IsClipRequestSpeaking(requestData);
		}
		return true;
	}

	private bool IsClipRequestLoading(TTSSpeakerRequestData requestData)
	{
		return _queuedRequests.Contains(requestData);
	}

	private bool IsClipRequestSpeaking(TTSSpeakerRequestData requestData)
	{
		if (_speakingRequest != null)
		{
			return _speakingRequest.Equals(requestData);
		}
		return false;
	}

	public List<string> GetFinalText(string textToSpeak)
	{
		List<string> list = new List<string>();
		list.Add(textToSpeak);
		if (_textPreprocessors != null)
		{
			ISpeakerTextPreprocessor[] textPreprocessors = _textPreprocessors;
			for (int i = 0; i < textPreprocessors.Length; i++)
			{
				textPreprocessors[i].OnPreprocessTTS(this, list);
			}
		}
		if (!string.IsNullOrEmpty(PrependedText) || !string.IsNullOrEmpty(AppendedText))
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (!string.IsNullOrEmpty(list[j].Trim()))
				{
					string text = list[j];
					text = (PrependedText + text + AppendedText).Trim();
					list[j] = text;
				}
			}
		}
		if (_textPostprocessors != null)
		{
			ISpeakerTextPostprocessor[] textPostprocessors = _textPostprocessors;
			for (int i = 0; i < textPostprocessors.Length; i++)
			{
				textPostprocessors[i].OnPostprocessTTS(this, list);
			}
		}
		return list;
	}

	public List<string> GetFinalTextFormatted(string format, params string[] textsToSpeak)
	{
		return GetFinalText(GetFormattedText(format, textsToSpeak));
	}

	public string GetFormattedText(string format, params string[] textsToSpeak)
	{
		if (textsToSpeak != null && !string.IsNullOrEmpty(format))
		{
			object[] array = new object[textsToSpeak.Length];
			textsToSpeak.CopyTo(array, 0);
			return string.Format(format, array);
		}
		return null;
	}

	public void Speak(string textToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		Load(textToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: true).WrapErrors();
	}

	public void Speak(string textToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		Speak(textToSpeak, null, playbackEvents);
	}

	public void Speak(string textToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		Speak(textToSpeak, diskCacheSettings, null);
	}

	public void Speak(string textToSpeak)
	{
		Speak(textToSpeak, null, null);
	}

	public void SpeakFormat(string format, params string[] textsToSpeak)
	{
		Speak(GetFormattedText(format, textsToSpeak), null, null);
	}

	public IEnumerator SpeakAsync(string textToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		yield return ThreadUtility.CoroutineAwait(delegate
		{
			Load(textToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: true).WrapErrors();
			return Task.CompletedTask;
		});
	}

	public IEnumerator SpeakAsync(string textToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		yield return SpeakAsync(textToSpeak, null, playbackEvents);
	}

	public IEnumerator SpeakAsync(string textToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		yield return SpeakAsync(textToSpeak, diskCacheSettings, null);
	}

	public IEnumerator SpeakAsync(string textToSpeak)
	{
		yield return SpeakAsync(textToSpeak, null, null);
	}

	public Task SpeakTask(string textToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		return Load(textToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: true);
	}

	public Task SpeakTask(string textToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		return SpeakTask(textToSpeak, null, playbackEvents);
	}

	public Task SpeakTask(string textToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		return SpeakTask(textToSpeak, diskCacheSettings, null);
	}

	public Task SpeakTask(string textToSpeak)
	{
		return SpeakTask(textToSpeak, null, null);
	}

	public Task SpeakTask(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		return Load(responseNode, null, playbackEvents, clearQueue: true);
	}

	public void SpeakQueued(string textToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		Load(textToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: false).WrapErrors();
	}

	public void SpeakQueued(string textToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		SpeakQueued(textToSpeak, null, playbackEvents);
	}

	public void SpeakQueued(string textToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		SpeakQueued(textToSpeak, diskCacheSettings, null);
	}

	public void SpeakQueued(string textToSpeak)
	{
		SpeakQueued(textToSpeak, null, null);
	}

	public void SpeakFormatQueued(string format, params string[] textsToSpeak)
	{
		SpeakQueued(GetFormattedText(format, textsToSpeak), null, null);
	}

	public IEnumerator SpeakQueuedAsync(string[] textsToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		yield return ThreadUtility.CoroutineAwait(() => Load(textsToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: false));
	}

	public IEnumerator SpeakQueuedAsync(string[] textsToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		yield return SpeakQueuedAsync(textsToSpeak, null, playbackEvents);
	}

	public IEnumerator SpeakQueuedAsync(string[] textsToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		yield return SpeakQueuedAsync(textsToSpeak, diskCacheSettings, null);
	}

	public IEnumerator SpeakQueuedAsync(string[] textsToSpeak)
	{
		yield return SpeakQueuedAsync(textsToSpeak, null, null);
	}

	public Task SpeakQueuedTask(string[] textsToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		return Load(textsToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: false);
	}

	public Task SpeakQueuedTask(string[] textsToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		return SpeakQueuedTask(textsToSpeak, null, playbackEvents);
	}

	public Task SpeakQueuedTask(string[] textsToSpeak, TTSDiskCacheSettings diskCacheSettings)
	{
		return SpeakQueuedTask(textsToSpeak, diskCacheSettings, null);
	}

	public Task SpeakQueuedTask(string[] textsToSpeak)
	{
		return SpeakQueuedTask(textsToSpeak, null, null);
	}

	public void SetVoiceOverride(TTSVoiceSettings overrideVoiceSettings)
	{
		_overrideVoiceSettings = overrideVoiceSettings;
	}

	public void ClearVoiceOverride()
	{
		SetVoiceOverride(null);
	}

	public bool Speak(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		if (!TTSService.DecodeTts(responseNode, out var textToSpeak, out var voiceSettings))
		{
			return false;
		}
		Load(textToSpeak, voiceSettings, diskCacheSettings, playbackEvents, responseNode, clearQueue: true).WrapErrors();
		return true;
	}

	public bool Speak(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings)
	{
		return Speak(responseNode, diskCacheSettings, null);
	}

	public bool Speak(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		return Speak(responseNode, null, playbackEvents);
	}

	public bool Speak(WitResponseNode responseNode)
	{
		return Speak(responseNode, null, null);
	}

	public IEnumerator SpeakAsync(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		if (TTSService.DecodeTts(responseNode, out var textToSpeak, out var voiceSettings))
		{
			yield return ThreadUtility.CoroutineAwait(() => Load(textToSpeak, voiceSettings, diskCacheSettings, playbackEvents, responseNode, clearQueue: true));
		}
	}

	public IEnumerator SpeakAsync(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		yield return SpeakAsync(responseNode, null, playbackEvents);
	}

	public IEnumerator SpeakAsync(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings)
	{
		yield return SpeakAsync(responseNode, diskCacheSettings, null);
	}

	public bool SpeakQueued(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		if (!TTSService.DecodeTts(responseNode, out var textToSpeak, out var voiceSettings))
		{
			return false;
		}
		Load(textToSpeak, voiceSettings, diskCacheSettings, playbackEvents, responseNode, clearQueue: false).WrapErrors();
		return true;
	}

	public bool SpeakQueued(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		return SpeakQueued(responseNode, null, playbackEvents);
	}

	public bool SpeakQueued(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings)
	{
		return SpeakQueued(responseNode, diskCacheSettings, null);
	}

	public bool SpeakQueued(WitResponseNode responseNode)
	{
		return SpeakQueued(responseNode, null, null);
	}

	public IEnumerator SpeakQueuedAsync(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		if (TTSService.DecodeTts(responseNode, out var textToSpeak, out var voiceSettings))
		{
			yield return ThreadUtility.CoroutineAwait(() => Load(textToSpeak, voiceSettings, diskCacheSettings, playbackEvents, responseNode, clearQueue: false));
		}
	}

	public IEnumerator SpeakQueuedAsync(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		yield return SpeakQueuedAsync(responseNode, null, playbackEvents);
	}

	public IEnumerator SpeakQueuedAsync(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings)
	{
		yield return SpeakQueuedAsync(responseNode, diskCacheSettings, null);
	}

	public IEnumerator SpeakQueuedAsync(WitResponseNode responseNode)
	{
		yield return SpeakQueuedAsync(responseNode, null, null);
	}

	public Task SpeakQueuedTask(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		return Load(responseNode, diskCacheSettings, playbackEvents, clearQueue: false);
	}

	public Task SpeakQueuedTask(string textToSpeak, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents)
	{
		return Load(textToSpeak, null, diskCacheSettings, playbackEvents, null, clearQueue: false);
	}

	public Task SpeakQueuedTask(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents)
	{
		return SpeakQueuedTask(responseNode, null, playbackEvents);
	}

	public Task SpeakQueuedTask(string textToSpeak, TTSSpeakerClipEvents playbackEvents)
	{
		return SpeakQueuedTask(textToSpeak, null, playbackEvents);
	}

	public Task SpeakQueuedTask(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings)
	{
		return SpeakQueuedTask(responseNode, diskCacheSettings, null);
	}

	public Task SpeakQueuedTask(WitResponseNode responseNode)
	{
		return SpeakQueuedTask(responseNode, null, null);
	}

	public virtual void Stop(string textToSpeak, bool allInstances = false)
	{
		bool flag = SpeakingClip != null && SpeakingClip.textToSpeak.Equals(textToSpeak);
		if (flag)
		{
			StopSpeaking();
		}
		if (allInstances)
		{
			UnloadQueuedText(textToSpeak);
		}
		else if (!flag)
		{
			UnloadQueuedClipRequest(GetFirstQueuedRequest(textToSpeak));
		}
	}

	public virtual void Stop(TTSClipData clipData, bool allInstances = false)
	{
		bool flag = SpeakingClip != null && clipData.Equals(SpeakingClip);
		if (allInstances)
		{
			UnloadQueuedClip(clipData);
		}
		else if (!flag)
		{
			UnloadQueuedClipRequest(GetFirstQueuedRequest(clipData));
		}
		if (flag)
		{
			StopSpeaking();
		}
	}

	private void StopLoadingButKeepQueue()
	{
		_queueNotYetComplete = true;
		StopLoading();
		_queueNotYetComplete = false;
	}

	public virtual void StopLoading()
	{
		if (!IsLoading)
		{
			return;
		}
		for (int i = 0; i < _queuedRequests.Count; i++)
		{
			TTSSpeakerRequestData tTSSpeakerRequestData = _queuedRequests[i];
			if (tTSSpeakerRequestData != null)
			{
				RaiseEvents(RaiseOnLoadAborted, tTSSpeakerRequestData);
			}
		}
		lock (_queuedRequests)
		{
			_queuedRequests.Clear();
		}
		RefreshQueueEvents();
	}

	public virtual void StopSpeaking()
	{
		if (IsSpeaking)
		{
			HandlePlaybackComplete(stopped: true);
		}
	}

	public virtual void Stop()
	{
		StopLoading();
		StopSpeaking();
	}

	private bool DecodeTts(WitResponseNode responseNode, out string textToSpeak, out TTSVoiceSettings voiceSettings)
	{
		return TTSService.DecodeTts(responseNode, out textToSpeak, out voiceSettings);
	}

	private TTSSpeakerRequestData CreateRequest(TTSSpeakerClipEvents playbackEvents, WitResponseNode speechNode, bool clearQueue, bool add = true)
	{
		TTSSpeakerRequestData requestData = new TTSSpeakerRequestData();
		requestData.OnReady = delegate
		{
			TryPlayLoadedClip(requestData);
		};
		requestData.IsReady = false;
		requestData.StartTime = DateTime.UtcNow;
		requestData.PlaybackCompletion = new TaskCompletionSource<bool>();
		requestData.PlaybackEvents = playbackEvents ?? new TTSSpeakerClipEvents();
		requestData.StopPlaybackOnLoad = clearQueue;
		requestData.SpeechNode = speechNode;
		if (add)
		{
			lock (_queuedRequests)
			{
				_queuedRequests.Add(requestData);
			}
		}
		return requestData;
	}

	private async Task Load(WitResponseNode responseNode, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents, bool clearQueue)
	{
		if (clearQueue)
		{
			StopLoadingButKeepQueue();
		}
		TTSSpeakerRequestData requestData = CreateRequest(playbackEvents, responseNode, clearQueue);
		string textToSpeak = null;
		TTSVoiceSettings voiceSettings = null;
		await ThreadUtility.Background(Logger, delegate
		{
			DecodeTts(responseNode, out textToSpeak, out voiceSettings);
		});
		if (requestData.PlaybackCompletion.Task.IsCompleted)
		{
			Logger.Verbose("Canceled request during decode\nText: {0}", textToSpeak ?? "Null", null, null, null, "Load", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Features\\TTS\\Scripts\\Runtime\\Utilities\\TTSSpeaker.cs", 1176);
			RemoveQueuedRequest(requestData);
			throw new Exception("Cancelled");
		}
		await Load(textToSpeak, voiceSettings, diskCacheSettings, playbackEvents, responseNode, clearQueue: false, requestData);
	}

	private Task Load(string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents, WitResponseNode speechNode, bool clearQueue, TTSSpeakerRequestData requestPlaceholder = null)
	{
		return Load(new string[1] { textToSpeak }, voiceSettings, diskCacheSettings, playbackEvents, speechNode, clearQueue, requestPlaceholder);
	}

	private async Task Load(string[] textsToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings, TTSSpeakerClipEvents playbackEvents, WitResponseNode speechNode, bool clearQueue, TTSSpeakerRequestData requestPlaceholder = null)
	{
		if (voiceSettings == null)
		{
			voiceSettings = VoiceSettings;
		}
		if (voiceSettings == null)
		{
			string text = "No voice provided";
			Logger.Error("{0}\nPreset: {1}", text, presetVoiceID);
			RemoveQueuedRequest(requestPlaceholder);
			throw new Exception(text);
		}
		List<string> list = new List<string>();
		foreach (string textToSpeak in textsToSpeak)
		{
			List<string> finalText = GetFinalText(textToSpeak);
			if (finalText != null && finalText.Count > 0)
			{
				list.AddRange(finalText);
			}
		}
		if (list == null || list.Count == 0)
		{
			string message = "No phrases provided";
			Logger.Error(message);
			RemoveQueuedRequest(requestPlaceholder);
			throw new Exception(message);
		}
		if (clearQueue)
		{
			StopLoadingButKeepQueue();
		}
		string text2 = speechNode?["operation_id"]?.Value;
		if (string.IsNullOrEmpty(text2))
		{
			text2 = Guid.NewGuid().ToString();
		}
		Task[] tasks = new Task[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			TTSSpeakerRequestData tTSSpeakerRequestData;
			if (requestPlaceholder == null)
			{
				tTSSpeakerRequestData = CreateRequest(playbackEvents, speechNode, clearQueue);
			}
			else if (j == 0)
			{
				tTSSpeakerRequestData = requestPlaceholder;
			}
			else
			{
				tTSSpeakerRequestData = CreateRequest(playbackEvents, speechNode, clearQueue, add: false);
				lock (_queuedRequests)
				{
					int num = _queuedRequests.IndexOf(requestPlaceholder);
					_queuedRequests.Insert(num + j, tTSSpeakerRequestData);
				}
			}
			TTSClipData clipData = TTSService.GetClipData(list[j], voiceSettings, diskCacheSettings);
			clipData.queryOperationId = text2;
			tTSSpeakerRequestData.ClipData = clipData;
			RaiseEvents(RaiseOnBegin, tTSSpeakerRequestData);
			RaiseEvents(RaiseOnLoadBegin, tTSSpeakerRequestData);
			RefreshQueueEvents();
			tasks[j] = LoadClip(tTSSpeakerRequestData);
			clearQueue = false;
		}
		await Task.WhenAll(tasks);
		foreach (Task task in tasks)
		{
			if (task.Exception != null && task.Exception.InnerException != null)
			{
				throw task.Exception.InnerException;
			}
		}
	}

	private async Task LoadClip(TTSSpeakerRequestData requestData)
	{
		string errors = await TTSService.LoadAsync(requestData.ClipData, requestData.OnReady);
		FinalizeLoadedClip(requestData, errors);
		await requestData.PlaybackCompletion.Task;
		if (!string.IsNullOrEmpty(errors))
		{
			throw new Exception(errors);
		}
		if (!string.IsNullOrEmpty(requestData?.ClipData?.LoadError))
		{
			throw new Exception(requestData?.ClipData?.LoadError);
		}
	}

	private void TryPlayLoadedClip(TTSSpeakerRequestData requestData)
	{
		if (!requestData.IsReady)
		{
			requestData.IsReady = true;
			RaiseEvents(RaiseOnPlaybackReady, requestData);
			if (requestData.StopPlaybackOnLoad && IsSpeaking)
			{
				StopSpeaking();
			}
			else
			{
				RefreshPlayback();
			}
		}
	}

	private void FinalizeLoadedClip(TTSSpeakerRequestData requestData, string error)
	{
		if (string.IsNullOrEmpty(error) && (requestData.ClipData == null || !string.IsNullOrEmpty(requestData.ClipData.textToSpeak)))
		{
			if (requestData.ClipData == null)
			{
				error = "No TTSClipData found";
			}
			else if (requestData.ClipData.clipStream == null)
			{
				error = "No AudioClip found";
			}
			else if (requestData.ClipData.loadState == TTSClipLoadState.Error)
			{
				error = "Error without message";
			}
			else if (requestData.ClipData.loadState == TTSClipLoadState.Unloaded)
			{
				error = "Cancelled";
			}
		}
		if (!string.IsNullOrEmpty(error))
		{
			requestData.Error = error;
			UnloadQueuedClipRequest(requestData);
		}
		else if (!requestData.IsReady)
		{
			TryPlayLoadedClip(requestData);
		}
	}

	private void RefreshPlayback()
	{
		if (SpeakingClip != null || _queuedRequests == null || _queuedRequests.Count == 0 || _audioPlayer == null)
		{
			return;
		}
		string playbackErrors = AudioPlayer.GetPlaybackErrors();
		if (!string.IsNullOrEmpty(playbackErrors))
		{
			Logger.Error("Refresh Playback Failed\nError: " + playbackErrors);
			return;
		}
		TTSSpeakerRequestData tTSSpeakerRequestData;
		lock (_queuedRequests)
		{
			tTSSpeakerRequestData = _queuedRequests[0];
			if (tTSSpeakerRequestData == null || tTSSpeakerRequestData.ClipData == null || tTSSpeakerRequestData.ClipData.loadState != TTSClipLoadState.Loaded)
			{
				return;
			}
			_queuedRequests.RemoveAt(0);
		}
		_speakingRequest = tTSSpeakerRequestData;
		RaiseEvents(RaiseOnPlaybackBegin, _speakingRequest);
		if (_speakingRequest.StopPlaybackOnLoad && IsPaused)
		{
			Resume();
		}
		if (string.IsNullOrEmpty(_speakingRequest.ClipData.textToSpeak))
		{
			HandlePlaybackComplete(stopped: false);
			return;
		}
		if (_speakingRequest.ClipData.clipStream == null)
		{
			HandlePlaybackComplete(stopped: true);
			return;
		}
		ThreadUtility.CallOnMainThread(delegate
		{
			AudioPlayer.Play(_speakingRequest.ClipData.clipStream, 0, _speakingRequest.SpeechNode);
		});
		if (_waitForCompletion != null)
		{
			StopCoroutine(_waitForCompletion);
			_waitForCompletion = null;
		}
		_waitForCompletion = StartCoroutine(WaitForPlaybackComplete());
	}

	private IEnumerator WaitForPlaybackComplete()
	{
		int sample = -1;
		_elapsedPlayTime = 0f;
		while (!IsPlaybackComplete())
		{
			yield return null;
			if (!IsPaused)
			{
				_elapsedPlayTime += Time.deltaTime;
			}
			bool flag = !AudioPlayer.IsPlaying;
			if (IsPaused != flag)
			{
				if (IsPaused)
				{
					AudioPlayer.Pause();
				}
				else
				{
					AudioPlayer.Resume();
				}
			}
			int elapsedSamples = ElapsedSamples;
			if (sample != elapsedSamples)
			{
				sample = elapsedSamples;
				RaisePlaybackSampleUpdated(sample);
			}
		}
		HandlePlaybackComplete(stopped: false);
	}

	protected virtual bool IsPlaybackComplete()
	{
		if (!AudioPlayer.IsPlaying && !IsPaused)
		{
			return true;
		}
		if (AudioPlayer?.ClipStream == null)
		{
			return true;
		}
		if (AudioPlayer.ClipStream.IsComplete)
		{
			return ElapsedSamples >= TotalSamples;
		}
		return false;
	}

	protected virtual void HandlePlaybackComplete(bool stopped)
	{
		if (_waitForCompletion != null)
		{
			StopCoroutine(_waitForCompletion);
			_waitForCompletion = null;
		}
		TTSSpeakerRequestData speakingRequest = _speakingRequest;
		_speakingRequest = null;
		RaisePlaybackSampleUpdated(0);
		ThreadUtility.CallOnMainThread(delegate
		{
			AudioPlayer.Stop();
		}).WrapErrors();
		if (stopped)
		{
			RaiseEvents(RaiseOnPlaybackCancelled, speakingRequest, "Playback stopped manually");
		}
		else if (speakingRequest.ClipData.loadState == TTSClipLoadState.Unloaded)
		{
			RaiseEvents(RaiseOnPlaybackCancelled, speakingRequest, "TTSClipData was unloaded");
		}
		else if (speakingRequest.ClipData.clipStream == null)
		{
			RaiseEvents(RaiseOnPlaybackCancelled, speakingRequest, "AudioClip no longer exists");
		}
		else
		{
			RaiseEvents(RaiseOnPlaybackComplete, speakingRequest);
		}
		RefreshQueueEvents();
		RefreshPlayback();
	}

	public void Pause()
	{
		SetPause(toPaused: true);
	}

	public void Resume()
	{
		SetPause(toPaused: false);
	}

	public void PrepareToSpeak()
	{
	}

	public void StartTextBlock()
	{
	}

	public void EndTextBlock()
	{
	}

	protected virtual void SetPause(bool toPaused)
	{
		if (IsPaused == toPaused)
		{
			return;
		}
		IsPaused = toPaused;
		Log("Speak Audio " + (IsPaused ? "Paused" : "Resumed"));
		if (IsSpeaking)
		{
			if (IsPaused)
			{
				AudioPlayer.Pause();
			}
			else if (!IsPaused)
			{
				AudioPlayer.Resume();
			}
		}
	}

	private bool UnloadQueuedClipRequest(TTSSpeakerRequestData requestData)
	{
		return FindAndUnloadRequests(RequestEquals, requestData);
	}

	private bool UnloadQueuedClip(TTSClipData clipData)
	{
		return FindAndUnloadRequests(RequestHasClipData, clipData);
	}

	private bool UnloadQueuedText(string textToSpeak)
	{
		return FindAndUnloadRequests(RequestHasClipText, textToSpeak);
	}

	private void RemoveQueuedRequest(TTSSpeakerRequestData requestData)
	{
		if (_queuedRequests == null || !_queuedRequests.Contains(requestData))
		{
			return;
		}
		lock (_queuedRequests)
		{
			_queuedRequests.Remove(requestData);
		}
	}

	private bool FindAndUnloadRequests<T>(Func<TTSSpeakerRequestData, T, bool> findMethod, T findParameter)
	{
		if (_queuedRequests == null)
		{
			return false;
		}
		bool flag = false;
		int num = 0;
		while (num < _queuedRequests.Count)
		{
			TTSSpeakerRequestData tTSSpeakerRequestData = _queuedRequests[num];
			if (tTSSpeakerRequestData == null || findMethod(tTSSpeakerRequestData, findParameter))
			{
				flag = true;
				lock (_queuedRequests)
				{
					_queuedRequests.RemoveAt(num);
				}
				RaiseUnloadEvents(tTSSpeakerRequestData);
			}
			else
			{
				num++;
			}
		}
		if (!flag)
		{
			return false;
		}
		RefreshQueueEvents();
		return true;
	}

	private void RaiseUnloadEvents(TTSSpeakerRequestData requestData)
	{
		string text = requestData?.Error;
		if (string.IsNullOrEmpty(text))
		{
			text = requestData?.ClipData?.LoadError;
		}
		if (requestData != null && requestData.Equals(_speakingRequest))
		{
			RaiseEvents(RaiseOnPlaybackCancelled, requestData, text);
		}
		else if (string.IsNullOrEmpty(text) || string.Equals(text, "Cancelled"))
		{
			RaiseEvents(RaiseOnLoadAborted, requestData);
		}
		else
		{
			RaiseEvents(RaiseOnLoadFailed, requestData, text);
		}
	}

	private void Log(string format, params object[] parameters)
	{
		if (verboseLogging)
		{
			Logger.Verbose(format, parameters);
		}
	}

	private void Error(string format, params object[] parameters)
	{
		Logger.Warning(format, parameters);
	}

	private void LogRequest(string comment, TTSSpeakerRequestData requestData, string error = null)
	{
		if (verboseLogging || !string.IsNullOrEmpty(error))
		{
			if (!string.IsNullOrEmpty(error))
			{
				Error("{0}\n{1}\nElapsed: {2:0.00} seconds\nAudio Player Type: {3}\nError: {4}", comment, requestData.ClipData, (DateTime.UtcNow - requestData.StartTime).TotalSeconds, _audioPlayer?.GetType().Name ?? "Null", error);
			}
			else
			{
				Log("{0}\n{1}\nElapsed: {2:0.00} seconds\nAudio Player Type: {3}", comment, requestData.ClipData, (DateTime.UtcNow - requestData.StartTime).TotalSeconds, _audioPlayer?.GetType().Name ?? "Null");
			}
		}
	}

	private void RaiseEvents(Action events)
	{
		ThreadUtility.CallOnMainThread(Logger, events.Invoke).WrapErrors();
	}

	private void RaiseEvents<T>(Action<T> events, T parameter)
	{
		RaiseEvents(delegate
		{
			events?.Invoke(parameter);
		});
	}

	private void RaiseEvents<T1, T2>(Action<T1, T2> events, T1 parameter1, T2 parameter2)
	{
		RaiseEvents(delegate
		{
			events?.Invoke(parameter1, parameter2);
		});
	}

	protected virtual void RaiseOnPlaybackQueueBegin()
	{
		Log("Playback Queue Begin");
		Events?.OnPlaybackQueueBegin?.Invoke();
	}

	protected virtual void RaiseOnPlaybackQueueComplete()
	{
		Log("Playback Queue Complete");
		Events?.OnPlaybackQueueComplete?.Invoke();
	}

	private void RaiseOnBegin(TTSSpeakerRequestData requestData)
	{
		LogRequest("Speak Begin", requestData);
		Events?.OnInit?.Invoke(this, requestData.ClipData);
		requestData.ClipData?.onRequestBegin?.Invoke(requestData.ClipData);
		requestData.PlaybackEvents?.OnInit?.Invoke(this, requestData.ClipData);
	}

	private void RaiseOnLoadBegin(TTSSpeakerRequestData requestData)
	{
		LogRequest("Load Begin", requestData);
		Events?.OnClipDataQueued?.Invoke(requestData.ClipData);
		Events?.OnClipDataLoadBegin?.Invoke(requestData.ClipData);
		Events?.OnClipLoadBegin?.Invoke(this, requestData.ClipData?.textToSpeak);
		Events?.OnLoadBegin?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnLoadBegin?.Invoke(this, requestData.ClipData);
	}

	private void RaiseOnLoadAborted(TTSSpeakerRequestData requestData)
	{
		LogRequest("Load Aborted", requestData);
		Events?.OnClipDataLoadAbort?.Invoke(requestData.ClipData);
		Events?.OnClipLoadAbort?.Invoke(this, requestData.ClipData?.textToSpeak);
		Events?.OnLoadAbort?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnLoadAbort?.Invoke(this, requestData.ClipData);
		RaiseOnComplete(requestData);
	}

	private void RaiseOnLoadFailed(TTSSpeakerRequestData requestData, string error)
	{
		if (string.Equals(error, "Cancelled"))
		{
			RaiseOnLoadAborted(requestData);
			return;
		}
		LogRequest("Load Failed", requestData, error);
		Events?.OnClipDataLoadFailed?.Invoke(requestData.ClipData);
		Events?.OnClipLoadFailed?.Invoke(this, requestData.ClipData?.textToSpeak);
		Events?.OnLoadFailed?.Invoke(this, requestData.ClipData, error);
		requestData.PlaybackEvents?.OnLoadFailed?.Invoke(this, requestData.ClipData, error);
		RaiseOnComplete(requestData);
	}

	private void RaiseOnPlaybackReady(TTSSpeakerRequestData requestData)
	{
		LogRequest("Playback Ready", requestData);
		Events?.OnClipDataLoadSuccess?.Invoke(requestData.ClipData);
		Events?.OnClipLoadSuccess?.Invoke(this, requestData.ClipData?.textToSpeak);
		Events?.OnClipDataPlaybackReady?.Invoke(requestData.ClipData);
		Events?.OnLoadSuccess?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnLoadSuccess?.Invoke(this, requestData.ClipData);
		Events?.OnAudioClipPlaybackReady?.Invoke(requestData.ClipData?.clip);
		requestData.PlaybackEvents?.OnAudioClipPlaybackReady?.Invoke(requestData.ClipData?.clip);
		requestData.ClipData?.onPlaybackQueued?.Invoke(requestData.ClipData);
		Events?.OnPlaybackReady?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnPlaybackReady?.Invoke(this, requestData.ClipData);
	}

	private void RaiseOnPlaybackBegin(TTSSpeakerRequestData requestData)
	{
		LogRequest("Playback Begin", requestData);
		Events?.OnTextPlaybackStart?.Invoke(requestData.ClipData?.textToSpeak);
		requestData.PlaybackEvents?.OnTextPlaybackStart?.Invoke(requestData.ClipData?.textToSpeak);
		Events?.OnAudioClipPlaybackStart?.Invoke(requestData.ClipData?.clip);
		requestData.PlaybackEvents?.OnAudioClipPlaybackStart?.Invoke(requestData.ClipData?.clip);
		Events?.OnClipDataPlaybackStart?.Invoke(requestData.ClipData);
		Events?.OnStartSpeaking?.Invoke(this, requestData.ClipData?.textToSpeak);
		requestData.ClipData?.onPlaybackBegin?.Invoke(requestData.ClipData);
		Events?.OnPlaybackStart?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnPlaybackStart?.Invoke(this, requestData.ClipData);
	}

	private void RaiseOnPlaybackCancelled(TTSSpeakerRequestData requestData, string reason)
	{
		LogRequest("Playback Cancelled\nReason: " + reason, requestData);
		Events?.OnTextPlaybackCancelled?.Invoke(requestData.ClipData?.textToSpeak);
		requestData.PlaybackEvents?.OnTextPlaybackCancelled?.Invoke(requestData.ClipData?.textToSpeak);
		Events?.OnAudioClipPlaybackCancelled?.Invoke(requestData.ClipData?.clip);
		requestData.PlaybackEvents?.OnAudioClipPlaybackCancelled?.Invoke(requestData.ClipData?.clip);
		Events?.OnClipDataPlaybackCancelled?.Invoke(requestData.ClipData);
		Events?.OnCancelledSpeaking?.Invoke(this, requestData.ClipData?.textToSpeak);
		requestData.ClipData?.onPlaybackComplete?.Invoke(requestData.ClipData);
		Events?.OnPlaybackCancelled?.Invoke(this, requestData.ClipData, reason);
		requestData.PlaybackEvents?.OnPlaybackCancelled?.Invoke(this, requestData.ClipData, reason);
		RaiseOnComplete(requestData);
	}

	private void RaiseOnPlaybackComplete(TTSSpeakerRequestData requestData)
	{
		LogRequest("Playback Complete", requestData);
		Events?.OnTextPlaybackFinished?.Invoke(requestData.ClipData?.textToSpeak);
		requestData.PlaybackEvents?.OnTextPlaybackFinished?.Invoke(requestData.ClipData?.textToSpeak);
		Events?.OnAudioClipPlaybackFinished?.Invoke(requestData.ClipData?.clip);
		requestData.PlaybackEvents?.OnAudioClipPlaybackFinished?.Invoke(requestData.ClipData?.clip);
		Events?.OnClipDataPlaybackFinished?.Invoke(requestData.ClipData);
		Events?.OnFinishedSpeaking?.Invoke(this, requestData.ClipData?.textToSpeak);
		requestData.ClipData?.onPlaybackComplete?.Invoke(requestData.ClipData);
		Events?.OnPlaybackComplete?.Invoke(this, requestData.ClipData);
		requestData.PlaybackEvents?.OnPlaybackComplete?.Invoke(this, requestData.ClipData);
		RaiseOnComplete(requestData);
	}

	private void RaiseOnComplete(TTSSpeakerRequestData requestData)
	{
		LogRequest("Speak Complete", requestData);
		Events?.OnComplete?.Invoke(this, requestData.ClipData);
		requestData.ClipData?.onRequestComplete?.Invoke(requestData.ClipData);
		requestData.PlaybackEvents?.OnComplete?.Invoke(this, requestData.ClipData);
		requestData.PlaybackCompletion?.TrySetResult(result: true);
	}

	protected virtual void RaisePlaybackSampleUpdated(int sample)
	{
		OnSampleUpdated?.Invoke(sample);
	}
}
