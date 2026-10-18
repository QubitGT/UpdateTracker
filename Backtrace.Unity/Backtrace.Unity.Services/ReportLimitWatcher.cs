using System;
using System.Collections.Generic;
using Backtrace.Unity.Common;
using Backtrace.Unity.Model;
using UnityEngine;

namespace Backtrace.Unity.Services;

public class ReportLimitWatcher
{
	internal readonly Queue<long> _reportQueue;

	internal readonly object _object = new object();

	private readonly long _queueReportTime = 60L;

	private bool _watcherEnable;

	private int _reportPerMin;

	private bool _displayMessage;

	private bool _limitHit;

	internal ReportLimitWatcher(uint reportPerMin)
	{
		if (reportPerMin < 0)
		{
			throw new ArgumentException("reportPerMin have to be greater than or equal to zero");
		}
		int num = checked((int)reportPerMin);
		_reportQueue = new Queue<long>(num);
		_reportPerMin = num;
		_watcherEnable = reportPerMin != 0;
	}

	internal void SetClientReportLimit(uint reportPerMin)
	{
		int reportPerMin2 = checked((int)reportPerMin);
		_reportPerMin = reportPerMin2;
		_watcherEnable = reportPerMin != 0;
	}

	public bool WatchReport(long timestamp, bool displayMessageOnLimitHit = true)
	{
		if (!_watcherEnable)
		{
			return true;
		}
		lock (_object)
		{
			Clear();
			if (_reportQueue.Count + 1 > _reportPerMin)
			{
				_limitHit = true;
				if (displayMessageOnLimitHit)
				{
					DisplayReportLimitHitMessage();
				}
				return false;
			}
			_limitHit = false;
			_displayMessage = true;
			_reportQueue.Enqueue(timestamp);
		}
		return true;
	}

	public bool WatchReport(BacktraceReport report, bool displayMessageOnLimitHit = true)
	{
		return WatchReport(report.Timestamp, displayMessageOnLimitHit);
	}

	internal bool ShouldDisplayMessage()
	{
		if (_limitHit)
		{
			return _displayMessage;
		}
		return false;
	}

	private void DisplayReportLimitHitMessage()
	{
		if (ShouldDisplayMessage())
		{
			_displayMessage = false;
			Debug.LogWarning($"Backtrace report limit hit({_reportPerMin}/min) – Ignoring errors for 1 minute");
		}
	}

	private void Clear()
	{
		long num = DateTimeHelper.Timestamp();
		bool flag = false;
		while (!flag && _reportQueue.Count != 0)
		{
			long num2 = _reportQueue.Peek();
			flag = num - num2 < _queueReportTime;
			if (!flag)
			{
				_reportQueue.Dequeue();
			}
		}
	}

	internal void Reset()
	{
		_reportQueue.Clear();
	}
}
