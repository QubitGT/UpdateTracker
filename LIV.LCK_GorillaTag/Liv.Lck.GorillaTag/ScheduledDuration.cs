using System;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

[Serializable]
public struct ScheduledDuration
{
	[SerializeField]
	private long _startTimeTicks;

	[SerializeField]
	private long _endTimeTicks;

	public ScheduledDuration(long startTimeTicks, long endTimeTicks)
	{
		_startTimeTicks = startTimeTicks;
		_endTimeTicks = endTimeTicks;
	}

	public bool IsActive()
	{
		long ticks = DateTime.Now.Ticks;
		if (ticks >= _startTimeTicks)
		{
			return ticks <= _endTimeTicks;
		}
		return false;
	}
}
