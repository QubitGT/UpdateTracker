using System;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Voice.Unity;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Photon Voice/Speaker")]
[DisallowMultipleComponent]
public class Speaker : VoiceComponent
{
	private IAudioOut<float> audioOutput;

	private RemoteVoiceLink remoteVoiceLink;

	[SerializeField]
	private bool playbackOnlyWhenEnabled;

	[SerializeField]
	[HideInInspector]
	private int playDelayMs = 200;

	[SerializeField]
	protected PlaybackDelaySettings playbackDelaySettings = new PlaybackDelaySettings
	{
		MinDelaySoft = 200,
		MaxDelaySoft = 400,
		MaxDelayHard = 1000
	};

	private bool playbackExplicitlyStopped;

	public Func<IAudioOut<float>> CustomAudioOutFactory;

	[Obsolete("Use SetPlaybackDelaySettings methods instead")]
	public int PlayDelayMs
	{
		get
		{
			return playbackDelaySettings.MinDelaySoft;
		}
		set
		{
			if (value >= 0 && value < playbackDelaySettings.MaxDelaySoft)
			{
				playbackDelaySettings.MinDelaySoft = value;
			}
		}
	}

	public bool IsPlaying
	{
		get
		{
			if (IsInitialized)
			{
				return audioOutput.IsPlaying;
			}
			return false;
		}
	}

	public int Lag
	{
		get
		{
			if (!IsPlaying)
			{
				return -1;
			}
			return audioOutput.Lag;
		}
	}

	public Action<Speaker> OnRemoteVoiceRemoveAction { get; set; }

	public Player Actor { get; protected internal set; }

	public bool IsLinked => remoteVoiceLink != null;

	internal RemoteVoiceLink RemoteVoiceLink => remoteVoiceLink;

	public bool PlaybackOnlyWhenEnabled
	{
		get
		{
			return playbackOnlyWhenEnabled;
		}
		set
		{
			if (playbackOnlyWhenEnabled == value)
			{
				return;
			}
			playbackOnlyWhenEnabled = value;
			if (!IsLinked)
			{
				return;
			}
			if (playbackOnlyWhenEnabled)
			{
				if (base.isActiveAndEnabled == PlaybackStarted)
				{
					return;
				}
				if (base.isActiveAndEnabled)
				{
					if (!playbackExplicitlyStopped)
					{
						StartPlaying();
					}
				}
				else
				{
					StopPlaying();
				}
			}
			else if (!PlaybackStarted && !playbackExplicitlyStopped)
			{
				StartPlaying();
			}
		}
	}

	public bool PlaybackStarted { get; private set; }

	public int PlaybackDelayMinSoft => playbackDelaySettings.MinDelaySoft;

	public int PlaybackDelayMaxSoft => playbackDelaySettings.MaxDelaySoft;

	public int PlaybackDelayMaxHard => playbackDelaySettings.MaxDelayHard;

	protected bool IsInitialized => audioOutput != null;

	private void OnEnable()
	{
		if (IsLinked && !PlaybackStarted && !playbackExplicitlyStopped)
		{
			StartPlaying();
		}
	}

	private void OnDisable()
	{
		if (PlaybackOnlyWhenEnabled && PlaybackStarted)
		{
			StopPlaying();
		}
	}

