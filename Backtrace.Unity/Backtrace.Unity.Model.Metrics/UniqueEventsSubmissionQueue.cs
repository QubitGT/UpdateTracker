using System.Collections.Generic;
using Backtrace.Unity.Common;
using Backtrace.Unity.Json;
using Backtrace.Unity.Model.JsonData;

namespace Backtrace.Unity.Model.Metrics;

internal sealed class UniqueEventsSubmissionQueue : MetricsSubmissionQueue<UniqueEvent>
{
	private const string Name = "unique_events";

	private readonly AttributeProvider _attributeProvider;

	public UniqueEventsSubmissionQueue(string submissionUrl, AttributeProvider attributeProvider)
		: base("unique_events", submissionUrl)
	{
		_attributeProvider = attributeProvider;
	}

	public override void StartWithEvent(string eventName)
	{
		IDictionary<string, string> uniqueEventAttributes = GetUniqueEventAttributes();
		if (uniqueEventAttributes.TryGetValue(eventName, out var value) && !string.IsNullOrEmpty(value))
		{
			Events.AddLast(new UniqueEvent(eventName, DateTimeHelper.Timestamp(), uniqueEventAttributes));
		}
		Send();
	}

	internal override IEnumerable<BacktraceJObject> GetEventsPayload(ICollection<UniqueEvent> events)
	{
		List<BacktraceJObject> list = new List<BacktraceJObject>();
		foreach (UniqueEvent @event in events)
		{
			list.Add(@event.ToJson());
			@event.UpdateTimestamp(DateTimeHelper.Timestamp(), GetUniqueEventAttributes());
		}
		return list;
	}

	private IDictionary<string, string> GetUniqueEventAttributes()
	{
		return _attributeProvider.GenerateAttributes(includeDynamic: false);
	}
}
