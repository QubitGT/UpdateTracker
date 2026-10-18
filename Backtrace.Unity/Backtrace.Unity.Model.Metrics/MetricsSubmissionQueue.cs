using System;
using System.Collections.Generic;
using System.Linq;
using Backtrace.Unity.Common;
using Backtrace.Unity.Json;
using UnityEngine;

namespace Backtrace.Unity.Model.Metrics;

internal abstract class MetricsSubmissionQueue<T> where T : EventAggregationBase
{
	public const int DefaultTimeInSecBetweenRequests = 10;

	private readonly string _name;

	private readonly List<MetricsSubmissionJob<T>> _submissionJobs = new List<MetricsSubmissionJob<T>>();

	internal LinkedList<T> Events = new LinkedList<T>();

	private int _numberOfDroppedRequests;

	internal IBacktraceHttpClient RequestHandler = new BacktraceHttpClient();

	private readonly string _applicationName = Application.productName;

	private readonly string _applicationVersion = Application.version;

	public int Count => Events.Count;

	public uint MaximumEvents { get; set; }

	internal string SubmissionUrl { get; set; }

	internal MetricsSubmissionQueue(string name, string submissionUrl)
	{
		_name = name;
		SubmissionUrl = submissionUrl;
		MaximumEvents = 50u;
	}

	public bool ReachedLimit()
	{
		if (MaximumEvents == Events.Count)
		{
			return MaximumEvents != 0;
		}
		return false;
	}

	public bool ShouldProcessEvent(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogWarning("Skipping report: attribute name is null or empty");
			return false;
		}
		if (ReachedLimit())
		{
			Debug.LogWarning("Skipping report: Reached store limit.");
			return false;
		}
		return true;
	}

	public abstract void StartWithEvent(string eventName);

	internal void Send()
	{
		SendPayload(new LinkedList<T>(Events));
	}

	internal void SendPayload(ICollection<T> events, uint attempts = 0u)
	{
		if (events.Count == 0)
		{
			return;
		}
		BacktraceJObject jObject = CreateJsonPayload(events);
		RequestHandler.Post(SubmissionUrl, jObject, delegate(long statusCode, bool httpError, string response)
		{
			if (statusCode == 200)
			{
				OnRequestCompleted();
			}
			else if (httpError || (statusCode > 501 && statusCode != 505))
			{
				_numberOfDroppedRequests++;
				if (attempts + 1 == 3)
				{
					OnMaximumAttemptsReached(events);
				}
				else
				{
					_submissionJobs.Add(new MetricsSubmissionJob<T>
					{
						Events = events,
						NextInvokeTime = CalculateNextRetryTime(attempts + 1) + (double)Time.unscaledTime,
						NumberOfAttempts = attempts + 1
					});
				}
			}
		});
	}

	public void SendPendingEvents(float time)
	{
		int num = 0;
		while (num < _submissionJobs.Count)
		{
			MetricsSubmissionJob<T> metricsSubmissionJob = _submissionJobs.ElementAt(num);
			if (metricsSubmissionJob.NextInvokeTime < (double)time)
			{
				SendPayload(metricsSubmissionJob.Events, metricsSubmissionJob.NumberOfAttempts);
				_submissionJobs.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	internal virtual void OnMaximumAttemptsReached(ICollection<T> events)
	{
	}

	internal abstract IEnumerable<BacktraceJObject> GetEventsPayload(ICollection<T> events);

	internal virtual BacktraceJObject CreateJsonPayload(ICollection<T> events)
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		backtraceJObject.Add("application", _applicationName);
		backtraceJObject.Add("appversion", _applicationVersion);
		backtraceJObject.Add("metadata", CreatePayloadMetadata());
		backtraceJObject.Add(_name, GetEventsPayload(events));
		return backtraceJObject;
	}

	private double CalculateNextRetryTime(uint attemps)
	{
		double num = MathHelper.Clamp(10.0 * Math.Pow(10.0, attemps), 0.0, 300.0);
		double maximum = num + num * 1.0;
		return MathHelper.Uniform(num, maximum);
	}

	private BacktraceJObject CreatePayloadMetadata()
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		backtraceJObject.Add("dropped_events", _numberOfDroppedRequests);
		return backtraceJObject;
	}

	private void OnRequestCompleted()
	{
		_numberOfDroppedRequests = 0;
	}
}
