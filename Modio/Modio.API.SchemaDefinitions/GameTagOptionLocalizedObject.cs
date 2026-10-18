using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameTagOptionLocalizedObject
{
	internal readonly string Name;

	internal readonly string NameLocalized;

	internal readonly string Type;

	internal readonly string[] Tags;

	internal readonly JObject TagsLocalized;

	internal readonly JObject TagCountMap;

	internal readonly bool Hidden;

	internal readonly bool Locked;

	[JsonConstructor]
	public GameTagOptionLocalizedObject(string name, string name_localized, string type, string[] tags, JObject tags_localized, JObject tag_count_map, bool hidden, bool locked)
	{
		Name = name;
		NameLocalized = name_localized;
		Type = type;
		Tags = tags;
		TagsLocalized = tags_localized;
		TagCountMap = tag_count_map;
		Hidden = hidden;
		Locked = locked;
	}
}
