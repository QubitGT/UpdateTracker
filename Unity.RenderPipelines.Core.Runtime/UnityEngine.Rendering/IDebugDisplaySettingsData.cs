namespace UnityEngine.Rendering;

public interface IDebugDisplaySettingsData : IDebugDisplaySettingsQuery
{
	IDebugDisplaySettingsPanelDisposable CreatePanel();

	void Reset()
	{
	}
}
