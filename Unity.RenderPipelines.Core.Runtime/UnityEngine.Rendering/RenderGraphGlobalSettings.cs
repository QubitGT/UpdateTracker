using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "Render Graph", Order = 50)]
[ElementInfo(Order = 0)]
public class RenderGraphGlobalSettings : IRenderPipelineGraphicsSettings
{
	private enum Version
	{
		Initial = 0,
		Count = 1,
		Last = 0
	}

	[SerializeField]
	[HideInInspector]
	private Version m_version;

	[RecreatePipelineOnChange]
	[SerializeField]
	[Tooltip("Enable caching of render graph compilation from one frame to another.")]
	private bool m_EnableCompilationCaching = true;

	[RecreatePipelineOnChange]
	[SerializeField]
	[Tooltip("Enable validity checks of render graph in Editor and Development mode. Always disabled in Release build.")]
	private bool m_EnableValidityChecks = true;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	int IRenderPipelineGraphicsSettings.version => (int)m_version;

	public bool enableCompilationCaching
	{
		get
		{
			return m_EnableCompilationCaching;
		}
		set
		{
			this.SetValueAndNotify(ref m_EnableCompilationCaching, value, "enableCompilationCaching");
		}
	}

	public bool enableValidityChecks
	{
		get
		{
			return m_EnableValidityChecks;
		}
		set
		{
			this.SetValueAndNotify(ref m_EnableValidityChecks, value, "enableValidityChecks");
		}
	}
}
