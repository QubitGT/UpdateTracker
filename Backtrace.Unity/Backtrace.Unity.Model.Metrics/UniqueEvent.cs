using System.Collections.Generic;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.Metrics;

public sealed class UniqueEvent : EventAggregationBase
{
	internal const string UniqueEventName = "unique";

	internal IDictionary<string, string> Attributes;

	internal UniqueEvent(string name, long timestamp, IDictionary<string, string> attributes)
		: base(name, timestamp)
	{
		Attributes = attributes;
	}

	internal void UpdateTimestamp(long timestamp, IDictionary<string, string> attributes)
	{
		base.Timestamp = timestamp;
		if (attributes != null && attributes.TryGetValue(base.Name, out var value) && !string.IsNullOrEmpty(value))
		{
			Attributes = attributes;
		}
	}

	internal BacktraceJObject ToJson()
	{
		BacktraceJObject backtraceJObject = ToBaseObject(Attributes);
		backtraceJObject.Add("unique", new string[1] { base.Name });
		return backtraceJObject;
	}
}
