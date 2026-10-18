using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "R: Adaptive Probe Volumes", Order = 1000)]
[HideInInspector]
internal class ProbeVolumeRuntimeResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[Header("Runtime")]
	[ResourcePath("Runtime/Lighting/ProbeVolume/ProbeVolumeBlendStates.compute", SearchType.ProjectPath)]
	public ComputeShader probeVolumeBlendStatesCS;

	[ResourcePath("Runtime/Lighting/ProbeVolume/ProbeVolumeUploadData.compute", SearchType.ProjectPath)]
	public ComputeShader probeVolumeUploadDataCS;

	[ResourcePath("Runtime/Lighting/ProbeVolume/ProbeVolumeUploadDataL2.compute", SearchType.ProjectPath)]
	public ComputeShader probeVolumeUploadDataL2CS;

	public int version => m_Version;
}
