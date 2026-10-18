using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "R: Debug Shaders", Order = 1000)]
[HideInInspector]
public class UniversalRenderPipelineDebugShaders : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[ResourcePath("Shaders/Debug/DebugReplacement.shader", SearchType.ProjectPath)]
	private Shader m_DebugReplacementPS;

	[SerializeField]
	[ResourcePath("Shaders/Debug/HDRDebugView.shader", SearchType.ProjectPath)]
	private Shader m_HdrDebugViewPS;

	[SerializeField]
	[ResourcePath("Shaders/Debug/ProbeVolumeSamplingDebugPositionNormal.compute", SearchType.ProjectPath)]
	private ComputeShader m_ProbeVolumeSamplingDebugComputeShader;

	public int version => 0;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => false;

	public Shader debugReplacementPS
	{
		get
		{
			return m_DebugReplacementPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_DebugReplacementPS, value, "m_DebugReplacementPS");
		}
	}

	public Shader hdrDebugViewPS
	{
		get
		{
			return m_HdrDebugViewPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_HdrDebugViewPS, value, "m_HdrDebugViewPS");
		}
	}

	public ComputeShader probeVolumeSamplingDebugComputeShader
	{
		get
		{
			return m_ProbeVolumeSamplingDebugComputeShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_ProbeVolumeSamplingDebugComputeShader, value, "m_ProbeVolumeSamplingDebugComputeShader");
		}
	}
}
