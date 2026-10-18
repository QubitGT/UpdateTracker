using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class UniversalRenderPipelineDebugDisplayStats : DebugDisplayStats<URPProfileId>
{
	private DebugFrameTiming m_DebugFrameTiming = new DebugFrameTiming();

	private List<URPProfileId> m_RecordedSamplers = new List<URPProfileId>();

	public override void EnableProfilingRecorders()
	{
		m_RecordedSamplers = GetProfilerIdsToDisplay();
	}

	public override void DisableProfilingRecorders()
	{
		foreach (URPProfileId recordedSampler in m_RecordedSamplers)
		{
			ProfilingSampler.Get(recordedSampler).enableRecording = false;
		}
		m_RecordedSamplers.Clear();
	}

	public override void RegisterDebugUI(List<DebugUI.Widget> list)
	{
		m_DebugFrameTiming.RegisterDebugUI(list);
		DebugUI.Foldout foldout = new DebugUI.Foldout
		{
			displayName = "Detailed Stats",
			opened = false,
			children = 
			{
				(DebugUI.Widget)new DebugUI.BoolField
				{
					displayName = "Update every second with average",
					getter = () => averageProfilerTimingsOverASecond,
					setter = delegate(bool value)
					{
						averageProfilerTimingsOverASecond = value;
					}
				},
				(DebugUI.Widget)new DebugUI.BoolField
				{
					displayName = "Hide empty scopes",
					tooltip = "Hide profiling scopes where elapsed time in each category is zero",
					getter = () => hideEmptyScopes,
					setter = delegate(bool value)
					{
						hideEmptyScopes = value;
					}
				}
			}
		};
		foldout.children.Add(BuildDetailedStatsList("Profiling Scopes", m_RecordedSamplers));
		list.Add(foldout);
	}

	public override void Update()
	{
		m_DebugFrameTiming.UpdateFrameTiming();
		UpdateDetailedStats(m_RecordedSamplers);
	}
}
