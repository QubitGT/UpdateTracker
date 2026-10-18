using Modio.API;
using Modio.API.Interfaces;
using Modio.Extensions;
using Modio.FileIO;
using Modio.Platforms;
using UnityEngine;

namespace Modio.Unity;

internal static class ModioUnity
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		ModioUnitySettings modioUnitySettings = Resources.Load<ModioUnitySettings>("mod.io/v3_config_local");
		if (modioUnitySettings == null)
		{
			modioUnitySettings = Resources.Load<ModioUnitySettings>("mod.io/v3_config");
		}
		if (ModioCommandLine.TryGet("gameid", out var value))
		{
			modioUnitySettings.Settings.GameId = int.Parse(value);
		}
		if (ModioCommandLine.TryGet("apikey", out var value2))
		{
			modioUnitySettings.Settings.APIKey = value2;
		}
		if (ModioCommandLine.TryGet("url", out var value3))
		{
			modioUnitySettings.Settings.ServerURL = value3;
		}
		ModioServices.Bind<IModioLogHandler>().FromNew<ModioUnityLogger>(ModioServicePriority.EngineImplementation);
		string text = $"Unity; {Application.unityVersion}; {Application.platform}";
		ModioLog.Verbose?.Log(text);
		Version.AddEnvironmentDetails(text);
		if (modioUnitySettings != null)
		{
			ModioServices.BindInstance(modioUnitySettings.Settings);
		}
		else
		{
			ModioLog.Message?.Log("Couldn't find a ModioUnitySettings named 'mod.io/v3_config' to load in a Resources folder");
		}
		ModioServices.Bind<IModioAPIInterface>().FromNew<ModioAPIUnityClient>(ModioServicePriority.EngineImplementation);
		ModioServices.Bind<IModioRootPathProvider>().FromNew<WindowsRootPathProvider>(ModioServicePriority.PlatformProvided, WindowsRootPathProvider.IsPublicEnvironmentVariableSet);
		if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			ModioServices.Bind<IModioDataStorage>().FromNew<LinuxDataStorage>(ModioServicePriority.PlatformProvided);
		}
		if (Application.platform == RuntimePlatform.OSXPlayer)
		{
			ModioServices.Bind<IModioDataStorage>().FromNew<MacDataStorage>(ModioServicePriority.PlatformProvided);
		}
		ModioServices.Bind<IModioRootPathProvider>().FromNew<UnityRootPathProvider>(ModioServicePriority.Default);
		ModioServices.Bind<IWebBrowserHandler>().FromNew<UnityWebBrowserHandler>(ModioServicePriority.EngineImplementation);
		ModioServices.BindErrorMessage<ModioSettings>("Please ensure you've bound a ModioSettings. You can create one using the menu item 'Tools/mod.io/Edit Settings'", (ModioServicePriority)1);
		Application.quitting += delegate
		{
			ModioClient.Shutdown().ForgetTaskSafely();
		};
		InitPlatform();
	}

	private static void Log(LogLevel logLevel, object message)
	{
		(logLevel switch
		{
			LogLevel.Error => Debug.LogError, 
			LogLevel.Warning => Debug.LogWarning, 
			_ => Debug.Log, 
		})(message);
	}

	private static void InitPlatform()
	{
		ModioAPI.Platform platform = Application.platform switch
		{
			RuntimePlatform.OSXEditor => ModioAPI.Platform.Mac, 
			RuntimePlatform.OSXPlayer => ModioAPI.Platform.Mac, 
			RuntimePlatform.WindowsPlayer => ModioAPI.Platform.Windows, 
			RuntimePlatform.WindowsEditor => ModioAPI.Platform.Windows, 
			RuntimePlatform.IPhonePlayer => ModioAPI.Platform.IOS, 
			RuntimePlatform.Android => ModioAPI.Platform.Android, 
			RuntimePlatform.LinuxPlayer => ModioAPI.Platform.Linux, 
			RuntimePlatform.LinuxEditor => ModioAPI.Platform.Linux, 
			RuntimePlatform.PS4 => ModioAPI.Platform.PlayStation4, 
			RuntimePlatform.XboxOne => ModioAPI.Platform.XboxOne, 
			RuntimePlatform.Switch => ModioAPI.Platform.Switch, 
			RuntimePlatform.GameCoreXboxSeries => ModioAPI.Platform.XboxSeriesX, 
			RuntimePlatform.GameCoreXboxOne => ModioAPI.Platform.XboxOne, 
			RuntimePlatform.PS5 => ModioAPI.Platform.PlayStation5, 
			_ => ModioAPI.Platform.None, 
		};
		if (platform != ModioAPI.Platform.None)
		{
			ModioAPI.SetPlatform(platform);
		}
	}
}
