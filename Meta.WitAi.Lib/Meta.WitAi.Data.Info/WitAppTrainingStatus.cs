using Meta.WitAi.Json;

namespace Meta.WitAi.Data.Info;

public enum WitAppTrainingStatus
{
	Unknown,
	[JsonProperty("done")]
	Done,
	[JsonProperty("scheduled")]
	Scheduled,
	[JsonProperty("ongoing")]
	Ongoing
}
