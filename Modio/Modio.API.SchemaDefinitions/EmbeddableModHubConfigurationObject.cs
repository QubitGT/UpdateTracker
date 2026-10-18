using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct EmbeddableModHubConfigurationObject
{
	internal readonly long Id;

	internal readonly string Name;

	internal readonly string[] Urls;

	internal readonly string Style;

	internal readonly string Css;

	internal readonly bool AllowSubscribing;

	internal readonly bool AllowRating;

	internal readonly bool AllowReporting;

	internal readonly bool AllowDownloading;

	internal readonly bool AllowCommenting;

	internal readonly bool AllowFiltering;

	internal readonly bool AllowSearching;

	internal readonly bool AllowInfiniteScroll;

	internal readonly bool AllowEmailAuth;

	internal readonly bool AllowSsoAuth;

	internal readonly bool AllowSteamAuth;

	internal readonly bool AllowPsnAuth;

	internal readonly bool AllowXboxAuth;

	internal readonly bool AllowEgsAuth;

	internal readonly bool AllowDiscordAuth;

	internal readonly bool AllowGoogleAuth;

	internal readonly bool ShowCollection;

	internal readonly bool ShowComments;

	internal readonly bool ShowGuides;

	internal readonly bool ShowUserAvatars;

	internal readonly bool ShowSortTabs;

	internal readonly bool AllowLinks;

	internal readonly bool FilterRightSide;

	internal readonly bool NameRightSide;

	internal readonly long ResultsPerPage;

	internal readonly long MinAge;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly string CompanyName;

	internal readonly object[] AgreementUrls;

	[JsonConstructor]
	public EmbeddableModHubConfigurationObject(long id, string name, string[] urls, string style, string css, bool allow_subscribing, bool allow_rating, bool allow_reporting, bool allow_downloading, bool allow_commenting, bool allow_filtering, bool allow_searching, bool allow_infinite_scroll, bool allow_email_auth, bool allow_sso_auth, bool allow_steam_auth, bool allow_PSN_auth, bool allow_xbox_auth, bool allow_egs_auth, bool allow_discord_auth, bool allow_google_auth, bool show_collection, bool show_comments, bool show_guides, bool show_user_avatars, bool show_sort_tabs, bool allow_links, bool filter_right_side, bool name_right_side, long results_per_page, long min_age, long date_added, long date_updated, string company_name, object[] agreement_urls)
	{
		Id = id;
		Name = name;
		Urls = urls;
		Style = style;
		Css = css;
		AllowSubscribing = allow_subscribing;
		AllowRating = allow_rating;
		AllowReporting = allow_reporting;
		AllowDownloading = allow_downloading;
		AllowCommenting = allow_commenting;
		AllowFiltering = allow_filtering;
		AllowSearching = allow_searching;
		AllowInfiniteScroll = allow_infinite_scroll;
		AllowEmailAuth = allow_email_auth;
		AllowSsoAuth = allow_sso_auth;
		AllowSteamAuth = allow_steam_auth;
		AllowPsnAuth = allow_PSN_auth;
		AllowXboxAuth = allow_xbox_auth;
		AllowEgsAuth = allow_egs_auth;
		AllowDiscordAuth = allow_discord_auth;
		AllowGoogleAuth = allow_google_auth;
		ShowCollection = show_collection;
		ShowComments = show_comments;
		ShowGuides = show_guides;
		ShowUserAvatars = show_user_avatars;
		ShowSortTabs = show_sort_tabs;
		AllowLinks = allow_links;
		FilterRightSide = filter_right_side;
		NameRightSide = name_right_side;
		ResultsPerPage = results_per_page;
		MinAge = min_age;
		DateAdded = date_added;
		DateUpdated = date_updated;
		CompanyName = company_name;
		AgreementUrls = agreement_urls;
	}
}
