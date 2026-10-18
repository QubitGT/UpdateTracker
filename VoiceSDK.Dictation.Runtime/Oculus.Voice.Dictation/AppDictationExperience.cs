using System;
using System.Globalization;
using Meta.Voice;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Dictation;
using Meta.WitAi.Dictation.Data;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using Oculus.Voice.Core.Bindings.Android.PlatformLogger;
using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.Voice.Dictation.Bindings.Android;
using Oculus.VoiceSDK.Utilities;
using UnityEngine;

namespace Oculus.Voice.Dictation;

public class AppDictationExperience : DictationService, IWitRuntimeConfigProvider, IWitConfigurationProvider
{
	[SerializeField]
	private WitDictationRuntimeConfiguration runtimeConfiguration;

	[Tooltip("Uses platform dictation service instead of accessing wit directly from within the application.")]
	[SerializeField]
	private bool usePlatformServices;

	[Tooltip("Dictation will not fallback to Wit if platform dictation is not available. Not applicable in Unity Editor")]
	[SerializeField]
	private bool doNotFallbackToWit;

	[Tooltip("Enables logs related to the interaction to be displayed on console")]
	[SerializeField]
	private bool enableConsoleLogging;

	private IDictationService _dictationServiceImpl;

	private IVoiceSDKLogger _voiceSDKLogger;

	private bool _isActive;

	private DictationSession _activeSession;

	private WitRequestOptions _activeRequestOptions;

	public WitRuntimeConfiguration RuntimeConfiguration => runtimeConfiguration;

	public WitDictationRuntimeConfiguration RuntimeDictationConfiguration
	{
		get
		{
			return runtimeConfiguration;
		}
		set
		{
			runtimeConfiguration = value;
		}
	}

	public WitConfiguration Configuration => RuntimeConfiguration?.witConfiguration;

	public DictationSession ActiveSession => _activeSession;

	public WitRequestOptions ActiveRequestOptions => _activeRequestOptions;

	private static string PACKAGE_VERSION => VoiceSDKConstants.SdkVersion;

	public bool HasPlatformIntegrations => false;

	public bool UsePlatformIntegrations
	{
		get
		{
			return usePlatformServices;
		}
		set
		{
			if (usePlatformServices != value || HasPlatformIntegrations != value)
			{
				usePlatformServices = value;
			}
		}
	}

	public bool DoNotFallbackToWit
	{
		get
		{
			return doNotFallbackToWit;
		}
		set
		{
			doNotFallbackToWit = value;
		}
	}

	public override bool Active
	{
		get
		{
			if (_dictationServiceImpl != null)
			{
				return _dictationServiceImpl.Active;
			}
			return false;
		}
	}

	public override bool IsRequestActive
	{
		get
		{
			if (_dictationServiceImpl != null)
			{
				return _dictationServiceImpl.IsRequestActive;
			}
			return false;
		}
	}

	public override ITranscriptionProvider TranscriptionProvider
	{
		get
		{
			return _dictationServiceImpl.TranscriptionProvider;
		}
		set
		{
			_dictationServiceImpl.TranscriptionProvider = value;
		}
	}

	public override bool MicActive
	{
		get
		{
			if (_dictationServiceImpl != null)
			{
				return _dictationServiceImpl.MicActive;
			}
			return false;
		}
	}

	protected override bool ShouldSendMicData
	{
		get
		{
			if (!RuntimeConfiguration.sendAudioToWit)
			{
				return TranscriptionProvider == null;
			}
			return true;
		}
	}

	public event Action OnInitialized;

