using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct DownloadObject
{
	internal readonly string BinaryUrl;

	internal readonly long DateExpires;

	[JsonConstructor]
	internal DownloadObject(string binary_url, long date_expires)
	{
		BinaryUrl = binary_url;
		DateExpires = date_expires;
	}
}
