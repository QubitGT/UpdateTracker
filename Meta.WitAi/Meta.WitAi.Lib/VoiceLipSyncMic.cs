using System;
using Meta.WitAi.Data;
using Meta.WitAi.Events;
using UnityEngine;

namespace Meta.WitAi.Lib;

public class VoiceLipSyncMic : MonoBehaviour
{
	[Tooltip("Audio desired sample size for lipsync. The mic frequency will be adjusted to match this.")]
	public int AudioSampleRate = 48000;

	[Tooltip("Manual specification of Audio Source. Default will use any attached to the same object.")]
	public AudioSource AudioSource;

	private void Awake()
	{
		if (!AudioSource)
		{
			AudioSource = GetComponent<AudioSource>();
			if (!AudioSource)
			{
				AudioSource = base.gameObject.AddComponent<AudioSource>();
			}
		}
		AudioSource.loop = true;
		AudioSource.playOnAwake = false;
		if (AudioSource.isPlaying)
		{
			AudioSource.Stop();
		}
		if (AudioBuffer.Instance?.MicInput is Mic mic)
		{
			mic.SetAudioSampleRate(AudioSampleRate);
		}
		else
		{
			Debug.LogError("VoiceMicLipSync only works with Mic script.");
		}
	}

	private void OnEnable()
	{
		AudioBuffer instance = AudioBuffer.Instance;
		if (!(instance == null))
		{
			if (AudioBuffer.Instance?.MicInput is Mic mic)
			{
				AudioSource.clip = mic.Clip;
			}
			AudioBufferEvents events = instance.Events;
			events.OnSampleReady = (AudioBufferEvents.OnSampleReadyEvent)Delegate.Combine(events.OnSampleReady, new AudioBufferEvents.OnSampleReadyEvent(OnMicSampleReady));
			instance.StartRecording(this);
		}
	}

	private void OnMicSampleReady(RingBuffer<byte>.Marker marker, float levelMax)
	{
		if (!AudioSource.isPlaying && AudioSource.clip != null)
		{
			AudioSource.Play();
		}
	}

	private void OnDisable()
	{
		if (AudioSource.isPlaying)
		{
			AudioSource.Stop();
		}
		AudioSource.clip = null;
		AudioBuffer instance = AudioBuffer.Instance;
		if (!(instance == null))
		{
			instance.StopRecording(this);
			AudioBufferEvents events = instance.Events;
			events.OnSampleReady = (AudioBufferEvents.OnSampleReadyEvent)Delegate.Remove(events.OnSampleReady, new AudioBufferEvents.OnSampleReadyEvent(OnMicSampleReady));
		}
	}
}
