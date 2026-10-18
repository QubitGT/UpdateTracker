using System;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class DroneRecordingStateData
{
	public delegate void OnDroneRecordingState(RecordingState state);

	private TimeSpan _span;

	private RecordingState _recordingState;

	public RecordingState State
	{
		get
		{
			return _recordingState;
		}
		set
		{
			_recordingState = value;
			this.OnDroneRecordingStateChanged?.Invoke(_recordingState);
		}
	}

	public TimeSpan Span
	{
		get
		{
			return _span;
		}
		set
		{
			_span = value;
		}
	}

	public string FormattedDuration
	{
		get
		{
			int num = Mathf.FloorToInt(_span.Hours);
			int num2 = Mathf.FloorToInt(_span.Minutes);
			int num3 = Mathf.FloorToInt(_span.Seconds);
			if (num != 0)
			{
				return $"{num:00}:{num2:00}:{num3:00}";
			}
			return $"{num2:00}:{num3:00}";
		}
	}

	public event OnDroneRecordingState OnDroneRecordingStateChanged;
}
