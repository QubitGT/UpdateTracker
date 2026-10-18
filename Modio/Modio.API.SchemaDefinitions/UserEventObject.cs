using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UserEventObject
{
	internal readonly long Id;

	internal readonly long GameId;

	internal readonly long ModId;

	internal readonly long UserId;

	internal readonly long DateAdded;

	internal readonly string EventType;

	[JsonConstructor]
	public UserEventObject(long id, long game_id, long mod_id, long user_id, long date_added, string event_type)
	{
		Id = id;
		GameId = game_id;
		ModId = mod_id;
		UserId = user_id;
		DateAdded = date_added;
		EventType = event_type;
	}
}
