using System.Collections.Generic;
using Backtrace.Unity.Model.Attributes;

namespace Backtrace.Unity.Interfaces;

public interface IBacktraceMetrics : IScopeAttributeProvider
{
	uint MaximumSummedEvents { get; set; }

	uint MaximumUniqueEvents { get; set; }

	string UniqueEventsSubmissionUrl { get; set; }

	string SummedEventsSubmissionUrl { get; set; }

	void Send();

	bool AddSummedEvent(string metricsGroupName);

	bool AddSummedEvent(string metricsGroupName, IDictionary<string, string> attributes);
}
