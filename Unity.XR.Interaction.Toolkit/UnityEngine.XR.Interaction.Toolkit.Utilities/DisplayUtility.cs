namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal static class DisplayUtility
{
	private static float s_ScreenDpi = 100f;

	private static float s_OneOverScreenDpi = 1f / s_ScreenDpi;

	private static bool s_ScreenDpiChecked;

	public static float screenDpi
	{
		get
		{
			CacheScreenDpi();
			return s_ScreenDpi;
		}
	}

	public static float screenDpiRatio
	{
		get
		{
			CacheScreenDpi();
			return s_OneOverScreenDpi;
		}
	}

	private static void CacheScreenDpi()
	{
		if (!s_ScreenDpiChecked)
		{
			if (Screen.dpi > 0f)
			{
				s_ScreenDpi = Screen.dpi;
			}
			else
			{
				Debug.LogWarning("Platform has reported a screen DPI of 0. Using default value of 100.");
			}
			s_OneOverScreenDpi = 1f / s_ScreenDpi;
			s_ScreenDpiChecked = true;
		}
	}

	public static float PixelsToInches(float pixels)
	{
		return pixels * screenDpiRatio;
	}

	public static float InchesToPixels(float inches)
	{
		return inches * screenDpi;
	}
}
