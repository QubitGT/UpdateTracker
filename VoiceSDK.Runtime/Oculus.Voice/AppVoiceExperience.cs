using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using Meta.Voice;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using Oculus.Voice.Bindings.Android;
using Oculus.Voice.Core.Bindings.Android.PlatformLogger;
using Oculus.Voice.Core.Bindings.Interfaces;
using Oculus.VoiceSDK.Utilities;
using UnityEngine;

namespace Oculus.Voice;

[HelpURL("https://developer.oculus.com/experimental/voice-sdk/tutorial-overview/")]
public class AppVoiceExperience : VoiceService, IWitRuntimeConfigProvider, IWitConfigurationProvider
{
	[SerializeField]
	private WitRuntimeConfiguration witRuntimeConfiguration;

	[Tooltip("Uses platform services to access wit.ai instead of accessing wit directly from within the application.")]
	[SerializeField]
	private bool usePlatformServices;

	[Tooltip("Enables logs related to the interaction to be displayed on console")]
	[SerializeField]
	private bool enableConsoleLogging;

	[Tooltip("If true, the OnFullTranscriptionEvent events will be triggered when calling Activate(string)")]
	[SerializeField]
	private bool sendTranscriptionEventsForMessages;

	private IVoiceService voiceServiceImpl;

	private IVoiceSDKLogger voiceSDKLoggerImpl;

	public WitRuntimeConfiguration RuntimeConfiguration
	{
		get
		{
			return witRuntimeConfiguration;
		}
		set
		{
			witRuntimeConfiguration = value;
			if (voiceServiceImpl is IWitRuntimeConfigSetter witRuntimeConfigSetter)
			{
				witRuntimeConfigSetter.RuntimeConfiguration = witRuntimeConfiguration;
			}
		}
	}

	public WitConfiguration Configuration => witRuntimeConfiguration?.witConfiguration;

	private static string PACKAGE_VERSION => VoiceSDKConstants.SdkVersion;

	private bool Initialized => voiceServiceImpl != null;

	public override bool Active
	{
		get
		{
			if (!base.Active)
			{
				if (voiceServiceImpl != null)
				{
					return voiceServiceImpl.Active;
				}
				return false;
			}
			return true;
		}
	}

	public override bool IsRequestActive
	{
		get
		{
			if (!base.IsRequestActive)
			{
				if (voiceServiceImpl != null)
				{
					return voiceServiceImpl.IsRequestActive;
				}
				return false;
			}
			return true;
		}
	}

	public override ITranscriptionProvider TranscriptionProvider
	{
		get
		{
			return voiceServiceImpl?.TranscriptionProvider;
		}
		set
		{
			if (voiceServiceImpl != null)
			{
				voiceServiceImpl.TranscriptionProvider = value;
			}
		}
	}

	public override bool MicActive
	{
		get
		{
			if (voiceServiceImpl != null)
			{
				return voiceServiceImpl.MicActive;
			}
			return false;
		}
	}

	protected override bool ShouldSendMicData
	{
		get
		{
			if (!witRuntimeConfiguration.sendAudioToWit)
			{
				return TranscriptionProvider == null;
			}
			return true;
		}
	}

	public bool HasPlatformIntegrations => false;

	public bool EnableConsoleLogging => enableConsoleLogging;

	public override bool UsePlatformIntegrations
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

	public event Action OnInitialized;

	public override bool CanSend()
	{
		if (base.CanSend() && voiceServiceImpl != null)
		{
			return voiceServiceImpl.CanSend();
		}
		return false;
	}

