using System;
using System.Linq;
using POpusCodec.Enums;
using Photon.Voice.Windows;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Voice.Unity;

[AddComponentMenu("Photon Voice/Recorder")]
[HelpURL("https://doc.photonengine.com/en-us/voice/v2/getting-started/recorder")]
[DisallowMultipleComponent]
public class Recorder : VoiceComponent
{
	public enum InputSourceType
	{
		Microphone,
		AudioClip,
		Factory
	}

	public enum MicType
	{
		Unity,
		Photon
	}

	[Obsolete("No longer needed. Implicit conversion is done internally when needed.")]
	public enum SampleTypeConv
	{
		None,
		Short
	}

	[Obsolete("Use Photon.Voice.Unity.PhotonVoiceCreatedParams")]
	public class PhotonVoiceCreatedParams : Photon.Voice.Unity.PhotonVoiceCreatedParams
	{
	}

	public const int MIN_OPUS_BITRATE = 6000;

	public const int MAX_OPUS_BITRATE = 510000;

	private static readonly Array samplingRateValues = Enum.GetValues(typeof(SamplingRate));

	[SerializeField]
	private bool voiceDetection;

	[SerializeField]
	private float voiceDetectionThreshold = 0.01f;

	[SerializeField]
	private int voiceDetectionDelayMs = 500;

	private object userData;

	private LocalVoice voice = LocalVoiceAudioDummy.Dummy;

	private string unityMicrophoneDevice;

	private int photonMicrophoneDeviceId = -1;

	private IAudioDesc inputSource;

	private VoiceClient client;

	private VoiceConnection voiceConnection;

	[SerializeField]
	[FormerlySerializedAs("audioGroup")]
	private byte interestGroup;

	[SerializeField]
	private bool debugEchoMode;

	[SerializeField]
	private bool reliableMode;

	[SerializeField]
	private bool encrypt;

	[SerializeField]
	private bool transmitEnabled;

	[SerializeField]
	private SamplingRate samplingRate = SamplingRate.Sampling24000;

	[SerializeField]
	private OpusCodec.FrameDuration frameDuration = OpusCodec.FrameDuration.Frame20ms;

	[SerializeField]
	[Range(6000f, 510000f)]
	private int bitrate = 30000;

	[SerializeField]
	private InputSourceType sourceType;

	[SerializeField]
	private MicType microphoneType;

	[SerializeField]
	private AudioClip audioClip;

	[SerializeField]
	private bool loopAudioClip = true;

	private bool isRecording;

	private Func<IAudioDesc> inputFactory;

	[Obsolete]
	private static IDeviceEnumerator photonMicrophoneEnumerator;

	private IAudioInChangeNotifier photonMicChangeNotifier;

	[SerializeField]
	private bool reactOnSystemChanges;

	private bool subscribedToSystemChangesPhoton;

	private bool subscribedToSystemChangesUnity;

	[SerializeField]
	private bool autoStart = true;

	[SerializeField]
	private bool recordOnlyWhenEnabled;

	[SerializeField]
	private bool skipDeviceChangeChecks;

	private bool wasRecordingBeforePause;

	private bool isPausedOrInBackground;

	[SerializeField]
	private bool stopRecordingWhenPaused;

	[SerializeField]
	private bool useOnAudioFilterRead;

	[SerializeField]
	private bool trySamplingRateMatch;

	[SerializeField]
	private bool useMicrophoneTypeFallback = true;

	[SerializeField]
	private bool recordOnlyWhenJoined = true;

	private bool recordingStoppedExplicitly;

	private IDeviceEnumerator photonMicrophonesEnumerator;

	private AudioInEnumerator unityMicrophonesEnumerator;

	private object microphoneDeviceChangeDetectedLock = new object();

	internal bool microphoneDeviceChangeDetected;

	public LocalVoice Voice => voice;

	public IAudioDesc InputSource => inputSource;

	internal bool MicrophoneDeviceChangeDetected
	{
		get
		{
			lock (microphoneDeviceChangeDetectedLock)
			{
				return microphoneDeviceChangeDetected;
			}
		}
		set
		{
			lock (microphoneDeviceChangeDetectedLock)
			{
				if (microphoneDeviceChangeDetected == value)
				{
					if (base.Logger.IsWarningEnabled)
					{
						base.Logger.LogWarning("Unexpected: MicrophoneDeviceChangeDetected to be overriden with same value: {0}", value);
					}
				}
				else
				{
					microphoneDeviceChangeDetected = value;
				}
			}
		}
	}

	private bool subscribedToSystemChanges
	{
		get
		{
			if (!subscribedToSystemChangesUnity)
			{
				return subscribedToSystemChangesPhoton;
			}
			return true;
		}
	}

	[Obsolete("Use the generic unified non-static MicrophonesEnumerator")]
	public static IDeviceEnumerator PhotonMicrophoneEnumerator
	{
		get
		{
			if (photonMicrophoneEnumerator == null)
			{
				photonMicrophoneEnumerator = CreatePhotonDeviceEnumerator(new VoiceLogger("PhotonMicrophoneEnumerator"));
			}
			return photonMicrophoneEnumerator;
		}
	}

	public bool IsInitialized => client != null;

	[Obsolete("Renamed to RequiresRestart")]
	public bool RequiresInit => RequiresRestart;

	public bool RequiresRestart { get; protected set; }

	public bool TransmitEnabled
	{
		get
		{
			return transmitEnabled;
		}
		set
		{
			if (value != transmitEnabled)
			{
				transmitEnabled = value;
				if (voice != LocalVoiceAudioDummy.Dummy)
				{
					voice.TransmitEnabled = value;
				}
			}
		}
	}

	public bool Encrypt
	{
		get
		{
			return encrypt;
		}
		set
		{
			if (encrypt != value)
			{
				encrypt = value;
				voice.Encrypt = value;
			}
		}
	}

