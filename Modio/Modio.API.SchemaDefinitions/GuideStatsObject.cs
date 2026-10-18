using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GuideStatsObject
{
	internal readonly long GuideId;

	internal readonly long VisitsToday;

	internal readonly long VisitsTotal;

	internal readonly long CommentsTotal;

	[JsonConstructor]
	public GuideStatsObject(long guide_id, long visits_today, long visits_total, long comments_total)
	{
		GuideId = guide_id;
		VisitsToday = visits_today;
		VisitsTotal = visits_total;
		CommentsTotal = comments_total;
	}
}