	protected virtual void Initialize()
	{
		if (IsInitialized)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Already initialized.");
			}
			return;
		}
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Initializing.");
		}
		Func<IAudioOut<float>> func = ((CustomAudioOutFactory == null) ? GetDefaultAudioOutFactory() : CustomAudioOutFactory);
		audioOutput = func();
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("Initialized.");
		}
	}

	internal Func<IAudioOut<float>> GetDefaultAudioOutFactory()
	{
		AudioOutDelayControl.PlayDelayConfig pdc = new AudioOutDelayControl.PlayDelayConfig
		{
			Low = playbackDelaySettings.MinDelaySoft,
			High = playbackDelaySettings.MaxDelaySoft,
			Max = playbackDelaySettings.MaxDelayHard
		};
		return () => new UnityAudioOut(GetComponent<AudioSource>(), pdc, base.Logger, string.Empty, base.Logger.IsDebugEnabled);
	}

	internal bool OnRemoteVoiceInfo(RemoteVoiceLink stream)
	{
		if (stream == null)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("RemoteVoiceLink is null, cancelled linking");
			}
			return false;
		}
		if (!IsInitialized)
		{
			Initialize();
		}
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnRemoteVoiceInfo {0}", stream);
		}
		if (IsLinked)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Speaker already linked to {0}, cancelled linking to {1}", remoteVoiceLink, stream);
			}
			return false;
		}
		if (stream.Info.Channels <= 0)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Received voice info channels is not expected (<= 0), cancelled linking to {0}", stream);
			}
			return false;
		}
		remoteVoiceLink = stream;
		remoteVoiceLink.RemoteVoiceRemoved += OnRemoteVoiceRemove;
		if (IsInitialized)
		{
			if (!PlaybackOnlyWhenEnabled || base.isActiveAndEnabled)
			{
				return StartPlayback();
			}
			return true;
		}
		return false;
	}

	internal void OnRemoteVoiceRemove()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnRemoteVoiceRemove {0}", remoteVoiceLink);
		}
		StopPlaying();
		if (OnRemoteVoiceRemoveAction != null)
		{
			OnRemoteVoiceRemoveAction(this);
		}
		CleanUp();
	}

	protected virtual void OnAudioFrame(FrameOut<float> frame)
	{
		audioOutput.Push(frame.Buf);
		if (frame.EndOfStream)
		{
			audioOutput.Flush();
		}
	}

	private bool StartPlaying()
	{
		if (!IsLinked)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot start playback because speaker is not linked");
			}
			return false;
		}
		if (PlaybackStarted)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Playback is already started");
			}
			return false;
		}
		if (!IsInitialized)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot start playback because not initialized yet");
			}
			return false;
		}
		if (!base.isActiveAndEnabled && PlaybackOnlyWhenEnabled)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot start playback because PlaybackOnlyWhenEnabled is true and Speaker is not enabled or its GameObject is not active in the hierarchy.");
			}
			return false;
		}
		VoiceInfo info = remoteVoiceLink.Info;
		if (info.Channels == 0)
		{
			if (base.Logger.IsErrorEnabled)
			{
				base.Logger.LogError("Cannot start playback because Channels == 0, stream {0}", remoteVoiceLink);
			}
			return false;
		}
		if (base.Logger.IsInfoEnabled)
		{
			base.Logger.LogInfo("Speaker about to start playback stream {0}, delay {1}", remoteVoiceLink, playbackDelaySettings);
		}
		AudioOutputStart(info.SamplingRate, info.Channels, info.FrameDurationSamples);
		remoteVoiceLink.FloatFrameDecoded += OnAudioFrame;
		PlaybackStarted = true;
		playbackExplicitlyStopped = false;
		return true;
	}

	protected virtual void AudioOutputStart(int frequency, int channels, int frameSamplesPerChannel)
	{
		audioOutput.Start(frequency, channels, frameSamplesPerChannel);
	}

	private void OnDestroy()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("OnDestroy");
		}
		StopPlaying(force: true);
		CleanUp();
	}

	private bool StopPlaying(bool force = false)
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("StopPlaying");
		}
		if (!force && !PlaybackStarted)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot stop playback because it's not started");
			}
			return false;
		}
		if (IsLinked)
		{
			remoteVoiceLink.FloatFrameDecoded -= OnAudioFrame;
		}
		else if (!force && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("Speaker not linked while stopping playback");
		}
		if (IsInitialized)
		{
			AudioOutputStop();
		}
		else if (!force && base.Logger.IsWarningEnabled)
		{
			base.Logger.LogWarning("audioOutput is null while stopping playback");
		}
		PlaybackStarted = false;
		return true;
	}

	protected virtual void AudioOutputStop()
	{
		audioOutput.Stop();
	}

	private void CleanUp()
	{
		if (base.Logger.IsDebugEnabled)
		{
			base.Logger.LogDebug("CleanUp");
		}
		if (remoteVoiceLink != null)
		{
			remoteVoiceLink.RemoteVoiceRemoved -= OnRemoteVoiceRemove;
			remoteVoiceLink = null;
		}
		Actor = null;
	}

	internal void Service()
	{
		if (PlaybackStarted)
		{
			AudioOutputService();
		}
	}

	protected virtual void AudioOutputService()
	{
		audioOutput.Service();
	}

	public bool StartPlayback()
	{
		return StartPlaying();
	}

	public bool StopPlayback()
	{
		if (playbackExplicitlyStopped)
		{
			if (base.Logger.IsWarningEnabled)
			{
				base.Logger.LogWarning("Cannot stop playback because it was already been explicitly stopped.");
			}
			return false;
		}
		playbackExplicitlyStopped = StopPlaying();
		return playbackExplicitlyStopped;
	}

	public bool RestartPlayback(bool reinit = false)
	{
		if (!StopPlayback())
		{
			return false;
		}
		if (reinit)
		{
			audioOutput = null;
			Initialize();
		}
		return StartPlayback();
	}

	public bool SetPlaybackDelaySettings(PlaybackDelaySettings pdc)
	{
		return SetPlaybackDelaySettings(pdc.MinDelaySoft, pdc.MaxDelaySoft, pdc.MaxDelayHard);
	}

	public bool SetPlaybackDelaySettings(int low, int high, int max)
	{
		if (low >= 0 && low < high)
		{
			if (playbackDelaySettings.MaxDelaySoft != high || playbackDelaySettings.MinDelaySoft != low || playbackDelaySettings.MaxDelayHard != max)
			{
				if (max < high)
				{
					max = high;
				}
				playbackDelaySettings.MaxDelaySoft = high;
				playbackDelaySettings.MinDelaySoft = low;
				playbackDelaySettings.MaxDelayHard = max;
				if (IsPlaying)
				{
					RestartPlayback(reinit: true);
				}
				else if (IsInitialized)
				{
					audioOutput = null;
					Initialize();
				}
				return true;
			}
		}
		else if (base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("Wrong playback delay config values, make sure 0 <= Low < High, low={0}, high={1}, max={2}", low, high, max);
		}
		return false;
	}
}
