using System;
using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi.Composer.Data.Info;

public static class WitAppInfoComposerExtensions
{
	public static WitComposerData Composer(this IWitRequestConfiguration configuration)
	{
		return (WitComposerData)Array.Find(configuration.GetConfigData(), (WitConfigurationAssetData d) => d is WitComposerData);
	}
}
