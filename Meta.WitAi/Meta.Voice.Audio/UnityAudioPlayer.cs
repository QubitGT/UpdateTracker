using System;
using Meta.WitAi;
using UnityEngine;

namespace Meta.Voice.Audio;

[Serializable]
public class UnityAudioPlayer : BaseAudioPlayer, IAudioSourceProvider
{
	[Header("Playback Settings")]
	[Tooltip("Audio source to be used for text-to-speech playback")]
	[SerializeField]
	private AudioSource _audioSource;

	[Tooltip("Duplicates audio source reference on awake instead of using it directly.")]
	[SerializeField]
	private bool _cloneAudioSource;

	private bool _local;

	private int _offset;

	public AudioSource AudioSource => _audioSource;

	public bool CloneAudioSource => _cloneAudioSource;

	public override bool IsPlaying
	{
		get
		{
			if (AudioSource != null)
			{
				return AudioSource.isPlaying;
			}
			return false;
		}
	}

	public override bool CanSetElapsedSamples => true;

	public override int ElapsedSamples
	{
		get
		{
			if (!(AudioSource != null))
			{
				return 0;
			}
			return AudioSource.timeSamples;
		}
	}

	private void Awake()
	{
		if (!AudioSource)
		{
			_audioSource = base.gameObject.GetComponentInChildren<AudioSource>();
		}
	}

	public override void Init()
	{
		if (!AudioSource)
		{
			_audioSource = base.gameObject.GetComponentInChildren<AudioSource>();
		}
		if (CloneAudioSource)
		{
			AudioSource audioSource = new GameObject(base.gameObject.name + "_AudioOneShot").AddComponent<AudioSource>();
			audioSource.PreloadCopyData();
			if (AudioSource == null)
			{
				audioSource.transform.SetParent(base.transform, worldPositionStays: false);
				audioSource.spread = 1f;
			}
			else
			{
				audioSource.transform.SetParent(AudioSource.transform, worldPositionStays: false);
				audioSource.Copy(AudioSource);
			}
			audioSource.transform.localPosition = Vector3.zero;
			audioSource.transform.localRotation = Quaternion.identity;
			audioSource.transform.localScale = Vector3.one;
			_audioSource = audioSource;
		}
		AudioSource.playOnAwake = false;
	}

	public override string GetPlaybackErrors()
	{
		if (AudioSource == null)
		{
			return "Audio source is missing";
		}
		return string.Empty;
	}

	protected override void Play(int offsetSamples = 0)
	{
		AudioClip audioClip = null;
		if (base.ClipStream is IAudioClipProvider audioClipProvider)
		{
			audioClip = audioClipProvider.Clip;
		}
		else if (base.ClipStream is RawAudioClipStream rawAudioClipStream)
		{
			audioClip = AudioClip.Create("CustomClip", rawAudioClipStream.SampleBuffer.Length, rawAudioClipStream.Channels, rawAudioClipStream.SampleRate, stream: true, OnReadRawSamples, OnSetRawPosition);
			_local = true;
		}
		if (audioClip == null)
		{
			VLog.E($"{GetType()} cannot play null AudioClip");
			return;
		}
		AudioSource.loop = false;
		AudioSource.clip = audioClip;
		AudioSource.timeSamples = offsetSamples;
		AudioSource.Play();
	}

	private void OnSetRawPosition(int offset)
	{
		_offset = offset;
	}

	private void OnReadRawSamples(float[] samples)
	{
		int num = 0;
		if (base.ClipStream is RawAudioClipStream rawAudioClipStream)
		{
			int offset = _offset;
			int b = Mathf.Max(0, rawAudioClipStream.AddedSamples - offset);
			num = Mathf.Min(samples.Length, b);
			if (num > 0)
			{
				Array.Copy(rawAudioClipStream.SampleBuffer, offset, samples, 0, num);
				_offset += num;
			}
		}
		if (num < samples.Length)
		{
			int num2 = samples.Length - num;
			Array.Clear(samples, num, num2);
			_offset += num2;
		}
	}

	public override void Pause()
	{
		if (IsPlaying)
		{
			AudioSource.Pause();
		}
	}

	public override void Resume()
	{
		if (!IsPlaying)
		{
			AudioSource.UnPause();
		}
	}

	public override void Stop()
	{
		if (IsPlaying)
		{
			AudioSource.Stop();
		}
		if (_local)
		{
			if (AudioSource.clip != null)
			{
				UnityEngine.Object.Destroy(AudioSource.clip);
			}
			_local = false;
		}
		AudioSource.clip = null;
		base.Stop();
	}
}
