using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameMonetizationTeamObject
{
	internal readonly long TeamId;

	[JsonConstructor]
	public GameMonetizationTeamObject(long team_id)
	{
		TeamId = team_id;
	}
}
