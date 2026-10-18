using System.Collections.Generic;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.Metrics;

public abstract class EventAggregationBase
{
	private const string TimestampName = "timestamp";

	private const string AttributesName = "attributes";

	public long Timestamp { get; set; }

	public string Name { get; private set; }

	public EventAggregationBase(string name, long timestamp)
	{
		Name = name;
		Timestamp = timestamp;
	}

	internal BacktraceJObject ToBaseObject(IDictionary<string, string> attributes)
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		backtraceJObject.Add("timestamp", Timestamp);
		backtraceJObject.Add("attributes", new BacktraceJObject(attributes));
		return backtraceJObject;
	}
}
