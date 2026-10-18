using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct PreviewObject
{
	internal readonly string ResourceUrl;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	[JsonConstructor]
	public PreviewObject(string resource_url, long date_added, long date_updated)
	{
		ResourceUrl = resource_url;
		DateAdded = date_added;
		DateUpdated = date_updated;
	}
}
