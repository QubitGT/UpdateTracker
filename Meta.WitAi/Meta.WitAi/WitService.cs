using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.Voice.Logging;
using Meta.Voice.Net.Encoding.Wit;
using Meta.Voice.Net.PubSub;
using Meta.Voice.Net.WebSockets;
using Meta.Voice.Net.WebSockets.Requests;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Events;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Requests;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Meta.WitAi;

[LogCategory(LogCategory.Requests)]
public class WitService : MonoBehaviour, IVoiceEventProvider, IVoiceActivationHandler, ITelemetryEventsProvider, IWitRuntimeConfigProvider, IWitConfigurationProvider
{
	private float _lastMinVolumeLevelTime;

	private WitWebSocketAdapter _webSocketAdapter;

	private VoiceServiceRequest _recordingRequest;

	private bool _isSoundWakeActive;

	private RingBuffer<byte>.Marker _lastSampleMarker;

	private bool _minKeepAliveWasHit;

	private bool _isActive;

	private long _minSampleByteCount = 10240L;

	private IVoiceEventProvider _voiceEventProvider;

	private ITelemetryEventsProvider _telemetryEventsProvider;

	private IWitRuntimeConfigProvider _runtimeConfigProvider;

	private ITranscriptionProvider _activeTranscriptionProvider;

	private Coroutine _timeLimitCoroutine;

	private bool _receivedTranscription;

	private float _lastWordTime;

	private ConcurrentDictionary<string, VoiceServiceRequest> _transmitRequests = new ConcurrentDictionary<string, VoiceServiceRequest>();

	private Coroutine _queueHandler;

	private IWitByteDataReadyHandler[] _dataReadyHandlers;

	private IWitByteDataSentHandler[] _dataSentHandlers;

	private IDynamicEntitiesProvider[] _dynamicEntityProviders;

	private float _time;

	private AudioBuffer _buffer;

	private bool _bufferDelegates;

	public IVLogger _log { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Requests);


	public IPubSubAdapter PubSub
	{
		get
		{
			SetupWebSockets();
			return _webSocketAdapter;
		}
	}

	public WitConfiguration Configuration => RuntimeConfiguration?.witConfiguration;

	public bool Active
	{
		get
		{
			if (!_isActive)
			{
				return IsRequestActive;
			}
			return true;
		}
	}

	public bool IsRequestActive
	{
		get
		{
			if (_recordingRequest != null && _recordingRequest.IsActive)
			{
				return true;
			}
			return false;
		}
	}

	public IVoiceEventProvider VoiceEventProvider
	{
		get
		{
			return _voiceEventProvider;
		}
		set
		{
			_voiceEventProvider = value;
		}
	}

	public ITelemetryEventsProvider TelemetryEventsProvider
	{
		get
		{
			return _telemetryEventsProvider;
		}
		set
		{
			_telemetryEventsProvider = value;
		}
	}

	public IWitRuntimeConfigProvider ConfigurationProvider
	{
		get
		{
			return _runtimeConfigProvider;
		}
		set
		{
			_runtimeConfigProvider = value;
		}
	}

	public WitRuntimeConfiguration RuntimeConfiguration => _runtimeConfigProvider?.RuntimeConfiguration;

	public VoiceEvents VoiceEvents => _voiceEventProvider.VoiceEvents;

	public TelemetryEvents TelemetryEvents => _telemetryEventsProvider.TelemetryEvents;

	public ITranscriptionProvider TranscriptionProvider
	{
		get
		{
			return _activeTranscriptionProvider;
		}
		set
		{
			if (_activeTranscriptionProvider != null)
			{
				_activeTranscriptionProvider.OnPartialTranscription.RemoveListener(OnPartialTranscription);
				_activeTranscriptionProvider.OnMicLevelChanged.RemoveListener(OnTranscriptionMicLevelChanged);
			}
			_activeTranscriptionProvider = value;
			if (_activeTranscriptionProvider != null)
			{
				_activeTranscriptionProvider.OnPartialTranscription.AddListener(OnPartialTranscription);
				_activeTranscriptionProvider.OnMicLevelChanged.AddListener(OnTranscriptionMicLevelChanged);
			}
		}
	}

