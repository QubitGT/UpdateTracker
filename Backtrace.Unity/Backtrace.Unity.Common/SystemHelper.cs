using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Backtrace.Unity.Common;

internal static class SystemHelper
{
	[DllImport("kernel32.dll", ExactSpelling = true)]
	internal static extern uint GetCurrentThreadId();

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern IntPtr LoadLibrary(string lpFileName);

	internal static bool IsLibraryAvailable(string libraryName)
	{
		try
		{
			return LoadLibrary(libraryName) != IntPtr.Zero;
		}
		catch (TypeLoadException)
		{
		}
		catch (Exception)
		{
		}
		return false;
	}

	internal static bool IsLibraryAvailable(string[] libraries)
	{
		if (libraries == null || libraries.Length == 0)
		{
			return true;
		}
		return !libraries.Any((string n) => !IsLibraryAvailable(n));
	}

	internal static string Name()
	{
		switch (Application.platform)
		{
		case RuntimePlatform.Android:
			return "Android";
		case RuntimePlatform.IPhonePlayer:
			return "iOS";
		case RuntimePlatform.LinuxPlayer:
		case RuntimePlatform.LinuxEditor:
			return "Linux";
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
			return "Mac OS";
		case RuntimePlatform.PS3:
			return "ps3";
		case RuntimePlatform.PS4:
			return "ps4";
		case RuntimePlatform.TizenPlayer:
		case RuntimePlatform.SamsungTVPlayer:
			return "Samsung TV";
		case RuntimePlatform.tvOS:
			return "tvOS";
		case RuntimePlatform.WebGLPlayer:
			return "WebGL";
		case RuntimePlatform.WiiU:
			return "WiiU";
		case RuntimePlatform.Switch:
			return "switch";
		case RuntimePlatform.WindowsPlayer:
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.MetroPlayerX86:
		case RuntimePlatform.MetroPlayerX64:
		case RuntimePlatform.MetroPlayerARM:
			return "Windows";
		case RuntimePlatform.XBOX360:
		case RuntimePlatform.XboxOne:
			return "Xbox";
		default:
			return Application.platform.ToString();
		}
	}

	internal static string CpuArchitecture()
	{
		return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")?.ToLower();
	}
}
