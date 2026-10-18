using System;
using System.Collections;
using System.Collections.Generic;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice;
using Meta.Voice.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Lib;

[LogCategory(LogCategory.Audio, LogCategory.Input)]
public class Mic : BaseAudioClipInput, ILogSource
{
	private AudioClip _audioClip;

	[SerializeField]
	private bool _activateOnEnable = true;

	[SerializeField]
	[Tooltip("Searches for mics for this long following an activation request.")]
	public float MicStartTimeout = 5f;

	private const float MIC_CHECK = 0.5f;

	[SerializeField]
	[Tooltip("Total amount of seconds included within the mic audio clip buffer")]
	public int MicBufferLength = 2;

	[SerializeField]
	[Tooltip("Sample rate for mic audio capture in samples per second.")]
	[FormerlySerializedAs("_audioClipSampleRate")]
	private int _micSampleRate = 16000;

	private List<string> _devices = new List<string>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Input);


	public override AudioClip Clip => _audioClip;

	public override int ClipPosition => MicrophoneGetPosition(CurrentDeviceName);

	public override bool CanActivateAudio => true;

	public override bool ActivateOnEnable => _activateOnEnable;

	public override int AudioSampleRate => _micSampleRate;

	public List<string> Devices
	{
		get
		{
			if (_devices == null || _devices.Count == 0)
			{
				RefreshMicDevices();
			}
			return _devices;
		}
	}

	public int CurrentDeviceIndex { get; private set; } = -1;


	public string CurrentDeviceName
	{
		get
		{
			if (_devices == null || CurrentDeviceIndex < 0 || CurrentDeviceIndex >= _devices.Count)
			{
				return string.Empty;
			}
			return _devices[CurrentDeviceIndex];
		}
	}

	public void SetAudioSampleRate(int newSampleRate)
	{
		if (base.ActivationState == VoiceAudioInputState.On)
		{
			VLog.E(GetType().Name, $"Cannot set audio sample rate while Mic is {base.ActivationState}");
		}
		else
		{
			_micSampleRate = newSampleRate;
		}
	}

	protected override IEnumerator HandleActivation()
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime start = utcNow;
		DateTime lastRefresh = DateTime.MinValue;
		while (string.IsNullOrEmpty(CurrentDeviceName) && (utcNow - start).TotalSeconds < (double)MicStartTimeout)
		{
			if ((utcNow - lastRefresh).TotalSeconds > 0.5)
			{
				lastRefresh = utcNow;
				RefreshMicDevices();
				if (_devices.Count > 0 && CurrentDeviceIndex < 0)
				{
					CurrentDeviceIndex = 0;
				}
			}
			if (string.IsNullOrEmpty(CurrentDeviceName))
			{
				yield return null;
				utcNow = DateTime.UtcNow;
			}
		}
		if (string.IsNullOrEmpty(CurrentDeviceName))
		{
			VLog.W(GetType().Name, $"No mics found after {MicStartTimeout} seconds");
			SetActivationState(VoiceAudioInputState.Off);
			yield break;
		}
		StartMicrophone();
		if (_audioClip == null)
		{
			SetActivationState(VoiceAudioInputState.Off);
		}
	}

	private void StartMicrophone()
	{
		string currentDeviceName = CurrentDeviceName;
		if (!string.IsNullOrEmpty(currentDeviceName))
		{
			MicBufferLength = Mathf.Max(1, MicBufferLength);
			Logger.Info("Start Microphone '{0}'", currentDeviceName, null, null, null, "StartMicrophone", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Lib\\Mic\\Other\\Mic.cs", 184);
			_audioClip = MicrophoneStart(currentDeviceName, loop: true, MicBufferLength, AudioSampleRate);
			if (_audioClip == null)
			{
				VLog.W(GetType().Name, "Microphone.Start() did not return an AudioClip\nMic Name: " + currentDeviceName);
			}
		}
	}

	protected override void HandleDeactivation()
	{
		StopMicrophone();
	}

	private void StopMicrophone()
	{
		string currentDeviceName = CurrentDeviceName;
		if (!string.IsNullOrEmpty(currentDeviceName))
		{
			if (MicrophoneIsRecording(currentDeviceName))
			{
				Logger.Info("Stop Microphone '{0}'", currentDeviceName, null, null, null, "StopMicrophone", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Lib\\Mic\\Other\\Mic.cs", 217);
				MicrophoneEnd(currentDeviceName);
			}
			if (_audioClip != null)
			{
				UnityEngine.Object.DestroyImmediate(_audioClip);
				_audioClip = null;
			}
		}
	}

	private void RefreshMicDevices()
	{
		string currentDeviceName = CurrentDeviceName;
		_devices.Clear();
		string[] array = MicrophoneGetDevices();
		if (array != null)
		{
			_devices.AddRange(array);
		}
		CurrentDeviceIndex = _devices.IndexOf(currentDeviceName);
	}

	public void ChangeMicDevice(int index)
	{
		StopMicrophone();
		CurrentDeviceIndex = index;
		StartMicrophone();
	}

	private AudioClip MicrophoneStart(string deviceName, bool loop, int lengthSeconds, int frequency)
	{
		return Microphone.Start(deviceName, loop, lengthSeconds, frequency);
	}

	private void MicrophoneEnd(string deviceName)
	{
		Microphone.End(deviceName);
	}

	private bool MicrophoneIsRecording(string device)
	{
		if (!string.IsNullOrEmpty(device))
		{
			return Microphone.IsRecording(device);
		}
		return false;
	}

	private string[] MicrophoneGetDevices()
	{
		return Microphone.devices;
	}

	private int MicrophoneGetPosition(string device)
	{
		return Microphone.GetPosition(device);
	}
}