	public override async Task<VoiceServiceRequest> Activate(string text, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		if (CanSend())
		{
			SetupRequestParameters(ref requestOptions, ref requestEvents);
			VoiceServiceRequest request = await voiceServiceImpl.Activate(text, requestOptions, requestEvents);
			if (sendTranscriptionEventsForMessages && !string.IsNullOrEmpty(text))
			{
				request.Events.OnFullResponse.AddListener(delegate(WitResponseNode r)
				{
					if (string.IsNullOrEmpty(r.GetTranscription()))
					{
						r["text"] = text;
					}
				});
				request.Events.OnSend.AddListener(delegate
				{
					request.Events?.OnFullTranscription?.Invoke(text);
					VoiceEvents.OnFullTranscription?.Invoke(text);
				});
			}
			return request;
		}
		return null;
	}

	public override bool CanActivateAudio()
	{
		if (base.CanActivateAudio() && voiceServiceImpl != null)
		{
			return voiceServiceImpl.CanActivateAudio();
		}
		return false;
	}

	public override string GetActivateAudioError()
	{
		if (!HasPlatformIntegrations && !AudioBuffer.Instance.IsInputAvailable)
		{
			return "No Microphone(s)/recording devices found.  You will be unable to capture audio on this device.";
		}
		return base.GetActivateAudioError();
	}

	public override VoiceServiceRequest Activate(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		if (CanActivateAudio() && CanSend())
		{
			return voiceServiceImpl.Activate(requestOptions, requestEvents);
		}
		if (voiceServiceImpl == null)
		{
			VLog.D("Voice is not initialized. Attempting to initialize before activating.");
			InitVoiceSDK();
			if (CanActivateAudio() && CanSend())
			{
				return voiceServiceImpl?.Activate(requestOptions, requestEvents);
			}
		}
		VLog.W("Cannot currently activate\nAudio Activation Error: " + GetActivateAudioError() + "\nSend Error: " + GetSendError());
		return null;
	}

	public override VoiceServiceRequest ActivateImmediately(WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents)
	{
		SetupRequestParameters(ref requestOptions, ref requestEvents);
		if (CanActivateAudio() && CanSend())
		{
			return voiceServiceImpl.ActivateImmediately(requestOptions, requestEvents);
		}
		if (voiceServiceImpl == null)
		{
			VLog.D("Voice is not initialized. Attempting to initialize before immediate activation");
			InitVoiceSDK();
			if (CanActivateAudio() && CanSend())
			{
				return voiceServiceImpl?.ActivateImmediately(requestOptions, requestEvents);
			}
		}
		VLog.W("Cannot currently activate\nAudio Activation Error: " + GetActivateAudioError() + "\nSend Error: " + GetSendError());
		return null;
	}

	public override void Deactivate()
	{
		voiceServiceImpl?.Deactivate();
	}

	public override void DeactivateAndAbortRequest()
	{
		voiceServiceImpl?.DeactivateAndAbortRequest();
	}

	private void InitVoiceSDK()
	{
		if (string.IsNullOrEmpty(PACKAGE_VERSION))
		{
			VLog.E("No SDK Version Set");
		}
		if (!UsePlatformIntegrations)
		{
			if (voiceServiceImpl is VoiceSDKImpl)
			{
				((VoiceSDKImpl)voiceServiceImpl).Disconnect();
			}
			if (voiceSDKLoggerImpl is VoiceSDKPlatformLoggerImpl)
			{
				try
				{
					((VoiceSDKPlatformLoggerImpl)voiceSDKLoggerImpl).Disconnect();
				}
				catch (Exception ex)
				{
					VLog.E("Disconnection error: " + ex.Message);
				}
			}
		}
		bool flag = voiceServiceImpl != null;
		if (!flag)
		{
			voiceServiceImpl = base.gameObject.GetComponent<IPlatformIntegrationOverride>();
			flag = voiceServiceImpl != null;
			if (flag)
			{
				VLog.I($"Using PI override\nClass: {voiceServiceImpl.GetType()}");
				UsePlatformIntegrations = false;
			}
		}
		voiceSDKLoggerImpl = new VoiceSDKConsoleLoggerImpl();
		if (!flag)
		{
			RevertToWitUnity();
		}
		if (voiceServiceImpl is IWitRuntimeConfigSetter witRuntimeConfigSetter)
		{
			witRuntimeConfigSetter.RuntimeConfiguration = witRuntimeConfiguration;
		}
		voiceServiceImpl.VoiceEvents = VoiceEvents;
		voiceServiceImpl.TelemetryEvents = TelemetryEvents;
		voiceSDKLoggerImpl.IsUsingPlatformIntegration = UsePlatformIntegrations;
		voiceSDKLoggerImpl.WitApplication = RuntimeConfiguration?.witConfiguration?.GetLoggerAppId();
		voiceSDKLoggerImpl.ShouldLogToConsole = EnableConsoleLogging;
		this.OnInitialized?.Invoke();
	}

