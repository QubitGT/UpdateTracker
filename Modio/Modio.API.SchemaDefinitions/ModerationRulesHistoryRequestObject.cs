using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ModerationRulesHistoryRequestObject
{
	internal readonly string Timeframe;

	[JsonConstructor]
	public ModerationRulesHistoryRequestObject(string timeframe)
	{
		Timeframe = timeframe;
	}
}
