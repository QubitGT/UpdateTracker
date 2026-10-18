using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "Render Graph", Order = 50)]
[ElementInfo(Order = -10)]
public class RenderGraphSettings : IRenderPipelineGraphicsSettings
{
	internal enum Version
	{
		Initial
	}

	[SerializeField]
	[HideInInspector]
	private Version m_Version;

	[SerializeField]
	[Tooltip("When enabled, URP does not use the Render Graph API to construct and execute the frame. Use this option only for compatibility purposes.")]
	[RecreatePipelineOnChange]
	private bool m_EnableRenderCompatibilityMode;

	public int version => (int)m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	public bool enableRenderCompatibilityMode
	{
		get
		{
			if (m_EnableRenderCompatibilityMode)
			{
				return !RenderGraphGraphicsAutomatedTests.enabled;
			}
			return false;
		}
		set
		{
			this.SetValueAndNotify(ref m_EnableRenderCompatibilityMode, value, "m_EnableRenderCompatibilityMode");
		}
	}
}
