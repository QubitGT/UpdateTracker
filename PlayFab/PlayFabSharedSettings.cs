using PlayFab;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayFabSharedSettings", menuName = "PlayFab/CreateSharedSettings", order = 1)]
public class PlayFabSharedSettings : ScriptableObject
{
	public string TitleId;

	internal string VerticalName;

	public string ProductionEnvironmentUrl;

	public WebRequestType RequestType;

	public string AdvertisingIdType;

	public string AdvertisingIdValue;

	public bool DisableAdvertising;

	public bool DisableDeviceInfo;

	public bool DisableFocusTimeCollection;

	public int RequestTimeout = 2000;

	public bool RequestKeepAlive = true;

	public bool CompressApiData = true;

	public PlayFabLogLevel LogLevel = PlayFabLogLevel.Warning | PlayFabLogLevel.Error;

	public string LoggerHost = "";

	public int LoggerPort;

	public bool EnableRealTimeLogging;

	public int LogCapLimit = 30;
}
