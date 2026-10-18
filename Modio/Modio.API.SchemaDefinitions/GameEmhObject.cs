using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
internal readonly struct GameEmhObject
{
	internal readonly long Id;

	internal readonly long Status;

	internal readonly long CommunityOptions;

	internal readonly string UgcName;

	internal readonly string Name;

	internal readonly string NameId;

	internal readonly GameTagOptionLocalizedObject[] TagOptions;

	internal readonly ThemeObject Theme;

	internal readonly GamePlatformsObject[] Platforms;

	[JsonConstructor]
	public GameEmhObject(long id, long status, long communityOptions, string ugcName, string name, string nameId, GameTagOptionLocalizedObject[] tagOptions, ThemeObject theme, GamePlatformsObject[] platforms)
	{
		Id = id;
		Status = status;
		CommunityOptions = communityOptions;
		UgcName = ugcName;
		Name = name;
		NameId = nameId;
		TagOptions = tagOptions;
		Theme = theme;
		Platforms = platforms;
	}
}
