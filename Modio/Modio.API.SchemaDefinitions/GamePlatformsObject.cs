using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GamePlatformsObject
{
	internal readonly string Platform;

	internal readonly string Label;

	internal readonly bool Moderated;

	internal readonly bool Locked;

	[JsonConstructor]
	public GamePlatformsObject(string platform, string label, bool moderated, bool locked)
	{
		Platform = platform;
		Label = label;
		Moderated = moderated;
		Locked = locked;
	}
}
