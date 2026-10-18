using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "R: Adaptive Probe Volumes", Order = 1000)]
[HideInInspector]
internal class ProbeVolumeDebugResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[Header("Debug")]
	[ResourcePath("Runtime/Debug/ProbeVolumeDebug.shader", SearchType.ProjectPath)]
	public Shader probeVolumeDebugShader;

	[ResourcePath("Runtime/Debug/ProbeVolumeFragmentationDebug.shader", SearchType.ProjectPath)]
	public Shader probeVolumeFragmentationDebugShader;

	[ResourcePath("Runtime/Debug/ProbeVolumeSamplingDebug.shader", SearchType.ProjectPath)]
	public Shader probeVolumeSamplingDebugShader;

	[ResourcePath("Runtime/Debug/ProbeVolumeOffsetDebug.shader", SearchType.ProjectPath)]
	public Shader probeVolumeOffsetDebugShader;

	[ResourcePath("Runtime/Debug/ProbeSamplingDebugMesh.fbx", SearchType.ProjectPath)]
	public Mesh probeSamplingDebugMesh;

	[ResourcePath("Runtime/Debug/ProbeVolumeNumbersDisplayTex.png", SearchType.ProjectPath)]
	public Texture2D numbersDisplayTex;

	public int version => m_Version;
}