	private void RevertToWitUnity()
	{
		VLog.I("Initializing Wit Unity...");
		Wit wit = GetComponent<Wit>();
		if (null == wit)
		{
			wit = base.gameObject.AddComponent<Wit>();
			wit.hideFlags = HideFlags.HideInInspector;
		}
		wit.ShouldWrap = false;
		voiceServiceImpl = wit;
		UsePlatformIntegrations = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (MicPermissionsManager.HasMicPermission())
		{
			InitVoiceSDK();
		}
		else
		{
			MicPermissionsManager.RequestMicPermission();
		}
		VoiceEvents.OnMinimumWakeThresholdHit?.AddListener(OnMinimumWakeThresholdHit);
		VoiceEvents.OnMicDataSent?.AddListener(OnMicDataSent);
		VoiceEvents.OnStoppedListeningDueToTimeout?.AddListener(OnStoppedListeningDueToTimeout);
		VoiceEvents.OnStoppedListeningDueToInactivity?.AddListener(OnStoppedListeningDueToInactivity);
		VoiceEvents.OnStoppedListeningDueToDeactivation?.AddListener(OnStoppedListeningDueToDeactivation);
		TelemetryEvents.OnAudioTrackerFinished?.AddListener(OnAudioDurationTrackerFinished);
		StartCoroutine(RetryInit());
	}

