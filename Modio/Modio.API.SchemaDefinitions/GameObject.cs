using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
internal readonly struct GameObject
{
	internal readonly long Id;

	internal readonly long Status;

	internal readonly JObject SubmittedBy;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly long DateLive;

	internal readonly long PresentationOption;

	internal readonly long SubmissionOption;

	internal readonly long DependencyOption;

	internal readonly long CurationOption;

	internal readonly long CommunityOptions;

	internal readonly long MonetizationOptions;

	internal readonly GameMonetizationTeamObject MonetizationTeam;

	internal readonly long RevenueOptions;

	internal readonly long MaxStock;

	internal readonly long ApiAccessOptions;

	internal readonly long MaturityOptions;

	internal readonly string UgcName;

	internal readonly string TokenName;

	internal readonly IconObject Icon;

	internal readonly LogoObject Logo;

	internal readonly HeaderImageObject Header;

	internal readonly string Name;

	internal readonly string NameId;

	internal readonly string Summary;

	internal readonly string Instructions;

	internal readonly string InstructionsUrl;

	internal readonly string ProfileUrl;

	internal readonly GameOtherUrlsObject[] OtherUrls;

	internal readonly GameTagOptionLocalizedObject[] TagOptions;

	internal readonly GameStatsObject Stats;

	internal readonly ThemeObject Theme;

	internal readonly GamePlatformsObject[] Platforms;

	[JsonConstructor]
	public GameObject(long id, long status, JObject submittedBy, long dateAdded, long dateUpdated, long dateLive, long presentationOption, long submissionOption, long dependencyOption, long curationOption, long communityOptions, long monetizationOptions, GameMonetizationTeamObject monetizationTeam, long revenueOptions, long maxStock, long apiAccessOptions, long maturityOptions, string ugcName, string tokenName, IconObject icon, LogoObject logo, HeaderImageObject header, string name, string nameId, string summary, string instructions, string instructionsUrl, string profileUrl, GameOtherUrlsObject[] otherUrls, GameTagOptionLocalizedObject[] tagOptions, GameStatsObject stats, ThemeObject theme, GamePlatformsObject[] platforms)
	{
		Id = id;
		Status = status;
		SubmittedBy = submittedBy;
		DateAdded = dateAdded;
		DateUpdated = dateUpdated;
		DateLive = dateLive;
		PresentationOption = presentationOption;
		SubmissionOption = submissionOption;
		DependencyOption = dependencyOption;
		CurationOption = curationOption;
		CommunityOptions = communityOptions;
		MonetizationOptions = monetizationOptions;
		MonetizationTeam = monetizationTeam;
		RevenueOptions = revenueOptions;
		MaxStock = maxStock;
		ApiAccessOptions = apiAccessOptions;
		MaturityOptions = maturityOptions;
		UgcName = ugcName;
		TokenName = tokenName;
		Icon = icon;
		Logo = logo;
		Header = header;
		Name = name;
		NameId = nameId;
		Summary = summary;
		Instructions = instructions;
		InstructionsUrl = instructionsUrl;
		ProfileUrl = profileUrl;
		OtherUrls = otherUrls;
		TagOptions = tagOptions;
		Stats = stats;
		Theme = theme;
		Platforms = platforms;
	}
}
