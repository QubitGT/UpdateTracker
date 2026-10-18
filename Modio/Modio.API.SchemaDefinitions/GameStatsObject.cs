using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GameStatsObject
{
	internal readonly long GameId;

	internal readonly long ModsCountTotal;

	internal readonly long ModsDownloadsToday;

	internal readonly long ModsDownloadsTotal;

	internal readonly long ModsDownloadsDailyAverage;

	internal readonly long ModsSubscribersTotal;

	internal readonly long DateExpires;

	[JsonConstructor]
	public GameStatsObject(long game_id, long mods_count_total, long mods_downloads_today, long mods_downloads_total, long mods_downloads_daily_average, long mods_subscribers_total, long date_expires)
	{
		GameId = game_id;
		ModsCountTotal = mods_count_total;
		ModsDownloadsToday = mods_downloads_today;
		ModsDownloadsTotal = mods_downloads_total;
		ModsDownloadsDailyAverage = mods_downloads_daily_average;
		ModsSubscribersTotal = mods_subscribers_total;
		DateExpires = date_expires;
	}
}
