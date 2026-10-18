using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

public abstract class DebugDisplayStats<TProfileId> where TProfileId : Enum
{
	private class AccumulatedTiming
	{
		public float accumulatedValue;

		public float lastAverage;

		internal void UpdateLastAverage(int frameCount)
		{
			lastAverage = accumulatedValue / (float)frameCount;
			accumulatedValue = 0f;
		}
	}

	private enum DebugProfilingType
	{
		CPU,
		InlineCPU,
		GPU
	}

	private static readonly string[] k_DetailedStatsColumnLabels = new string[3] { "CPU", "CPUInline", "GPU" };

	private Dictionary<TProfileId, AccumulatedTiming>[] m_AccumulatedTiming = new Dictionary<TProfileId, AccumulatedTiming>[3]
	{
		new Dictionary<TProfileId, AccumulatedTiming>(),
		new Dictionary<TProfileId, AccumulatedTiming>(),
		new Dictionary<TProfileId, AccumulatedTiming>()
	};

	private float m_TimeSinceLastAvgValue;

	private int m_AccumulatedFrames;

	private HashSet<TProfileId> m_HiddenProfileIds = new HashSet<TProfileId>();

	private const float k_AccumulationTimeInSeconds = 1f;

	protected bool averageProfilerTimingsOverASecond;

	protected bool hideEmptyScopes = true;

	public abstract void EnableProfilingRecorders();

	public abstract void DisableProfilingRecorders();

	public abstract void RegisterDebugUI(List<DebugUI.Widget> list);

	public abstract void Update();

	protected List<TProfileId> GetProfilerIdsToDisplay()
	{
		List<TProfileId> list = new List<TProfileId>();
		Type type = typeof(TProfileId);
		foreach (object value in Enum.GetValues(type))
		{
			if (Attribute.GetCustomAttribute(type.GetMember(value.ToString()).First((MemberInfo m) => m.DeclaringType == type), typeof(HideInDebugUIAttribute)) == null)
			{
				list.Add((TProfileId)value);
			}
		}
		return list;
	}

	protected void UpdateDetailedStats(List<TProfileId> samplers)
	{
		m_HiddenProfileIds.Clear();
		m_TimeSinceLastAvgValue += Time.unscaledDeltaTime;
		m_AccumulatedFrames++;
		bool flag = m_TimeSinceLastAvgValue >= 1f;
		UpdateListOfAveragedProfilerTimings(flag, samplers);
		if (flag)
		{
			m_TimeSinceLastAvgValue = 0f;
			m_AccumulatedFrames = 0;
		}
	}

	protected DebugUI.Widget BuildDetailedStatsList(string title, List<TProfileId> samplers)
	{
		return new DebugUI.Foldout(title, BuildProfilingSamplerWidgetList(samplers), k_DetailedStatsColumnLabels)
		{
			opened = true
		};
	}

	private void UpdateListOfAveragedProfilerTimings(bool needUpdatingAverages, List<TProfileId> samplers)
	{
		foreach (TProfileId sampler in samplers)
		{
			ProfilingSampler profilingSampler = ProfilingSampler.Get(sampler);
			bool flag = true;
			if (m_AccumulatedTiming[0].TryGetValue(sampler, out var value))
			{
				value.accumulatedValue += profilingSampler.cpuElapsedTime;
				flag &= value.accumulatedValue == 0f;
			}
			if (m_AccumulatedTiming[1].TryGetValue(sampler, out var value2))
			{
				value2.accumulatedValue += profilingSampler.inlineCpuElapsedTime;
				flag &= value2.accumulatedValue == 0f;
			}
			if (m_AccumulatedTiming[2].TryGetValue(sampler, out var value3))
			{
				value3.accumulatedValue += profilingSampler.gpuElapsedTime;
				flag &= value3.accumulatedValue == 0f;
			}
			if (needUpdatingAverages)
			{
				value?.UpdateLastAverage(m_AccumulatedFrames);
				value2?.UpdateLastAverage(m_AccumulatedFrames);
				value3?.UpdateLastAverage(m_AccumulatedFrames);
			}
			if (flag)
			{
				m_HiddenProfileIds.Add(sampler);
			}
		}
	}

	private float GetSamplerTiming(TProfileId samplerId, ProfilingSampler sampler, DebugProfilingType type)
	{
		if (averageProfilerTimingsOverASecond && m_AccumulatedTiming[(int)type].TryGetValue(samplerId, out var value))
		{
			return value.lastAverage;
		}
		return type switch
		{
			DebugProfilingType.GPU => sampler.gpuElapsedTime, 
			DebugProfilingType.CPU => sampler.cpuElapsedTime, 
			_ => sampler.inlineCpuElapsedTime, 
		};
	}

	private ObservableList<DebugUI.Widget> BuildProfilingSamplerWidgetList(IEnumerable<TProfileId> samplers)
	{
		ObservableList<DebugUI.Widget> observableList = new ObservableList<DebugUI.Widget>();
		foreach (TProfileId samplerId2 in samplers)
		{
			ProfilingSampler sampler2 = ProfilingSampler.Get(samplerId2);
			if (sampler2 != null)
			{
				sampler2.enableRecording = true;
				observableList.Add(new DebugUI.ValueTuple
				{
					displayName = sampler2.name,
					isHiddenCallback = () => (hideEmptyScopes && m_HiddenProfileIds.Contains(samplerId2)) ? true : false,
					values = (from DebugProfilingType e in Enum.GetValues(typeof(DebugProfilingType))
						select CreateWidgetForSampler(samplerId2, sampler2, e)).ToArray()
				});
			}
		}
		return observableList;
		DebugUI.Value CreateWidgetForSampler(TProfileId samplerId, ProfilingSampler sampler, DebugProfilingType type)
		{
			Dictionary<TProfileId, AccumulatedTiming> dictionary = m_AccumulatedTiming[(int)type];
			if (!dictionary.ContainsKey(samplerId))
			{
				dictionary.Add(samplerId, new AccumulatedTiming());
			}
			return new DebugUI.Value
			{
				formatString = "{0:F2}ms",
				refreshRate = 0.2f,
				getter = () => GetSamplerTiming(samplerId, sampler, type)
			};
		}
	}
}
