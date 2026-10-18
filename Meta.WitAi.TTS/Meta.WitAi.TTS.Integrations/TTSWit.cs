using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Audio;
using Meta.Voice.Net.WebSockets;
using Meta.Voice.Net.WebSockets.Requests;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Integrations;

public class TTSWit : TTSService, ITTSVoiceProvider, ITTSWebHandler, IWitConfigurationProvider, IWitConfigurationSetter, ILogSource
{
	private WitWebSocketAdapter _webSocketAdapter;

	[Header("Web Request Settings")]
	[FormerlySerializedAs("_settings")]
	public TTSWitRequestSettings RequestSettings = new TTSWitRequestSettings
	{
		audioType = TTSWitAudioType.MPEG,
		audioReadyDuration = 1.5f,
		audioMaxDuration = 15f,
		audioStreamPreloadCount = 5,
		audioStream = true,
		useEvents = true
	};

	private ConcurrentDictionary<string, WitTTSVRequest> _httpRequests = new ConcurrentDictionary<string, WitTTSVRequest>();

	private ConcurrentDictionary<string, WitWebSocketTtsRequest> _webSocketRequests = new ConcurrentDictionary<string, WitWebSocketTtsRequest>();

	[Header("Voice Settings")]
	[SerializeField]
	private TTSWitVoiceSettings[] _presetVoiceSettings;

	public override ITTSVoiceProvider VoiceProvider => this;

	public override ITTSWebHandler WebHandler => this;

	public WitConfiguration Configuration
	{
		get
		{
			return RequestSettings._configuration;
		}
		set
		{
			RequestSettings._configuration = value;
			this.OnConfigurationUpdated?.Invoke(RequestSettings._configuration);
			RefreshWebSocketSettings();
		}
	}

	public TTSWitVoiceSettings[] PresetWitVoiceSettings => _presetVoiceSettings;

	public TTSVoiceSettings[] PresetVoiceSettings
	{
		get
		{
			if (_presetVoiceSettings == null)
			{
				_presetVoiceSettings = new TTSWitVoiceSettings[0];
			}
			return _presetVoiceSettings;
		}
	}

	public TTSVoiceSettings VoiceDefaultSettings
	{
		get
		{
			if (PresetVoiceSettings != null && PresetVoiceSettings.Length != 0)
			{
				return PresetVoiceSettings[0];
			}
			return null;
		}
	}

