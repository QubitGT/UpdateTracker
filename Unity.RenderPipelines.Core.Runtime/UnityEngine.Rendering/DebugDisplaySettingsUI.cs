using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public class DebugDisplaySettingsUI : IDebugData
{
	private IEnumerable<IDebugDisplaySettingsPanelDisposable> m_DisposablePanels;

	private IDebugDisplaySettings m_Settings;

	private void Reset()
	{
		if (m_Settings != null)
		{
			m_Settings.Reset();
			UnregisterDebug();
			RegisterDebug(m_Settings);
			DebugManager.instance.RefreshEditor();
		}
	}

	public void RegisterDebug(IDebugDisplaySettings settings)
	{
		DebugManager debugManager = DebugManager.instance;
		List<IDebugDisplaySettingsPanelDisposable> panels = new List<IDebugDisplaySettingsPanelDisposable>();
		debugManager.RegisterData(this);
		m_Settings = settings;
		m_DisposablePanels = panels;
		m_Settings.Add(new DebugDisplaySettingsRenderGraph());
		Action<IDebugDisplaySettingsData> onExecute = delegate(IDebugDisplaySettingsData data)
		{
			IDebugDisplaySettingsPanelDisposable debugDisplaySettingsPanelDisposable = data.CreatePanel();
			DebugUI.Widget[] widgets = debugDisplaySettingsPanelDisposable.Widgets;
			DebugUI.Panel panel = debugManager.GetPanel(debugDisplaySettingsPanelDisposable.PanelName, createIfNull: true, (debugDisplaySettingsPanelDisposable is DebugDisplaySettingsPanel debugDisplaySettingsPanel) ? debugDisplaySettingsPanel.Order : 0);
			ObservableList<DebugUI.Widget> children = panel.children;
			panel.flags = debugDisplaySettingsPanelDisposable.Flags;
			panels.Add(debugDisplaySettingsPanelDisposable);
			children.Add(widgets);
		};
		m_Settings.ForEach(onExecute);
	}

	public void UnregisterDebug()
	{
		DebugManager instance = DebugManager.instance;
		if (m_DisposablePanels != null)
		{
			foreach (IDebugDisplaySettingsPanelDisposable disposablePanel in m_DisposablePanels)
			{
				DebugUI.Widget[] widgets = disposablePanel.Widgets;
				string panelName = disposablePanel.PanelName;
				ObservableList<DebugUI.Widget> children = instance.GetPanel(panelName, createIfNull: true).children;
				disposablePanel.Dispose();
				children.Remove(widgets);
			}
			m_DisposablePanels = null;
		}
		instance.UnregisterData(this);
	}

	public Action GetReset()
	{
		return Reset;
	}
}
