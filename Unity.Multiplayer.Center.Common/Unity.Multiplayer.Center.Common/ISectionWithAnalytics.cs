using Unity.Multiplayer.Center.Common.Analytics;

namespace Unity.Multiplayer.Center.Common;

public interface ISectionWithAnalytics
{
	IOnboardingSectionAnalyticsProvider AnalyticsProvider { get; set; }
}