	private IEnumerator RetryInit()
	{
		int waitSeconds = 1;
		while (voiceServiceImpl == null)
		{
			VLog.W($"Voice Service still not initialized yet. Retrying in {waitSeconds} seconds.");
			yield return new WaitForSeconds(waitSeconds);
			if (voiceServiceImpl == null)
			{
				InitVoiceSDK();
				waitSeconds++;
				if (waitSeconds == 10)
				{
					waitSeconds = 1;
				}
				continue;
			}
			break;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		voiceServiceImpl = null;
		voiceSDKLoggerImpl = null;
		VoiceEvents.OnMinimumWakeThresholdHit?.RemoveListener(OnMinimumWakeThresholdHit);
		VoiceEvents.OnMicDataSent?.RemoveListener(OnMicDataSent);
		VoiceEvents.OnStoppedListeningDueToTimeout?.RemoveListener(OnStoppedListeningDueToTimeout);
		VoiceEvents.OnStoppedListeningDueToInactivity?.RemoveListener(OnStoppedListeningDueToInactivity);
		VoiceEvents.OnStoppedListeningDueToDeactivation?.RemoveListener(OnStoppedListeningDueToDeactivation);
		TelemetryEvents.OnAudioTrackerFinished?.RemoveListener(OnAudioDurationTrackerFinished);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (base.enabled && hasFocus && !Initialized && MicPermissionsManager.HasMicPermission())
		{
			InitVoiceSDK();
		}
	}

	protected override void OnRequestInit(VoiceServiceRequest request)
	{
		base.OnRequestInit(request);
		_waitingForFirstPartialAudio = true;
		voiceSDKLoggerImpl.LogInteractionStart(request.Options?.RequestId, (request.InputType == NLPRequestInputType.Text) ? "message" : "speech");
		voiceSDKLoggerImpl.LogAnnotation("minWakeThreshold", RuntimeConfiguration?.soundWakeThreshold.ToString(CultureInfo.InvariantCulture));
		voiceSDKLoggerImpl.LogAnnotation("minKeepAliveTimeSec", RuntimeConfiguration?.minKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
		voiceSDKLoggerImpl.LogAnnotation("minTranscriptionKeepAliveTimeSec", RuntimeConfiguration?.minTranscriptionKeepAliveTimeInSeconds.ToString(CultureInfo.InvariantCulture));
		voiceSDKLoggerImpl.LogAnnotation("maxRecordingTime", RuntimeConfiguration?.maxRecordingTime.ToString(CultureInfo.InvariantCulture));
	}

	protected override void OnRequestStartListening(VoiceServiceRequest request)
	{
		base.OnRequestStartListening(request);
		voiceSDKLoggerImpl.LogInteractionPoint("startedListening");
	}

	protected override void OnRequestStopListening(VoiceServiceRequest request)
	{
		base.OnRequestStopListening(request);
		voiceSDKLoggerImpl.LogInteractionPoint("stoppedListening");
	}

	protected override void OnRequestSend(VoiceServiceRequest request)
	{
		base.OnRequestSend(request);
		voiceSDKLoggerImpl.LogInteractionPoint("witRequestCreated");
		if (request != null)
		{
			voiceSDKLoggerImpl.LogAnnotation("requestIdOverride", request.Options?.RequestId);
		}
	}

	protected override void OnRequestPartialTranscription(VoiceServiceRequest request, string transcription)
	{
		base.OnRequestPartialTranscription(request, transcription);
		voiceSDKLoggerImpl.LogFirstTranscriptionTime();
	}

	protected override void OnRequestFullTranscription(VoiceServiceRequest request, string transcription)
	{
		base.OnRequestFullTranscription(request, transcription);
		voiceSDKLoggerImpl.LogInteractionPoint("fullTranscriptionTime");
	}

	private void OnMinimumWakeThresholdHit()
	{
		voiceSDKLoggerImpl.LogInteractionPoint("minWakeThresholdHit");
	}

	private void OnStoppedListeningDueToTimeout()
	{
		voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningTimeout");
	}

	private void OnStoppedListeningDueToInactivity()
	{
		voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningInactivity");
	}

	private void OnStoppedListeningDueToDeactivation()
	{
		voiceSDKLoggerImpl.LogInteractionPoint("stoppedListeningDeactivate");
	}

	private void OnMicDataSent()
	{
		voiceSDKLoggerImpl.LogInteractionPoint("micDataSent");
	}

	private void OnAudioDurationTrackerFinished(long timestamp, double audioDuration)
	{
		voiceSDKLoggerImpl.LogAnnotation("adt_duration", audioDuration.ToString(CultureInfo.InvariantCulture));
		voiceSDKLoggerImpl.LogAnnotation("adt_finished", timestamp.ToString());
	}

	protected override void OnRequestSuccess(VoiceServiceRequest request)
	{
		base.OnRequestSuccess(request);
		WitResponseNode witResponseNode = (request?.ResponseData)?["speech"]?["tokens"];
		if (witResponseNode != null)
		{
			int count = witResponseNode.Count;
			string annotationValue = witResponseNode[count - 1]?["end"]?.Value;
			voiceSDKLoggerImpl.LogAnnotation("audioLength", annotationValue);
		}
	}

	protected override void OnRequestComplete(VoiceServiceRequest request)
	{
		base.OnRequestComplete(request);
		if (voiceSDKLoggerImpl == null)
		{
			VLog.W("voiceSDKLoggerImpl is null");
		}
		else if (request.State == VoiceRequestState.Failed)
		{
			voiceSDKLoggerImpl.LogInteractionEndFailure(request.Results.Message);
		}
		else if (request.State == VoiceRequestState.Canceled)
		{
			voiceSDKLoggerImpl.LogInteractionEndFailure("aborted");
		}
		else
		{
			voiceSDKLoggerImpl.LogInteractionEndSuccess();
		}
	}
}
