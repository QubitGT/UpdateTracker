using System;

namespace UnityEngine.Rendering;

public struct ProbeVolumeSystemParameters
{
	public ProbeVolumeTextureMemoryBudget memoryBudget;

	public ProbeVolumeBlendingTextureMemoryBudget blendingMemoryBudget;

	public ProbeVolumeSHBands shBands;

	public bool supportScenarios;

	public bool supportScenarioBlending;

	public bool supportGPUStreaming;

	public bool supportDiskStreaming;

	[Obsolete("This field is not used anymore.")]
	public Shader probeDebugShader;

	[Obsolete("This field is not used anymore.")]
	public Shader probeSamplingDebugShader;

	[Obsolete("This field is not used anymore.")]
	public Texture probeSamplingDebugTexture;

	[Obsolete("This field is not used anymore.")]
	public Mesh probeSamplingDebugMesh;

	[Obsolete("This field is not used anymore.")]
	public Shader offsetDebugShader;

	[Obsolete("This field is not used anymore.")]
	public Shader fragmentationDebugShader;

	[Obsolete("This field is not used anymore.")]
	public ComputeShader scenarioBlendingShader;

	[Obsolete("This field is not used anymore.")]
	public ComputeShader streamingUploadShader;

	[Obsolete("This field is not used anymore.")]
	public ProbeVolumeSceneData sceneData;

	[Obsolete("This field is not used anymore. Used with the current Shader Stripping Settings. #from(2023.3)")]
	public bool supportsRuntimeDebug;
}
