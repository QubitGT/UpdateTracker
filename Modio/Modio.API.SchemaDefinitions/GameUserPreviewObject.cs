using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameUserPreviewObject
{
	internal readonly UserObject User;

	internal readonly UserObject UserFrom;

	internal readonly string ResourceUrl;

	internal readonly long DateAdded;

	[JsonConstructor]
	public GameUserPreviewObject(UserObject user, UserObject user_from, string resource_url, long date_added)
	{
		User = user;
		UserFrom = user_from;
		ResourceUrl = resource_url;
		DateAdded = date_added;
	}
}
