using System;
using UnityEngine.Categorization;

namespace UnityEngine.Rendering;

[Serializable]
[SupportedOnRenderPipeline(new Type[] { })]
[CategoryInfo(Name = "Adaptive Probe Volumes", Order = 20)]
internal class ProbeVolumeGlobalSettings : IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[SerializeField]
	[Tooltip("Enabling this will make APV baked data assets compatible with Addressables and Asset Bundles. This will also make Disk Streaming unavailable. After changing this setting, a clean rebuild may be required for data assets to be included in Adressables and Asset Bundles.")]
	private bool m_ProbeVolumeDisableStreamingAssets;

	public int version => m_Version;

	public bool probeVolumeDisableStreamingAssets
	{
		get
		{
			return m_ProbeVolumeDisableStreamingAssets;
		}
		set
		{
			this.SetValueAndNotify(ref m_ProbeVolumeDisableStreamingAssets, value, "m_ProbeVolumeDisableStreamingAssets");
		}
	}
}
