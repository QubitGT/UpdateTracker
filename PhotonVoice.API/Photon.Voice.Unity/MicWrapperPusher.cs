using System;
using System.Linq;
using UnityEngine;

namespace Photon.Voice.Unity;

public class MicWrapperPusher : IAudioPusher<float>, IAudioDesc, IDisposable
{
	private AudioSource audioSource;

	private AudioClip mic;

	private string device;

	private ILogger logger;

	private AudioOutCapture audioOutCapture;

	private int sampleRate;

	private int channels;

	private bool destroyGameObjectOnStop;

	private float[] frame2 = new float[0];

	private Action<float[]> pushCallback;

	public int SamplingRate
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return sampleRate;
		}
	}

	public int Channels
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return channels;
		}
	}

	public string Error { get; private set; }

	public MicWrapperPusher(string device, AudioSource aS, int suggestedFrequency, ILogger lg, bool destroyOnStop = true)
	{
		try
		{
			logger = lg;
			this.device = device;
			audioSource = aS;
			destroyGameObjectOnStop = destroyOnStop;
			if (UnityMicrophone.devices.Length < 1)
			{
				Error = "No microphones found (Microphone.devices is empty)";
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			if (!string.IsNullOrEmpty(device) && !UnityMicrophone.devices.Contains(device))
			{
				logger.LogError("[PV] MicWrapperPusher: \"{0}\" is not a valid Unity microphone device, falling back to default one", device);
				device = UnityMicrophone.devices[0];
			}
			sampleRate = AudioSettings.outputSampleRate;
			switch (AudioSettings.speakerMode)
			{
			case AudioSpeakerMode.Mono:
				channels = 1;
				break;
			case AudioSpeakerMode.Stereo:
				channels = 2;
				break;
			default:
				Error = "Only Mono and Stereo project speaker mode supported. Current mode is " + AudioSettings.speakerMode;
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			logger.LogInfo("[PV] MicWrapperPusher: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency);
			UnityMicrophone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
			int frequency = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				logger.LogWarning("[PV] MicWrapperPusher does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
				frequency = maxFreq;
			}
			if (!audioSource.enabled)
			{
				logger.LogWarning("[PV] MicWrapperPusher: AudioSource component disabled, enabling it.");
				audioSource.enabled = true;
			}
			if (!audioSource.gameObject.activeSelf)
			{
				logger.LogWarning("[PV] MicWrapperPusher: AudioSource GameObject inactive, activating it.");
				audioSource.gameObject.SetActive(value: true);
			}
			if (!audioSource.gameObject.activeInHierarchy)
			{
				Error = "AudioSource GameObject is not active in hierarchy, audio input can't work.";
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			audioOutCapture = audioSource.gameObject.GetComponent<AudioOutCapture>();
			if ((object)audioOutCapture == null || !audioOutCapture)
			{
				audioOutCapture = audioSource.gameObject.AddComponent<AudioOutCapture>();
			}
			if (!audioOutCapture.enabled)
			{
				logger.LogWarning("[PV] MicWrapperPusher: AudioOutCapture component disabled, enabling it.");
				audioOutCapture.enabled = true;
			}
			mic = UnityMicrophone.Start(device, loop: true, 1, frequency);
			audioSource.mute = true;
			audioSource.volume = 0f;
			audioSource.clip = mic;
			audioSource.loop = true;
			audioSource.Play();
			logger.LogInfo("[PV] MicWrapperPusher: microphone '{0}' initialized, frequency = in:{1}|out:{2}, channels = in:{3}|out:{4}.", device, mic.frequency, SamplingRate, mic.channels, Channels);
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in MicWrapperPusher constructor";
			}
			logger.LogError("[PV] MicWrapperPusher: {0}", Error);
		}
	}

	public MicWrapperPusher(string device, GameObject gO, int suggestedFrequency, ILogger lg, bool destroyOnStop = true)
	{
		try
		{
			logger = lg;
			this.device = device;
			destroyGameObjectOnStop = destroyOnStop;
			if (UnityMicrophone.devices.Length < 1)
			{
				Error = "No microphones found (Microphone.devices is empty)";
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			if (!string.IsNullOrEmpty(device) && !UnityMicrophone.devices.Contains(device))
			{
				logger.LogError("[PV] MicWrapperPusher: \"{0}\" is not a valid Unity microphone device, falling back to default one", device);
				device = UnityMicrophone.devices[0];
			}
			sampleRate = AudioSettings.outputSampleRate;
			switch (AudioSettings.speakerMode)
			{
			case AudioSpeakerMode.Mono:
				channels = 1;
				break;
			case AudioSpeakerMode.Stereo:
				channels = 2;
				break;
			default:
				Error = "Only Mono and Stereo project speaker mode supported. Current mode is " + AudioSettings.speakerMode;
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			logger.LogInfo("[PV] MicWrapperPusher: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency);
			UnityMicrophone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
			int frequency = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				logger.LogWarning("[PV] MicWrapperPusher does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
				frequency = maxFreq;
			}
			if (!gO || gO == null)
			{
				logger.LogWarning("[PV] MicWrapperPusher: AudioSource GameObject is destroyed or null. Creating a new one.");
				gO = new GameObject("[PV] MicWrapperPusher: AudioSource + AudioOutCapture");
				audioSource = gO.AddComponent<AudioSource>();
				audioOutCapture = audioSource.gameObject.AddComponent<AudioOutCapture>();
			}
			else
			{
				if (!gO.activeSelf)
				{
					logger.LogWarning("[PV] MicWrapperPusher: AudioSource GameObject inactive, activating it.");
					gO.SetActive(value: true);
				}
				if (!gO.activeInHierarchy)
				{
					Error = "AudioSource GameObject is not active in hierarchy, audio input can't work.";
					logger.LogError("[PV] MicWrapperPusher: {0}", Error);
					return;
				}
				audioSource = gO.GetComponent<AudioSource>();
				if ((object)audioSource == null || !audioSource)
				{
					audioSource = gO.AddComponent<AudioSource>();
				}
				if (!audioSource.enabled)
				{
					logger.LogWarning("[PV] MicWrapperPusher: AudioSource component disabled, enabling it.");
					audioSource.enabled = true;
				}
				if (!audioSource.gameObject.activeSelf)
				{
					logger.LogWarning("[PV] MicWrapperPusher: AudioSource GameObject inactive, activating it.");
					audioSource.gameObject.SetActive(value: true);
				}
				if (!audioSource.gameObject.activeInHierarchy)
				{
					Error = "AudioSource GameObject is not active in hierarchy, audio input can't work.";
					logger.LogError("[PV] MicWrapperPusher: {0}", Error);
					return;
				}
				audioOutCapture = audioSource.gameObject.GetComponent<AudioOutCapture>();
				if ((object)audioOutCapture == null || !audioOutCapture)
				{
					audioOutCapture = audioSource.gameObject.AddComponent<AudioOutCapture>();
				}
				if (!audioOutCapture.enabled)
				{
					logger.LogWarning("[PV] MicWrapperPusher: AudioOutCapture component disabled, enabling it.");
					audioOutCapture.enabled = true;
				}
			}
			mic = UnityMicrophone.Start(device, loop: true, 1, frequency);
			audioSource.mute = true;
			audioSource.volume = 0f;
			audioSource.clip = mic;
			audioSource.loop = true;
			audioSource.Play();
			logger.LogInfo("[PV] MicWrapperPusher: microphone '{0}' initialized, frequency = in:{1}|out:{2}, channels = in:{3}|out:{4}.", device, mic.frequency, SamplingRate, mic.channels, Channels);
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in MicWrapperPusher constructor";
			}
			logger.LogError("[PV] MicWrapperPusher: {0}", Error);
		}
	}

	public MicWrapperPusher(string device, Transform parentTransform, int suggestedFrequency, ILogger lg, bool destroyOnStop = true)
	{
		try
		{
			logger = lg;
			this.device = device;
			destroyGameObjectOnStop = destroyOnStop;
			if (UnityMicrophone.devices.Length < 1)
			{
				Error = "No microphones found (Microphone.devices is empty)";
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			if (!string.IsNullOrEmpty(device) && !UnityMicrophone.devices.Contains(device))
			{
				logger.LogError("[PV] MicWrapperPusher: \"{0}\" is not a valid Unity microphone device, falling back to default one", device);
				device = UnityMicrophone.devices[0];
			}
			sampleRate = AudioSettings.outputSampleRate;
			switch (AudioSettings.speakerMode)
			{
			case AudioSpeakerMode.Mono:
				channels = 1;
				break;
			case AudioSpeakerMode.Stereo:
				channels = 2;
				break;
			default:
				Error = "Only Mono and Stereo project speaker mode supported. Current mode is " + AudioSettings.speakerMode;
				logger.LogError("[PV] MicWrapperPusher: {0}", Error);
				return;
			}
			logger.LogInfo("[PV] MicWrapperPusher: initializing microphone '{0}', suggested frequency = {1}).", device, suggestedFrequency);
			UnityMicrophone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
			int frequency = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				logger.LogWarning("[PV] MicWrapperPusher does not support suggested frequency {0} (min: {1}, max: {2}). Setting to {2}", suggestedFrequency, minFreq, maxFreq);
				frequency = maxFreq;
			}
			GameObject gameObject = new GameObject("[PV] MicWrapperPusher: AudioSource + AudioOutCapture");
			if ((object)parentTransform == null || !parentTransform)
			{
				logger.LogWarning("[PV] MicWrapperPusher: Parent transform passed is destroyed or null. Creating AudioSource GameObject at root.");
			}
			else
			{
				gameObject.transform.SetParent(parentTransform, worldPositionStays: false);
				if (!gameObject.activeSelf)
				{
					logger.LogWarning("[PV] MicWrapperPusher: AudioSource GameObject inactive, activating it.");
					gameObject.gameObject.SetActive(value: true);
				}
				if (!gameObject.activeInHierarchy)
				{
					Error = "AudioSource GameObject is not active in hierarchy, audio input can't work.";
					logger.LogError("[PV] MicWrapperPusher: {0}", Error);
					return;
				}
			}
			audioSource = gameObject.AddComponent<AudioSource>();
			audioOutCapture = audioSource.gameObject.AddComponent<AudioOutCapture>();
			mic = UnityMicrophone.Start(device, loop: true, 1, frequency);
			audioSource.mute = true;
			audioSource.volume = 0f;
			audioSource.clip = mic;
			audioSource.loop = true;
			audioSource.Play();
			logger.LogInfo("[PV] MicWrapperPusher: microphone '{0}' initialized, frequency = in:{1}|out:{2}, channels = in:{3}|out:{4}.", device, mic.frequency, SamplingRate, mic.channels, Channels);
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in MicWrapperPusher constructor";
			}
			logger.LogError("[PV] MicWrapperPusher: {0}", Error);
		}
	}

	private void AudioOutCaptureOnOnAudioFrame(float[] frame, int channelsNumber)
	{
		if (channelsNumber != Channels)
		{
			logger.LogWarning("[PV] MicWrapperPusher: channels number mismatch; expected:{0} got:{1}.", Channels, channelsNumber);
		}
		if (frame2.Length != frame.Length)
		{
			frame2 = new float[frame.Length];
		}
		Array.Copy(frame, frame2, frame.Length);
		pushCallback(frame);
		Array.Clear(frame, 0, frame.Length);
	}

	public void SetCallback(Action<float[]> callback, ObjectFactory<float[], int> bufferFactory)
	{
		pushCallback = callback;
		audioOutCapture.OnAudioFrame += AudioOutCaptureOnOnAudioFrame;
	}

	public void Dispose()
	{
		if (pushCallback != null && audioOutCapture != null)
		{
			audioOutCapture.OnAudioFrame -= AudioOutCaptureOnOnAudioFrame;
		}
		UnityMicrophone.End(device);
		if (destroyGameObjectOnStop && audioSource != null)
		{
			UnityEngine.Object.Destroy(audioSource.gameObject);
		}
	}
}
