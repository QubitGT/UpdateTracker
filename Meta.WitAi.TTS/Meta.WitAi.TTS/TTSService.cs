using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Audio;
using Meta.Voice.Logging;
using Meta.WitAi.Attributes;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Events;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Interfaces;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.TTS;

[LogCategory(LogCategory.TextToSpeech)]
public abstract class TTSService : MonoBehaviour, ILogSource
{
	[SerializeField]
	private bool verboseLogging;

	private static TTSService _instance;

	[Header("TTS Modules")]
	[Tooltip("Audio system to be used for obtaining audio clip streams.")]
	[SerializeField]
	[ObjectType(typeof(IAudioSystem), new Type[] { })]
	private UnityEngine.Object _audioSystem;

	[Tooltip("Runtime cache that assists with the temporary storage of audio clips.")]
	[SerializeField]
	[ObjectType(typeof(ITTSRuntimeCacheHandler), new Type[] { })]
	private UnityEngine.Object _runtimeCacheHandler;

	[Tooltip("Disk cache that assists with the backup and retrieval of audio clips saved to disk.")]
	[SerializeField]
	[ObjectType(typeof(ITTSDiskCacheHandler), new Type[] { })]
	private UnityEngine.Object _diskCacheHandler;

	[Header("Event Settings")]
	[SerializeField]
	private TTSServiceEvents _events = new TTSServiceEvents();

	private bool _isActive;

