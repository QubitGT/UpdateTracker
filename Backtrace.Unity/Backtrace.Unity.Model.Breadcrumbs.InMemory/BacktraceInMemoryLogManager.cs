using System.Collections.Generic;
using Backtrace.Unity.Common;

namespace Backtrace.Unity.Model.Breadcrumbs.InMemory;

internal sealed class BacktraceInMemoryLogManager : IBacktraceLogManager
{
	public const int DefaultMaximumNumberOfInMemoryBreadcrumbs = 100;

	private object _lockObject = new object();

	internal readonly Queue<InMemoryBreadcrumb> Breadcrumbs;

	private double _breadcrumbId;

	public int MaximumNumberOfBreadcrumbs { get; set; }

	public string BreadcrumbsFilePath => string.Empty;

	public BacktraceInMemoryLogManager()
	{
		_breadcrumbId = DateTimeHelper.TimestampMs();
		MaximumNumberOfBreadcrumbs = 100;
		Breadcrumbs = new Queue<InMemoryBreadcrumb>(100);
	}

	public bool Add(string message, BreadcrumbLevel type, UnityEngineLogLevel level, IDictionary<string, string> attributes)
	{
		lock (_lockObject)
		{
			if (Breadcrumbs.Count + 1 > MaximumNumberOfBreadcrumbs)
			{
				while (Breadcrumbs.Count + 1 > MaximumNumberOfBreadcrumbs)
				{
					Breadcrumbs.Dequeue();
				}
			}
		}
		Breadcrumbs.Enqueue(new InMemoryBreadcrumb
		{
			Message = message,
			Timestamp = DateTimeHelper.TimestampMs(),
			Level = level,
			Type = type,
			Attributes = attributes
		});
		_breadcrumbId += 1.0;
		return true;
	}

	public bool Clear()
	{
		Breadcrumbs.Clear();
		return true;
	}

	public bool Enable()
	{
		return true;
	}

	public int Length()
	{
		return Breadcrumbs.Count;
	}

	public double BreadcrumbId()
	{
		return _breadcrumbId;
	}
}
