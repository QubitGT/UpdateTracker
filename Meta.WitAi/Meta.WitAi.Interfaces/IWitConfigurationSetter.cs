using System;
using Meta.WitAi.Data.Configuration;

namespace Meta.WitAi.Interfaces;

public interface IWitConfigurationSetter
{
	WitConfiguration Configuration { get; set; }

	event Action<WitConfiguration> OnConfigurationUpdated;
}
