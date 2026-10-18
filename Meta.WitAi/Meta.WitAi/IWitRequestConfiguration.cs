using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Data.Info;

namespace Meta.WitAi;

public interface IWitRequestConfiguration
{
	WitRequestType RequestType { get; }

	int RequestTimeoutMs { get; }

	string GetConfigurationId();

	string GetVersionTag();

	string GetApplicationId();

	WitAppInfo GetApplicationInfo();

	WitConfigurationAssetData[] GetConfigData();

	IWitRequestEndpointInfo GetEndpointInfo();

	string GetClientAccessToken();

	void UpdateDataAssets();
}
