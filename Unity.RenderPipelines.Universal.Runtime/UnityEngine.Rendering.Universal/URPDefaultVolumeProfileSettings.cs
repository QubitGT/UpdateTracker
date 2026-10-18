using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering.Universal;

[Serializable]
[SupportedOnRenderPipeline(typeof(UniversalRenderPipelineAsset))]
[CategoryInfo(Name = "Volume", Order = 0)]
public class URPDefaultVolumeProfileSettings : IDefaultVolumeProfileSettings, IRenderPipelineGraphicsSettings
{
	internal enum Version
	{
		Initial
	}

	[SerializeField]
	[HideInInspector]
	private Version m_Version;

	[SerializeField]
	private VolumeProfile m_VolumeProfile;

	public int version => (int)m_Version;

	public VolumeProfile volumeProfile
	{
		get
		{
			return m_VolumeProfile;
		}
		set
		{
			this.SetValueAndNotify(ref m_VolumeProfile, value, "volumeProfile");
		}
	}
}
