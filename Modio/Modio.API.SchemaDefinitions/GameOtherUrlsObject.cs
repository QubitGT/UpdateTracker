using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameOtherUrlsObject
{
	internal readonly string Label;

	internal readonly string Url;

	[JsonConstructor]
	public GameOtherUrlsObject(string label, string url)
	{
		Label = label;
		Url = url;
	}
}
