using System;
using System.Collections.Generic;
using System.Linq;
using Backtrace.Unity.Common;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Attributes;
using Backtrace.Unity.Model.JsonData;
using Backtrace.Unity.Model.Metrics;
using UnityEngine;

namespace Backtrace.Unity.Services;

internal sealed class BacktraceMetrics : IBacktraceMetrics, IScopeAttributeProvider
{
	public readonly Guid SessionId = Guid.NewGuid();

	public const string DefaultSubmissionUrl = "https://events.backtrace.io/api/";

	public const uint DefaultTimeIntervalInMin = 30u;

	public const uint DefaultTimeIntervalInSec = 1800u;

	public const string DefaultUniqueAttributeName = "guid";

	public const int MaxTimeBetweenRequests = 300;

	public const int MaxNumberOfAttempts = 3;

	internal const string ApplicationSessionKey = "application.session";

	internal readonly UniqueEventsSubmissionQueue _uniqueEventsSubmissionQueue;

	internal readonly SummedEventsSubmissionQueue _summedEventsSubmissionQueue;

	private const string StartupEventName = "Application Launches";

	private readonly long _timeIntervalInSec;

	private float _lastUpdateTime;

	private readonly AttributeProvider _attributeProvider;

	private object _object = new object();

	private readonly string _sessionId;

	public string StartupUniqueAttributeName { get; set; }

	public uint MaximumUniqueEvents
	{
		get
		{
			return _uniqueEventsSubmissionQueue.MaximumEvents;
		}
		set
		{
			_uniqueEventsSubmissionQueue.MaximumEvents = value;
		}
	}

	public uint MaximumSummedEvents
	{
		get
		{
			return _summedEventsSubmissionQueue.MaximumEvents;
		}
		set
		{
			_summedEventsSubmissionQueue.MaximumEvents = value;
		}
	}

	public string UniqueEventsSubmissionUrl
	{
		get
		{
			return _uniqueEventsSubmissionQueue.SubmissionUrl;
		}
		set
		{
			_uniqueEventsSubmissionQueue.SubmissionUrl = value;
		}
	}

	public string SummedEventsSubmissionUrl
	{
		get
		{
			return _summedEventsSubmissionQueue.SubmissionUrl;
		}
		set
		{
			_summedEventsSubmissionQueue.SubmissionUrl = value;
		}
	}

	public bool IgnoreSslValidation
	{
		set
		{
			_uniqueEventsSubmissionQueue.RequestHandler.IgnoreSslValidation = value;
			_summedEventsSubmissionQueue.RequestHandler.IgnoreSslValidation = value;
		}
	}

	public LinkedList<UniqueEvent> UniqueEvents => _uniqueEventsSubmissionQueue.Events;

	internal LinkedList<SummedEvent> SummedEvents => _summedEventsSubmissionQueue.Events;

	public BacktraceMetrics(AttributeProvider attributeProvider, long timeIntervalInSec, string uniqueEventsSubmissionUrl, string summedEventsSubmissionUrl)
	{
		_attributeProvider = attributeProvider;
		_timeIntervalInSec = timeIntervalInSec;
		_uniqueEventsSubmissionQueue = new UniqueEventsSubmissionQueue(uniqueEventsSubmissionUrl, _attributeProvider);
		_summedEventsSubmissionQueue = new SummedEventsSubmissionQueue(summedEventsSubmissionUrl, _attributeProvider);
		_sessionId = SessionId.ToString();
		StartupUniqueAttributeName = "guid";
	}

	internal void OverrideHttpClient(IBacktraceHttpClient client)
	{
		_uniqueEventsSubmissionQueue.RequestHandler = client;
		_summedEventsSubmissionQueue.RequestHandler = client;
	}

	public void SendStartupEvent()
	{
		_uniqueEventsSubmissionQueue.StartWithEvent(StartupUniqueAttributeName);
		_summedEventsSubmissionQueue.StartWithEvent("Application Launches");
	}

	public void Tick(float time)
	{
		lock (_object)
		{
			SendPendingSubmissionJobs(time);
		}
		if (_timeIntervalInSec == 0L)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		lock (_object)
		{
			flag3 = time - _lastUpdateTime >= (float)_timeIntervalInSec;
			flag = _summedEventsSubmissionQueue.ReachedLimit();
			flag2 = _summedEventsSubmissionQueue.ReachedLimit();
			if (flag3 == flag != flag2)
			{
				return;
			}
			_lastUpdateTime = time;
		}
		if (flag3)
		{
			Send();
			return;
		}
		if (flag)
		{
			_summedEventsSubmissionQueue.Send();
		}
		if (flag2)
		{
			_summedEventsSubmissionQueue.Send();
		}
	}

	public void Send()
	{
		_uniqueEventsSubmissionQueue.Send();
		_summedEventsSubmissionQueue.Send();
	}

	public bool AddUniqueEvent(string attributeName)
	{
		return AddUniqueEvent(attributeName, null);
	}

	public bool AddUniqueEvent(string attributeName, IDictionary<string, string> attributes = null)
	{
		if (!_uniqueEventsSubmissionQueue.ShouldProcessEvent(attributeName))
		{
			return false;
		}
		if (attributes == null)
		{
			attributes = new Dictionary<string, string>();
		}
		_attributeProvider.AddAttributes(attributes);
		if (!attributes.TryGetValue(attributeName, out var value) || string.IsNullOrEmpty(value))
		{
			Debug.LogWarning("Attribute name is not available in attribute scope. Please define attribute to set unique event.");
			return false;
		}
		if (UniqueEvents.Any((UniqueEvent n) => n.Name == attributeName))
		{
			return false;
		}
		UniqueEvent value2 = new UniqueEvent(attributeName, DateTimeHelper.Timestamp(), attributes);
		_uniqueEventsSubmissionQueue.Events.AddLast(value2);
		return true;
	}

	public int Count()
	{
		return _uniqueEventsSubmissionQueue.Count + _summedEventsSubmissionQueue.Count;
	}

	public bool AddSummedEvent(string metricsGroupName)
	{
		return AddSummedEvent(metricsGroupName, null);
	}

	public bool AddSummedEvent(string metricsGroupName, IDictionary<string, string> attributes = null)
	{
		if (!_summedEventsSubmissionQueue.ShouldProcessEvent(metricsGroupName))
		{
			return false;
		}
		SummedEvent value = new SummedEvent(metricsGroupName, DateTimeHelper.Timestamp(), attributes);
		_summedEventsSubmissionQueue.Events.AddLast(value);
		return true;
	}

	private void SendPendingSubmissionJobs(float time)
	{
		_uniqueEventsSubmissionQueue.SendPendingEvents(time);
		_summedEventsSubmissionQueue.SendPendingEvents(time);
	}

	internal static string GetDefaultUniqueEventsUrl(string universeName, string token)
	{
		return GetDefaultSubmissionUrl("unique-events", universeName, token);
	}

	internal static string GetDefaultSummedEventsUrl(string universeName, string token)
	{
		return GetDefaultSubmissionUrl("summed-events", universeName, token);
	}

	private static string GetDefaultSubmissionUrl(string serviceName, string universeName, string token)
	{
		return string.Format("{0}{1}/submit?token={2}&universe={3}", "https://events.backtrace.io/api/", serviceName, token, universeName);
	}

	public void GetAttributes(IDictionary<string, string> attributes)
	{
		attributes["application.session"] = _sessionId;
	}
}
