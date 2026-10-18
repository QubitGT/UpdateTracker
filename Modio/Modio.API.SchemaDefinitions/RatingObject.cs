using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct RatingObject
{
	internal readonly long GameId;

	internal readonly long ModId;

	internal readonly long Rating;

	internal readonly long DateAdded;

	[JsonConstructor]
	public RatingObject(long game_id, long mod_id, long rating, long date_added)
	{
		GameId = game_id;
		ModId = mod_id;
		Rating = rating;
		DateAdded = date_added;
	}
}
