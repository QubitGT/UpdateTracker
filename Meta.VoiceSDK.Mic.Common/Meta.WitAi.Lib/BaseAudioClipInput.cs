using System;
using System.Collections;
using Meta.Voice;
using Meta.WitAi.Data;
using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Lib;

public abstract class BaseAudioClipInput : MonoBehaviour, IAudioInputSource, IAudioLevelRangeProvider
{
	private AudioEncoding _audioEncoding;

	private Coroutine _activateCoroutine;

	private Coroutine _recordCoroutine;

	public abstract AudioClip Clip { get; }

	public abstract int ClipPosition { get; }

	public abstract bool CanActivateAudio { get; }

	public virtual bool ActivateOnEnable => false;

	public virtual int AudioChannels => 1;

	public virtual int AudioSampleRate => 16000;

	public virtual int AudioSampleLength { get; private set; }

	public virtual float MinAudioLevel => 0.5f;

	public virtual float MaxAudioLevel => 1f;

	public AudioEncoding AudioEncoding
	{
		get
		{
			if (_audioEncoding == null)
			{
				_audioEncoding = new AudioEncoding();
			}
			_audioEncoding.numChannels = AudioChannels;
			_audioEncoding.samplerate = AudioSampleRate;
			_audioEncoding.encoding = "signed-integer";
			return _audioEncoding;
		}
	}

	public VoiceAudioInputState ActivationState { get; private set; }

	public virtual bool IsRecording { get; private set; }

	public virtual bool IsMuted { get; private set; }

	public event Action<VoiceAudioInputState> OnActivationStateChange;

	public event Action OnStartRecording;

	public event Action OnStartRecordingFailed;

	public event Action OnStopRecording;

	public event Action<int, float[], float> OnSampleReady;

	public event Action OnMicMuted;

	public event Action OnMicUnmuted;

	protected void SetActivationState(VoiceAudioInputState newActivationState)
	{
		ActivationState = newActivationState;
		this.OnActivationStateChange?.Invoke(ActivationState);
	}

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

	protected virtual void OnEnable()
	{
		if (ActivateOnEnable && ActivationState != VoiceAudioInputState.Activating && ActivationState != VoiceAudioInputState.On)
		{
			ActivateAudio();
		}
	}

	private void ActivateAudio()
	{
		if (ActivationState == VoiceAudioInputState.On || ActivationState == VoiceAudioInputState.Activating)
		{
			VLog.W(GetType().Name, $"Cannot activate when audio is already {ActivationState}");
			return;
		}
		if (!base.gameObject.activeInHierarchy)
		{
			VLog.W(GetType().Name, "Audio activation is disabled while GameObject is inactive");
			return;
		}
		if (!CanActivateAudio)
		{
			VLog.W(GetType().Name, "Audio activation is currently restricted");
			return;
		}
		if (_activateCoroutine != null)
		{
			StopCoroutine(_activateCoroutine);
			_activateCoroutine = null;
		}
		_activateCoroutine = StartCoroutine(PerformActivation());
	}

	private IEnumerator PerformActivation()
	{
		SetActivationState(VoiceAudioInputState.Activating);
		yield return HandleActivation();
		if (ActivationState == VoiceAudioInputState.Activating)
		{
			SetActivationState(VoiceAudioInputState.On);
		}
		_activateCoroutine = null;
	}

	protected abstract IEnumerator HandleActivation();

	protected virtual void OnDisable()
	{
		if (IsRecording)
		{
			StopRecording();
		}
		if (ActivateOnEnable && ActivationState != VoiceAudioInputState.Deactivating && ActivationState != 0)
		{
			DeactivateAudio();
		}
	}

	private void DeactivateAudio()
	{
		if (ActivationState == VoiceAudioInputState.Off || ActivationState == VoiceAudioInputState.Deactivating)
		{
			VLog.W(GetType().Name, $"Cannot deactivate when audio is already {ActivationState}");
			return;
		}
		if (_activateCoroutine != null)
		{
			StopCoroutine(_activateCoroutine);
			_activateCoroutine = null;
		}
		SetActivationState(VoiceAudioInputState.Deactivating);
		HandleDeactivation();
		if (ActivationState == VoiceAudioInputState.Deactivating)
		{
			SetActivationState(VoiceAudioInputState.Off);
		}
	}

	protected abstract void HandleDeactivation();

	public virtual void StartRecording(int sampleDurationMS)
	{
		if (IsRecording)
		{
			VLog.W(GetType().Name, "Cannot start recording when already recording");
			this.OnStartRecordingFailed?.Invoke();
			return;
		}
		IsRecording = true;
		AudioSampleLength = sampleDurationMS;
		if (_recordCoroutine != null)
		{
			StopCoroutine(_recordCoroutine);
			_recordCoroutine = null;
		}
		_recordCoroutine = StartCoroutine(ReadRawAudio());
	}

	private IEnumerator ReadRawAudio()
	{
		if (ActivationState != VoiceAudioInputState.On)
		{
			if (ActivationState != VoiceAudioInputState.Activating)
			{
				ActivateAudio();
			}
			while (ActivationState == VoiceAudioInputState.Activating)
			{
				yield return null;
			}
			if (ActivationState != VoiceAudioInputState.On)
			{
				IsRecording = false;
				this.OnStartRecordingFailed?.Invoke();
				yield break;
			}
		}
		AudioClip micClip = Clip;
		if (micClip == null)
		{
			VLog.W(GetType().Name, "No AudioClip found following activation");
			IsRecording = false;
			this.OnStartRecordingFailed?.Invoke();
			yield break;
		}
		this.OnStartRecording?.Invoke();
		float num = (float)AudioSampleLength / 1000f;
		int audioChannels = AudioChannels;
		int audioSampleRate = AudioSampleRate;
		int num2 = Mathf.CeilToInt((float)(audioChannels * audioSampleRate) * num);
		float[] samples = new float[num2];
		int prevMicPosition = ClipPosition;
		int readAbsPosition = prevMicPosition;
		int loops = 0;
		while (micClip != null && IsRecording)
		{
			yield return null;
			bool flag = true;
			while (micClip != null && flag)
			{
				int clipPosition = ClipPosition;
				if (clipPosition < prevMicPosition)
				{
					loops++;
				}
				prevMicPosition = clipPosition;
				int num3 = loops * micClip.samples + clipPosition;
				int num4 = readAbsPosition + samples.Length;
				flag = num4 < num3;
				if (flag && micClip.GetData(samples, readAbsPosition % micClip.samples))
				{
					this.OnSampleReady?.Invoke(0, samples, 0f);
					readAbsPosition = num4;
				}
			}
		}
		if (IsRecording)
		{
			StopRecording();
		}
	}

	public virtual void StopRecording()
	{
		if (!IsRecording)
		{
			VLog.E(GetType().Name, "Cannot stop recording when not recording");
			return;
		}
		if (!ActivateOnEnable || !base.gameObject.activeInHierarchy)
		{
			DeactivateAudio();
		}
		IsRecording = false;
		this.OnStopRecording?.Invoke();
	}
}
