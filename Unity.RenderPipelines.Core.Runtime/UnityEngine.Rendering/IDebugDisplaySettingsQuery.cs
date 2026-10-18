namespace UnityEngine.Rendering;

public interface IDebugDisplaySettingsQuery
{
	bool AreAnySettingsActive { get; }

	bool IsPostProcessingAllowed => true;

	bool IsLightingActive => true;

	bool TryGetScreenClearColor(ref Color color)
	{
		return false;
	}
}