	private void InitDictation()
	{
		if (string.IsNullOrEmpty(PACKAGE_VERSION))
		{
			VLog.E("No SDK Version Set");
		}
		if (!UsePlatformIntegrations)
		{
			if (_dictationServiceImpl is PlatformDictationImpl)
			{
				((PlatformDictationImpl)_dictationServiceImpl).Disconnect();
			}
			if (_voiceSDKLogger is VoiceSDKPlatformLoggerImpl)
			{
				try
				{
					((VoiceSDKPlatformLoggerImpl)_voiceSDKLogger).Disconnect();
				}
				catch (Exception ex)
				{
					VLog.E("Disconnection error: " + ex.Message);
				}
			}
		}
		_voiceSDKLogger = new VoiceSDKConsoleLoggerImpl();
		RevertToWitDictation();
		_voiceSDKLogger.WitApplication = RuntimeDictationConfiguration?.witConfiguration?.GetLoggerAppId();
		_voiceSDKLogger.ShouldLogToConsole = enableConsoleLogging;
		this.OnInitialized?.Invoke();
	}

	private void OnPlatformServiceNotAvailable()
	{
		if (DoNotFallbackToWit)
		{
			VLog.D("Platform dictation service unavailable. Falling back to WitDictation is disabled");
			DictationEvents.OnError?.Invoke("Platform dictation unavailable", "Platform dictation service is not available");
		}
		else
		{
			VLog.D("Platform dictation service unavailable. Falling back to WitDictation");
			RevertToWitDictation();
		}
	}

	private void OnDictationServiceNotAvailable()
	{
		VLog.D("Dictation service unavailable");
		DictationEvents.OnError?.Invoke("Dictation unavailable", "Dictation service is not available");
	}

	private void RevertToWitDictation()
	{
		WitDictation witDictation = GetComponent<WitDictation>();
		if (null == witDictation)
		{
			witDictation = base.gameObject.AddComponent<WitDictation>();
			witDictation.hideFlags = HideFlags.HideInInspector;
		}
		witDictation.RuntimeConfiguration = RuntimeDictationConfiguration;
		witDictation.DictationEvents = DictationEvents;
		witDictation.TelemetryEvents = base.TelemetryEvents;
		witDictation.ShouldWrap = false;
		_dictationServiceImpl = witDictation;
		VLog.D("WitDictation init complete");
		_voiceSDKLogger.IsUsingPlatformIntegration = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (MicPermissionsManager.HasMicPermission())
		{
			InitDictation();
		}
		else
		{
			MicPermissionsManager.RequestMicPermission(delegate
			{
				InitDictation();
			});
		}
		DictationEvents.OnDictationSessionStarted.AddListener(OnDictationSessionStarted);
		base.TelemetryEvents.OnAudioTrackerFinished.AddListener(OnAudioDurationTrackerFinished);
	}

	protected override void OnDisable()
	{
		_dictationServiceImpl = null;
		_voiceSDKLogger = null;
		DictationEvents.OnDictationSessionStarted.RemoveListener(OnDictationSessionStarted);
		base.TelemetryEvents.OnAudioTrackerFinished.RemoveListener(OnAudioDurationTrackerFinished);
		base.OnDisable();
	}

	public void Toggle()
	{
		if (Active)
		{
			Deactivate();
		}
		else
		{
			Activate();
		}
	}

