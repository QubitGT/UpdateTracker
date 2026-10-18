using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ModUserPreviewObject
{
	internal readonly UserObject User;

	internal readonly UserObject UserFrom;

	internal readonly string ResourceUrl;

	internal readonly bool Subscribed;

	internal readonly long DateAdded;

	[JsonConstructor]
	public ModUserPreviewObject(UserObject user, UserObject user_from, string resource_url, bool subscribed, long date_added)
	{
		User = user;
		UserFrom = user_from;
		ResourceUrl = resource_url;
		Subscribed = subscribed;
		DateAdded = date_added;
	}
}
