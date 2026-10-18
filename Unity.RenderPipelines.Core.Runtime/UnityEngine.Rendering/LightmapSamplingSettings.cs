using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "Lightmap Sampling Settings", Order = 20)]
public class LightmapSamplingSettings : IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[SerializeField]
	[Tooltip("Use Bicubic Lightmap Sampling. Enabling this will improve the appearance of lightmaps, but may worsen performance on lower end platforms.")]
	private bool m_UseBicubicLightmapSampling;

	int IRenderPipelineGraphicsSettings.version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public bool useBicubicLightmapSampling
	{
		get
		{
			return m_UseBicubicLightmapSampling;
		}
		set
		{
			this.SetValueAndNotify(ref m_UseBicubicLightmapSampling, value, "m_UseBicubicLightmapSampling");
		}
	}
}
