using System;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Voice.Unity;

[RequireComponent(typeof(Recorder))]
[DisallowMultipleComponent]
public class WebRtcAudioDsp : VoiceComponent
{
	[SerializeField]
	private bool aec = true;

	[SerializeField]
	private bool aecHighPass;

	[SerializeField]
	private bool agc = true;

	[SerializeField]
	private int agcCompressionGain = 9;

	[SerializeField]
	private bool vad = true;

	[SerializeField]
	private bool highPass;

	[SerializeField]
	private bool bypass;

	[SerializeField]
	private bool noiseSuppression;

	[SerializeField]
	private int reverseStreamDelayMs = 120;

	private int reverseChannels;

	private WebRTCAudioProcessor proc;

	private AudioListener audioListener;

	private AudioOutCapture audioOutCapture;

	private bool aecStarted;

	private bool autoDestroyAudioOutCapture;

	private static readonly Dictionary<AudioSpeakerMode, int> channelsMap = new Dictionary<AudioSpeakerMode, int>
	{
		{
			AudioSpeakerMode.Mono,
			1
		},
		{
			AudioSpeakerMode.Stereo,
			2
		},
		{
			AudioSpeakerMode.Quad,
			4
		},
		{
			AudioSpeakerMode.Surround,
			5
		},
		{
			AudioSpeakerMode.Mode5point1,
			6
		},
		{
			AudioSpeakerMode.Mode7point1,
			8
		},
		{
			AudioSpeakerMode.Prologic,
			2
		}
	};

	private LocalVoiceAudioShort localVoice;

	private int outputSampleRate;

	private Recorder recorder;

	[SerializeField]
	private bool aecOnlyWhenEnabled = true;

	public bool AutoRestartOnAudioChannelsMismatch = true;

	private object threadSafety = new object();

	[Obsolete("Obsolete as it's not recommended to set this to true. https://forum.photonengine.com/discussion/comment/48017/#Comment_48017")]
	public bool AECMobileComfortNoise;

	public bool AEC
	{
		get
		{
			lock (threadSafety)
			{
				if (IsInitialized && (!aecOnlyWhenEnabled || base.isActiveAndEnabled))
				{
					return aecStarted;
				}
			}
			return aec;
		}
		set
		{
			if (value == aec)
			{
				return;
			}
			aec = value;
			lock (threadSafety)
			{
				ToggleAec();
			}
		}
	}

	[Obsolete("Use AEC instead on all platforms, internally according AEC will be used either mobile or not.")]
	public bool AECMobile
	{
		get
		{
			return AEC;
		}
		set
		{
			AEC = value;
		}
	}

