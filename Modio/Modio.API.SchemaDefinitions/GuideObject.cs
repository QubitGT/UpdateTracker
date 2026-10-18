using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GuideObject
{
	internal readonly long Id;

	internal readonly long GameId;

	internal readonly string GameName;

	internal readonly LogoObject Logo;

	internal readonly UserObject User;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly long DateLive;

	internal readonly long Status;

	internal readonly string Url;

	internal readonly string Name;

	internal readonly string NameId;

	internal readonly string Summary;

	internal readonly string Description;

	internal readonly long CommunityOptions;

	internal readonly GuideTagObject[] Tags;

	internal readonly GuideStatsObject[] Stats;

	[JsonConstructor]
	public GuideObject(long id, long game_id, string game_name, LogoObject logo, UserObject user, long date_added, long date_updated, long date_live, long status, string url, string name, string name_id, string summary, string description, long community_options, GuideTagObject[] tags, GuideStatsObject[] stats)
	{
		Id = id;
		GameId = game_id;
		GameName = game_name;
		Logo = logo;
		User = user;
		DateAdded = date_added;
		DateUpdated = date_updated;
		DateLive = date_live;
		Status = status;
		Url = url;
		Name = name;
		NameId = name_id;
		Summary = summary;
		Description = description;
		CommunityOptions = community_options;
		Tags = tags;
		Stats = stats;
	}
}
