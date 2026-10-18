using Meta.WitAi;
using UnityEngine;

namespace Meta.Voice.Audio;

public abstract class BaseAudioClipStream : IAudioClipStream
{
	public int Channels { get; }

	public int SampleRate { get; }

	public float StreamReadyLength { get; }

	public bool IsReady { get; private set; }

	public bool IsComplete { get; private set; }

	public int AddedSamples { get; protected set; }

	public int ExpectedSamples { get; protected set; }

	public int TotalSamples => Mathf.Max(AddedSamples, ExpectedSamples);

	public float Length => GetSampleLength(TotalSamples);

	public AudioClipStreamSampleDelegate OnAddSamples { get; set; }

	public AudioClipStreamDelegate OnStreamReady { get; set; }

	public AudioClipStreamDelegate OnStreamUpdated { get; set; }

	public AudioClipStreamDelegate OnStreamComplete { get; set; }

	public AudioClipStreamDelegate OnStreamUnloaded { get; set; }

	protected BaseAudioClipStream(int newChannels, int newSampleRate, float newStreamReadyLength = 1.5f)
	{
		Channels = newChannels;
		SampleRate = newSampleRate;
		StreamReadyLength = newStreamReadyLength;
	}

	protected virtual void Reset()
	{
		AddedSamples = 0;
		ExpectedSamples = 0;
		IsReady = false;
		IsComplete = false;
		OnAddSamples = null;
		OnStreamReady = null;
		OnStreamUpdated = null;
		OnStreamComplete = null;
		OnStreamUnloaded = null;
	}

	public abstract void AddSamples(float[] samples, int offset, int length);

	public virtual void SetExpectedSamples(int expectedSamples)
	{
		if (expectedSamples > 0)
		{
			ExpectedSamples = expectedSamples;
			UpdateState();
		}
	}

	public virtual void UpdateState()
	{
		if (!IsReady && IsEnoughBuffered())
		{
			HandleStreamReady();
		}
		if (!IsComplete && ExpectedSamples > 0 && AddedSamples >= ExpectedSamples)
		{
			HandleStreamComplete();
		}
	}

	protected virtual bool IsEnoughBuffered()
	{
		float num = StreamReadyLength;
		if (num <= 0f)
		{
			return AddedSamples > 0;
		}
		if (ExpectedSamples > 0)
		{
			num = Mathf.Min(StreamReadyLength, GetSampleLength(ExpectedSamples));
		}
		return GetSampleLength(AddedSamples) >= num;
	}

	private void HandleStreamReady()
	{
		if (!IsReady)
		{
			IsReady = true;
			ThreadUtility.CallOnMainThread(RaiseStreamReady);
		}
	}

	protected virtual void RaiseStreamReady()
	{
		OnStreamReady?.Invoke(this);
	}

	protected virtual void HandleStreamUpdated()
	{
		if (IsReady)
		{
			ThreadUtility.CallOnMainThread(RaiseStreamUpdated);
		}
	}

	protected virtual void RaiseStreamUpdated()
	{
		OnStreamUpdated?.Invoke(this);
	}

	private void HandleStreamComplete()
	{
		if (!IsComplete)
		{
			IsComplete = true;
			ThreadUtility.CallOnMainThread(RaiseStreamComplete);
		}
	}

	protected virtual void RaiseStreamComplete()
	{
		OnStreamComplete?.Invoke(this);
	}

	public virtual void Unload()
	{
		AudioClipStreamDelegate onStreamUnloaded = OnStreamUnloaded;
		Reset();
		onStreamUnloaded?.Invoke(this);
	}

	private float GetSampleLength(int totalSamples)
	{
		return GetLength(totalSamples, Channels, SampleRate);
	}

	public static float GetLength(int totalSamples, int channels, int samplesPerSecond)
	{
		return (float)totalSamples / (float)(channels * samplesPerSecond);
	}
}
