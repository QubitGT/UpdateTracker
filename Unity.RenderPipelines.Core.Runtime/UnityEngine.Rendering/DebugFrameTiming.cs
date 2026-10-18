using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering;

public class DebugFrameTiming
{
	private const string k_FpsFormatString = "{0:F1}";

	private const string k_MsFormatString = "{0:F2}ms";

	private const float k_RefreshRate = 0.2f;

	internal FrameTimeSampleHistory m_FrameHistory;

	internal BottleneckHistory m_BottleneckHistory;

	private FrameTiming[] m_Timing = new FrameTiming[1];

	private FrameTimeSample m_Sample;

	public int bottleneckHistorySize { get; set; } = 60;


	public int sampleHistorySize { get; set; } = 30;


	public DebugFrameTiming()
	{
		m_FrameHistory = new FrameTimeSampleHistory(sampleHistorySize);
		m_BottleneckHistory = new BottleneckHistory(bottleneckHistorySize);
	}

	public void UpdateFrameTiming()
	{
		m_Timing[0] = default(FrameTiming);
		m_Sample = default(FrameTimeSample);
		FrameTimingManager.CaptureFrameTimings();
		FrameTimingManager.GetLatestTimings(1u, m_Timing);
		if (m_Timing.Length != 0)
		{
			m_Sample.FullFrameTime = (float)m_Timing.First().cpuFrameTime;
			m_Sample.FramesPerSecond = ((m_Sample.FullFrameTime > 0f) ? (1000f / m_Sample.FullFrameTime) : 0f);
			m_Sample.MainThreadCPUFrameTime = (float)m_Timing.First().cpuMainThreadFrameTime;
			m_Sample.MainThreadCPUPresentWaitTime = (float)m_Timing.First().cpuMainThreadPresentWaitTime;
			m_Sample.RenderThreadCPUFrameTime = (float)m_Timing.First().cpuRenderThreadFrameTime;
			m_Sample.GPUFrameTime = (float)m_Timing.First().gpuFrameTime;
		}
		m_FrameHistory.DiscardOldSamples(sampleHistorySize);
		m_FrameHistory.Add(m_Sample);
		m_FrameHistory.ComputeAggregateValues();
		m_BottleneckHistory.DiscardOldSamples(bottleneckHistorySize);
		m_BottleneckHistory.AddBottleneckFromAveragedSample(m_FrameHistory.SampleAverage);
		m_BottleneckHistory.ComputeHistogram();
	}

	public void RegisterDebugUI(List<DebugUI.Widget> list)
	{
		list.Add(new DebugUI.Foldout
		{
			displayName = "Frame Stats",
			opened = true,
			columnLabels = new string[3] { "Avg", "Min", "Max" },
			children = 
			{
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "Frame Rate (FPS)",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F1}",
							getter = () => m_FrameHistory.SampleAverage.FramesPerSecond
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F1}",
							getter = () => m_FrameHistory.SampleMin.FramesPerSecond
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F1}",
							getter = () => m_FrameHistory.SampleMax.FramesPerSecond
						}
					}
				},
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "Frame Time",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleAverage.FullFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMin.FullFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMax.FullFrameTime
						}
					}
				},
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "CPU Main Thread Frame",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleAverage.MainThreadCPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMin.MainThreadCPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMax.MainThreadCPUFrameTime
						}
					}
				},
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "CPU Render Thread Frame",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleAverage.RenderThreadCPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMin.RenderThreadCPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMax.RenderThreadCPUFrameTime
						}
					}
				},
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "CPU Present Wait",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleAverage.MainThreadCPUPresentWaitTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMin.MainThreadCPUPresentWaitTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMax.MainThreadCPUPresentWaitTime
						}
					}
				},
				(DebugUI.Widget)new DebugUI.ValueTuple
				{
					displayName = "GPU Frame",
					values = new DebugUI.Value[3]
					{
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleAverage.GPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMin.GPUFrameTime
						},
						new DebugUI.Value
						{
							refreshRate = 0.2f,
							formatString = "{0:F2}ms",
							getter = () => m_FrameHistory.SampleMax.GPUFrameTime
						}
					}
				}
			}
		});
		list.Add(new DebugUI.Foldout
		{
			displayName = "Bottlenecks",
			children = 
			{
				(DebugUI.Widget)new DebugUI.ProgressBarValue
				{
					displayName = "CPU",
					getter = () => m_BottleneckHistory.Histogram.CPU
				},
				(DebugUI.Widget)new DebugUI.ProgressBarValue
				{
					displayName = "GPU",
					getter = () => m_BottleneckHistory.Histogram.GPU
				},
				(DebugUI.Widget)new DebugUI.ProgressBarValue
				{
					displayName = "Present limited",
					getter = () => m_BottleneckHistory.Histogram.PresentLimited
				},
				(DebugUI.Widget)new DebugUI.ProgressBarValue
				{
					displayName = "Balanced",
					getter = () => m_BottleneckHistory.Histogram.Balanced
				}
			}
		});
	}

	internal void Reset()
	{
		m_BottleneckHistory.Clear();
		m_FrameHistory.Clear();
	}
}
