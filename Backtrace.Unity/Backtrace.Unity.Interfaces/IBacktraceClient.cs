using System;
using System.Collections.Generic;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Breadcrumbs;

namespace Backtrace.Unity.Interfaces;

public interface IBacktraceClient
{
	IBacktraceBreadcrumbs Breadcrumbs { get; }

	IBacktraceMetrics Metrics { get; }

	void Send(BacktraceReport report, Action<BacktraceResult> sendCallback);

	void Send(string message, List<string> attachmentPaths = null, Dictionary<string, string> attributes = null);

	void Send(Exception exception, List<string> attachmentPaths = null, Dictionary<string, string> attributes = null);

	void SetClientReportLimit(uint reportPerMin);

	void Refresh();

	bool EnableBreadcrumbsSupport();

	bool EnableMetrics();

	bool EnableMetrics(string uniqueEventsSubmissionUrl, string summedEventsSubmissionUrl, uint timeIntervalInSec, string uniqueEventName);
}
