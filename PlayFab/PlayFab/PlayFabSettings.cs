using System;
using System.Collections.Generic;
using System.Text;
using PlayFab.Internal;
using UnityEngine;

namespace PlayFab;

public static class PlayFabSettings
{
	private static PlayFabSharedSettings _playFabShared;

	public static readonly PlayFabApiSettings staticSettings;

	public static PlayFabAuthenticationContext staticPlayer;

	public const string SdkVersion = "2.87.200602";

	public const string BuildIdentifier = "jbuild_unitysdk__sdk-unity-3-slave_0";

	public const string VersionString = "UnitySDK-2.87.200602";

	public const string AD_TYPE_IDFA = "Idfa";

	public const string AD_TYPE_ANDROID_ID = "Adid";

	public const string DefaultPlayFabApiUrl = "playfabapi.com";

	private static string _localApiServer;

	private static PlayFabSharedSettings PlayFabSharedPrivate
	{
		get
		{
			if (_playFabShared == null)
			{
				_playFabShared = GetSharedSettingsObjectPrivate();
			}
			return _playFabShared;
		}
	}

	public static string DeviceUniqueIdentifier => SystemInfo.deviceUniqueIdentifier;

	public static string TitleId
	{
		get
		{
			return staticSettings.TitleId;
		}
		set
		{
			staticSettings.TitleId = value;
		}
	}

	internal static string VerticalName
	{
		get
		{
			return staticSettings.VerticalName;
		}
		set
		{
			staticSettings.VerticalName = value;
		}
	}

	public static bool DisableAdvertising
	{
		get
		{
			return staticSettings.DisableAdvertising;
		}
		set
		{
			staticSettings.DisableAdvertising = value;
		}
	}

	public static bool DisableDeviceInfo
	{
		get
		{
			return staticSettings.DisableDeviceInfo;
		}
		set
		{
			staticSettings.DisableDeviceInfo = value;
		}
	}

	public static bool DisableFocusTimeCollection
	{
		get
		{
			return staticSettings.DisableFocusTimeCollection;
		}
		set
		{
			staticSettings.DisableFocusTimeCollection = value;
		}
	}

	[Obsolete("LogLevel has been deprecated, please use UnityEngine.Debug.Log for your logging needs.")]
	public static PlayFabLogLevel LogLevel
	{
		get
		{
			return PlayFabSharedPrivate.LogLevel;
		}
		set
		{
			PlayFabSharedPrivate.LogLevel = value;
		}
	}

	public static WebRequestType RequestType
	{
		get
		{
			return PlayFabSharedPrivate.RequestType;
		}
		set
		{
			PlayFabSharedPrivate.RequestType = value;
		}
	}

	public static int RequestTimeout
	{
		get
		{
			return PlayFabSharedPrivate.RequestTimeout;
		}
		set
		{
			PlayFabSharedPrivate.RequestTimeout = value;
		}
	}

	public static bool RequestKeepAlive
	{
		get
		{
			return PlayFabSharedPrivate.RequestKeepAlive;
		}
		set
		{
			PlayFabSharedPrivate.RequestKeepAlive = value;
		}
	}

	public static bool CompressApiData
	{
		get
		{
			return PlayFabSharedPrivate.CompressApiData;
		}
		set
		{
			PlayFabSharedPrivate.CompressApiData = value;
		}
	}

	public static string LoggerHost
	{
		get
		{
			return PlayFabSharedPrivate.LoggerHost;
		}
		set
		{
			PlayFabSharedPrivate.LoggerHost = value;
		}
	}

	public static int LoggerPort
	{
		get
		{
			return PlayFabSharedPrivate.LoggerPort;
		}
		set
		{
			PlayFabSharedPrivate.LoggerPort = value;
		}
	}

	public static bool EnableRealTimeLogging
	{
		get
		{
			return PlayFabSharedPrivate.EnableRealTimeLogging;
		}
		set
		{
			PlayFabSharedPrivate.EnableRealTimeLogging = value;
		}
	}

	public static int LogCapLimit
	{
		get
		{
			return PlayFabSharedPrivate.LogCapLimit;
		}
		set
		{
			PlayFabSharedPrivate.LogCapLimit = value;
		}
	}

	public static string LocalApiServer
	{
		get
		{
			return _localApiServer ?? PlayFabUtil.GetLocalSettingsFileProperty("LocalApiServer");
		}
		set
		{
			_localApiServer = value;
		}
	}

	static PlayFabSettings()
	{
		_playFabShared = null;
		staticSettings = new PlayFabSettingsRedirect(() => PlayFabSharedPrivate);
		staticPlayer = new PlayFabAuthenticationContext();
	}

	private static PlayFabSharedSettings GetSharedSettingsObjectPrivate()
	{
		PlayFabSharedSettings[] array = Resources.LoadAll<PlayFabSharedSettings>("PlayFabSharedSettings");
		if (array.Length != 1)
		{
			Debug.LogWarning("The number of PlayFabSharedSettings objects should be 1: " + array.Length);
			Debug.LogWarning("If you are upgrading your SDK, you can ignore this warning as PlayFabSharedSettings will be imported soon. If you are not upgrading your SDK and you see this message, you should re-download the latest PlayFab source code.");
		}
		return array[0];
	}

	public static string GetFullUrl(string apiCall, Dictionary<string, string> getParams, PlayFabApiSettings apiSettings = null)
	{
		StringBuilder stringBuilder = new StringBuilder(1000);
		string text = null;
		string text2 = null;
		string text3 = null;
		if (apiSettings != null)
		{
			if (!string.IsNullOrEmpty(apiSettings.ProductionEnvironmentUrl))
			{
				text = apiSettings.ProductionEnvironmentUrl;
			}
			if (!string.IsNullOrEmpty(apiSettings.VerticalName))
			{
				text2 = apiSettings.VerticalName;
			}
			if (!string.IsNullOrEmpty(apiSettings.TitleId))
			{
				text3 = apiSettings.TitleId;
			}
		}
		if (text == null)
		{
			text = ((!string.IsNullOrEmpty(PlayFabSharedPrivate.ProductionEnvironmentUrl)) ? PlayFabSharedPrivate.ProductionEnvironmentUrl : "playfabapi.com");
		}
		if (text2 == null && apiSettings != null && !string.IsNullOrEmpty(apiSettings.VerticalName))
		{
			text2 = apiSettings.VerticalName;
		}
		if (text3 == null)
		{
			text3 = PlayFabSharedPrivate.TitleId;
		}
		string text4 = text;
		if (!text4.StartsWith("http"))
		{
			stringBuilder.Append("https://");
			if (!string.IsNullOrEmpty(text3))
			{
				stringBuilder.Append(text3).Append(".");
			}
			if (!string.IsNullOrEmpty(text2))
			{
				stringBuilder.Append(text2).Append(".");
			}
		}
		stringBuilder.Append(text4).Append(apiCall);
		if (getParams != null)
		{
			bool flag = true;
			foreach (KeyValuePair<string, string> getParam in getParams)
			{
				if (flag)
				{
					stringBuilder.Append("?");
					flag = false;
				}
				else
				{
					stringBuilder.Append("&");
				}
				stringBuilder.Append(getParam.Key).Append("=").Append(getParam.Value);
			}
		}
		return stringBuilder.ToString();
	}
}
