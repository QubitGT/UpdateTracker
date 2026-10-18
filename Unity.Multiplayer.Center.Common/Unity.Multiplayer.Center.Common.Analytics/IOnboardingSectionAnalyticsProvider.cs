namespace Unity.Multiplayer.Center.Common.Analytics;

public interface IOnboardingSectionAnalyticsProvider
{
	void SendInteractionEvent(InteractionDataType type, string displayName);
}