	public event Action<WitConfiguration> OnConfigurationUpdated;

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshWebSocketSettings();
		RefreshAudioSystemSettings();
		if (base.AudioSystem != null)
		{
			base.AudioSystem.PreloadClipStreams(RequestSettings.audioStreamPreloadCount);
		}
	}

	protected virtual void RefreshWebSocketSettings()
	{
		_webSocketAdapter = GetOrCreateInterface<WitWebSocketAdapter, WitWebSocketAdapter>(_webSocketAdapter);
		WitConfiguration configuration = Configuration;
		_webSocketAdapter.SetClientProvider((configuration != null && configuration.RequestType == WitRequestType.WebSocket) ? configuration : null);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)_webSocketAdapter)
		{
			_webSocketAdapter.SetClientProvider(null);
		}
	}

	public override string GetInvalidError()
	{
		string invalidError = base.GetInvalidError();
		if (!string.IsNullOrEmpty(invalidError))
		{
			return invalidError;
		}
		if (Configuration == null)
		{
			return "No WitConfiguration Set";
		}
		if (string.IsNullOrEmpty(Configuration.GetClientAccessToken()))
		{
			return "No WitConfiguration Client Token";
		}
		return string.Empty;
	}

	public string GetWebErrors(TTSClipData clipData)
	{
		string invalidError = GetInvalidError();
		if (!string.IsNullOrEmpty(invalidError))
		{
			return invalidError;
		}
		string ttsErrors = WitRequestSettings.GetTtsErrors(clipData?.textToSpeak, Configuration);
		if (!string.IsNullOrEmpty(ttsErrors))
		{
			return ttsErrors;
		}
		return string.Empty;
	}

	public TTSClipData CreateClipData(string clipId, string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings)
	{
		return new TTSClipData
		{
			clipID = clipId,
			textToSpeak = textToSpeak,
			voiceSettings = voiceSettings,
			diskCacheSettings = diskCacheSettings,
			loadState = TTSClipLoadState.Unloaded,
			loadProgress = 0f,
			queryParameters = voiceSettings?.EncodedValues,
			clipStream = (string.IsNullOrEmpty(textToSpeak) ? null : CreateClipStream()),
			extension = WitRequestSettings.GetAudioExtension(RequestSettings.audioType, RequestSettings.useEvents),
			queryStream = RequestSettings.audioStream,
			useEvents = RequestSettings.useEvents
		};
	}

	private IAudioClipStream CreateClipStream()
	{
		if (base.AudioSystem == null)
		{
			return new RawAudioClipStream(1, 24000, RequestSettings.audioReadyDuration);
		}
		RefreshAudioSystemSettings();
		return base.AudioSystem.GetAudioClipStream();
	}

	private void RefreshAudioSystemSettings()
	{
		if (base.AudioSystem != null)
		{
			base.AudioSystem.ClipSettings = new AudioClipSettings
			{
				Channels = 1,
				SampleRate = 24000,
				ReadyDuration = RequestSettings.audioReadyDuration,
				MaxDuration = RequestSettings.audioMaxDuration
			};
		}
	}

	private WitTTSVRequest CreateHttpRequest(TTSClipData clipData)
	{
		WitTTSVRequest witTTSVRequest = new WitTTSVRequest(Configuration, clipData.queryRequestId, clipData.queryOperationId);
		witTTSVRequest.TimeoutMs = Configuration.RequestTimeoutMs;
		witTTSVRequest.TextToSpeak = clipData.textToSpeak;
		witTTSVRequest.TtsParameters = clipData.queryParameters;
		witTTSVRequest.FileType = RequestSettings.audioType;
		witTTSVRequest.Stream = clipData.queryStream;
		witTTSVRequest.UseEvents = clipData.useEvents;
		_httpRequests[clipData.clipID] = witTTSVRequest;
		return witTTSVRequest;
	}

	private WitWebSocketTtsRequest CreateWebSocketRequest(TTSClipData clipData, string downloadPath)
	{
		WitWebSocketTtsRequest witWebSocketTtsRequest = new WitWebSocketTtsRequest(clipData.queryRequestId, clipData.textToSpeak, clipData.queryParameters, RequestSettings.audioType, clipData.useEvents, downloadPath, clipData.queryOperationId);
		witWebSocketTtsRequest.TimeoutMs = Configuration.RequestTimeoutMs;
		witWebSocketTtsRequest.OnSamplesReceived = clipData.clipStream.AddSamples;
		witWebSocketTtsRequest.OnEventsReceived = clipData.Events.AddEvents;
		if (base.SimulatedErrorType != (VoiceErrorSimulationType)(-1))
		{
			witWebSocketTtsRequest.SimulatedErrorType = base.SimulatedErrorType;
			base.SimulatedErrorType = (VoiceErrorSimulationType)(-1);
		}
		_webSocketRequests[clipData.clipID] = witWebSocketTtsRequest;
		return witWebSocketTtsRequest;
	}

	public bool DecodeTtsFromJson(WitResponseNode responseNode, out string textToSpeak, out TTSVoiceSettings voiceSettings)
	{
		WitResponseClass witResponseClass = responseNode?.AsObject;
		if (witResponseClass != null && TTSWitVoiceSettings.CanDecode(witResponseClass))
		{
			TTSWitVoiceSettings tTSWitVoiceSettings = new TTSWitVoiceSettings();
			if (tTSWitVoiceSettings.DeserializeObject(witResponseClass))
			{
				voiceSettings = tTSWitVoiceSettings;
				textToSpeak = witResponseClass["q"].Value;
				return true;
			}
		}
		textToSpeak = null;
		voiceSettings = null;
		return false;
	}

	public async Task<string> RequestStreamFromWeb(TTSClipData clipData, Action<TTSClipData> onReady)
	{
		CancelRequests(clipData);
		if (clipData.clipStream == null)
		{
			return "Cannot load without a clip stream";
		}
		DateTime startTime = DateTime.UtcNow;
		IAudioClipStream clipStream = clipData.clipStream;
		clipStream.OnStreamReady = (AudioClipStreamDelegate)Delegate.Combine(clipStream.OnStreamReady, (AudioClipStreamDelegate)delegate
		{
			clipData.readyDuration = (float)(DateTime.UtcNow - startTime).TotalSeconds;
			onReady?.Invoke(clipData);
		});
		string text = ((!(Configuration != null) || Configuration.RequestType != WitRequestType.WebSocket || !_webSocketAdapter) ? (await RequestStreamViaHttp(clipData)) : (await RequestStreamFromWebSocket(clipData)));
		clipData.completeDuration = (float)(DateTime.UtcNow - startTime).TotalSeconds;
		if (string.IsNullOrEmpty(text) && (clipData?.clipStream == null || clipData.clipStream.AddedSamples == 0))
		{
			text = "No audio samples added during stream";
		}
		if (string.IsNullOrEmpty(text))
		{
			clipData.clipStream.SetExpectedSamples(clipData.clipStream.AddedSamples);
		}
		return text;
	}

	private async Task<string> RequestStreamFromWebSocket(TTSClipData clipData)
	{
		WitWebSocketTtsRequest wsRequest = CreateWebSocketRequest(clipData, null);
		TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
		wsRequest.OnComplete = delegate
		{
			completion.SetResult(result: true);
		};
		RefreshWebSocketSettings();
		_webSocketAdapter.SendRequest(wsRequest);
		await completion.Task;
		clipData.LoadStatusCode = wsRequest.Code;
		clipData.LoadError = wsRequest.Error;
		return wsRequest.Error;
	}

	private Task<string> RequestStreamViaHttp(TTSClipData clipData)
	{
		string clipId = clipData.clipID;
		WitTTSVRequest request = CreateHttpRequest(clipData);
		_httpRequests[clipId] = request;
		return ThreadUtility.BackgroundAsync(base.Logger, async delegate
		{
			VRequestResponse<bool> vRequestResponse = await request.RequestStream(clipData.clipStream.AddSamples, clipData.Events.AddEvents);
			clipData.LoadStatusCode = vRequestResponse.Code;
			clipData.LoadError = vRequestResponse.Error;
			_httpRequests.TryRemove(clipId, out var _);
			return vRequestResponse.Error;
		});
	}

	public Task<string> RequestDownloadFromWeb(TTSClipData clipData, string diskPath)
	{
		CancelRequests(clipData);
		if (Configuration != null && Configuration.RequestType == WitRequestType.WebSocket && (bool)_webSocketAdapter)
		{
			return RequestDownloadFromWebSocket(clipData, diskPath);
		}
		return RequestDownloadViaHttp(clipData, diskPath);
	}

	private async Task<string> RequestDownloadFromWebSocket(TTSClipData clipData, string diskPath)
	{
		WitWebSocketTtsRequest wsRequest = CreateWebSocketRequest(clipData, diskPath);
		TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
		wsRequest.OnComplete = delegate
		{
			completion.SetResult(result: true);
		};
		RefreshWebSocketSettings();
		_webSocketAdapter.SendRequest(wsRequest);
		await completion.Task;
		return wsRequest.Error;
	}

	private Task<string> RequestDownloadViaHttp(TTSClipData clipData, string diskPath)
	{
		string clipId = clipData.clipID;
		WitTTSVRequest request = CreateHttpRequest(clipData);
		_httpRequests[clipId] = request;
		return ThreadUtility.BackgroundAsync(base.Logger, async delegate
		{
			VRequestResponse<bool> obj = await request.RequestDownload(diskPath);
			_httpRequests.TryRemove(clipId, out var _);
			return obj.Error;
		});
	}

	public async Task<string> IsDownloadedToDisk(string diskPath)
	{
		string error = null;
		await ThreadUtility.BackgroundAsync(base.Logger, async delegate
		{
			VRequestResponse<bool> vRequestResponse = await new VRequest().RequestFileExists(diskPath);
			error = vRequestResponse.Error;
			if (string.IsNullOrEmpty(error) && !vRequestResponse.Value)
			{
				error = "File Not Found";
			}
		});
		return error;
	}

	public async Task<string> RequestStreamFromDisk(TTSClipData clipData, string diskPath, Action<TTSClipData> onReady)
	{
		CancelRequests(clipData);
		if (clipData.clipStream == null)
		{
			return "Cannot load without a clip stream";
		}
		DateTime startTime = DateTime.UtcNow;
		IAudioClipStream clipStream = clipData.clipStream;
		clipStream.OnStreamReady = (AudioClipStreamDelegate)Delegate.Combine(clipStream.OnStreamReady, (AudioClipStreamDelegate)delegate
		{
			clipData.readyDuration = (float)(DateTime.UtcNow - startTime).TotalSeconds;
			onReady?.Invoke(clipData);
		});
		string text = await RequestStreamFromDiskViaVRequest(clipData, diskPath);
		clipData.completeDuration = (float)(DateTime.UtcNow - startTime).TotalSeconds;
		if (string.IsNullOrEmpty(text) && (clipData?.clipStream == null || clipData.clipStream.AddedSamples == 0))
		{
			text = "No audio samples added during stream";
		}
		if (string.IsNullOrEmpty(text))
		{
			clipData.clipStream.SetExpectedSamples(clipData.clipStream.AddedSamples);
		}
		return text;
	}

	private Task<string> RequestStreamFromDiskViaVRequest(TTSClipData clipData, string diskPath)
	{
		string clipId = clipData.clipID;
		WitTTSVRequest request = CreateHttpRequest(clipData);
		_httpRequests[clipId] = request;
		return ThreadUtility.BackgroundAsync(base.Logger, async delegate
		{
			VRequestResponse<bool> obj = await request.RequestStreamFromDisk(diskPath, clipData.clipStream.AddSamples, clipData.Events.AddEvents);
			_httpRequests.TryRemove(clipId, out var _);
			return obj.Error;
		});
	}

	public bool CancelRequests(TTSClipData clipData)
	{
		if (_httpRequests.TryGetValue(clipData.clipID, out var value))
		{
			value?.Cancel();
			return true;
		}
		if (_webSocketRequests.TryGetValue(clipData.clipID, out var value2))
		{
			value2?.Cancel();
			return true;
		}
		return false;
	}

	private string IsRequestValid(TTSClipData clipData, WitConfiguration configuration)
	{
		string invalidError = GetInvalidError();
		if (!string.IsNullOrEmpty(invalidError))
		{
			return invalidError;
		}
		if (clipData == null)
		{
			return "No clip data provided";
		}
		return string.Empty;
	}
}
