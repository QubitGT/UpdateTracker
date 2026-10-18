using System;

namespace UnityEngine.Rendering;

public interface IDebugDisplaySettings
{
	void Reset();

	void ForEach(Action<IDebugDisplaySettingsData> onExecute);

	IDebugDisplaySettingsData Add(IDebugDisplaySettingsData newData)
	{
		return null;
	}
}
