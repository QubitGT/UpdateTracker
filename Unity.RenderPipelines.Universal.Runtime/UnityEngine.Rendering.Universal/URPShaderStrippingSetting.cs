using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "Additional Shader Stripping Settings", Order = 40)]
[ElementInfo(Order = 10)]
public class URPShaderStrippingSetting : IRenderPipelineGraphicsSettings
{
	internal enum Version
	{
		Initial
	}

	[SerializeField]
	[HideInInspector]
	private Version m_Version;

	[SerializeField]
	[Tooltip("Controls whether to automatically strip post processing shader variants based on VolumeProfile components. Stripping is done based on VolumeProfiles in project, their usage in scenes is not considered.")]
	private bool m_StripUnusedPostProcessingVariants;

	[SerializeField]
	[Tooltip("Controls whether to strip variants if the feature is disabled.")]
	private bool m_StripUnusedVariants = true;

	[SerializeField]
	[Tooltip("Controls whether Screen Coordinates Override shader variants are automatically stripped.")]
	private bool m_StripScreenCoordOverrideVariants = true;

	public int version => (int)m_Version;

	public bool stripUnusedPostProcessingVariants
	{
		get
		{
			return m_StripUnusedPostProcessingVariants;
		}
		set
		{
			this.SetValueAndNotify(ref m_StripUnusedPostProcessingVariants, value, "stripUnusedPostProcessingVariants");
		}
	}

	public bool stripUnusedVariants
	{
		get
		{
			return m_StripUnusedVariants;
		}
		set
		{
			this.SetValueAndNotify(ref m_StripUnusedVariants, value, "stripUnusedVariants");
		}
	}

	public bool stripScreenCoordOverrideVariants
	{
		get
		{
			return m_StripScreenCoordOverrideVariants;
		}
		set
		{
			this.SetValueAndNotify(ref m_StripScreenCoordOverrideVariants, value, "stripScreenCoordOverrideVariants");
		}
	}
}
