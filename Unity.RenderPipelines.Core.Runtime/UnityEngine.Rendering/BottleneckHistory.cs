using System.Collections.Generic;

namespace UnityEngine.Rendering;

internal class BottleneckHistory
{
	private List<PerformanceBottleneck> m_Bottlenecks = new List<PerformanceBottleneck>();

	internal BottleneckHistogram Histogram;

	public BottleneckHistory(int initialCapacity)
	{
		m_Bottlenecks.Capacity = initialCapacity;
	}

	internal void DiscardOldSamples(int historySize)
	{
		while (m_Bottlenecks.Count >= historySize)
		{
			m_Bottlenecks.RemoveAt(0);
		}
		m_Bottlenecks.Capacity = historySize;
	}

	internal void AddBottleneckFromAveragedSample(FrameTimeSample frameHistorySampleAverage)
	{
		PerformanceBottleneck item = DetermineBottleneck(frameHistorySampleAverage);
		m_Bottlenecks.Add(item);
	}

	internal void ComputeHistogram()
	{
		BottleneckHistogram histogram = default(BottleneckHistogram);
		for (int i = 0; i < m_Bottlenecks.Count; i++)
		{
			switch (m_Bottlenecks[i])
			{
			case PerformanceBottleneck.Balanced:
				histogram.Balanced += 1f;
				break;
			case PerformanceBottleneck.CPU:
				histogram.CPU += 1f;
				break;
			case PerformanceBottleneck.GPU:
				histogram.GPU += 1f;
				break;
			case PerformanceBottleneck.PresentLimited:
				histogram.PresentLimited += 1f;
				break;
			}
		}
		histogram.Balanced /= m_Bottlenecks.Count;
		histogram.CPU /= m_Bottlenecks.Count;
		histogram.GPU /= m_Bottlenecks.Count;
		histogram.PresentLimited /= m_Bottlenecks.Count;
		Histogram = histogram;
	}

	private static PerformanceBottleneck DetermineBottleneck(FrameTimeSample s)
	{
		if (s.GPUFrameTime == 0f || s.MainThreadCPUFrameTime == 0f)
		{
			return PerformanceBottleneck.Indeterminate;
		}
		float num = 0.8f * s.FullFrameTime;
		if (s.GPUFrameTime > num && s.MainThreadCPUFrameTime < num && s.RenderThreadCPUFrameTime < num)
		{
			return PerformanceBottleneck.GPU;
		}
		if (s.GPUFrameTime < num && (s.MainThreadCPUFrameTime > num || s.RenderThreadCPUFrameTime > num))
		{
			return PerformanceBottleneck.CPU;
		}
		if (s.MainThreadCPUPresentWaitTime > 0.5f && s.GPUFrameTime < num && s.MainThreadCPUFrameTime < num && s.RenderThreadCPUFrameTime < num)
		{
			return PerformanceBottleneck.PresentLimited;
		}
		return PerformanceBottleneck.Balanced;
	}

	internal void Clear()
	{
		m_Bottlenecks.Clear();
		Histogram = default(BottleneckHistogram);
	}
}
