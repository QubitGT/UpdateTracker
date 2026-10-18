using System.Collections.Generic;
using System.Linq;
using Liv.Lck.Core;

namespace Liv.Lck.Telemetry;

public class LckTelemetryEvent
{
	public LckTelemetryEventType EventType { get; set; }

	public Dictionary<string, object> Context { get; set; }

	public LckTelemetryEvent(LckTelemetryEventType eventType)
	{
		EventType = eventType;
	}

	public LckTelemetryEvent(LckTelemetryEventType eventType, Dictionary<string, object> context)
	{
		EventType = eventType;
		Context = context;
	}

	public override string ToString()
	{
		string text = string.Join(", ", Context.Select((KeyValuePair<string, object> kvp) => $"{kvp.Key}: {kvp.Value}"));
		return string.Format("{0}={1} | {2}={{{3}}}", "EventType", EventType, "Context", text);
	}
}
