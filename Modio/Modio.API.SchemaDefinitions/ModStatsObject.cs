using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModStatsObject
{
	internal readonly long ModId;

	internal readonly long PopularityRankPosition;

	internal readonly long PopularityRankTotalMods;

	internal readonly long DownloadsToday;

	internal readonly long DownloadsTotal;

	internal readonly long SubscribersTotal;

	internal readonly long RatingsTotal;

	internal readonly long RatingsPositive;

	internal readonly long RatingsNegative;

	internal readonly long RatingsPercentagePositive;

	internal readonly float RatingsWeightedAggregate;

	internal readonly string RatingsDisplayText;

	internal readonly long DateExpires;

	[JsonConstructor]
	public ModStatsObject(long mod_id, long popularity_rank_position, long popularity_rank_total_mods, long downloads_today, long downloads_total, long subscribers_total, long ratings_total, long ratings_positive, long ratings_negative, long ratings_percentage_positive, float ratings_weighted_aggregate, string ratings_display_text, long date_expires)
	{
		ModId = mod_id;
		PopularityRankPosition = popularity_rank_position;
		PopularityRankTotalMods = popularity_rank_total_mods;
		DownloadsToday = downloads_today;
		DownloadsTotal = downloads_total;
		SubscribersTotal = subscribers_total;
		RatingsTotal = ratings_total;
		RatingsPositive = ratings_positive;
		RatingsNegative = ratings_negative;
		RatingsPercentagePositive = ratings_percentage_positive;
		RatingsWeightedAggregate = ratings_weighted_aggregate;
		RatingsDisplayText = ratings_display_text;
		DateExpires = date_expires;
	}
}
