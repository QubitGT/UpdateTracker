using Meta.WitAi.Configuration;

namespace Meta.WitAi;

public interface IWitRuntimeConfigProvider
{
	WitRuntimeConfiguration RuntimeConfiguration { get; }
}
