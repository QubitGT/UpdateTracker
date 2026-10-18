using System;
using UnityEngine;

namespace Meta.Voice.Audio;

public class RawAudioClipStream : BaseAudioClipStream
{
	public float[] SampleBuffer { get; }

	public RawAudioClipStream(float newReadyLength = 1.5f, float newMaxLength = 15f)
		: this(1, 24000, newReadyLength, newMaxLength)
	{
	}

	public RawAudioClipStream(int newChannels, int newSampleRate, float newReadyLength = 1.5f, float newMaxLength = 15f)
		: base(newChannels, newSampleRate, newReadyLength)
	{
		SampleBuffer = new float[Mathf.CeilToInt((float)(newChannels * newSampleRate) * newMaxLength)];
	}

	public override void AddSamples(float[] buffer, int bufferOffset, int bufferLength)
	{
		int addedSamples = base.AddedSamples;
		int num = Mathf.Min(bufferLength, SampleBuffer.Length - addedSamples);
		if (num > 0)
		{
			Array.Copy(buffer, bufferOffset, SampleBuffer, addedSamples, num);
			base.AddedSamples += num;
			base.OnAddSamples?.Invoke(SampleBuffer, addedSamples, num);
			UpdateState();
		}
	}
}
