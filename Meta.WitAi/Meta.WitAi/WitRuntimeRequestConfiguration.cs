using System;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Data.Info;

namespace Meta.WitAi;

public class WitRuntimeRequestConfiguration : IWitRequestConfiguration, IWitRequestEndpointInfo
{
	private WitAppInfo _appInfo;

	private string _userToken;

	private WitConfigurationAssetData[] _configurationData = Array.Empty<WitConfigurationAssetData>();

	public WitRequestType RequestType { get; set; }

	public int RequestTimeoutMs => 10000;

	public string UriScheme => "https";

	public string Authority => "graph.wit.ai/myprofile";

	public string WitApiVersion => "20250213";

	public int Port => -1;

	public string Message => "message";

	public string Speech => "speech";

	public string Dictation => "dictation";

	public string Synthesize => "synthesize";

	public string Event => "event";

	public string Converse => "converse";

	public WitRuntimeRequestConfiguration(string userToken)
	{
		_userToken = userToken;
		_appInfo = default(WitAppInfo);
	}

	public string GetConfigurationId()
	{
		return null;
	}

	public string GetApplicationId()
	{
		return _appInfo.id;
	}

	public WitAppInfo GetApplicationInfo()
	{
		return _appInfo;
	}

	public WitConfigurationAssetData[] GetConfigData()
	{
		return _configurationData;
	}

	public IWitRequestEndpointInfo GetEndpointInfo()
	{
		return this;
	}

	public string GetClientAccessToken()
	{
		return _userToken;
	}

	public void SetClientAccessToken(string newToken)
	{
		_userToken = newToken;
	}

	public string GetServerAccessToken()
	{
		throw new NotImplementedException();
	}

	public string GetVersionTag()
	{
		return string.Empty;
	}

	public void SetApplicationInfo(WitAppInfo newInfo)
	{
		_appInfo = newInfo;
	}

	public void SetConfigData(WitConfigurationAssetData[] configData)
	{
		_configurationData = configData;
	}

	public void UpdateDataAssets()
	{
	}
}
