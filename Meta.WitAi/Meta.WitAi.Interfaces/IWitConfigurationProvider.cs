using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi.Interfaces;

public interface IWitConfigurationProvider
{
	WitConfiguration Configuration { get; }
}