	public override VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (_dictationServiceImpl == null)
		{
			OnDictationServiceNotAvailable();
			return null;
		}
		if (!_isActive)
		{
			_activeSession = new DictationSession();
			DictationEvents.OnDictationSessionStarted.Invoke(_activeSession);
		}
		_isActive = true;
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return _dictationServiceImpl.Activate(requestOptions, requestEvents);
	}

	public override VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (_dictationServiceImpl == null)
		{
			OnDictationServiceNotAvailable();
			return null;
		}
		if (!_isActive)
		{
			_activeSession = new DictationSession();
			DictationEvents.OnDictationSessionStarted.Invoke(_activeSession);
		}
		_isActive = true;
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		return _dictationServiceImpl.ActivateImmediately(requestOptions, requestEvents);
	}

	public override void Deactivate()
	{
		if (_dictationServiceImpl == null)
		{
			OnDictationServiceNotAvailable();
			return;
		}
		_isActive = false;
		_dictationServiceImpl.Deactivate();
	}

	public override void Cancel()
	{
		if (_dictationServiceImpl == null)
		{
			OnDictationServiceNotAvailable();
			return;
		}
		_dictationServiceImpl.Cancel();
		CleanupSession();
	}

	protected override void OnRequestInit(VoiceServiceRequest request)
	{
		base.OnRequestInit(request);
		_activeRequestOptions = request?.Options;
		_voiceSDKLogger.LogInteractionStart(request?.Options?.RequestId, "dictation");
		_voiceSDKLogger.LogAnnotation("minWakeThreshold", RuntimeConfiguration?.soundWakeThreshold.ToString(CultureInfo.InvariantCulture));
		_voiceSDKLogger.LogAnnotation("minKeepAliveTimeSec", RuntimeConfiguration?.minKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
		_voiceSDKLogger.LogAnnotation("minTranscriptionKeepAliveTimeSec", RuntimeConfiguration?.minTranscriptionKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
		_voiceSDKLogger.LogAnnotation("maxRecordingTime", RuntimeConfiguration?.maxRecordingTime.ToString(CultureInfo.InvariantCulture));
	}

	protected override void OnRequestStartListening(VoiceServiceRequest request)
	{
		base.OnRequestStartListening(request);
		_voiceSDKLogger.LogInteractionPoint("startedListening");
	}

	protected override void OnRequestStopListening(VoiceServiceRequest request)
	{
		base.OnRequestStopListening(request);
		_voiceSDKLogger.LogInteractionPoint("stoppedListening");
		if (RuntimeDictationConfiguration.dictationConfiguration.multiPhrase && _isActive)
		{
			Activate(_activeRequestOptions);
		}
	}

	private void OnDictationSessionStarted(DictationSession session)
	{
		if (session is PlatformDictationSession platformDictationSession)
		{
			_activeSession = session;
			_voiceSDKLogger.LogAnnotation("platformInteractionId", platformDictationSession.platformSessionId);
		}
	}

	private void OnAudioDurationTrackerFinished(long timestamp, double audioDuration)
	{
		_voiceSDKLogger.LogAnnotation("adt_duration", audioDuration.ToString(CultureInfo.InvariantCulture));
		_voiceSDKLogger.LogAnnotation("adt_finished", timestamp.ToString());
	}

	protected override void OnRequestPartialTranscription(VoiceServiceRequest request, string transcription)
	{
		base.OnRequestPartialTranscription(request, transcription);
		_voiceSDKLogger.LogFirstTranscriptionTime();
	}

	protected override void OnRequestFullTranscription(VoiceServiceRequest request, string transcription)
	{
		base.OnRequestFullTranscription(request, transcription);
		_voiceSDKLogger.LogInteractionPoint("fullTranscriptionTime");
	}

	protected override void OnRequestComplete(VoiceServiceRequest request)
	{
		base.OnRequestComplete(request);
		if (request.State == VoiceRequestState.Failed)
		{
			_voiceSDKLogger.LogInteractionEndFailure(request.Results.Message);
		}
		else if (request.State == VoiceRequestState.Canceled)
		{
			_voiceSDKLogger.LogInteractionEndFailure("aborted");
		}
		else
		{
			WitResponseNode witResponseNode = request.ResponseData?["speech"]?["tokens"];
			if (witResponseNode != null)
			{
				int count = witResponseNode.Count;
				string annotationValue = request.ResponseData["speech"]["tokens"][count - 1]?["end"]?.Value;
				_voiceSDKLogger.LogAnnotation("audioLength", annotationValue);
			}
			_voiceSDKLogger.LogInteractionEndSuccess();
		}
		if (!_isActive)
		{
			DictationEvents.OnDictationSessionStopped?.Invoke(_activeSession);
			CleanupSession();
		}
	}

	private void CleanupSession()
	{
		_activeSession = null;
		_activeRequestOptions = null;
		_isActive = false;
	}
}
