using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

internal class FrameTimeSampleHistory
{
	private List<FrameTimeSample> m_Samples = new List<FrameTimeSample>();

	internal FrameTimeSample SampleAverage;

	internal FrameTimeSample SampleMin;

	internal FrameTimeSample SampleMax;

	private static Func<float, float, float> s_SampleValueAdd = (float value, float other) => value + other;

	private static Func<float, float, float> s_SampleValueMin = (float value, float other) => (!(other > 0f)) ? value : Mathf.Min(value, other);

	private static Func<float, float, float> s_SampleValueMax = (float value, float other) => Mathf.Max(value, other);

	private static Func<float, float, float> s_SampleValueCountValid = (float value, float other) => (!(other > 0f)) ? value : (value + 1f);

	private static Func<float, float, float> s_SampleValueEnsureValid = (float value, float other) => (!(other > 0f)) ? 0f : value;

	private static Func<float, float, float> s_SampleValueDivide = (float value, float other) => (!(other > 0f)) ? 0f : (value / other);

	public FrameTimeSampleHistory(int initialCapacity)
	{
		m_Samples.Capacity = initialCapacity;
	}

	internal void Add(FrameTimeSample sample)
	{
		m_Samples.Add(sample);
	}

	internal void ComputeAggregateValues()
	{
		FrameTimeSample aggregate2 = default(FrameTimeSample);
		FrameTimeSample aggregate3 = new FrameTimeSample(float.MaxValue);
		FrameTimeSample aggregate4 = new FrameTimeSample(float.MinValue);
		FrameTimeSample aggregate5 = default(FrameTimeSample);
		for (int i = 0; i < m_Samples.Count; i++)
		{
			FrameTimeSample sample2 = m_Samples[i];
			ForEachSampleMember(ref aggregate3, sample2, s_SampleValueMin);
			ForEachSampleMember(ref aggregate4, sample2, s_SampleValueMax);
			ForEachSampleMember(ref aggregate2, sample2, s_SampleValueAdd);
			ForEachSampleMember(ref aggregate5, sample2, s_SampleValueCountValid);
		}
		ForEachSampleMember(ref aggregate3, aggregate5, s_SampleValueEnsureValid);
		ForEachSampleMember(ref aggregate4, aggregate5, s_SampleValueEnsureValid);
		ForEachSampleMember(ref aggregate2, aggregate5, s_SampleValueDivide);
		SampleAverage = aggregate2;
		SampleMin = aggregate3;
		SampleMax = aggregate4;
		static void ForEachSampleMember(ref FrameTimeSample aggregate, FrameTimeSample sample, Func<float, float, float> func)
		{
			aggregate.FramesPerSecond = func(aggregate.FramesPerSecond, sample.FramesPerSecond);
			aggregate.FullFrameTime = func(aggregate.FullFrameTime, sample.FullFrameTime);
			aggregate.MainThreadCPUFrameTime = func(aggregate.MainThreadCPUFrameTime, sample.MainThreadCPUFrameTime);
			aggregate.MainThreadCPUPresentWaitTime = func(aggregate.MainThreadCPUPresentWaitTime, sample.MainThreadCPUPresentWaitTime);
			aggregate.RenderThreadCPUFrameTime = func(aggregate.RenderThreadCPUFrameTime, sample.RenderThreadCPUFrameTime);
			aggregate.GPUFrameTime = func(aggregate.GPUFrameTime, sample.GPUFrameTime);
		}
	}

	internal void DiscardOldSamples(int sampleHistorySize)
	{
		while (m_Samples.Count >= sampleHistorySize)
		{
			m_Samples.RemoveAt(0);
		}
		m_Samples.Capacity = sampleHistorySize;
	}

	internal void Clear()
	{
		m_Samples.Clear();
	}
}
