using System;
using System.Collections;
using Meta.WitAi.Data;
using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Lib;

public abstract class MicBase : MonoBehaviour, IAudioInputSource
{
	private int _sampleCount;

	private Coroutine _reader;

	public abstract int MicPosition { get; }

	public bool IsRecording { get; private set; }

	public virtual bool IsMicListening => Microphone.IsRecording(GetMicName());

	public bool IsInputAvailable => GetMicClip() != null;

	public AudioEncoding AudioEncoding { get; set; } = new AudioEncoding();


	public virtual bool IsMuted { get; private set; }

	public event Action OnStartRecording;

	public event Action OnStartRecordingFailed;

	public event Action OnStopRecording;

	public event Action<int, float[], float> OnSampleReady;

	public event Action OnMicMuted;

	public event Action OnMicUnmuted;

	public abstract string GetMicName();

	public abstract int GetMicSampleRate();

	public abstract AudioClip GetMicClip();

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

	public virtual void CheckForInput()
	{
	}

	public virtual void StartRecording(int sampleDurationMS)
	{
		if (IsRecording)
		{
			StopRecording();
		}
		if (!IsInputAvailable)
		{
			this.OnStartRecordingFailed();
			return;
		}
		IsRecording = true;
		_reader = StartCoroutine(ReadRawAudio(sampleDurationMS));
	}

	protected virtual IEnumerator ReadRawAudio(int sampleDurationMS)
	{
		this.OnStartRecording?.Invoke();
		AudioClip micClip = GetMicClip();
		GetMicName();
		int micSampleRate = GetMicSampleRate();
		int num = AudioEncoding.samplerate / 1000 * sampleDurationMS * micClip.channels;
		float[] sample = new float[num];
		int loops = 0;
		int readAbsPos = MicPosition;
		int prevPos = readAbsPos;
		int micTempTotal = micSampleRate / 1000 * sampleDurationMS * micClip.channels;
		int micDif = micTempTotal / num;
		float[] temp = new float[micTempTotal];
		while (micClip != null && IsMicListening && IsRecording)
		{
			bool flag = true;
			while (flag && micClip != null)
			{
				int micPosition = MicPosition;
				if (micPosition < prevPos)
				{
					loops++;
				}
				prevPos = micPosition;
				int num2 = loops * micClip.samples + micPosition;
				int num3 = readAbsPos + micTempTotal;
				if (num3 < num2)
				{
					micClip.GetData(temp, readAbsPos % micClip.samples);
					float num4 = 0f;
					int num5 = 0;
					for (int i = 0; i < temp.Length; i++)
					{
						float num6 = temp[i] * temp[i];
						if (num4 < num6)
						{
							num4 = num6;
						}
						if (i % micDif == 0 && num5 < sample.Length)
						{
							sample[num5] = temp[i];
							num5++;
						}
					}
					_sampleCount++;
					this.OnSampleReady?.Invoke(_sampleCount, sample, num4);
					readAbsPos = num3;
				}
				else
				{
					flag = false;
				}
			}
			yield return null;
		}
		if (IsRecording)
		{
			StopRecording();
		}
	}

	public virtual void StopRecording()
	{
		if (IsRecording)
		{
			IsRecording = false;
			if (_reader != null)
			{
				StopCoroutine(_reader);
				_reader = null;
			}
			this.OnStopRecording?.Invoke();
		}
	}
}
