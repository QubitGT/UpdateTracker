namespace UnityEngine.Rendering;

public interface IDebugDisplaySettingsPanel
{
	string PanelName { get; }

	DebugUI.Widget[] Widgets { get; }

	DebugUI.Flags Flags { get; }
}
