using System;
using UnityEngine;

namespace Oculus.Platform;

public class VoipAudioSourceHiLevel : MonoBehaviour
{
	public class FilterReadDelegate : MonoBehaviour
	{
		public VoipAudioSourceHiLevel parent;

		private float[] scratchBuffer;

		private void Awake()
		{
			int num = (int)(uint)CAPI.ovr_Voip_GetOutputBufferMaxSize();
			scratchBuffer = new float[num];
		}

		private void OnAudioFilterRead(float[] data, int channels)
		{
			int num = data.Length / channels;
			int num2 = num;
			if (num2 > scratchBuffer.Length)
			{
				Array.Clear(data, 0, data.Length);
				throw new Exception($"Audio system tried to pull {num} bytes, max voip internal ring buffer size {scratchBuffer.Length}");
			}
			int num3 = parent.pcmSource.PeekSizeElements();
			if (num3 < num2)
			{
				if (verboseLogging)
				{
					Debug.LogFormat("Voip starved! Want {0}, but only have {1} available", num2, num3);
				}
				return;
			}
			int pCM = parent.pcmSource.GetPCM(scratchBuffer, num2);
			if (pCM < num2)
			{
				Debug.LogWarningFormat("GetPCM() returned {0} samples, expected {1}", pCM, num2);
				return;
			}
			int num4 = 0;
			float num5 = -1f;
			for (int i = 0; i < num; i++)
			{
				float num6 = scratchBuffer[i];
				for (int j = 0; j < channels; j++)
				{
					data[num4++] = num6;
					if (num6 > num5)
					{
						num5 = num6;
					}
				}
			}
			parent.peakAmplitude = num5;
		}
	}

	private int initialPlaybackDelayMS;

	public AudioSource audioSource;

	public float peakAmplitude;

	protected IVoipPCMSource pcmSource;

	private static int audioSystemPlaybackFrequency;

	private static bool verboseLogging;

	public ulong senderID
	{
		set
		{
			pcmSource.SetSenderID(value);
		}
	}

	protected void Stop()
	{
	}

	private VoipSampleRate SampleRateToEnum(int rate)
	{
		return rate switch
		{
			48000 => VoipSampleRate.HZ48000, 
			44100 => VoipSampleRate.HZ44100, 
			24000 => VoipSampleRate.HZ24000, 
			_ => VoipSampleRate.Unknown, 
		};
	}

	protected void Awake()
	{
		CreatePCMSource();
		if (audioSource == null)
		{
			audioSource = base.gameObject.AddComponent<AudioSource>();
		}
		audioSource.gameObject.AddComponent<FilterReadDelegate>();
		audioSource.gameObject.GetComponent<FilterReadDelegate>().parent = this;
		initialPlaybackDelayMS = 40;
		audioSystemPlaybackFrequency = AudioSettings.outputSampleRate;
		CAPI.ovr_Voip_SetOutputSampleRate(SampleRateToEnum(audioSystemPlaybackFrequency));
		if (verboseLogging)
		{
			Debug.LogFormat("freq {0}", audioSystemPlaybackFrequency);
		}
	}

	private void Start()
	{
		audioSource.Stop();
	}

	protected virtual void CreatePCMSource()
	{
		pcmSource = new VoipPCMSourceNative();
	}

	protected static int MSToElements(int ms)
	{
		return ms * audioSystemPlaybackFrequency / 1000;
	}

	private void Update()
	{
		pcmSource.Update();
		if (!audioSource.isPlaying && pcmSource.PeekSizeElements() >= MSToElements(initialPlaybackDelayMS))
		{
			if (verboseLogging)
			{
				Debug.LogFormat("buffered {0} elements, starting playback", pcmSource.PeekSizeElements());
			}
			audioSource.Play();
		}
	}
}
