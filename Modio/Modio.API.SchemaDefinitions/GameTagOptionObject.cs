using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameTagOptionObject
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal readonly struct EmbeddedTagsLocalization
	{
		internal readonly string Tag;

		internal readonly Dictionary<string, string> Translations;

		[JsonConstructor]
		public EmbeddedTagsLocalization(string tag, Dictionary<string, string> translations)
		{
			Tag = tag;
			Translations = translations;
		}
	}

	internal readonly string Name;

	internal readonly Dictionary<string, string> NameLocalization;

	internal readonly string Type;

	internal readonly string[] Tags;

	internal readonly EmbeddedTagsLocalization[] TagsLocalization;

	internal readonly Dictionary<string, int> TagCountMap;

	internal readonly bool Hidden;

	internal readonly bool Locked;

	[JsonConstructor]
	public GameTagOptionObject(string name, Dictionary<string, string> name_localization, string type, string[] tags, EmbeddedTagsLocalization[] tags_localization, Dictionary<string, int> tag_count_map, bool hidden, bool locked)
	{
		Name = name;
		NameLocalization = name_localization;
		Type = type;
		Tags = tags;
		TagsLocalization = tags_localization;
		TagCountMap = tag_count_map;
		Hidden = hidden;
		Locked = locked;
	}
}
