using System.Collections.Generic;

namespace PlayFab;

public class PlayFabApiSettings
{
	private string _ProductionEnvironmentUrl = "playfabapi.com";

	public readonly Dictionary<string, string> _requestGetParams = new Dictionary<string, string> { { "sdk", "UnitySDK-2.87.200602" } };

	public virtual Dictionary<string, string> RequestGetParams => _requestGetParams;

	public virtual string ProductionEnvironmentUrl
	{
		get
		{
			return _ProductionEnvironmentUrl;
		}
		set
		{
			_ProductionEnvironmentUrl = value;
		}
	}

	public virtual string TitleId { get; set; }

	internal virtual string VerticalName { get; set; }

	public virtual string AdvertisingIdType { get; set; }

	public virtual string AdvertisingIdValue { get; set; }

	public virtual bool DisableAdvertising { get; set; }

	public virtual bool DisableDeviceInfo { get; set; }

	public virtual bool DisableFocusTimeCollection { get; set; }

	public virtual string GetFullUrl(string apiCall, Dictionary<string, string> getParams)
	{
		return PlayFabSettings.GetFullUrl(apiCall, getParams, this);
	}
}
