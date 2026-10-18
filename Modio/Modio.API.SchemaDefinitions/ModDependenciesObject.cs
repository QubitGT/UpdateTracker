using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModDependenciesObject
{
	internal readonly long Id;

	internal readonly long GameId;

	internal readonly long Status;

	internal readonly long Visible;

	internal readonly UserObject SubmittedBy;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly long DateLive;

	internal readonly long MaturityOption;

	internal readonly long CommunityOptions;

	internal readonly long MonetizationOptions;

	internal readonly long Stock;

	internal readonly long Price;

	internal readonly long Tax;

	internal readonly LogoObject Logo;

	internal readonly string HomepageUrl;

	internal readonly string Name;

	internal readonly string NameId;

	internal readonly string Summary;

	internal readonly string Description;

	internal readonly string DescriptionPlaintext;

	internal readonly string MetadataBlob;

	internal readonly string ProfileUrl;

	internal readonly ModMediaObject Media;

	internal readonly ModfileObject Modfile;

	internal readonly bool Dependencies;

	internal readonly ModPlatformsObject[] Platforms;

	internal readonly MetadataKvpObject[] MetadataKvp;

	internal readonly ModTagObject[] Tags;

	internal readonly ModStatsObject Stats;

	internal readonly long DependencyDepth;

	[JsonConstructor]
	public ModDependenciesObject(long id, long game_id, long status, long visible, UserObject submitted_by, long date_added, long date_updated, long date_live, long maturity_option, long community_options, long monetization_options, long stock, long price, long tax, LogoObject logo, string homepage_url, string name, string name_id, string summary, string description, string description_plaintext, string metadata_blob, string profile_url, ModMediaObject media, ModfileObject modfile, bool dependencies, ModPlatformsObject[] platforms, MetadataKvpObject[] metadata_kvp, ModTagObject[] tags, ModStatsObject stats, long dependency_depth)
	{
		Id = id;
		GameId = game_id;
		Status = status;
		Visible = visible;
		SubmittedBy = submitted_by;
		DateAdded = date_added;
		DateUpdated = date_updated;
		DateLive = date_live;
		MaturityOption = maturity_option;
		CommunityOptions = community_options;
		MonetizationOptions = monetization_options;
		Stock = stock;
		Price = price;
		Tax = tax;
		Logo = logo;
		HomepageUrl = homepage_url;
		Name = name;
		NameId = name_id;
		Summary = summary;
		Description = description;
		DescriptionPlaintext = description_plaintext;
		MetadataBlob = metadata_blob;
		ProfileUrl = profile_url;
		Media = media;
		Modfile = modfile;
		Dependencies = dependencies;
		Platforms = platforms;
		MetadataKvp = metadata_kvp;
		Tags = tags;
		Stats = stats;
		DependencyDepth = dependency_depth;
	}
}