	public bool DebugEchoMode
	{
		get
		{
			if (debugEchoMode && InterestGroup != 0)
			{
				voice.DebugEchoMode = false;
				debugEchoMode = false;
			}
			return debugEchoMode;
		}
		set
		{
			if (debugEchoMode == value)
			{
				return;
			}
			if (InterestGroup != 0)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Cannot enable DebugEchoMode when InterestGroup value ({0}) is different than 0.", interestGroup);
				}
			}
			else
			{
				debugEchoMode = value;
				voice.DebugEchoMode = value;
			}
		}
	}

	public bool ReliableMode
	{
		get
		{
			return reliableMode;
		}
		set
		{
			if (voice != LocalVoiceAudioDummy.Dummy)
			{
				voice.Reliable = value;
			}
			reliableMode = value;
		}
	}

	public bool VoiceDetection
	{
		get
		{
			GetStatusFromDetector();
			return voiceDetection;
		}
		set
		{
			voiceDetection = value;
			if (VoiceDetector != null)
			{
				VoiceDetector.On = value;
			}
		}
	}

	public float VoiceDetectionThreshold
	{
		get
		{
			GetThresholdFromDetector();
			return voiceDetectionThreshold;
		}
		set
		{
			if (voiceDetectionThreshold.Equals(value))
			{
				return;
			}
			if (value < 0f || value > 1f)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Value out of range: VAD Threshold needs to be between [0..1], requested value: {0}", value);
				}
			}
			else
			{
				voiceDetectionThreshold = value;
				if (VoiceDetector != null)
				{
					VoiceDetector.Threshold = voiceDetectionThreshold;
				}
			}
		}
	}

	public int VoiceDetectionDelayMs
	{
		get
		{
			GetActivityDelayFromDetector();
			return voiceDetectionDelayMs;
		}
		set
		{
			if (voiceDetectionDelayMs != value)
			{
				voiceDetectionDelayMs = value;
				if (VoiceDetector != null)
				{
					VoiceDetector.ActivityDelayMs = value;
				}
			}
		}
	}

	public object UserData
	{
		get
		{
			return userData;
		}
		set
		{
			if (userData != value)
			{
				userData = value;
				if (IsRecording)
				{
					RequiresRestart = true;
					_ = base.Logger.IsInfoEnabled;
				}
			}
		}
	}

	public Func<IAudioDesc> InputFactory
	{
		get
		{
			return inputFactory;
		}
		set
		{
			if (!(inputFactory != value))
			{
				return;
			}
			inputFactory = value;
			if (IsRecording && SourceType == InputSourceType.Factory)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "InputFactory");
				}
			}
		}
	}

	public AudioUtil.IVoiceDetector VoiceDetector
	{
		get
		{
			if (voiceAudio == null)
			{
				return null;
			}
			return voiceAudio.VoiceDetector;
		}
	}

	public string UnityMicrophoneDevice
	{
		get
		{
			if (!IsValidUnityMic(unityMicrophoneDevice))
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("\"{0}\" is not a valid Unity microphone device, switching to default", unityMicrophoneDevice);
				}
				unityMicrophoneDevice = null;
				if (UnityMicrophone.devices.Length != 0)
				{
					unityMicrophoneDevice = UnityMicrophone.devices[0];
				}
			}
			return unityMicrophoneDevice;
		}
		set
		{
			if (!IsValidUnityMic(value))
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("\"{0}\" is not a valid Unity microphone device", value);
				}
			}
			else
			{
				if (CompareUnityMicNames(unityMicrophoneDevice, value))
				{
					return;
				}
				unityMicrophoneDevice = value;
				if (string.IsNullOrEmpty(unityMicrophoneDevice) && UnityMicrophone.devices.Length != 0)
				{
					unityMicrophoneDevice = UnityMicrophone.devices[0];
				}
				if (IsRecording && SourceType == InputSourceType.Microphone && MicrophoneType == MicType.Unity)
				{
					RequiresRestart = true;
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "UnityMicrophoneDevice");
					}
				}
				CheckAndSetSamplingRate();
			}
		}
	}

	public int PhotonMicrophoneDeviceId
	{
		get
		{
			if (!IsValidPhotonMic())
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("\"{0}\" is not a valid Photon microphone device ID, switching to default (-1)", photonMicrophoneDeviceId);
				}
				photonMicrophoneDeviceId = -1;
			}
			return photonMicrophoneDeviceId;
		}
		set
		{
			if (!IsValidPhotonMic(value))
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("\"{0}\" is not a valid Photon microphone device ID", value);
				}
			}
			else
			{
				if (photonMicrophoneDeviceId == value)
				{
					return;
				}
				photonMicrophoneDeviceId = value;
				if (IsRecording && SourceType == InputSourceType.Microphone && MicrophoneType == MicType.Photon)
				{
					RequiresRestart = true;
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "PhotonMicrophoneDeviceId");
					}
				}
			}
		}
	}

	[Obsolete("Use InterestGroup instead")]
	public byte AudioGroup
	{
		get
		{
			return InterestGroup;
		}
		set
		{
			InterestGroup = value;
		}
	}

	public byte InterestGroup
	{
		get
		{
			if (isRecording && voice.InterestGroup != interestGroup)
			{
				interestGroup = voice.InterestGroup;
				if (debugEchoMode && interestGroup != 0)
				{
					debugEchoMode = false;
				}
			}
			return interestGroup;
		}
		set
		{
			if (interestGroup == value)
			{
				return;
			}
			if (debugEchoMode && value != 0)
			{
				debugEchoMode = false;
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("DebugEchoMode disabled because InterestGroup changed to {0}. DebugEchoMode works only with Interest Group 0.", value);
				}
			}
			interestGroup = value;
			voice.InterestGroup = value;
		}
	}

	public bool IsCurrentlyTransmitting
	{
		get
		{
			if (IsRecording && TransmitEnabled)
			{
				return voice.IsCurrentlyTransmitting;
			}
			return false;
		}
	}

	public AudioUtil.ILevelMeter LevelMeter
	{
		get
		{
			if (voiceAudio == null)
			{
				return null;
			}
			return voiceAudio.LevelMeter;
		}
	}

	public bool VoiceDetectorCalibrating
	{
		get
		{
			if (voiceAudio != null && TransmitEnabled)
			{
				return voiceAudio.VoiceDetectorCalibrating;
			}
			return false;
		}
	}

	protected ILocalVoiceAudio voiceAudio => voice as ILocalVoiceAudio;

	public InputSourceType SourceType
	{
		get
		{
			return sourceType;
		}
		set
		{
			if (sourceType == value)
			{
				return;
			}
			sourceType = value;
			if (IsRecording)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "Source");
				}
			}
			CheckAndSetSamplingRate();
		}
	}

	public MicType MicrophoneType
	{
		get
		{
			return microphoneType;
		}
		set
		{
			if (microphoneType == value)
			{
				return;
			}
			microphoneType = value;
			if (IsRecording && SourceType == InputSourceType.Microphone)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "MicrophoneType");
				}
			}
			CheckAndSetSamplingRate();
		}
	}

	[Obsolete("No longer used. Implicit conversion is done internally when needed.")]
	public SampleTypeConv TypeConvert { get; set; }

	public AudioClip AudioClip
	{
		get
		{
			return audioClip;
		}
		set
		{
			if (!(audioClip != value))
			{
				return;
			}
			audioClip = value;
			if (IsRecording && SourceType == InputSourceType.AudioClip)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "AudioClip");
				}
			}
			CheckAndSetSamplingRate();
		}
	}

	public bool LoopAudioClip
	{
		get
		{
			return loopAudioClip;
		}
		set
		{
			if (loopAudioClip == value)
			{
				return;
			}
			loopAudioClip = value;
			if (IsRecording && SourceType == InputSourceType.AudioClip)
			{
				if (inputSource is AudioClipWrapper audioClipWrapper)
				{
					audioClipWrapper.Loop = value;
				}
				else if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unexpected: Recorder inputSource is not of AudioClipWrapper type or is null.");
				}
			}
		}
	}

	public SamplingRate SamplingRate
	{
		get
		{
			return samplingRate;
		}
		set
		{
			CheckAndSetSamplingRate(value);
		}
	}

	public OpusCodec.FrameDuration FrameDuration
	{
		get
		{
			return frameDuration;
		}
		set
		{
			if (frameDuration == value)
			{
				return;
			}
			frameDuration = value;
			if (IsRecording)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "FrameDuration");
				}
			}
		}
	}

	public int Bitrate
	{
		get
		{
			return bitrate;
		}
		set
		{
			if (bitrate == value)
			{
				return;
			}
			if (value < 6000 || value > 510000)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unsupported bitrate value {0}, valid range: {1}-{2}", value, 6000, 510000);
				}
				return;
			}
			bitrate = value;
			if (IsRecording)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "Bitrate");
				}
			}
		}
	}

	public bool IsRecording
	{
		get
		{
			return isRecording;
		}
		set
		{
			if (isRecording != value)
			{
				if (isRecording)
				{
					StopRecording();
				}
				else
				{
					StartRecording();
				}
			}
		}
	}

	public bool ReactOnSystemChanges
	{
		get
		{
			return reactOnSystemChanges;
		}
		set
		{
			if (reactOnSystemChanges == value)
			{
				return;
			}
			reactOnSystemChanges = value;
			if (!IsRecording)
			{
				return;
			}
			if (reactOnSystemChanges)
			{
				if (!subscribedToSystemChanges)
				{
					SubscribeToSystemChanges();
				}
			}
			else if (subscribedToSystemChanges)
			{
				UnsubscribeFromSystemChanges();
			}
		}
	}

	public bool AutoStart
	{
		get
		{
			return autoStart;
		}
		set
		{
			if (autoStart != value)
			{
				autoStart = value;
				CheckAndAutoStart();
			}
		}
	}

	public bool RecordOnlyWhenEnabled
	{
		get
		{
			return recordOnlyWhenEnabled;
		}
		set
		{
			if (recordOnlyWhenEnabled == value)
			{
				return;
			}
			recordOnlyWhenEnabled = value;
			if (recordOnlyWhenEnabled)
			{
				if (!base.isActiveAndEnabled && IsRecording)
				{
					StopRecordingInternal();
				}
			}
			else
			{
				CheckAndAutoStart();
			}
		}
	}

	public bool SkipDeviceChangeChecks
	{
		get
		{
			return skipDeviceChangeChecks;
		}
		set
		{
			skipDeviceChangeChecks = value;
		}
	}

	public bool StopRecordingWhenPaused
	{
		get
		{
			return stopRecordingWhenPaused;
		}
		set
		{
			stopRecordingWhenPaused = value;
		}
	}

	public bool UseOnAudioFilterRead
	{
		get
		{
			return useOnAudioFilterRead;
		}
		set
		{
			if (useOnAudioFilterRead == value)
			{
				return;
			}
			useOnAudioFilterRead = value;
			if (IsRecording && SourceType == InputSourceType.Microphone && MicrophoneType == MicType.Unity)
			{
				RequiresRestart = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "UseOnAudioFilterRead");
				}
			}
		}
	}

	public bool TrySamplingRateMatch
	{
		get
		{
			return trySamplingRateMatch;
		}
		set
		{
			if (trySamplingRateMatch != value)
			{
				trySamplingRateMatch = value;
				if (trySamplingRateMatch)
				{
					CheckAndSetSamplingRate();
				}
			}
		}
	}

	public bool UseMicrophoneTypeFallback
	{
		get
		{
			return useMicrophoneTypeFallback;
		}
		set
		{
			useMicrophoneTypeFallback = value;
		}
	}

	public bool RecordOnlyWhenJoined
	{
		get
		{
			return recordOnlyWhenJoined;
		}
		set
		{
			if (recordOnlyWhenJoined == value)
			{
				return;
			}
			recordOnlyWhenJoined = value;
			if (recordOnlyWhenJoined)
			{
				if (IsRecording && voiceConnection.Client != null && !voiceConnection.Client.InRoom)
				{
					StopRecordingInternal();
				}
			}
			else
			{
				CheckAndAutoStart();
			}
		}
	}

	public IDeviceEnumerator MicrophonesEnumerator => GetMicrophonesEnumerator(MicrophoneType);

	public DeviceInfo MicrophoneDevice
	{
		get
		{
			switch (MicrophoneType)
			{
			case MicType.Unity:
			{
				string text = UnityMicrophoneDevice;
				if (string.IsNullOrEmpty(text))
				{
					return MicrophonesEnumerator.First();
				}
				return GetDeviceById(text);
			}
			case MicType.Photon:
			{
				int num = PhotonMicrophoneDeviceId;
				if (num != -1)
				{
					return GetDeviceById(num);
				}
				break;
			}
			}
			return DeviceInfo.Default;
		}
		set
		{
			switch (MicrophoneType)
			{
			case MicType.Unity:
				UnityMicrophoneDevice = value.IDString;
				break;
			case MicType.Photon:
				PhotonMicrophoneDeviceId = (value.IsDefault ? (-1) : value.IDInt);
				break;
			}
		}
	}

	public void Init(VoiceConnection connection)
	{
		if ((object)connection == null)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("voiceConnection is null.");
			}
			return;
		}
		if (!connection)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("voiceConnection is destroyed.");
			}
			return;
		}
		if (!base.IgnoreGlobalLogLevel)
		{
			base.LogLevel = connection.GlobalRecordersLogLevel;
		}
		if (IsInitialized)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder already initialized.");
			}
		}
		else if (connection.VoiceClient == null)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("voiceConnection.VoiceClient is null.");
			}
		}
		else
		{
			voiceConnection = connection;
			client = connection.VoiceClient;
			voiceConnection.AddInitializedRecorder(this);
			CheckAndAutoStart();
		}
	}

	[Obsolete("Renamed to RestartRecording")]
	public void ReInit()
	{
		RestartRecording();
	}

	public void RestartRecording(bool force = false)
	{
		if (!force && !RequiresRestart)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder does not require restart.");
			}
			return;
		}
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Restarting recording, RequiresRestart?={0} forcedRestart?={1}", RequiresRestart, force);
		}
		StopRecording();
		StartRecording();
	}

	public void VoiceDetectorCalibrate(int durationMs, Action<float> detectionEndedCallback = null)
	{
		if (voiceAudio == null)
		{
			return;
		}
		if (!TransmitEnabled)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot start voice detection calibration when transmission is not enabled");
			}
			return;
		}
		voiceAudio.VoiceDetectorCalibrate(durationMs, delegate
		{
			GetThresholdFromDetector();
			if (detectionEndedCallback != null)
			{
				detectionEndedCallback(voiceDetectionThreshold);
			}
		});
	}

	public void StartRecording()
	{
		if (IsRecording)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder is already started.");
			}
		}
		else if (!IsInitialized)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recording can't be started if Recorder is not initialized. Call Recorder.Init(VoiceConnection) first.");
			}
		}
		else if (RecordOnlyWhenEnabled && !base.isActiveAndEnabled)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recording can't be started because RecordOnlyWhenEnabled is true and Recorder is not enabled or its GameObject is not active in hierarchy.");
			}
		}
		else if (RecordOnlyWhenJoined && voiceConnection.Client != null && !voiceConnection.Client.InRoom)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recording can't be started because RecordOnlyWhenJoined is true and voice networking client is not joined to a room.");
			}
		}
		else
		{
			StartRecordingInternal();
		}
	}

	public void StopRecording()
	{
		wasRecordingBeforePause = false;
		if (!IsRecording)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Recorder is not started.");
			}
		}
		else
		{
			StopRecordingInternal();
			recordingStoppedExplicitly = true;
		}
	}

	public bool ResetLocalAudio()
	{
		if (inputSource != null && inputSource is IResettable)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Resetting local audio.");
			}
			(inputSource as IResettable).Reset();
			return true;
		}
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("InputSource is null or not resettable.");
		}
		return false;
	}

	public static bool CompareUnityMicNames(string mic1, string mic2)
	{
		if (IsDefaultUnityMic(mic1) && IsDefaultUnityMic(mic2))
		{
			return true;
		}
		if (mic1 != null && mic1.Equals(mic2))
		{
			return true;
		}
		return false;
	}

	public static bool IsDefaultUnityMic(string mic)
	{
		if (!string.IsNullOrEmpty(mic))
		{
			return Array.IndexOf(UnityMicrophone.devices, mic) == 0;
		}
		return true;
	}

	private void Setup()
	{
		voice = CreateLocalVoiceAudioAndSource();
		if (voice == LocalVoiceAudioDummy.Dummy)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Local input source setup and voice stream creation failed. No recording or transmission will be happening. See previous error log messages for more details.");
			}
			if (inputSource != null)
			{
				inputSource.Dispose();
				inputSource = null;
			}
			if (MicrophoneDeviceChangeDetected)
			{
				MicrophoneDeviceChangeDetected = false;
			}
			return;
		}
		SubscribeToSystemChanges();
		if (VoiceDetector != null)
		{
			VoiceDetector.Threshold = voiceDetectionThreshold;
			VoiceDetector.ActivityDelayMs = voiceDetectionDelayMs;
			VoiceDetector.On = voiceDetection;
		}
		voice.InterestGroup = InterestGroup;
		voice.DebugEchoMode = DebugEchoMode;
		voice.Encrypt = Encrypt;
		voice.Reliable = ReliableMode;
		RequiresRestart = false;
		isRecording = true;
		SendPhotonVoiceCreatedMessage();
		voice.TransmitEnabled = TransmitEnabled;
	}

	private LocalVoice CreateLocalVoiceAudioAndSource()
	{
		SamplingRate samplingRate = this.samplingRate;
		int num = (int)samplingRate;
		bool flag;
		DeviceInfo microphoneDevice;
		int deviceID;
		string text;
		switch (SourceType)
		{
		case InputSourceType.Microphone:
		{
			if (!CheckIfThereIsAtLeastOneMic())
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("No microphone detected.");
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			flag = false;
			MicType micType = MicrophoneType;
			if (micType == MicType.Unity)
			{
				goto IL_0075;
			}
			if (micType != MicType.Photon)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("unknown MicrophoneType value {0}", MicrophoneType);
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			goto IL_014d;
		}
		case InputSourceType.AudioClip:
		{
			if ((object)AudioClip == null)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("AudioClip property must be set for AudioClip audio source");
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			AudioClipWrapper audioClipWrapper = new AudioClipWrapper(AudioClip);
			audioClipWrapper.Loop = LoopAudioClip;
			inputSource = audioClipWrapper;
			break;
		}
		case InputSourceType.Factory:
			if (InputFactory == null)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Recorder.InputFactory must be specified if Recorder.Source set to Factory");
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			inputSource = InputFactory();
			if (inputSource.Error != null && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("InputFactory creation failure: {0}.", inputSource.Error);
			}
			break;
		default:
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("unknown Source value {0}", SourceType);
				}
				return LocalVoiceAudioDummy.Dummy;
			}
			IL_014d:
			microphoneDevice = MicrophoneDevice;
			deviceID = (microphoneDevice.IsDefault ? (-1) : microphoneDevice.IDInt);
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Setting recorder's source to Photon microphone device={0}", microphoneDevice);
			}
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Setting recorder's source to WindowsAudioInPusher");
			}
			inputSource = new WindowsAudioInPusher(deviceID, base.Logger);
			if (inputSource != null)
			{
				if (inputSource.Error == null)
				{
					break;
				}
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Photon microphone input source creation failure: {0}", inputSource.Error);
				}
			}
			if (!UseMicrophoneTypeFallback || flag)
			{
				break;
			}
			flag = true;
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Photon microphone failed. Falling back to Unity microphone");
			}
			goto IL_0075;
			IL_0075:
			text = UnityMicrophoneDevice;
			_ = base.Logger.IsInfoEnabled;
			if (UseOnAudioFilterRead)
			{
				Debug.Log("Using MicWrapperPusher");
				inputSource = new MicWrapperPusher(text, base.transform, num, base.Logger);
			}
			else
			{
				inputSource = CreateMicWrapper(text, num, base.Logger);
			}
			if (inputSource != null)
			{
				if (inputSource.Error == null)
				{
					break;
				}
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unity microphone input source creation failure: {0}", inputSource.Error);
				}
			}
			if (!UseMicrophoneTypeFallback || flag)
			{
				break;
			}
			flag = true;
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Unity microphone failed. Falling back to Photon microphone");
			}
			goto IL_014d;
		}
		if (inputSource == null || inputSource.Error != null)
		{
			return LocalVoiceAudioDummy.Dummy;
		}
		if (inputSource.Channels == 0)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("inputSource.Channels is zero");
			}
			return LocalVoiceAudioDummy.Dummy;
		}
		if (TrySamplingRateMatch && inputSource.SamplingRate != num)
		{
			samplingRate = GetSupportedSamplingRate(inputSource.SamplingRate);
			if (samplingRate != this.samplingRate && base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Sampling rate requested ({0}Hz) is not used, input source is expecting {1}Hz instead so switching to the closest supported value: {1}Hz.", num, inputSource.SamplingRate, (int)samplingRate);
			}
		}
		AudioSampleType sampleType = AudioSampleType.Source;
		WebRtcAudioDsp component = GetComponent<WebRtcAudioDsp>();
		if ((object)component != null && (bool)component && component.enabled)
		{
			sampleType = AudioSampleType.Short;
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Type Conversion set to Short. Audio samples will be converted if source samples types differ.");
			}
			num = (int)samplingRate;
			switch (samplingRate)
			{
			case SamplingRate.Sampling12000:
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Sampling rate requested (12kHz) is not supported by WebRTC Audio DSP, switching to the closest supported value: 16kHz.");
				}
				samplingRate = SamplingRate.Sampling16000;
				break;
			case SamplingRate.Sampling24000:
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Sampling rate requested (24kHz) is not supported by WebRTC Audio DSP, switching to the closest supported value: 48kHz.");
				}
				samplingRate = SamplingRate.Sampling48000;
				break;
			}
			OpusCodec.FrameDuration frameDuration = FrameDuration;
			if (frameDuration == OpusCodec.FrameDuration.Frame2dot5ms || frameDuration == OpusCodec.FrameDuration.Frame5ms)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Frame duration requested ({0}ms) is not supported by WebRTC Audio DSP (it needs to be N x 10ms), switching to the closest supported value: 10ms.", (int)FrameDuration / 1000);
				}
				FrameDuration = OpusCodec.FrameDuration.Frame10ms;
			}
		}
		this.samplingRate = samplingRate;
		VoiceInfo voiceInfo = VoiceInfo.CreateAudioOpus(samplingRate, inputSource.Channels, FrameDuration, Bitrate, UserData);
		return client.CreateLocalVoiceAudioFromSource(voiceInfo, inputSource, sampleType);
	}

	protected virtual MicWrapper CreateMicWrapper(string micDev, int samplingRateInt, VoiceLogger logger)
	{
		return new MicWrapper(micDev, samplingRateInt, logger);
	}

	protected virtual void SendPhotonVoiceCreatedMessage()
	{
		base.gameObject.SendMessage("PhotonVoiceCreated", new Photon.Voice.Unity.PhotonVoiceCreatedParams
		{
			Voice = voice,
			AudioDesc = inputSource
		}, SendMessageOptions.DontRequireReceiver);
	}

	private void OnDestroy()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Recorder is about to be destroyed, removing local voice.");
		}
		RemoveVoice();
		if (IsInitialized)
		{
			voiceConnection.RemoveInitializedRecorder(this);
		}
	}

	private void RemoveVoice()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("RemovingVoice()");
		}
		if (subscribedToSystemChanges)
		{
			UnsubscribeFromSystemChanges();
		}
		GetThresholdFromDetector();
		GetStatusFromDetector();
		GetActivityDelayFromDetector();
		if (voice != LocalVoiceAudioDummy.Dummy)
		{
			interestGroup = voice.InterestGroup;
			if (debugEchoMode && interestGroup != 0)
			{
				debugEchoMode = false;
			}
			voice.RemoveSelf();
			voice = LocalVoiceAudioDummy.Dummy;
		}
		if (inputSource != null)
		{
			inputSource.Dispose();
			inputSource = null;
		}
		base.gameObject.SendMessage("PhotonVoiceRemoved", SendMessageOptions.DontRequireReceiver);
		isRecording = false;
		RequiresRestart = false;
	}

	private void OnAudioConfigChanged(bool deviceWasChanged)
	{
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("OnAudioConfigChanged deviceWasChanged={0}", deviceWasChanged);
		}
		if (SkipDeviceChangeChecks || deviceWasChanged)
		{
			MicrophoneDeviceChangeDetected = true;
		}
	}

	private void PhotonMicrophoneChangeDetected()
	{
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Microphones change detected by Photon native plugin");
		}
		MicrophoneDeviceChangeDetected = true;
	}

	internal void HandleDeviceChange()
	{
		if (!MicrophoneDeviceChangeDetected && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("Unexpected: HandleDeviceChange called while MicrophoneDeviceChangedDetected is false.");
		}
		if (photonMicrophoneEnumerator != null)
		{
			photonMicrophoneEnumerator.Refresh();
		}
		if (photonMicrophonesEnumerator != null)
		{
			photonMicrophonesEnumerator.Refresh();
		}
		if (unityMicrophonesEnumerator != null)
		{
			unityMicrophonesEnumerator.Refresh();
		}
		if (IsRecording)
		{
			bool flag = false;
			if (SkipDeviceChangeChecks)
			{
				flag = true;
			}
			else if (SourceType == InputSourceType.Microphone)
			{
				flag = ((MicrophoneType != MicType.Photon) ? (string.IsNullOrEmpty(unityMicrophoneDevice) || !IsValidUnityMic(unityMicrophoneDevice)) : (photonMicrophoneDeviceId == -1 || !IsValidPhotonMic()));
			}
			if (!flag)
			{
				return;
			}
			if (ResetLocalAudio())
			{
				MicrophoneDeviceChangeDetected = false;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Local audio reset as a result of audio config/device change.");
				}
				return;
			}
			RequiresRestart = true;
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Restarting Recording as a result of audio config/device change.");
			}
			RestartRecording();
		}
		else
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("A microphone device may have been made available: will check auto start conditions and if all good will attempt to start recording.");
			}
			CheckAndAutoStart(autoStartFlag: true);
		}
	}

	private void SubscribeToSystemChanges()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Subscribing to system (audio) changes.");
		}
		if (!ReactOnSystemChanges)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("ReactOnSystemChanges is false, not subscribed to system (audio) changes.");
			}
			return;
		}
		if (subscribedToSystemChanges)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Already subscribed to system (audio) changes.");
			}
			return;
		}
		photonMicChangeNotifier = Platform.CreateAudioInChangeNotifier(PhotonMicrophoneChangeDetected, base.Logger);
		if (photonMicChangeNotifier.IsSupported)
		{
			if (photonMicChangeNotifier.Error == null)
			{
				subscribedToSystemChangesPhoton = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Subscribed to audio in change notifications via Photon plugin.");
				}
				return;
			}
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Error creating instance of photonMicChangeNotifier: {0}", photonMicChangeNotifier.Error);
			}
		}
		photonMicChangeNotifier.Dispose();
		photonMicChangeNotifier = null;
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigChanged;
		subscribedToSystemChangesUnity = true;
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Subscribed to audio configuration changes via Unity callback.");
		}
	}

	private void UnsubscribeFromSystemChanges()
	{
		if (subscribedToSystemChangesUnity)
		{
			AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigChanged;
			subscribedToSystemChangesUnity = false;
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Unsubscribed from audio configuration changes via Unity callback.");
			}
		}
		if (!subscribedToSystemChangesPhoton)
		{
			return;
		}
		if (photonMicChangeNotifier == null)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Unexpected: photonMicChangeNotifier is null while subscribedToSystemChangesPhoton is true.");
			}
		}
		else
		{
			photonMicChangeNotifier.Dispose();
			photonMicChangeNotifier = null;
		}
		subscribedToSystemChangesPhoton = false;
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Unsubscribed from audio in change notifications via Photon plugin.");
		}
	}

	private void GetThresholdFromDetector()
	{
		if (!IsRecording || VoiceDetector == null || voiceDetectionThreshold.Equals(VoiceDetector.Threshold))
		{
			return;
		}
		if (VoiceDetector.Threshold <= 1f && VoiceDetector.Threshold >= 0f)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("VoiceDetectionThreshold automatically changed from {0} to {1}", voiceDetectionThreshold, VoiceDetector.Threshold);
			}
			voiceDetectionThreshold = VoiceDetector.Threshold;
		}
		else if (base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("VoiceDetector.Threshold has unexpected value {0}", VoiceDetector.Threshold);
		}
	}

	private void GetActivityDelayFromDetector()
	{
		if (IsRecording && VoiceDetector != null && voiceDetectionDelayMs != VoiceDetector.ActivityDelayMs)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("VoiceDetectionDelayMs automatically changed from {0} to {1}", voiceDetectionDelayMs, VoiceDetector.ActivityDelayMs);
			}
			voiceDetectionDelayMs = VoiceDetector.ActivityDelayMs;
		}
	}

	private void GetStatusFromDetector()
	{
		if (IsRecording && VoiceDetector != null && voiceDetection != VoiceDetector.On)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("VoiceDetection automatically changed from {0} to {1}", voiceDetection, VoiceDetector.On);
			}
			voiceDetection = VoiceDetector.On;
		}
	}

	private static bool IsValidUnityMic(string mic)
	{
		if (!string.IsNullOrEmpty(mic))
		{
			return UnityMicrophone.devices.Contains(mic);
		}
		return true;
	}

	private void OnEnable()
	{
		wasRecordingBeforePause = false;
		isPausedOrInBackground = false;
		CheckAndAutoStart();
	}

	private void OnDisable()
	{
		if (RecordOnlyWhenEnabled && IsRecording)
		{
			StopRecordingInternal();
		}
	}

	private bool IsValidPhotonMic()
	{
		return IsValidPhotonMic(photonMicrophoneDeviceId);
	}

	public static bool CheckIfMicrophoneIdIsValid(IDeviceEnumerator audioInEnumerator, int id)
	{
		if (id == -1)
		{
			return true;
		}
		if (audioInEnumerator.IsSupported && audioInEnumerator.Error == null)
		{
			foreach (DeviceInfo item in audioInEnumerator)
			{
				if (item.IDInt == id)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsValidPhotonMic(int id)
	{
		return CheckIfMicrophoneIdIsValid(GetMicrophonesEnumerator(MicType.Photon), id);
	}

	private void OnApplicationPause(bool paused)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnApplicationPause({0})", paused);
		}
		HandleApplicationPause(paused);
	}

	private void OnApplicationFocus(bool focused)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnApplicationFocus({0})", focused);
		}
		HandleApplicationPause(!focused);
	}

	private void HandleApplicationPause(bool paused)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("App paused?= {0}, isPausedOrInBackground = {1}, wasRecordingBeforePause = {2}, StopRecordingWhenPaused = {3}, IsRecording = {4}", paused, isPausedOrInBackground, wasRecordingBeforePause, StopRecordingWhenPaused, IsRecording);
		}
		if (isPausedOrInBackground == paused)
		{
			return;
		}
		if (paused)
		{
			wasRecordingBeforePause = IsRecording;
			isPausedOrInBackground = true;
			if (StopRecordingWhenPaused && IsRecording)
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Stopping recording as application went to background or paused");
				}
				RemoveVoice();
			}
			return;
		}
		if (!StopRecordingWhenPaused)
		{
			if (ResetLocalAudio() && base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Local audio reset as application is back from background or unpaused");
			}
		}
		else if (wasRecordingBeforePause)
		{
			if (!IsRecording)
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Starting recording as application is back from background or unpaused");
				}
				Setup();
			}
			else if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Unexpected: Application back from background or unpaused, isPausedOrInBackground = true, wasRecordingBeforePause = true, StopRecordingWhenPaused = true, IsRecording = true");
			}
		}
		wasRecordingBeforePause = false;
		isPausedOrInBackground = false;
	}

	private SamplingRate GetSupportedSamplingRate(int requested)
	{
		if (Enum.IsDefined(typeof(SamplingRate), requested))
		{
			return (SamplingRate)requested;
		}
		int num = int.MaxValue;
		SamplingRate result = SamplingRate.Sampling48000;
		foreach (SamplingRate samplingRateValue in samplingRateValues)
		{
			int num2 = Math.Abs((int)(samplingRateValue - requested));
			if (num2 < num)
			{
				num = num2;
				result = samplingRateValue;
			}
		}
		return result;
	}

	private SamplingRate GetSupportedSamplingRateForUnityMicrophone(SamplingRate requested)
	{
		UnityMicrophone.GetDeviceCaps(UnityMicrophoneDevice, out var minFreq, out var maxFreq);
		return GetSupportedSamplingRate(requested, minFreq, maxFreq);
	}

	private SamplingRate GetSupportedSamplingRate(SamplingRate requested, int minFreq, int maxFreq)
	{
		SamplingRate result = requested;
		int num = (int)this.samplingRate;
		if (num < minFreq || (maxFreq != 0 && num > maxFreq))
		{
			if (Enum.IsDefined(typeof(SamplingRate), maxFreq))
			{
				result = (SamplingRate)maxFreq;
			}
			else
			{
				num = maxFreq;
				int num2 = int.MaxValue;
				foreach (SamplingRate samplingRateValue in samplingRateValues)
				{
					int num3 = (int)samplingRateValue;
					if (num3 >= minFreq && (maxFreq == 0 || num3 <= maxFreq))
					{
						int num4 = Math.Abs(num3 - num);
						if (num4 < num2)
						{
							num2 = num4;
							result = samplingRateValue;
						}
					}
				}
			}
		}
		return result;
	}

	private SamplingRate GetSupportedSamplingRate(SamplingRate sR)
	{
		switch (SourceType)
		{
		case InputSourceType.Microphone:
			return MicrophoneType switch
			{
				MicType.Unity => GetSupportedSamplingRateForUnityMicrophone(sR), 
				MicType.Photon => SamplingRate.Sampling16000, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		case InputSourceType.AudioClip:
			if (AudioClip != null)
			{
				return GetSupportedSamplingRate(AudioClip.frequency);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case InputSourceType.Factory:
			break;
		}
		return sR;
	}

	private void CheckAndSetSamplingRate(SamplingRate sR)
	{
		if (TrySamplingRateMatch)
		{
			SamplingRate supportedSamplingRate = GetSupportedSamplingRate(sR);
			if (supportedSamplingRate == samplingRate)
			{
				return;
			}
			if (supportedSamplingRate != sR && base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Sampling rate requested ({0}Hz) not supported using closest value ({1}Hz)", (int)sR, (int)supportedSamplingRate);
			}
			samplingRate = supportedSamplingRate;
		}
		else
		{
			if (sR == samplingRate)
			{
				return;
			}
			samplingRate = sR;
		}
		if (IsRecording)
		{
			RequiresRestart = true;
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Recorder.{0} changed, Recorder requires restart for this to take effect.", "SamplingRate");
			}
		}
	}

	private void CheckAndSetSamplingRate()
	{
		CheckAndSetSamplingRate(samplingRate);
	}

	internal void StopRecordingInternal()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Stopping recording");
		}
		wasRecordingBeforePause = false;
		RemoveVoice();
		if (MicrophoneDeviceChangeDetected)
		{
			MicrophoneDeviceChangeDetected = false;
		}
	}

	internal void CheckAndAutoStart()
	{
		CheckAndAutoStart(autoStart);
	}

	internal void CheckAndAutoStart(bool autoStartFlag)
	{
		bool flag = true;
		if (!autoStartFlag)
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: autoStart flag is false.");
			}
		}
		if (!IsInitialized)
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: recorder not initialized.");
			}
		}
		if (isRecording)
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: recorder is already started.");
			}
		}
		if (recordingStoppedExplicitly)
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: recorder was previously stopped explicitly.");
			}
		}
		if (recordOnlyWhenEnabled && !base.isActiveAndEnabled)
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: recorder not enabled and this is required.");
			}
		}
		if (recordOnlyWhenJoined && ((object)voiceConnection == null || !voiceConnection || voiceConnection.Client == null || !voiceConnection.Client.InRoom))
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: voice client not joined to a room yet and this is required.");
			}
		}
		if (SourceType == InputSourceType.Microphone && !CheckIfThereIsAtLeastOneMic())
		{
			flag = false;
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("Auto start check failure: no microphone detected.");
			}
		}
		if (flag)
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("AutoStart requirements met: going to auto start recording");
			}
			StartRecordingInternal();
		}
		else if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("AutoStart requirements NOT met: NOT going to auto start recording");
		}
	}

	internal void StartRecordingInternal()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Starting recording");
		}
		wasRecordingBeforePause = false;
		recordingStoppedExplicitly = false;
		Setup();
	}

	private IDeviceEnumerator GetMicrophonesEnumerator(MicType micType)
	{
		switch (micType)
		{
		case MicType.Unity:
			if (unityMicrophonesEnumerator == null)
			{
				unityMicrophonesEnumerator = new AudioInEnumerator(base.Logger);
				if (!unityMicrophonesEnumerator.IsSupported && base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("UnityMicrophonesEnumerator is not supported on this platform {0}.", VoiceComponent.CurrentPlatform);
				}
				else if (unityMicrophonesEnumerator.Error != null && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError(unityMicrophonesEnumerator.Error);
				}
			}
			return unityMicrophonesEnumerator;
		case MicType.Photon:
			if (photonMicrophonesEnumerator == null)
			{
				photonMicrophonesEnumerator = CreatePhotonDeviceEnumerator(base.Logger);
			}
			return photonMicrophonesEnumerator;
		default:
			return null;
		}
	}

	private DeviceInfo GetDeviceById(int id)
	{
		foreach (DeviceInfo item in MicrophonesEnumerator)
		{
			if (item.IDInt == id)
			{
				return item;
			}
		}
		return DeviceInfo.Default;
	}

	private DeviceInfo GetDeviceById(string id)
	{
		foreach (DeviceInfo item in MicrophonesEnumerator)
		{
			if (string.Equals(item.IDString, id))
			{
				return item;
			}
		}
		return DeviceInfo.Default;
	}

	private bool CheckIfThereIsAtLeastOneMic()
	{
		if (MicrophoneType == MicType.Photon)
		{
			IDeviceEnumerator microphonesEnumerator = MicrophonesEnumerator;
			if (microphonesEnumerator != null)
			{
				return microphonesEnumerator.Any();
			}
		}
		return UnityMicrophone.devices.Length != 0;
	}

	private static IDeviceEnumerator CreatePhotonDeviceEnumerator(VoiceLogger voiceLogger)
	{
		IDeviceEnumerator deviceEnumerator = Platform.CreateAudioInEnumerator(voiceLogger);
		if (!deviceEnumerator.IsSupported && voiceLogger.IsWarningEnabled)
		{
			voiceLogger.LogWarning("PhotonMicrophonesEnumerator is not supported on this platform {0}.", VoiceComponent.CurrentPlatform);
		}
		else if (deviceEnumerator.Error != null && voiceLogger.IsErrorEnabled)
		{
			voiceLogger.LogError(deviceEnumerator.Error);
		}
		return deviceEnumerator;
	}
}
