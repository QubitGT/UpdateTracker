using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public class DebugDisplaySettingsStats<TProfileId> : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery where TProfileId : Enum
{
	[DisplayInfo(name = "Display Stats", order = int.MinValue)]
	private class StatsPanel : DebugDisplaySettingsPanel
	{
		private readonly DebugDisplaySettingsStats<TProfileId> m_Data;

		public override DebugUI.Flags Flags => DebugUI.Flags.RuntimeOnly;

		public StatsPanel(DebugDisplaySettingsStats<TProfileId> displaySettingsStats)
		{
			m_Data = displaySettingsStats;
			m_Data.debugDisplayStats.EnableProfilingRecorders();
			List<DebugUI.Widget> list = new List<DebugUI.Widget>();
			m_Data.debugDisplayStats.RegisterDebugUI(list);
			foreach (DebugUI.Widget item in list)
			{
				AddWidget(item);
			}
		}

		public override void Dispose()
		{
			m_Data.debugDisplayStats.DisableProfilingRecorders();
			base.Dispose();
		}
	}

	public DebugDisplayStats<TProfileId> debugDisplayStats { get; }

	public bool AreAnySettingsActive => false;

	public DebugDisplaySettingsStats(DebugDisplayStats<TProfileId> debugDisplayStats)
	{
		this.debugDisplayStats = debugDisplayStats;
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new StatsPanel(this);
	}
}
