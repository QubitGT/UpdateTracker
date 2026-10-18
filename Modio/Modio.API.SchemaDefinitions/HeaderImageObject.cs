using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct HeaderImageObject
{
	internal readonly string Filename;

	internal readonly string Original;

	[JsonConstructor]
	public HeaderImageObject(string filename, string original)
	{
		Filename = filename;
		Original = original;
	}
}