	public IVoiceServiceRequestProvider RequestProvider { get; set; }

	public bool MicActive => _buffer.IsRecording(this);

	protected bool ShouldSendMicData
	{
		get
		{
			if (!RuntimeConfiguration.sendAudioToWit)
			{
				return _activeTranscriptionProvider == null;
			}
			return true;
		}
	}

	public virtual bool IsConfigurationValid()
	{
		if (RuntimeConfiguration.witConfiguration != null)
		{
			return !string.IsNullOrEmpty(RuntimeConfiguration.witConfiguration.GetClientAccessToken());
		}
		return false;
	}

	private VoiceServiceRequest GetTextRequest(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		WitConfiguration configuration = Configuration;
		WitRequestOptions setupOptions = WitRequestFactory.GetSetupOptions(configuration, requestOptions, _dynamicEntityProviders);
		VoiceServiceRequestEvents voiceServiceRequestEvents = requestEvents ?? new VoiceServiceRequestEvents();
		setupOptions.InputType = NLPRequestInputType.Text;
		if (configuration != null && configuration.RequestType == WitRequestType.WebSocket)
		{
			SetupWebSockets();
		}
		if (RequestProvider != null)
		{
			VoiceServiceRequest voiceServiceRequest = RequestProvider.CreateRequest(RuntimeConfiguration, setupOptions, voiceServiceRequestEvents);
			if (voiceServiceRequest != null)
			{
				return voiceServiceRequest;
			}
		}
		if (configuration != null && configuration.RequestType == WitRequestType.WebSocket)
		{
			return WitSocketRequest.GetMessageRequest(configuration, _webSocketAdapter, setupOptions, voiceServiceRequestEvents);
		}
		return configuration.CreateMessageRequest(setupOptions, voiceServiceRequestEvents, _dynamicEntityProviders);
	}

	private VoiceServiceRequest GetAudioRequest(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		WitConfiguration configuration = Configuration;
		WitRequestOptions setupOptions = WitRequestFactory.GetSetupOptions(configuration, requestOptions, _dynamicEntityProviders);
		VoiceServiceRequestEvents voiceServiceRequestEvents = requestEvents ?? new VoiceServiceRequestEvents();
		setupOptions.InputType = NLPRequestInputType.Audio;
		if (configuration != null && configuration.RequestType == WitRequestType.WebSocket)
		{
			SetupWebSockets();
		}
		if (RequestProvider != null)
		{
			VoiceServiceRequest voiceServiceRequest = RequestProvider.CreateRequest(RuntimeConfiguration, setupOptions, voiceServiceRequestEvents);
			if (voiceServiceRequest != null)
			{
				return voiceServiceRequest;
			}
		}
		if (configuration != null && configuration.RequestType == WitRequestType.WebSocket)
		{
			if (RuntimeConfiguration.transcribeOnly)
			{
				return WitSocketRequest.GetTranscribeRequest(configuration, _webSocketAdapter, _buffer, setupOptions, voiceServiceRequestEvents);
			}
			return WitSocketRequest.GetSpeechRequest(configuration, _webSocketAdapter, _buffer, setupOptions, voiceServiceRequestEvents);
		}
		if (RuntimeConfiguration.transcribeOnly)
		{
			_log.Warning("Transcribe request is not available with HTTP.");
		}
		return configuration.CreateSpeechRequest(setupOptions, voiceServiceRequestEvents, _dynamicEntityProviders);
	}

	private int GetTimeoutMs()
	{
		if (RuntimeConfiguration.overrideTimeoutMs <= 0)
		{
			return RuntimeConfiguration.witConfiguration.RequestTimeoutMs;
		}
		return RuntimeConfiguration.overrideTimeoutMs;
	}

	protected void Awake()
	{
		_dataReadyHandlers = GetComponents<IWitByteDataReadyHandler>();
		_dataSentHandlers = GetComponents<IWitByteDataSentHandler>();
		_runtimeConfigProvider = GetComponent<IWitRuntimeConfigProvider>();
	}

