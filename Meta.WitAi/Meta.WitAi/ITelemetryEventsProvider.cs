using Meta.WitAi.Events;

namespace Meta.WitAi;

public interface ITelemetryEventsProvider
{
	TelemetryEvents TelemetryEvents { get; }
}
