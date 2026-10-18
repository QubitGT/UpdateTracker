using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "R: VRS - Runtime Resources", Order = 1000)]
[HideInInspector]
public sealed class VrsRenderPipelineRuntimeResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[ResourcePath("Runtime/Vrs/Shaders/VrsTexture.compute", SearchType.ProjectPath)]
	private ComputeShader m_TextureComputeShader;

	[SerializeField]
	[ResourcePath("Runtime/Vrs/Shaders/VrsVisualization.shader", SearchType.ProjectPath)]
	private Shader m_VisualizationShader;

	[SerializeField]
	[Tooltip("Colors to visualize the shading rates")]
	private VrsLut m_VisualizationLookupTable = VrsLut.CreateDefault();

	[SerializeField]
	[Tooltip("Colors to convert between shading rates and textures")]
	private VrsLut m_ConversionLookupTable = VrsLut.CreateDefault();

	public int version => 0;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public ComputeShader textureComputeShader
	{
		get
		{
			return m_TextureComputeShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_TextureComputeShader, value, "m_TextureComputeShader");
		}
	}

	public Shader visualizationShader
	{
		get
		{
			return m_VisualizationShader;
		}
		set
		{
			this.SetValueAndNotify(ref m_VisualizationShader, value, "m_VisualizationShader");
		}
	}

	public VrsLut visualizationLookupTable
	{
		get
		{
			return m_VisualizationLookupTable;
		}
		set
		{
			this.SetValueAndNotify(ref m_VisualizationLookupTable, value, "m_VisualizationLookupTable");
		}
	}

	public VrsLut conversionLookupTable
	{
		get
		{
			return m_ConversionLookupTable;
		}
		set
		{
			this.SetValueAndNotify(ref m_ConversionLookupTable, value, "m_ConversionLookupTable");
		}
	}
}
