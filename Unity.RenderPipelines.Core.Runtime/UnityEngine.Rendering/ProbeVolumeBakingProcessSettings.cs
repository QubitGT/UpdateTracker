using System;

namespace UnityEngine.Rendering;

[Serializable]
internal struct ProbeVolumeBakingProcessSettings
{
	internal enum SettingsVersion
	{
		Initial = 0,
		ThreadedVirtualOffset = 1,
		Max = 2,
		Current = 1
	}

	[SerializeField]
	private SettingsVersion m_Version;

	public ProbeDilationSettings dilationSettings;

	public VirtualOffsetSettings virtualOffsetSettings;

	internal static ProbeVolumeBakingProcessSettings Default
	{
		get
		{
			ProbeVolumeBakingProcessSettings result = default(ProbeVolumeBakingProcessSettings);
			result.SetDefaults();
			return result;
		}
	}

	internal ProbeVolumeBakingProcessSettings(ProbeDilationSettings dilationSettings, VirtualOffsetSettings virtualOffsetSettings)
	{
		m_Version = SettingsVersion.ThreadedVirtualOffset;
		this.dilationSettings = dilationSettings;
		this.virtualOffsetSettings = virtualOffsetSettings;
	}

	internal void SetDefaults()
	{
		m_Version = SettingsVersion.ThreadedVirtualOffset;
		dilationSettings.SetDefaults();
		virtualOffsetSettings.SetDefaults();
	}

	internal void Upgrade()
	{
		if (m_Version != SettingsVersion.ThreadedVirtualOffset)
		{
			dilationSettings.UpgradeFromTo(m_Version, SettingsVersion.ThreadedVirtualOffset);
			virtualOffsetSettings.UpgradeFromTo(m_Version, SettingsVersion.ThreadedVirtualOffset);
			m_Version = SettingsVersion.ThreadedVirtualOffset;
		}
	}
}