	public bool AecHighPass
	{
		get
		{
			return aecHighPass;
		}
		set
		{
			if (value == aecHighPass)
			{
				return;
			}
			aecHighPass = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.AECHighPass = aecHighPass;
				}
			}
		}
	}

	public int ReverseStreamDelayMs
	{
		get
		{
			return reverseStreamDelayMs;
		}
		set
		{
			if (reverseStreamDelayMs == value)
			{
				return;
			}
			reverseStreamDelayMs = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.AECStreamDelayMs = reverseStreamDelayMs;
				}
			}
		}
	}

	public bool NoiseSuppression
	{
		get
		{
			return noiseSuppression;
		}
		set
		{
			if (value == noiseSuppression)
			{
				return;
			}
			noiseSuppression = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.NoiseSuppression = noiseSuppression;
				}
			}
		}
	}

	public bool HighPass
	{
		get
		{
			return highPass;
		}
		set
		{
			if (value == highPass)
			{
				return;
			}
			highPass = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.HighPass = highPass;
				}
			}
		}
	}

	public bool Bypass
	{
		get
		{
			return bypass;
		}
		set
		{
			if (value != bypass)
			{
				bypass = value;
				if (IsInitialized)
				{
					proc.Bypass = bypass;
				}
			}
		}
	}

	public bool AGC
	{
		get
		{
			return agc;
		}
		set
		{
			if (value == agc)
			{
				return;
			}
			agc = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.AGC = agc;
				}
			}
		}
	}

	public int AgcCompressionGain
	{
		get
		{
			return agcCompressionGain;
		}
		set
		{
			if (agcCompressionGain == value)
			{
				return;
			}
			if (value < 0 || value > 90)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("AgcCompressionGain value {0} not in range [0..90]", value);
				}
				return;
			}
			agcCompressionGain = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.AGCCompressionGain = agcCompressionGain;
				}
			}
		}
	}

	public bool VAD
	{
		get
		{
			return vad;
		}
		set
		{
			if (value == vad)
			{
				return;
			}
			vad = value;
			lock (threadSafety)
			{
				if (IsInitialized)
				{
					proc.VAD = vad;
				}
			}
		}
	}

	public bool IsInitialized => proc != null;

	public bool AecOnlyWhenEnabled
	{
		get
		{
			return aecOnlyWhenEnabled;
		}
		set
		{
			if (aecOnlyWhenEnabled != value)
			{
				aecOnlyWhenEnabled = value;
				lock (threadSafety)
				{
					ToggleAec();
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
		if (!SupportedPlatformCheck())
		{
			return;
		}
		recorder = GetComponent<Recorder>();
		if ((object)recorder == null || !recorder)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("A Recorder component needs to be attached to the same GameObject");
			}
			base.enabled = false;
		}
		else if (!base.IgnoreGlobalLogLevel)
		{
			base.LogLevel = recorder.LogLevel;
		}
	}

	private void OnEnable()
	{
		lock (threadSafety)
		{
			if (!SupportedPlatformCheck())
			{
				return;
			}
			if (IsInitialized)
			{
				ToggleAec();
			}
			else if (recorder.IsRecording)
			{
				if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("WebRtcAudioDsp is added after recording has started, restarting recording to take effect");
				}
				recorder.RestartRecording(force: true);
			}
		}
	}

	private void OnDisable()
	{
		lock (threadSafety)
		{
			if (aecOnlyWhenEnabled && aecStarted)
			{
				ToggleAecOutputListener(on: false);
			}
		}
	}

	private bool SupportedPlatformCheck()
	{
		return true;
	}

	private void ToggleAec()
	{
		if (!IsInitialized || (aecOnlyWhenEnabled && !base.isActiveAndEnabled) || aec == aecStarted)
		{
			return;
		}
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Toggling AEC to {0}", aec);
		}
		if (!ToggleAecOutputListener(aec))
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AEC failed to be toggled to {0}", aec);
			}
		}
		else if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("AEC successfully toggled to {0}", aec);
		}
	}

	private bool ToggleAecOutputListener(bool on)
	{
		if (on != aecStarted)
		{
			if (on)
			{
				if (aecOnlyWhenEnabled && !base.isActiveAndEnabled)
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Could not start AEC because AecOnlyWhenEnabled is true and isActiveAndEnabled is false");
					}
					return false;
				}
				if ((object)audioOutCapture == null || !audioOutCapture)
				{
					if (!InitAudioOutCapture())
					{
						if (base.Logger.IsErrorEnabled)
						{
							base.Logger.LogError("Could not start AEC OutputListener because a valid AudioOutCapture could not be set.");
						}
						return false;
					}
				}
				else
				{
					if (!AudioOutCaptureChecks(audioOutCapture, listenerChecks: true))
					{
						if (base.Logger.IsErrorEnabled)
						{
							base.Logger.LogError("Could not start AEC OutputListener because AudioOutCapture provided is not valid.");
						}
						return false;
					}
					AudioListener component = audioOutCapture.GetComponent<AudioListener>();
					if (audioListener != component)
					{
						if (base.Logger.IsWarningEnabled)
						{
							base.Logger.LogWarning("Unexpected: AudioListener changed but AudioOutCapture did not.");
						}
						audioListener = component;
					}
				}
				if (IsInitialized)
				{
					StartAec();
				}
			}
			else
			{
				if (UnsubscribeFromAudioOutCapture(autoDestroyAudioOutCapture))
				{
					if (base.Logger.IsDebugEnabled)
					{
						base.Logger.LogDebug("AEC OutputListener stopped.");
					}
				}
				else if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: AudioOutCapture is null but aecStarted == true");
				}
				if (IsInitialized)
				{
					proc.AEC = false;
					proc.AECMobile = false;
				}
				else if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: proc is null but aecStarted was true.");
				}
				aecStarted = false;
			}
			return true;
		}
		return false;
	}

	private void StartAec()
	{
		proc.AECStreamDelayMs = reverseStreamDelayMs;
		proc.AECHighPass = aecHighPass;
		proc.AEC = true;
		proc.AECMobile = false;
		aecStarted = true;
		audioOutCapture.OnAudioFrame += OnAudioOutFrameFloat;
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("AEC OutputListener started.");
		}
	}

	private void OnAudioConfigurationChanged(bool deviceWasChanged)
	{
		lock (threadSafety)
		{
			if (!IsInitialized)
			{
				return;
			}
			bool flag = false;
			if (outputSampleRate != AudioSettings.outputSampleRate)
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("AudioConfigChange: outputSampleRate from {0} to {1}. WebRtcAudioDsp will be restarted.", outputSampleRate, AudioSettings.outputSampleRate);
				}
				outputSampleRate = AudioSettings.outputSampleRate;
				flag = true;
			}
			if (reverseChannels != channelsMap[AudioSettings.speakerMode])
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("AudioConfigChange: speakerMode channels from {0} to {1}. WebRtcAudioDsp will be restarted.", reverseChannels, channelsMap[AudioSettings.speakerMode]);
				}
				reverseChannels = channelsMap[AudioSettings.speakerMode];
				flag = true;
			}
			if (flag)
			{
				Restart();
			}
		}
	}

	private void OnAudioOutFrameFloat(float[] data, int outChannels)
	{
		lock (threadSafety)
		{
			if (!IsInitialized)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unexpected: OnAudioOutFrame called while WebRtcAudioDsp is not initialized (proc == null).");
				}
				return;
			}
			if (!aecStarted && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Unexpected: OnAudioOutFrame called while aecStarted is false.");
			}
			if (outChannels != reverseChannels)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Unexpected: OnAudioOutFrame channel count {0} != initialized {1}. Switching channels and restarting.", outChannels, reverseChannels);
				}
				if (AutoRestartOnAudioChannelsMismatch)
				{
					reverseChannels = outChannels;
					Restart();
				}
			}
			else
			{
				proc.OnAudioOutFrameFloat(data);
			}
		}
	}

	private void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
	{
		lock (threadSafety)
		{
			if (!base.enabled)
			{
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Skipped PhotonVoiceCreated message because component is disabled.");
				}
				return;
			}
			if (recorder != null && recorder.SourceType != 0 && base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("WebRtcAudioDsp is better suited to be used with Microphone as Recorder Input Source Type.");
			}
			if (p.Voice.Info.Channels != 1)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Only mono audio signals supported. WebRtcAudioDsp component will be disabled.");
				}
				base.enabled = false;
			}
			else if (p.Voice is LocalVoiceAudioShort localVoiceAudioShort)
			{
				localVoice = localVoiceAudioShort;
				reverseChannels = channelsMap[AudioSettings.speakerMode];
				outputSampleRate = AudioSettings.outputSampleRate;
				Init();
				localVoice.AddPostProcessor(proc);
				ToggleAec();
			}
			else
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("Only short audio voice supported. WebRtcAudioDsp component will be disabled.");
				}
				base.enabled = false;
			}
		}
	}

	private void PhotonVoiceRemoved()
	{
		StopAllProcessing();
	}

	private void OnDestroy()
	{
		StopAllProcessing();
		AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
	}

	private void StopAllProcessing()
	{
		lock (threadSafety)
		{
			ToggleAecOutputListener(on: false);
			if (IsInitialized)
			{
				proc.Dispose();
				proc = null;
			}
			localVoice = null;
		}
	}

	private void Restart()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Restarting");
		}
		if (IsInitialized)
		{
			bool flag = false;
			if (aecStarted)
			{
				if (UnsubscribeFromAudioOutCapture(destroy: false))
				{
					if (base.Logger.IsDebugEnabled)
					{
						base.Logger.LogDebug("AEC OutputListener stopped.");
					}
					flag = true;
					aecStarted = false;
				}
				else if (base.Logger.IsWarningEnabled)
				{
					base.Logger.LogWarning("Unexpected: AudioOutCapture is null but aecStarted == true");
				}
			}
			proc.Dispose();
			proc = null;
			if (Init())
			{
				localVoice.AddPostProcessor(proc);
				if (flag)
				{
					StartAec();
				}
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Restart complete successfully.");
				}
			}
			else if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Restart failed because processor could not be re initialized.");
			}
		}
		else if (base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("Cannot restart if not initialized.");
		}
	}

	private bool Init()
	{
		if (IsInitialized)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Already initialized");
			}
			return false;
		}
		proc = new WebRTCAudioProcessor(base.Logger, localVoice.Info.FrameSize, localVoice.Info.SamplingRate, localVoice.Info.Channels, outputSampleRate, reverseChannels);
		proc.HighPass = highPass;
		proc.NoiseSuppression = noiseSuppression;
		proc.AGC = agc;
		proc.AGCCompressionGain = agcCompressionGain;
		proc.VAD = vad;
		proc.Bypass = bypass;
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Initialized");
		}
		return true;
	}

	private bool SetOrSwitchAudioListener(AudioListener listener, bool extraChecks, bool log = true)
	{
		if (extraChecks && !AudioListenerChecks(listener))
		{
			return false;
		}
		AudioOutCapture[] components = listener.GetComponents<AudioOutCapture>();
		if (components.Length > 1 && base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("{0} AudioOutCapture components attached to the same GameObject, is this expected?", components.Length);
		}
		for (int i = 0; i < components.Length; i++)
		{
			if (SetOrSwitchAudioOutCapture(components[i], extraChecks: false, log: false))
			{
				autoDestroyAudioOutCapture = false;
				return true;
			}
		}
		AudioOutCapture audioOutCapture = listener.gameObject.AddComponent<AudioOutCapture>();
		if (SetOrSwitchAudioOutCapture(audioOutCapture, extraChecks: false, log))
		{
			if (base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("AudioOutCapture component added to same GameObject as AudioListener.");
			}
			autoDestroyAudioOutCapture = true;
			return true;
		}
		UnityEngine.Object.Destroy(audioOutCapture);
		return false;
	}

	private bool SetOrSwitchAudioOutCapture(AudioOutCapture capture, bool extraChecks, bool log = true)
	{
		if (!AudioOutCaptureChecks(capture, extraChecks, log))
		{
			return false;
		}
		bool flag = aecStarted;
		bool flag2 = false;
		if ((object)audioOutCapture != null && (bool)audioOutCapture)
		{
			if (audioOutCapture != capture)
			{
				if (!UnsubscribeFromAudioOutCapture(autoDestroyAudioOutCapture))
				{
					if (base.Logger.IsErrorEnabled)
					{
						base.Logger.LogError("Could not unsubscribe from previous AudioOutCapture. Switching to a new one won't happen.");
					}
					return false;
				}
				flag2 = true;
			}
			else if (extraChecks)
			{
				if (log && base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("The same AudioOutCapture is being used already");
				}
				return false;
			}
		}
		audioOutCapture = capture;
		audioListener = capture.GetComponent<AudioListener>();
		if (flag && flag2)
		{
			audioOutCapture.OnAudioFrame += OnAudioOutFrameFloat;
		}
		return true;
	}

	private bool InitAudioOutCapture()
	{
		if ((object)audioOutCapture != null && (bool)audioOutCapture)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioOutCapture is already initialized.");
			}
			return false;
		}
		if ((object)audioListener == null)
		{
			AudioOutCapture[] array = UnityEngine.Object.FindObjectsByType<AudioOutCapture>(FindObjectsSortMode.None);
			if (array.Length > 1 && base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("{0} AudioOutCapture components found, is this expected?", array.Length);
			}
			foreach (AudioOutCapture capture in array)
			{
				if (SetOrSwitchAudioOutCapture(capture, extraChecks: true, log: false))
				{
					autoDestroyAudioOutCapture = false;
					return true;
				}
			}
			AudioListener[] array2 = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
			if (array2.Length == 0)
			{
				if (base.Logger.IsErrorEnabled)
				{
					base.Logger.LogError("No AudioListener component found, is this expected?");
				}
			}
			else if (array2.Length > 1 && base.Logger.IsDebugEnabled)
			{
				base.Logger.LogDebug("{0} AudioListener components found, is this expected?", array2.Length);
			}
			foreach (AudioListener listener in array2)
			{
				if (SetOrSwitchAudioListener(listener, extraChecks: true, log: false))
				{
					return true;
				}
			}
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioListener and AudioOutCapture components are required for AEC to work.");
			}
			return false;
		}
		return SetOrSwitchAudioListener(audioListener, extraChecks: true);
	}

	private bool UnsubscribeFromAudioOutCapture(bool destroy)
	{
		if ((object)audioOutCapture != null)
		{
			if (aecStarted)
			{
				audioOutCapture.OnAudioFrame -= OnAudioOutFrameFloat;
				if (base.Logger.IsDebugEnabled)
				{
					base.Logger.LogDebug("OnAudioFrame event unsubscribed.");
				}
			}
			if (destroy)
			{
				UnityEngine.Object.Destroy(audioOutCapture);
				if (base.Logger.IsDebugEnabled)
				{
					base.Logger.LogDebug("AudioOutCapture component destroyed.");
				}
				audioOutCapture = null;
			}
			return true;
		}
		if (aecStarted && base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("Unexpected: audioOutCapture is null but aecStarted is true");
		}
		return false;
	}

	private bool AudioListenerChecks(AudioListener listener, bool log = true)
	{
		if ((object)listener == null)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioListener is null.");
			}
			return false;
		}
		if (!listener)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioListener is destroyed.");
			}
			return false;
		}
		if (!listener.gameObject.activeInHierarchy)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("The GameObject to which the AudioListener is attached is not active in hierarchy.");
			}
			return false;
		}
		if (!listener.enabled)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioListener is disabled.");
			}
			return false;
		}
		return true;
	}

	private bool AudioOutCaptureChecks(AudioOutCapture capture, bool listenerChecks, bool log = true)
	{
		if ((object)capture == null)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioOutCapture is null.");
			}
			return false;
		}
		if (!capture)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioOutCapture is destroyed.");
			}
			return false;
		}
		if (!listenerChecks && !capture.gameObject.activeInHierarchy)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("The GameObject to which the AudioOutCapture is attached is not active in hierarchy.");
			}
			return false;
		}
		if (!capture.enabled)
		{
			if (log && base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("AudioOutCapture is disabled.");
			}
			return false;
		}
		if (listenerChecks)
		{
			return AudioListenerChecks(capture.GetComponent<AudioListener>(), log);
		}
		return true;
	}

	public bool SetOrSwitchAudioListener(AudioListener listener)
	{
		lock (threadSafety)
		{
			return SetOrSwitchAudioListener(listener, extraChecks: true);
		}
	}

	public bool SetOrSwitchAudioOutCapture(AudioOutCapture capture)
	{
		lock (threadSafety)
		{
			if (SetOrSwitchAudioOutCapture(capture, extraChecks: true))
			{
				autoDestroyAudioOutCapture = false;
				return true;
			}
			return false;
		}
	}
}
