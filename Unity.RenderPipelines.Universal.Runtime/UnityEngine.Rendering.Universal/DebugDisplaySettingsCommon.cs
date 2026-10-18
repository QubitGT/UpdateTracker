using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class DebugDisplaySettingsCommon : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	[DisplayInfo(name = "Frequently Used", order = -1)]
	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		private const string k_GoToSectionString = "Go to Section...";

		public override DebugUI.Flags Flags => DebugUI.Flags.FrequentlyUsed;

		public SettingsPanel()
		{
			AddWidget(new DebugUI.RuntimeDebugShadersMessageBox());
			DebugUI.Widget[] items = DebugManager.instance.GetItems(DebugUI.Flags.FrequentlyUsed);
			foreach (DebugUI.Widget widget in items)
			{
				DebugUI.Foldout foldout = widget as DebugUI.Foldout;
				if (foldout != null)
				{
					if (foldout.contextMenuItems == null)
					{
						foldout.contextMenuItems = new List<DebugUI.Foldout.ContextMenuItem>();
					}
					foldout.contextMenuItems.Add(new DebugUI.Foldout.ContextMenuItem
					{
						displayName = "Go to Section...",
						action = delegate
						{
							int num = DebugManager.instance.PanelIndex(foldout.panel.displayName);
							if (num >= 0)
							{
								DebugManager.instance.RequestEditorWindowPanelIndex(num);
							}
						}
					});
				}
				AddWidget(widget);
			}
		}
	}

	public bool AreAnySettingsActive => false;

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel();
	}
}