	private bool _hasListeners;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.TextToSpeech);


	public static TTSService Instance
	{
		get
		{
			if ((object)_instance == null)
			{
				_instance = GameObjectSearchUtility.FindSceneObject<TTSService>();
			}
			return _instance;
		}
	}

	public IAudioSystem AudioSystem
	{
		get
		{
			return _audioSystem as IAudioSystem;
		}
		set
		{
			_audioSystem = SetInterface(value);
		}
	}

	public ITTSRuntimeCacheHandler RuntimeCacheHandler
	{
		get
		{
			return _runtimeCacheHandler as ITTSRuntimeCacheHandler;
		}
		set
		{
			_runtimeCacheHandler = SetInterface(value);
		}
	}

	public ITTSDiskCacheHandler DiskCacheHandler
	{
		get
		{
			return _diskCacheHandler as ITTSDiskCacheHandler;
		}
		set
		{
			_diskCacheHandler = SetInterface(value);
		}
	}

	public abstract ITTSWebHandler WebHandler { get; }

	public abstract ITTSVoiceProvider VoiceProvider { get; }

	public TTSServiceEvents Events => _events;

	public VoiceErrorSimulationType SimulatedErrorType { get; set; } = (VoiceErrorSimulationType)(-1);


	public static event Action<TTSService> OnServiceStart;

	public static event Action<TTSService> OnServiceDestroy;

	public virtual string GetInvalidError()
	{
		if (WebHandler == null)
		{
			return "Web Handler Missing";
		}
		if (VoiceProvider == null)
		{
			return "Voice Provider Missing";
		}
		return string.Empty;
	}

	protected virtual void Awake()
	{
		_instance = this;
	}

	protected virtual void Start()
	{
		TTSService.OnServiceStart?.Invoke(this);
	}

	protected virtual void OnEnable()
	{
		_isActive = true;
		SetListeners(add: true);
		string invalidError = GetInvalidError();
		if (!string.IsNullOrEmpty(invalidError))
		{
			Log(invalidError, null, VLoggerVerbosity.Warning);
		}
	}

	protected virtual void OnDisable()
	{
		_isActive = false;
		SetListeners(add: false);
	}

	protected virtual void SetListeners(bool add)
	{
		if (_hasListeners == add)
		{
			return;
		}
		_hasListeners = add;
		if (add)
		{
			AudioSystem = GetOrCreateInterface<IAudioSystem, UnityAudioSystem>(AudioSystem);
			RuntimeCacheHandler = GetOrCreateInterface<ITTSRuntimeCacheHandler, TTSRuntimeLRUCache>(RuntimeCacheHandler);
			DiskCacheHandler = GetInterface(DiskCacheHandler);
		}
		if (RuntimeCacheHandler != null)
		{
			if (add)
			{
				RuntimeCacheHandler.OnClipAdded += OnRuntimeClipAdded;
				RuntimeCacheHandler.OnClipRemoved += OnRuntimeClipRemoved;
			}
			else
			{
				RuntimeCacheHandler.OnClipAdded -= OnRuntimeClipAdded;
				RuntimeCacheHandler.OnClipRemoved -= OnRuntimeClipRemoved;
			}
		}
	}

	protected TInterface GetInterface<TInterface>(TInterface current)
	{
		if (current is UnityEngine.Object @object && (bool)@object)
		{
			return current;
		}
		return base.gameObject.GetComponent<TInterface>();
	}

	protected TInterface GetOrCreateInterface<TInterface, TDefault>(TInterface current) where TDefault : MonoBehaviour, TInterface
	{
		TInterface @interface = GetInterface(current);
		if (@interface is UnityEngine.Object @object && (bool)@object)
		{
			return @interface;
		}
		return (TInterface)base.gameObject.AddComponent<TDefault>();
	}

	private UnityEngine.Object SetInterface<TInterface>(TInterface newValue)
	{
		if (newValue is UnityEngine.Object result)
		{
			return result;
		}
		if (newValue != null)
		{
			Logger.Error("Set {0} Failed\nCannot set {1} to a UnityEngine.Object property", typeof(TInterface).Name, newValue.GetType().Name);
		}
		return null;
	}

	protected virtual void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
		UnloadAll();
		TTSService.OnServiceDestroy?.Invoke(this);
	}

	private void Log(string logMessage, TTSClipData clipData = null, VLoggerVerbosity logLevel = VLoggerVerbosity.Verbose)
	{
		Logger.Log(Logger.CorrelationID, logLevel, "{0}\n{1}", logMessage, (clipData == null) ? ((object)"") : ((object)clipData));
	}

	private void LogState(TTSClipData clipData, string message, bool fromDisk, string error = null)
	{
		if (!string.IsNullOrEmpty(error))
		{
			Logger.Warning("{0} {1}\nText: {2}\nVoice: {3}\nReady: {4:0.00} seconds\nRequest Id: {5}\nError: {6}", fromDisk ? "Disk" : "Web", message, clipData?.textToSpeak ?? "Null", clipData?.voiceSettings?.SettingsId ?? "Null", clipData?.readyDuration ?? 0f, clipData?.queryRequestId ?? "Null", error);
		}
		else if (verboseLogging)
		{
			Logger.Verbose("{0} {1}\nText: {2}\nVoice: {3}\nReady: {4:0.00} seconds\nRequest Id: {5}", fromDisk ? "Disk" : "Web", message, clipData?.textToSpeak ?? "Null", clipData?.voiceSettings?.SettingsId ?? "Null", clipData?.readyDuration ?? 0f, clipData?.queryRequestId ?? "Null");
		}
	}

	public virtual string GetFinalText(string textToSpeak, TTSVoiceSettings voiceSettings)
	{
		if (voiceSettings == null)
		{
			voiceSettings = VoiceProvider?.VoiceDefaultSettings;
		}
		if (voiceSettings == null || string.IsNullOrEmpty(textToSpeak) || (string.IsNullOrEmpty(voiceSettings.PrependedText) && string.IsNullOrEmpty(voiceSettings.AppendedText)))
		{
			return textToSpeak;
		}
		return voiceSettings.PrependedText + textToSpeak + voiceSettings.AppendedText;
	}

	public string GetClipID(string textToSpeak, TTSVoiceSettings voiceSettings)
	{
		string finalText = GetFinalText(textToSpeak, voiceSettings);
		return GetClipIDWithFinalText(finalText, voiceSettings);
	}

	protected virtual string GetClipIDWithFinalText(string formattedText, TTSVoiceSettings voiceSettings)
	{
		if (string.IsNullOrEmpty(formattedText))
		{
			return "EMPTY";
		}
		string text = formattedText;
		if (VoiceProvider?.PresetVoiceSettings != null && VoiceProvider.PresetVoiceSettings.Length != 0)
		{
			if (voiceSettings == null)
			{
				voiceSettings = VoiceProvider?.VoiceDefaultSettings;
			}
			if (voiceSettings != null)
			{
				text = text + "|" + voiceSettings.UniqueId;
			}
		}
		if (DiskCacheHandler != null)
		{
			int hashCode = text.GetHashCode();
			text = string.Format("tts_{0}{1}", (hashCode < 0) ? "n" : "p", Mathf.Abs(hashCode));
		}
		return text;
	}

	public TTSClipData GetClipData(string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings)
	{
		SetListeners(add: true);
		if (voiceSettings == null)
		{
			voiceSettings = VoiceProvider?.VoiceDefaultSettings;
		}
		if (diskCacheSettings == null)
		{
			diskCacheSettings = DiskCacheHandler?.DiskCacheDefaultSettings;
		}
		string finalText = GetFinalText(textToSpeak, voiceSettings);
		string clipIDWithFinalText = GetClipIDWithFinalText(finalText, voiceSettings);
		TTSClipData runtimeCachedClip = GetRuntimeCachedClip(clipIDWithFinalText);
		if (runtimeCachedClip != null && string.Equals(runtimeCachedClip.clipID, clipIDWithFinalText))
		{
			return runtimeCachedClip;
		}
		return WebHandler.CreateClipData(clipIDWithFinalText, finalText, voiceSettings, diskCacheSettings);
	}

	protected virtual void SetClipLoadState(TTSClipData clipData, TTSClipLoadState loadState)
	{
		clipData.loadState = loadState;
		RaiseEvents(delegate
		{
			clipData.onStateChange?.Invoke(clipData, clipData.loadState);
		});
	}

	public bool DecodeTts(WitResponseNode responseNode, out string textToSpeak, out TTSVoiceSettings voiceSettings)
	{
		return WebHandler.DecodeTtsFromJson(responseNode, out textToSpeak, out voiceSettings);
	}

	public TTSClipData Load(string textToSpeak, string presetVoiceId = null, TTSDiskCacheSettings diskCacheSettings = null, Action<TTSClipData> onStreamReady = null, Action<TTSClipData, string> onStreamComplete = null)
	{
		return Load(textToSpeak, GetPresetVoiceSettings(presetVoiceId), diskCacheSettings, onStreamReady, onStreamComplete);
	}

	public TTSClipData Load(string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings = null, Action<TTSClipData> onStreamReady = null, Action<TTSClipData, string> onStreamComplete = null)
	{
		TTSClipData clipData = GetClipData(textToSpeak, voiceSettings, diskCacheSettings);
		LoadAsync(clipData, onStreamReady, onStreamComplete).WrapErrors();
		return clipData;
	}

	public async Task<string> LoadAsync(TTSClipData clipData, Action<TTSClipData> onStreamReady = null, Action<TTSClipData, string> onStreamComplete = null)
	{
		if (clipData == null)
		{
			string text = "No clip provided";
			Log(text, null, VLoggerVerbosity.Error);
			onStreamComplete?.Invoke(null, text);
			return text;
		}
		if (!_isActive)
		{
			string text2 = "Cannot load clip while inactive";
			Log(text2, null, VLoggerVerbosity.Error);
			onStreamComplete?.Invoke(clipData, text2);
			return text2;
		}
		bool flag = clipData.loadState == TTSClipLoadState.Unloaded;
		if (RuntimeCacheHandler != null)
		{
			if (!RuntimeCacheHandler.AddClip(clipData))
			{
				string text3 = "Runtime cache refused to load";
				Log(text3, clipData, VLoggerVerbosity.Error);
				onStreamComplete?.Invoke(clipData, text3);
				return text3;
			}
		}
		else
		{
			RaiseLoadBegin(clipData);
		}
		if (onStreamReady != null)
		{
			clipData.onPlaybackReady = (Action<TTSClipData>)Delegate.Combine(clipData.onPlaybackReady, new Action<TTSClipData>(onStreamReady.Invoke));
		}
		if (flag)
		{
			if (string.IsNullOrEmpty(clipData.textToSpeak))
			{
				RaiseWebStreamBegin(clipData);
				RaiseWebStreamReady(clipData);
				RaiseWebStreamComplete(clipData);
				onStreamComplete?.Invoke(clipData, string.Empty);
				return string.Empty;
			}
			if (ShouldCacheToDisk(clipData))
			{
				string text4 = await PerformDownloadAndStream(clipData);
				onStreamComplete?.Invoke(clipData, text4);
				return text4;
			}
			string text5 = await PerformStreamFromWeb(clipData);
			onStreamComplete?.Invoke(clipData, text5);
			return text5;
		}
		if (clipData.loadState == TTSClipLoadState.Preparing)
		{
			await clipData.LoadCompletion.Task;
		}
		else if (clipData.loadState == TTSClipLoadState.Loaded)
		{
			onStreamReady?.Invoke(clipData);
		}
		onStreamComplete?.Invoke(clipData, clipData.LoadError);
		return clipData.LoadError;
	}

	private async Task<string> PerformDownloadAndStream(TTSClipData clipData)
	{
		string text = await DownloadAsync(clipData);
		if (string.Equals(text, "Preloaded files cannot be downloaded at runtime. The file will be streamed instead. If you wish to download this file at runtime, use the temporary or permanent cache."))
		{
			return await PerformStreamFromWeb(clipData);
		}
		if (!string.IsNullOrEmpty(text))
		{
			RaiseDiskStreamBegin(clipData);
			RaiseDiskStreamError(clipData, -1, text);
			return text;
		}
		return await PerformStreamFromDisk(clipData);
	}

	private async Task<string> PerformStreamFromWeb(TTSClipData clipData)
	{
		RaiseWebStreamBegin(clipData);
		if (clipData.loadState != TTSClipLoadState.Preparing)
		{
			RaiseWebStreamCancel(clipData);
			return clipData.LoadError;
		}
		string text = ((WebHandler == null) ? "No web handler found" : WebHandler.GetWebErrors(clipData));
		if (!string.IsNullOrEmpty(text))
		{
			RaiseWebStreamError(clipData, -1, text);
			return clipData.LoadError;
		}
		string text2 = await WebHandler.RequestStreamFromWeb(clipData, RaiseWebStreamReady);
		if (!string.IsNullOrEmpty(text2))
		{
			RaiseWebStreamError(clipData, (clipData.LoadStatusCode != 0) ? clipData.LoadStatusCode : (-1), text2);
			return clipData.LoadError;
		}
		RaiseWebStreamComplete(clipData);
		return clipData.LoadError;
	}

	private async Task<string> PerformStreamFromDisk(TTSClipData clipData)
	{
		RaiseDiskStreamBegin(clipData);
		if (clipData.loadState != TTSClipLoadState.Preparing)
		{
			RaiseDiskStreamCancel(clipData);
			return clipData.LoadError;
		}
		string diskCachePath = DiskCacheHandler.GetDiskCachePath(clipData);
		string text = await WebHandler.RequestStreamFromDisk(clipData, diskCachePath, RaiseDiskStreamReady);
		if (!string.IsNullOrEmpty(text))
		{
			RaiseDiskStreamError(clipData, -1, text);
			return clipData.LoadError;
		}
		RaiseDiskStreamComplete(clipData);
		return clipData.LoadError;
	}

	public void UnloadAll()
	{
		TTSClipData[] array = RuntimeCacheHandler?.GetClips();
		if (array == null)
		{
			return;
		}
		foreach (TTSClipData item in new HashSet<TTSClipData>(array))
		{
			Unload(item);
		}
	}

	public void Unload(TTSClipData clipData)
	{
		if (RuntimeCacheHandler != null)
		{
			RuntimeCacheHandler.RemoveClip(clipData.clipID);
		}
		else
		{
			RaiseUnloadComplete(clipData);
		}
	}

	public TTSClipData GetRuntimeCachedClip(string clipID)
	{
		return RuntimeCacheHandler?.GetClip(clipID);
	}

	public TTSClipData[] GetAllRuntimeCachedClips()
	{
		return RuntimeCacheHandler?.GetClips();
	}

	protected virtual void OnRuntimeClipAdded(TTSClipData clipData)
	{
		RaiseLoadBegin(clipData);
	}

	protected virtual void OnRuntimeClipRemoved(TTSClipData clipData)
	{
		RaiseUnloadComplete(clipData);
	}

	public bool ShouldCacheToDisk(TTSClipData clipData)
	{
		if (DiskCacheHandler != null && DiskCacheHandler.ShouldCacheToDisk(clipData))
		{
			return !string.IsNullOrEmpty(clipData.textToSpeak);
		}
		return false;
	}

	public string GetDiskCachePath(string textToSpeak, string clipID, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings)
	{
		return DiskCacheHandler?.GetDiskCachePath(GetClipData(textToSpeak, voiceSettings, diskCacheSettings));
	}

	public TTSClipData DownloadToDiskCache(string textToSpeak, string presetVoiceId, TTSDiskCacheSettings diskCacheSettings = null, Action<TTSClipData, string, string> onDownloadComplete = null)
	{
		return DownloadToDiskCache(textToSpeak, GetPresetVoiceSettings(presetVoiceId), diskCacheSettings, onDownloadComplete);
	}

	public TTSClipData DownloadToDiskCache(string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings = null, Action<TTSClipData, string, string> onDownloadComplete = null)
	{
		TTSClipData clipData = GetClipData(textToSpeak, voiceSettings, diskCacheSettings);
		DownloadAsync(clipData, onDownloadComplete).WrapErrors();
		return clipData;
	}

	public async Task<string> DownloadAsync(string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings)
	{
		TTSClipData clipData = GetClipData(textToSpeak, voiceSettings, diskCacheSettings);
		return await DownloadAsync(clipData);
	}

	private async Task<string> DownloadAsync(TTSClipData clipData, Action<TTSClipData, string, string> onDownloadComplete = null)
	{
		SetListeners(add: true);
		if (clipData == null)
		{
			string text = "Cannot download with null clip data";
			onDownloadComplete?.Invoke(clipData, null, text);
			return text;
		}
		if (DiskCacheHandler == null)
		{
			string text2 = "Cannot download without disk cache handler";
			onDownloadComplete?.Invoke(clipData, null, text2);
			return text2;
		}
		string downloadPath = DiskCacheHandler.GetDiskCachePath(clipData);
		Tuple<bool, string> tuple = await ShouldDownload(clipData, downloadPath);
		if (!tuple.Item1)
		{
			if (!string.IsNullOrEmpty(tuple.Item2))
			{
				RaiseDownloadBegin(clipData, downloadPath);
				RaiseDownloadError(clipData, downloadPath, tuple.Item2);
			}
			onDownloadComplete?.Invoke(clipData, downloadPath, tuple.Item2);
			return tuple.Item2;
		}
		RaiseDownloadBegin(clipData, downloadPath);
		string text3 = await WebHandler.RequestDownloadFromWeb(clipData, downloadPath);
		if (string.Equals(clipData.LoadError, "Cancelled"))
		{
			RaiseDownloadCancel(clipData, downloadPath);
		}
		else if (!string.IsNullOrEmpty(text3))
		{
			RaiseDownloadError(clipData, downloadPath, text3);
		}
		else
		{
			RaiseDownloadSuccess(clipData, downloadPath);
		}
		onDownloadComplete?.Invoke(clipData, downloadPath, text3);
		return text3;
	}

	private async Task<Tuple<bool, string>> ShouldDownload(TTSClipData clipData, string downloadPath)
	{
		if (string.IsNullOrEmpty(clipData.textToSpeak))
		{
			return new Tuple<bool, string>(item1: false, string.Empty);
		}
		string text = await WebHandler.IsDownloadedToDisk(downloadPath);
		if (string.IsNullOrEmpty(text))
		{
			return new Tuple<bool, string>(item1: false, string.Empty);
		}
		if (string.Equals(clipData.LoadError, "Cancelled"))
		{
			return new Tuple<bool, string>(item1: false, "Cancelled");
		}
		if (Application.isPlaying && clipData.diskCacheSettings.DiskCacheLocation == TTSDiskCacheLocation.Preload)
		{
			return new Tuple<bool, string>(item1: false, "Preloaded files cannot be downloaded at runtime. The file will be streamed instead. If you wish to download this file at runtime, use the temporary or permanent cache.");
		}
		string webErrors = WebHandler.GetWebErrors(clipData);
		if (!string.IsNullOrEmpty(webErrors))
		{
			return new Tuple<bool, string>(item1: false, webErrors);
		}
		return new Tuple<bool, string>(item1: true, text);
	}

	public TTSVoiceSettings[] GetAllPresetVoiceSettings()
	{
		return VoiceProvider?.PresetVoiceSettings;
	}

	public TTSVoiceSettings GetPresetVoiceSettings(string presetVoiceId)
	{
		if (VoiceProvider == null || VoiceProvider.PresetVoiceSettings == null)
		{
			return null;
		}
		return Array.Find(VoiceProvider.PresetVoiceSettings, (TTSVoiceSettings v) => string.Equals(v.SettingsId, presetVoiceId, StringComparison.CurrentCultureIgnoreCase));
	}

	private void RaiseLoadBegin(TTSClipData clipData, bool download = false)
	{
		SetClipLoadState(clipData, TTSClipLoadState.Preparing);
		RaiseEvents(delegate
		{
			if (verboseLogging)
			{
				Logger.Verbose("Clip Loading\nText: {0}", clipData.textToSpeak, null, null, null, "RaiseLoadBegin", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Features\\TTS\\Scripts\\Runtime\\TTSService.cs", 905);
			}
			Events?.OnClipCreated?.Invoke(clipData);
		});
	}

	private void RaiseUnloadComplete(TTSClipData clipData, bool download = false)
	{
		WebHandler?.CancelRequests(clipData);
		clipData.clipStream?.Unload();
		clipData.clipStream = null;
		if (clipData.loadState == TTSClipLoadState.Preparing)
		{
			clipData.LoadStatusCode = -6;
			clipData.LoadError = "Cancelled";
		}
		if (clipData.loadState != TTSClipLoadState.Error)
		{
			SetClipLoadState(clipData, TTSClipLoadState.Unloaded);
		}
		RaiseEvents(delegate
		{
			if (verboseLogging)
			{
				Logger.Verbose("Clip Unloaded\nText: {0}", clipData.textToSpeak, null, null, null, "RaiseUnloadComplete", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Features\\TTS\\Scripts\\Runtime\\TTSService.cs", 930);
			}
			Events?.OnClipUnloaded?.Invoke(clipData);
		});
	}

	private void RaiseDiskStreamBegin(TTSClipData clipData)
	{
		RaiseStreamBegin(clipData, fromDisk: true);
	}

	private void RaiseWebStreamBegin(TTSClipData clipData)
	{
		RaiseStreamBegin(clipData, fromDisk: false);
	}

	private void RaiseStreamBegin(TTSClipData clipData, bool fromDisk)
	{
		RaiseEvents(delegate
		{
			LogState(clipData, "Stream Begin", fromDisk);
			Events?.Stream?.OnStreamBegin?.Invoke(clipData);
		});
	}

	private void RaiseDiskStreamError(TTSClipData clipData, int errorCode, string error)
	{
		RaiseStreamError(clipData, errorCode, error, fromDisk: true);
	}

	private void RaiseWebStreamError(TTSClipData clipData, int errorCode, string error)
	{
		RaiseStreamError(clipData, errorCode, error, fromDisk: false);
	}

	private void RaiseStreamError(TTSClipData clipData, int errorCode, string error, bool fromDisk)
	{
		if (error.Equals("Cancelled"))
		{
			RaiseStreamCancel(clipData, fromDisk);
			return;
		}
		clipData.LoadStatusCode = errorCode;
		clipData.LoadError = error;
		SetClipLoadState(clipData, TTSClipLoadState.Error);
		RaiseEvents(delegate
		{
			LogState(clipData, "Stream Error", fromDisk, error);
			Events?.Stream?.OnStreamError?.Invoke(clipData, error);
			if (!clipData.LoadReady.Task.IsCompleted)
			{
				clipData.LoadReady.SetResult(result: false);
			}
			RaiseStreamComplete(clipData, fromDisk);
		});
	}

	private void RaiseDiskStreamCancel(TTSClipData clipData)
	{
		RaiseStreamCancel(clipData, fromDisk: true);
	}

	private void RaiseWebStreamCancel(TTSClipData clipData)
	{
		RaiseStreamCancel(clipData, fromDisk: false);
	}

	private void RaiseStreamCancel(TTSClipData clipData, bool fromDisk)
	{
		clipData.LoadStatusCode = -6;
		clipData.LoadError = "Cancelled";
		SetClipLoadState(clipData, TTSClipLoadState.Error);
		RaiseEvents(delegate
		{
			LogState(clipData, "Stream Cancelled", fromDisk);
			Events?.Stream?.OnStreamCancel?.Invoke(clipData);
			if (!clipData.LoadReady.Task.IsCompleted)
			{
				clipData.LoadReady.SetResult(result: false);
			}
			RaiseStreamComplete(clipData, fromDisk);
		});
	}

	private void RaiseDiskStreamReady(TTSClipData clipData)
	{
		RaiseStreamReady(clipData, fromDisk: true);
	}

	private void RaiseWebStreamReady(TTSClipData clipData)
	{
		RaiseStreamReady(clipData, fromDisk: false);
	}

	private void RaiseStreamReady(TTSClipData clipData, bool fromDisk)
	{
		if (RuntimeCacheHandler != null)
		{
			RuntimeCacheHandler.OnClipRemoved -= OnRuntimeClipRemoved;
			bool num = !RuntimeCacheHandler.AddClip(clipData);
			RuntimeCacheHandler.OnClipRemoved += OnRuntimeClipRemoved;
			if (num)
			{
				RaiseStreamError(clipData, -1, "Removed from runtime cache due to file size", fromDisk);
				OnRuntimeClipRemoved(clipData);
				return;
			}
		}
		SetClipLoadState(clipData, TTSClipLoadState.Loaded);
		RaiseEvents(delegate
		{
			LogState(clipData, "Stream Ready", fromDisk);
			clipData.onPlaybackReady?.Invoke(clipData);
			clipData.onPlaybackReady = null;
			Events?.Stream?.OnStreamReady?.Invoke(clipData);
			if (!clipData.LoadReady.Task.IsCompleted)
			{
				clipData.LoadReady.SetResult(result: true);
			}
		});
	}

	private void RaiseDiskStreamComplete(TTSClipData clipData)
	{
		RaiseStreamComplete(clipData, fromDisk: true);
	}

	private void RaiseWebStreamComplete(TTSClipData clipData)
	{
		RaiseStreamComplete(clipData, fromDisk: false);
	}

	private void RaiseStreamComplete(TTSClipData clipData, bool fromDisk)
	{
		RaiseEvents(delegate
		{
			LogState(clipData, "Stream Complete", fromDisk);
			Events?.Stream?.OnStreamComplete?.Invoke(clipData);
			if (!fromDisk)
			{
				Events?.WebRequest?.OnRequestComplete.Invoke(clipData);
			}
			if (!clipData.LoadCompletion.Task.IsCompleted)
			{
				clipData.LoadCompletion.SetResult(result: true);
			}
			if (clipData.loadState == TTSClipLoadState.Error)
			{
				Unload(clipData);
			}
		});
	}

	private void RaiseDownloadBegin(TTSClipData clipData, string downloadPath)
	{
		RaiseEvents(delegate
		{
			LogState(clipData, "Download Begin", fromDisk: true);
			Events?.Download?.OnDownloadBegin?.Invoke(clipData, downloadPath);
		});
	}

	private void RaiseDownloadSuccess(TTSClipData clipData, string downloadPath)
	{
		RaiseEvents(delegate
		{
			LogState(clipData, "Download Success", fromDisk: true);
			clipData.onDownloadComplete?.Invoke(string.Empty);
			clipData.onDownloadComplete = null;
			Events?.Download?.OnDownloadSuccess?.Invoke(clipData, downloadPath);
		});
	}

	private void RaiseDownloadCancel(TTSClipData clipData, string downloadPath)
	{
		RaiseEvents(delegate
		{
			LogState(clipData, "Download Cancelled", fromDisk: true);
			clipData.onDownloadComplete?.Invoke("Cancelled");
			clipData.onDownloadComplete = null;
			Events?.Download?.OnDownloadCancel?.Invoke(clipData, downloadPath);
		});
	}

	private void RaiseDownloadError(TTSClipData clipData, string downloadPath, string error)
	{
		if (error.Equals("Cancelled"))
		{
			RaiseDownloadCancel(clipData, downloadPath);
			return;
		}
		RaiseEvents(delegate
		{
			LogState(clipData, "Download Failed", fromDisk: true, error);
			clipData.onDownloadComplete?.Invoke(error);
			clipData.onDownloadComplete = null;
			Events?.Download?.OnDownloadError?.Invoke(clipData, downloadPath, error);
		});
	}

	private void RaiseEvents(Action events)
	{
		ThreadUtility.CallOnMainThread(events).WrapErrors();
	}
}
