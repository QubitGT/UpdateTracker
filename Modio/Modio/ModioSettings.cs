using System;

namespace Modio;

[Serializable]
public class ModioSettings
{
	public long GameId;

	public string APIKey;

	public string ServerURL;

	public string DefaultLanguage = "en";

	public LogLevel LogLevel = LogLevel.Warning;

	public IModioServiceSettings[] PlatformSettings;

	public T GetPlatformSettings<T>() where T : IModioServiceSettings
	{
		if (PlatformSettings == null)
		{
			return default(T);
		}
		IModioServiceSettings[] platformSettings = PlatformSettings;
		foreach (IModioServiceSettings modioServiceSettings in platformSettings)
		{
			if (modioServiceSettings is T)
			{
				return (T)modioServiceSettings;
			}
		}
		return default(T);
	}

	public bool TryGetPlatformSettings<T>(out T settings) where T : IModioServiceSettings
	{
		if (PlatformSettings == null)
		{
			settings = default(T);
			return false;
		}
		IModioServiceSettings[] platformSettings = PlatformSettings;
		for (int i = 0; i < platformSettings.Length; i++)
		{
			if (platformSettings[i] is T val)
			{
				settings = val;
				return true;
			}
		}
		settings = default(T);
		return false;
	}

	public ModioSettings ShallowClone()
	{
		return MemberwiseClone() as ModioSettings;
	}
}
