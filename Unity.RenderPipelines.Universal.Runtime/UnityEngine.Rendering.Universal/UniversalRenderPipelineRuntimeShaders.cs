using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "R: Runtime Shaders", Order = 1000)]
[HideInInspector]
public class UniversalRenderPipelineRuntimeShaders : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Shaders/Utils/FallbackError.shader", SearchType.ProjectPath)]
	private Shader m_FallbackErrorShader;

	[SerializeField]
	[ResourcePath("Shaders/Utils/BlitHDROverlay.shader", SearchType.ProjectPath)]
	internal Shader m_BlitHDROverlay;

	[SerializeField]
	[ResourcePath("Shaders/Utils/CoreBlit.shader", SearchType.ProjectPath)]
	internal Shader m_CoreBlitPS;

	[SerializeField]
	[ResourcePath("Shaders/Utils/CoreBlitColorAndDepth.shader", SearchType.ProjectPath)]
	internal Shader m_CoreBlitColorAndDepthPS;

	[SerializeField]
	[ResourcePath("Shaders/Utils/Sampling.shader", SearchType.ProjectPath)]
	private Shader m_SamplingPS;

	[Header("Terrain")]
	[SerializeField]
	[ResourcePath("Shaders/Terrain/TerrainDetailLit.shader", SearchType.ProjectPath)]
	private Shader m_TerrainDetailLit;

	[SerializeField]
	[ResourcePath("Shaders/Terrain/WavingGrassBillboard.shader", SearchType.ProjectPath)]
	private Shader m_TerrainDetailGrassBillboard;

	[SerializeField]
	[ResourcePath("Shaders/Terrain/WavingGrass.shader", SearchType.ProjectPath)]
	private Shader m_TerrainDetailGrass;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public Shader fallbackErrorShader
	{
		get
		{
			return m_FallbackErrorShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_FallbackErrorShader, value, "m_FallbackErrorShader");
		}
	}

	public Shader blitHDROverlay
	{
		get
		{
			return m_BlitHDROverlay;
		}
		set
		{
			this.SetValueAndNotify(ref m_BlitHDROverlay, value, "m_BlitHDROverlay");
		}
	}

	public Shader coreBlitPS
	{
		get
		{
			return m_CoreBlitPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_CoreBlitPS, value, "m_CoreBlitPS");
		}
	}

	public Shader coreBlitColorAndDepthPS
	{
		get
		{
			return m_CoreBlitColorAndDepthPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_CoreBlitColorAndDepthPS, value, "m_CoreBlitColorAndDepthPS");
		}
	}

	public Shader samplingPS
	{
		get
		{
			return m_SamplingPS;
		}
		set
		{
			this.SetValueAndNotify(ref m_SamplingPS, value, "m_SamplingPS");
		}
	}

	public Shader terrainDetailLitShader
	{
		get
		{
			return m_TerrainDetailLit;
		}
		set
		{
			this.SetValueAndNotify(ref m_TerrainDetailLit, value, "terrainDetailLitShader");
		}
	}

	public Shader terrainDetailGrassBillboardShader
	{
		get
		{
			return m_TerrainDetailGrassBillboard;
		}
		set
		{
			this.SetValueAndNotify(ref m_TerrainDetailGrassBillboard, value, "terrainDetailGrassBillboardShader");
		}
	}

	public Shader terrainDetailGrassShader
	{
		get
		{
			return m_TerrainDetailGrass;
		}
		set
		{
			this.SetValueAndNotify(ref m_TerrainDetailGrass, value, "terrainDetailGrassShader");
		}
	}
}
