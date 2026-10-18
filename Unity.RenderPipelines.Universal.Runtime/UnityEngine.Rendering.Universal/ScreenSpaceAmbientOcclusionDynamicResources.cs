using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "R: SSAO Noise Textures", Order = 1000)]
[ElementInfo(Order = 0)]
[HideInInspector]
internal class ScreenSpaceAmbientOcclusionDynamicResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[ResourceFormattedPaths("Textures/BlueNoise256/LDR_LLL1_{0}.png", 0, 7, SearchType.ProjectPath)]
	private Texture2D[] m_BlueNoise256Textures;

	[SerializeField]
	[HideInInspector]
	private int m_Version;

	public Texture2D[] BlueNoise256Textures
	{
		get
		{
			return m_BlueNoise256Textures;
		}
		set
		{
			this.SetValueAndNotify(ref m_BlueNoise256Textures, value, "BlueNoise256Textures");
		}
	}

	public bool isAvailableInPlayerBuild => true;

	public int version => m_Version;
}
