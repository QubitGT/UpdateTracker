using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.Voice.Logging;
using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Interfaces;
using UnityEngine;

public class AudioClipAudioSource : MonoBehaviour, IAudioInputSource
{
	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private List<AudioClip> _audioClips;

	[Tooltip("If true, the associated clips will be played again from the beginning with multiple requests after the clip queue has been exhausted.")]
	[SerializeField]
	private bool _loopRequests;

	private bool _isRecording;

	private Queue<int> _audioQueue = new Queue<int>();

	private int clipIndex;

	private List<float[]> clipData = new List<float[]>();

	private float[] _buffer;

	private const float _samplesPerFrame = 0.01f;

	[SerializeField]
	private AudioEncoding _audioEncoding = new AudioEncoding();

	public IVLogger _log { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Audio);


	public virtual bool IsMuted { get; private set; }

	public bool IsRecording => _isRecording;

	public AudioEncoding AudioEncoding => _audioEncoding;

	public bool IsInputAvailable => true;

	public event Action OnMicMuted;

	public event Action OnMicUnmuted;

	public event Action OnStartRecording;

	public event Action OnStartRecordingFailed;

	public event Action<int, float[], float> OnSampleReady;

	public event Action OnStopRecording;

	protected virtual void SetMuted(bool muted)
	{
		if (IsMuted != muted)
		{
			IsMuted = muted;
			if (IsMuted)
			{
				this.OnMicMuted?.Invoke();
			}
			else
			{
				this.OnMicUnmuted?.Invoke();
			}
		}
	}

	private void Start()
	{
		foreach (AudioClip audioClip in _audioClips)
		{
			AddClipData(audioClip);
			VLog.D("Added " + audioClip.name + " to queue");
		}
	}

	public void StartRecording(int sampleLen)
	{
		if (_isRecording)
		{
			this.OnStartRecordingFailed?.Invoke();
			return;
		}
		_isRecording = true;
		PlayNextClip();
	}

	private void PlayNextClip()
	{
		if (clipIndex >= _audioClips.Count && _loopRequests)
		{
			clipIndex = 0;
		}
		if (clipIndex < _audioClips.Count)
		{
			VLog.D($"Starting clip {clipIndex}");
			_isRecording = true;
			VLog.D("Playing " + _audioClips[clipIndex].name);
			_audioSource.PlayOneShot(_audioClips[clipIndex]);
			this.OnStartRecording?.Invoke();
			TransmitAudio(clipData[clipIndex]).WrapErrors();
		}
		else
		{
			this.OnStartRecordingFailed?.Invoke();
		}
	}

	private async Task TransmitAudio(float[] samples)
	{
		int index = 0;
		if (_buffer == null)
		{
			int num = Mathf.CeilToInt((float)AudioEncoding.samplerate * 0.01f);
			_buffer = new float[num];
		}
		while (index < samples.Length)
		{
			int num2 = Math.Min(_buffer.Length, samples.Length - index);
			Array.Copy(samples, index, _buffer, 0, num2);
			this.OnSampleReady?.Invoke(num2, _buffer, float.MinValue);
			index += num2;
			await Task.Yield();
		}
		if (_loopRequests)
		{
			StopRecording();
			PlayNextClip();
		}
		else
		{
			StopRecording();
			clipIndex++;
		}
	}

	public void StopRecording()
	{
		_isRecording = false;
		this.OnStopRecording?.Invoke();
	}

	public void CheckForInput()
	{
	}

	public bool SetActiveClip(string clipName)
	{
		int num = _audioClips.FindIndex(0, (AudioClip clip) => (clip.name == clipName) ? true : false);
		if (num < 0 || num >= _audioClips.Count)
		{
			VLog.D("Couldn't find clip " + clipName);
			return false;
		}
		clipIndex = num;
		return true;
	}

	public void AddClip(AudioClip clip)
	{
		_audioClips.Add(clip);
		AddClipData(clip);
		VLog.D("Clip added " + clip.name);
	}

	private void AddClipData(AudioClip clip)
	{
		float[] array = new float[clip.samples];
		clip.GetData(array, 0);
		float[] item = QuickResample(array, clip.channels, clip.frequency, AudioEncoding.numChannels, AudioEncoding.samplerate);
		clipData.Add(item);
	}

	public static float[] QuickResample(float[] oldSamples, int oldChannels, int oldSampleRate, int newChannels, int newSampleRate)
	{
		if (oldSampleRate == newSampleRate && oldChannels == newChannels)
		{
			return oldSamples;
		}
		float num = (float)oldSampleRate / (float)newSampleRate;
		num *= (float)oldChannels / (float)newChannels;
		int num2 = (int)((float)oldSamples.Length / num);
		float[] array = new float[num2];
		for (int i = 0; i < num2; i++)
		{
			int num3 = (int)((float)i * num);
			array[i] = oldSamples[num3];
		}
		return array;
	}
}
