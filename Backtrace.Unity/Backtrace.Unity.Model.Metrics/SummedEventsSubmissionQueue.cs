using System.Collections.Generic;
using Backtrace.Unity.Json;
using Backtrace.Unity.Model.JsonData;

namespace Backtrace.Unity.Model.Metrics;

internal sealed class SummedEventsSubmissionQueue : MetricsSubmissionQueue<SummedEvent>
{
	private const string Name = "summed_events";

	private readonly AttributeProvider _attributeProvider;

	public SummedEventsSubmissionQueue(string submissionUrl, AttributeProvider attributeProvider)
		: base("summed_events", submissionUrl)
	{
		_attributeProvider = attributeProvider;
	}

	public override void StartWithEvent(string eventName)
	{
		Events.AddLast(new SummedEvent(eventName));
		Send();
	}

	internal override IEnumerable<BacktraceJObject> GetEventsPayload(ICollection<SummedEvent> events)
	{
		List<BacktraceJObject> list = new List<BacktraceJObject>();
		IDictionary<string, string> scopedAttributes = _attributeProvider.GenerateAttributes(includeDynamic: false);
		foreach (SummedEvent @event in events)
		{
			list.Add(@event.ToJson(scopedAttributes));
		}
		Events.Clear();
		return list;
	}

	internal override void OnMaximumAttemptsReached(ICollection<SummedEvent> events)
	{
		if (base.Count + events.Count >= base.MaximumEvents)
		{
			return;
		}
		foreach (SummedEvent @event in events)
		{
			Events.AddFirst(@event);
		}
	}
}
