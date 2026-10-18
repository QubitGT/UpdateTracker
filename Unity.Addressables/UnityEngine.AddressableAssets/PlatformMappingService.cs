using System.Collections.Generic;

namespace UnityEngine.AddressableAssets;

public class PlatformMappingService
{
	internal static readonly Dictionary<RuntimePlatform, AddressablesPlatform> s_RuntimeTargetMapping = new Dictionary<RuntimePlatform, AddressablesPlatform>
	{
		{
			RuntimePlatform.XboxOne,
			AddressablesPlatform.XboxOne
		},
		{
			RuntimePlatform.Switch,
			AddressablesPlatform.Switch
		},
		{
			RuntimePlatform.PS4,
			AddressablesPlatform.PS4
		},
		{
			RuntimePlatform.IPhonePlayer,
			AddressablesPlatform.iOS
		},
		{
			RuntimePlatform.Android,
			AddressablesPlatform.Android
		},
		{
			RuntimePlatform.WebGLPlayer,
			AddressablesPlatform.WebGL
		},
		{
			RuntimePlatform.WindowsPlayer,
			AddressablesPlatform.Windows
		},
		{
			RuntimePlatform.OSXPlayer,
			AddressablesPlatform.OSX
		},
		{
			RuntimePlatform.LinuxPlayer,
			AddressablesPlatform.Linux
		},
		{
			RuntimePlatform.WindowsEditor,
			AddressablesPlatform.Windows
		},
		{
			RuntimePlatform.OSXEditor,
			AddressablesPlatform.OSX
		},
		{
			RuntimePlatform.LinuxEditor,
			AddressablesPlatform.Linux
		},
		{
			RuntimePlatform.MetroPlayerARM,
			AddressablesPlatform.WindowsUniversal
		},
		{
			RuntimePlatform.MetroPlayerX64,
			AddressablesPlatform.WindowsUniversal
		},
		{
			RuntimePlatform.MetroPlayerX86,
			AddressablesPlatform.WindowsUniversal
		}
	};

	internal static AddressablesPlatform GetAddressablesPlatformInternal(RuntimePlatform platform)
	{
		if (s_RuntimeTargetMapping.ContainsKey(platform))
		{
			return s_RuntimeTargetMapping[platform];
		}
		return AddressablesPlatform.Unknown;
	}

	internal static string GetAddressablesPlatformPathInternal(RuntimePlatform platform)
	{
		if (s_RuntimeTargetMapping.ContainsKey(platform))
		{
			return s_RuntimeTargetMapping[platform].ToString();
		}
		return platform.ToString();
	}

	public static string GetPlatformPathSubFolder()
	{
		return GetAddressablesPlatformPathInternal(Application.platform);
	}
}
