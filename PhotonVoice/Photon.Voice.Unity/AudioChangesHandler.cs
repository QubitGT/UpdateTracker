using UnityEngine;

namespace Photon.Voice.Unity;

[RequireComponent(typeof(Recorder))]
public class AudioChangesHandler : VoiceComponent
{
	private IAudioInChangeNotifier photonMicChangeNotifier;

	private AudioConfiguration audioConfiguration;

	private Recorder recorder;

	[Tooltip("Try to start recording when we get devices change notification and recording is not started.")]
	public bool StartWhenDeviceChange = true;

	[Tooltip("Try to react to device change notification when Recorder is started.")]
	public bool HandleDeviceChange = true;

	[Tooltip("Try to react to audio config change notification when Recorder is started.")]
	public bool HandleConfigChange = true;

	[Tooltip("Whether or not to make use of Photon's AudioInChangeNotifier native plugin.")]
	public bool UseNativePluginChangeNotifier = true;

	[Tooltip("Whether or not to make use of Unity's OnAudioConfigurationChanged.")]
	public bool UseOnAudioConfigurationChanged = true;

	private bool subscribedToSystemChangesPhoton;

	private bool subscribedToSystemChangesUnity;

	protected override void Awake()
	{
		base.Awake();
		recorder = GetComponent<Recorder>();
		recorder.ReactOnSystemChanges = false;
		audioConfiguration = AudioSettings.GetConfiguration();
		SubscribeToSystemChanges();
	}

	private void OnDestroy()
	{
		UnsubscribeFromSystemChanges();
	}

	private void OnDeviceChange()
	{
		if (!recorder.IsRecording)
		{
			if (StartWhenDeviceChange)
			{
				recorder.MicrophoneDeviceChangeDetected = true;
				if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("An attempt to auto start recording should follow shortly.");
				}
			}
			else if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Device change detected but will not try to start recording as StartWhenDeviceChange is false.");
			}
		}
		else if (HandleDeviceChange)
		{
			recorder.MicrophoneDeviceChangeDetected = true;
		}
		else if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Device change detected but will not try to handle this as HandleDeviceChange is false.");
		}
	}

	private void SubscribeToSystemChanges()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Subscribing to system (audio) changes.");
		}
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Skipped subscribing to audio change notifications via Photon's AudioInChangeNotifier as not supported on current platform: {0}", VoiceComponent.CurrentPlatform);
		}
		if (subscribedToSystemChangesPhoton && base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("Unexpected: subscribedToSystemChangesPhoton is set to true while platform is not supported!.");
		}
		if (subscribedToSystemChangesUnity)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Already subscribed to audio changes via Unity OnAudioConfigurationChanged callback.");
			}
			return;
		}
		AudioSettings.OnAudioConfigurationChanged += OnAudioConfigChanged;
		subscribedToSystemChangesUnity = true;
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Subscribed to audio configuration changes via Unity OnAudioConfigurationChanged callback.");
		}
	}

	private void OnAudioConfigChanged(bool deviceWasChanged)
	{
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("OnAudioConfigurationChanged: {0}", deviceWasChanged ? "Device was changed." : "AudioSettings.Reset was called.");
		}
		AudioConfiguration configuration = AudioSettings.GetConfiguration();
		bool flag = false;
		if (configuration.dspBufferSize != audioConfiguration.dspBufferSize)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("OnAudioConfigurationChanged: dspBufferSize old={0} new={1}", audioConfiguration.dspBufferSize, configuration.dspBufferSize);
			}
			flag = true;
		}
		if (configuration.numRealVoices != audioConfiguration.numRealVoices)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("OnAudioConfigurationChanged: numRealVoices old={0} new={1}", audioConfiguration.numRealVoices, configuration.numRealVoices);
			}
			flag = true;
		}
		if (configuration.numVirtualVoices != audioConfiguration.numVirtualVoices)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("OnAudioConfigurationChanged: numVirtualVoices old={0} new={1}", audioConfiguration.numVirtualVoices, configuration.numVirtualVoices);
			}
			flag = true;
		}
		if (configuration.sampleRate != audioConfiguration.sampleRate)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("OnAudioConfigurationChanged: sampleRate old={0} new={1}", audioConfiguration.sampleRate, configuration.sampleRate);
			}
			flag = true;
		}
		if (configuration.speakerMode != audioConfiguration.speakerMode)
		{
			if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("OnAudioConfigurationChanged: speakerMode old={0} new={1}", audioConfiguration.speakerMode, configuration.speakerMode);
			}
			flag = true;
		}
		if (flag)
		{
			audioConfiguration = configuration;
		}
		if (recorder.MicrophoneDeviceChangeDetected)
		{
			return;
		}
		if (flag)
		{
			if (recorder.IsRecording)
			{
				if (HandleConfigChange)
				{
					if (base.Logger.IsInfoEnabled)
					{
						base.Logger.LogInfo("Config change detected; an attempt to auto start recording should follow shortly.");
					}
					recorder.MicrophoneDeviceChangeDetected = true;
				}
				else if (base.Logger.IsInfoEnabled)
				{
					base.Logger.LogInfo("Config change detected but will not try to handle this as HandleConfigChange is false.");
				}
			}
			else if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Config change detected but ignored as recording not started.");
			}
		}
		else if (deviceWasChanged)
		{
			if (UseOnAudioConfigurationChanged)
			{
				OnDeviceChange();
			}
			else if (base.Logger.IsInfoEnabled)
			{
				base.Logger.LogInfo("Device change detected but will not try to handle this as UseOnAudioConfigurationChanged is false.");
			}
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
				base.Logger.LogInfo("Unsubscribed from audio changes via Unity OnAudioConfigurationChanged callback.");
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
}