	protected void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
		_runtimeConfigProvider = GetComponent<IWitRuntimeConfigProvider>();
		_voiceEventProvider = GetComponent<IVoiceEventProvider>();
		if (_activeTranscriptionProvider == null && RuntimeConfiguration != null && (bool)RuntimeConfiguration.customTranscriptionProvider)
		{
			TranscriptionProvider = RuntimeConfiguration.customTranscriptionProvider;
		}
		if (RuntimeConfiguration != null)
		{
			WitRuntimeConfiguration runtimeConfiguration = RuntimeConfiguration;
			runtimeConfiguration.OnConfigurationUpdated = (Action)Delegate.Combine(runtimeConfiguration.OnConfigurationUpdated, new Action(RefreshConfigurationSettings));
		}
		SetMicDelegates(add: true);
		SetupWebSockets();
		if (_webSocketAdapter != null)
		{
			_webSocketAdapter.OnProcessForwardedResponse += ProcessForwardedWebSocketResponse;
			_webSocketAdapter.OnRequestGenerated += HandleWebSocketRequestGeneration;
		}
		_dynamicEntityProviders = GetComponents<IDynamicEntitiesProvider>();
	}

	protected void OnDisable()
	{
		if (RuntimeConfiguration != null)
		{
			WitRuntimeConfiguration runtimeConfiguration = RuntimeConfiguration;
			runtimeConfiguration.OnConfigurationUpdated = (Action)Delegate.Remove(runtimeConfiguration.OnConfigurationUpdated, new Action(RefreshConfigurationSettings));
		}
		if (_webSocketAdapter != null)
		{
			_webSocketAdapter.OnProcessForwardedResponse -= ProcessForwardedWebSocketResponse;
			_webSocketAdapter.OnRequestGenerated -= HandleWebSocketRequestGeneration;
		}
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SetMicDelegates(add: false);
	}

	protected virtual void RefreshConfigurationSettings()
	{
		SetupWebSockets();
	}

	protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SetMicDelegates(add: true);
	}

	protected void SetMicDelegates(bool add)
	{
		if (_buffer == null)
		{
			_buffer = AudioBuffer.Instance;
			_bufferDelegates = false;
		}
		AudioBufferEvents audioBufferEvents = _buffer?.Events;
		if (audioBufferEvents != null && _bufferDelegates != add)
		{
			_bufferDelegates = add;
			if (add)
			{
				audioBufferEvents.OnAudioStateChange = (Action<VoiceAudioInputState>)Delegate.Combine(audioBufferEvents.OnAudioStateChange, new Action<VoiceAudioInputState>(OnAudioBufferStateChange));
				audioBufferEvents.OnMicLevelChanged.AddListener(OnMicLevelChanged);
				audioBufferEvents.OnByteDataReady.AddListener(OnByteDataReady);
				audioBufferEvents.OnSampleReady = (AudioBufferEvents.OnSampleReadyEvent)Delegate.Combine(audioBufferEvents.OnSampleReady, new AudioBufferEvents.OnSampleReadyEvent(OnMicSampleReady));
			}
			else
			{
				audioBufferEvents.OnAudioStateChange = (Action<VoiceAudioInputState>)Delegate.Remove(audioBufferEvents.OnAudioStateChange, new Action<VoiceAudioInputState>(OnAudioBufferStateChange));
				audioBufferEvents.OnMicLevelChanged.RemoveListener(OnMicLevelChanged);
				audioBufferEvents.OnByteDataReady.RemoveListener(OnByteDataReady);
				audioBufferEvents.OnSampleReady = (AudioBufferEvents.OnSampleReadyEvent)Delegate.Remove(audioBufferEvents.OnSampleReady, new AudioBufferEvents.OnSampleReadyEvent(OnMicSampleReady));
			}
		}
	}

	private void SetupWebSockets()
	{
		if (!_webSocketAdapter)
		{
			_webSocketAdapter = base.gameObject.GetOrAddComponent<WitWebSocketAdapter>();
		}
		WitConfiguration configuration = Configuration;
		bool flag = configuration != null && configuration.RequestType == WitRequestType.WebSocket;
		_webSocketAdapter.SetClientProvider(flag ? configuration : null);
		_webSocketAdapter.SetSettings(flag ? RuntimeConfiguration.pubSubSettings : default(PubSubSettings));
	}

	private bool ProcessForwardedWebSocketResponse(string topicId, string requestId, string clientUserId, WitChunk responseChunk)
	{
		WitWebSocketMessageRequest witWebSocketMessageRequest = new WitWebSocketMessageRequest(responseChunk.jsonData, requestId, clientUserId, null, RuntimeConfiguration.transcribeOnly);
		witWebSocketMessageRequest.TimeoutMs = GetTimeoutMs();
		witWebSocketMessageRequest.TopicId = topicId;
		_webSocketAdapter.WebSocketClient.TrackRequest(witWebSocketMessageRequest);
		return true;
	}

	public void HandleWebSocketRequestGeneration(IWitWebSocketRequest webSocketRequest)
	{
		if (webSocketRequest is WitWebSocketMessageRequest webSocketRequest2 && !IsWebSocketRequestWrapped(webSocketRequest))
		{
			WitRequestOptions options = new WitRequestOptions(webSocketRequest.RequestId, webSocketRequest.ClientUserId, webSocketRequest.OperationId);
			WitSocketRequest externalRequest = WitSocketRequest.GetExternalRequest(webSocketRequest2, RuntimeConfiguration.witConfiguration, _webSocketAdapter, options);
			SetupRequest(externalRequest);
		}
	}

	private bool IsWebSocketRequestWrapped(IWitWebSocketRequest webSocketRequest)
	{
		if (IsWebSocketRequestWrapped(_recordingRequest, webSocketRequest))
		{
			return true;
		}
		return _transmitRequests.ContainsKey(webSocketRequest.RequestId);
	}

	private bool IsWebSocketRequestWrapped(VoiceServiceRequest voiceServiceRequest, IWitWebSocketRequest webSocketRequest)
	{
		if (voiceServiceRequest is WitSocketRequest witSocketRequest)
		{
			return witSocketRequest.WebSocketRequest == webSocketRequest;
		}
		return false;
	}

	public void Activate()
	{
		Activate(new WitRequestOptions());
	}

	public void Activate(WitRequestOptions requestOptions)
	{
		Activate(requestOptions, new VoiceServiceRequestEvents());
	}

	public VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (!IsConfigurationValid())
		{
			_log.Error("Your AppVoiceExperience \"" + base.gameObject.name + "\" does not have a wit config assigned. Understanding Viewer activations will not trigger in game events..");
			return null;
		}
		if (_isActive)
		{
			return null;
		}
		StopRecording();
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		_isActive = true;
		_lastSampleMarker = _buffer.CreateMarker(ConfigurationProvider.RuntimeConfiguration.preferredActivationOffset);
		_lastMinVolumeLevelTime = float.PositiveInfinity;
		_lastWordTime = float.PositiveInfinity;
		_receivedTranscription = false;
		VoiceServiceRequest audioRequest = GetAudioRequest(requestOptions, requestEvents);
		SetupRequest(audioRequest);
		if (ShouldSendMicData)
		{
			if (!_buffer.IsRecording(this))
			{
				_minKeepAliveWasHit = false;
				_isSoundWakeActive = true;
				StartRecording();
			}
			else
			{
				audioRequest.ActivateAudio();
			}
		}
		_activeTranscriptionProvider?.Activate();
		return _recordingRequest;
	}

	public void ActivateImmediately()
	{
		ActivateImmediately(new WitRequestOptions());
	}

	public void ActivateImmediately(WitRequestOptions requestOptions)
	{
		ActivateImmediately(requestOptions, new VoiceServiceRequestEvents());
	}

	public VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		VoiceServiceRequest voiceServiceRequest = Activate(requestOptions, requestEvents);
		if (voiceServiceRequest == null)
		{
			return null;
		}
		SendRecordingRequest();
		_lastSampleMarker = _buffer.CreateMarker(ConfigurationProvider.RuntimeConfiguration.preferredActivationOffset);
		return voiceServiceRequest;
	}

	protected virtual void SendRecordingRequest()
	{
		if (_recordingRequest != null && _recordingRequest.State == VoiceRequestState.Initialized)
		{
			_isSoundWakeActive = false;
			if (ShouldSendMicData)
			{
				ExecuteRequest(_recordingRequest);
			}
		}
	}

	protected void SetupRequest(VoiceServiceRequest newRequest)
	{
		newRequest.Options.TimeoutMs = GetTimeoutMs();
		if (newRequest.Options.InputType == NLPRequestInputType.Audio)
		{
			if (_recordingRequest == newRequest)
			{
				return;
			}
			_recordingRequest = newRequest;
			if (_recordingRequest is IAudioUploadHandler audioUploadHandler)
			{
				audioUploadHandler.AudioEncoding = _buffer.AudioEncoding;
				audioUploadHandler.OnInputStreamReady = OnWitReadyForData;
			}
			if (_recordingRequest is WitRequest witRequest)
			{
				witRequest.audioDurationTracker = new AudioDurationTracker(_recordingRequest.Options?.RequestId, witRequest.AudioEncoding);
			}
			_recordingRequest.Events.OnPartialTranscription.AddListener(OnPartialTranscription);
		}
		else
		{
			_transmitRequests[newRequest.Options.RequestId] = newRequest;
		}
		newRequest.Events.OnCancel.AddListener(HandleResult);
		newRequest.Events.OnFailed.AddListener(HandleResult);
		newRequest.Events.OnSuccess.AddListener(HandleResult);
		newRequest.Events.OnComplete.AddListener(HandleComplete);
		VoiceEvents?.OnRequestFinalize?.Invoke(newRequest);
		ThreadUtility.CallOnMainThread(delegate
		{
			VoiceEvents?.OnRequestInitialized?.Invoke(newRequest);
		}).WrapErrors();
	}

	public void ExecuteRequest(VoiceServiceRequest newRequest)
	{
		if (newRequest != null && newRequest.State == VoiceRequestState.Initialized)
		{
			SetupRequest(newRequest);
			_timeLimitCoroutine = StartCoroutine(DeactivateDueToTimeLimit());
			newRequest.Send();
		}
	}

	public void Activate(string text)
	{
		Activate(text, new WitRequestOptions());
	}

	public void Activate(string text, WitRequestOptions requestOptions)
	{
		Activate(text, requestOptions, new VoiceServiceRequestEvents()).WrapErrors();
	}

	public Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (!IsConfigurationValid())
		{
			_log.Error("Your AppVoiceExperience \"" + base.gameObject.name + "\" does not have a wit config assigned. Understanding Viewer activations will not trigger in game events..");
			return null;
		}
		if (requestOptions == null)
		{
			requestOptions = new WitRequestOptions();
		}
		requestOptions.Text = text;
		VoiceServiceRequest request = GetTextRequest(requestOptions, requestEvents);
		return ThreadUtility.BackgroundAsync(_log, delegate
		{
			SetupRequest(request);
			request.Send();
			return Task.FromResult(request);
		});
	}

	private void StopRecording()
	{
		if (_buffer.IsRecording(this))
		{
			_buffer.StopRecording(this);
		}
	}

	private void OnWitReadyForData()
	{
		_lastMinVolumeLevelTime = _time;
		if (!_buffer.IsRecording(this))
		{
			StartRecording();
		}
	}

	private void StartRecording()
	{
		if (!_buffer.IsInputAvailable)
		{
			VoiceEvents.OnError.Invoke("Input Error", "No input source was available. Cannot activate for voice input.");
		}
		else if (!_buffer.IsRecording(this))
		{
			_buffer.StartRecording(this);
		}
	}

	private void OnAudioBufferStateChange(VoiceAudioInputState audioInputState)
	{
		if (_recordingRequest == null)
		{
			return;
		}
		if (_buffer.IsRecording(this) && _recordingRequest.AudioInputState == VoiceAudioInputState.Off)
		{
			_recordingRequest.ActivateAudio();
		}
		else if (!_buffer.IsRecording(this))
		{
			if (_recordingRequest.AudioInputState == VoiceAudioInputState.On || _recordingRequest.AudioInputState == VoiceAudioInputState.Activating)
			{
				_recordingRequest.DeactivateAudio();
			}
			else if (_recordingRequest.AudioInputState == VoiceAudioInputState.Off && _recordingRequest.State == VoiceRequestState.Initialized)
			{
				_recordingRequest.Cancel("Failed to start audio input");
			}
		}
	}

	private void OnByteDataReady(byte[] buffer, int offset, int length)
	{
		VoiceEvents?.OnByteDataReady.Invoke(buffer, offset, length);
		int num = 0;
		while (_dataReadyHandlers != null && num < _dataReadyHandlers.Length)
		{
			_dataReadyHandlers[num].OnWitDataReady(buffer, offset, length);
			num++;
		}
	}

	private void OnMicSampleReady(RingBuffer<byte>.Marker marker, float levelMax)
	{
		if (_lastSampleMarker == null || _recordingRequest == null)
		{
			return;
		}
		if (_minSampleByteCount > _lastSampleMarker.RingBuffer.Capacity)
		{
			_minSampleByteCount = _lastSampleMarker.RingBuffer.Capacity;
		}
		if (_recordingRequest.State == VoiceRequestState.Transmitting && IsInputStreamReady() && _lastSampleMarker.AvailableByteCount >= _minSampleByteCount)
		{
			_lastSampleMarker.ReadIntoWriters(WriteAudio, delegate(byte[] buffer, int offset, int length)
			{
				VoiceEvents?.OnByteDataSent?.Invoke(buffer, offset, length);
			}, delegate(byte[] buffer, int offset, int length)
			{
				for (int i = 0; i < _dataSentHandlers.Length; i++)
				{
					_dataSentHandlers[i]?.OnWitDataSent(buffer, offset, length);
				}
			});
			if (_receivedTranscription)
			{
				float num = _time - _lastWordTime;
				if (num > RuntimeConfiguration.minTranscriptionKeepAliveTimeInSeconds)
				{
					_log.Verbose($"Deactivated due to inactivity. No new words detected in {num:0.00} seconds.", null, null, null, null, "OnMicSampleReady", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\WitService.cs", 761);
					DeactivateRequest(VoiceEvents?.OnStoppedListeningDueToInactivity);
				}
			}
			else
			{
				float num2 = _time - _lastMinVolumeLevelTime;
				if (num2 > RuntimeConfiguration.minKeepAliveTimeInSeconds)
				{
					_log.Verbose($"Deactivated due to inactivity. No sound detected in {num2:0.00} seconds.", null, null, null, null, "OnMicSampleReady", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\WitService.cs", 771);
					DeactivateRequest(VoiceEvents?.OnStoppedListeningDueToInactivity);
				}
			}
		}
		else if (_isSoundWakeActive && levelMax > RuntimeConfiguration.soundWakeThreshold)
		{
			VoiceEvents?.OnMinimumWakeThresholdHit?.Invoke();
			SendRecordingRequest();
			_lastSampleMarker.Offset(RuntimeConfiguration.sampleLengthInMs * -2);
		}
	}

	private bool IsInputStreamReady()
	{
		if (_recordingRequest is IAudioUploadHandler audioUploadHandler)
		{
			return audioUploadHandler.IsInputStreamReady;
		}
		return false;
	}

	private void WriteAudio(byte[] buffer, int offset, int length)
	{
		if (_recordingRequest is IDataUploadHandler dataUploadHandler)
		{
			dataUploadHandler.Write(buffer, offset, length);
		}
	}

	private void Update()
	{
		_time = Time.time;
	}

	private void OnMicLevelChanged(float level)
	{
		if (TranscriptionProvider == null || !TranscriptionProvider.OverrideMicLevel)
		{
			if (level > RuntimeConfiguration.minKeepAliveVolume)
			{
				_lastMinVolumeLevelTime = _time;
				_minKeepAliveWasHit = true;
			}
			VoiceEvents?.OnMicLevelChanged?.Invoke(level);
		}
	}

	private void OnTranscriptionMicLevelChanged(float level)
	{
		if (TranscriptionProvider != null && TranscriptionProvider.OverrideMicLevel)
		{
			OnMicLevelChanged(level);
		}
	}

	private void FinalizeAudioDurationTracker()
	{
		if (_recordingRequest == null)
		{
			return;
		}
		AudioDurationTracker audioDurationTracker = null;
		if (_recordingRequest is WitRequest witRequest)
		{
			audioDurationTracker = witRequest.audioDurationTracker;
		}
		if (audioDurationTracker != null)
		{
			string text = _recordingRequest.Options?.RequestId;
			if (!string.Equals(text, audioDurationTracker.GetRequestId()))
			{
				VLog.W("Mismatch in request IDs when finalizing AudioDurationTracker. Expected " + text + " but got " + audioDurationTracker.GetRequestId());
				return;
			}
			audioDurationTracker.FinalizeAudio();
			TelemetryEvents.OnAudioTrackerFinished?.Invoke(audioDurationTracker.GetFinalizeTimeStamp(), audioDurationTracker.GetAudioDuration());
		}
	}

	public void Deactivate()
	{
		DeactivateRequest((!_buffer.IsRecording(this)) ? null : VoiceEvents?.OnStoppedListeningDueToDeactivation);
	}

	public void DeactivateAndAbortRequest(VoiceServiceRequest request)
	{
		if (request != null)
		{
			VoiceEvents?.OnAborting?.Invoke();
			request.Cancel();
		}
	}

	public void DeactivateAndAbortRequest()
	{
		DeactivateRequest((!_buffer.IsRecording(this)) ? null : VoiceEvents?.OnStoppedListeningDueToDeactivation, abort: true);
	}

	private IEnumerator DeactivateDueToTimeLimit()
	{
		yield return new WaitForSeconds(RuntimeConfiguration.maxRecordingTime);
		if (IsRequestActive)
		{
			_log.Verbose($"Deactivated input due to timeout.\nMax Record Time: {RuntimeConfiguration.maxRecordingTime}", null, null, null, null, "DeactivateDueToTimeLimit", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\WitService.cs", 894);
			DeactivateRequest(VoiceEvents?.OnStoppedListeningDueToTimeout);
		}
	}

	private void DeactivateRequest(UnityEvent onComplete = null, bool abort = false)
	{
		if (abort)
		{
			VoiceEvents?.OnAborting?.Invoke();
		}
		if (_timeLimitCoroutine != null)
		{
			StopCoroutine(_timeLimitCoroutine);
			_timeLimitCoroutine = null;
		}
		_isActive = false;
		StopRecording();
		FinalizeAudioDurationTracker();
		_activeTranscriptionProvider?.Deactivate();
		VoiceServiceRequest recordingRequest = _recordingRequest;
		_recordingRequest = null;
		DeactivateWitRequest(recordingRequest, abort);
		if (abort)
		{
			string[] array = _transmitRequests.Keys.ToArray();
			foreach (string key in array)
			{
				if (_transmitRequests.TryRemove(key, out var value))
				{
					DeactivateWitRequest(value, abort: true);
				}
			}
		}
		else if (recordingRequest != null && recordingRequest.IsActive && _minKeepAliveWasHit)
		{
			_transmitRequests[recordingRequest.Options.RequestId] = recordingRequest;
			VoiceEvents?.OnMicDataSent?.Invoke();
		}
		_minKeepAliveWasHit = false;
		onComplete?.Invoke();
	}

	private void DeactivateWitRequest(VoiceServiceRequest request, bool abort)
	{
		if (request != null)
		{
			if (abort)
			{
				request.Cancel("Request was aborted by user.");
			}
			else if (request.IsAudioInputActivated)
			{
				request.DeactivateAudio();
			}
		}
	}

	private void OnPartialTranscription(string transcription)
	{
		_receivedTranscription = true;
		_lastWordTime = _time;
	}

	private void HandleResult(VoiceServiceRequest request)
	{
		if (request == _recordingRequest)
		{
			DeactivateRequest();
		}
	}

	private void HandleComplete(VoiceServiceRequest request)
	{
		if (request.InputType == NLPRequestInputType.Audio)
		{
			request.Events.OnPartialTranscription.RemoveListener(OnPartialTranscription);
		}
		request.Events.OnCancel.RemoveListener(HandleResult);
		request.Events.OnFailed.RemoveListener(HandleResult);
		request.Events.OnSuccess.RemoveListener(HandleResult);
		request.Events.OnComplete.RemoveListener(HandleComplete);
		_transmitRequests.TryRemove(request.Options.RequestId, out var _);
	}
}
