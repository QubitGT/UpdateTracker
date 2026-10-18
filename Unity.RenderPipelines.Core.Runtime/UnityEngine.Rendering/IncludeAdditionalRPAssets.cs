using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "H: RP Assets Inclusion", Order = 990)]
[HideInInspector]
public class IncludeAdditionalRPAssets : IRenderPipelineGraphicsSettings
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

	[SerializeField]
	private bool m_IncludeReferencedInScenes;

	[SerializeField]
	private bool m_IncludeAssetsByLabel;

	[SerializeField]
	private string m_LabelToInclude;

	int IRenderPipelineGraphicsSettings.version => (int)m_version;

	public bool includeReferencedInScenes
	{
		get
		{
			return m_IncludeReferencedInScenes;
		}
		set
		{
			this.SetValueAndNotify(ref m_IncludeReferencedInScenes, value, "m_IncludeReferencedInScenes");
		}
	}

	public bool includeAssetsByLabel
	{
		get
		{
			return m_IncludeAssetsByLabel;
		}
		set
		{
			this.SetValueAndNotify(ref m_IncludeAssetsByLabel, value, "m_IncludeAssetsByLabel");
		}
	}

	public string labelToInclude
	{
		get
		{
			return m_LabelToInclude;
		}
		set
		{
			this.SetValueAndNotify(ref m_LabelToInclude, value, "m_LabelToInclude");
		}
	}
}
