using System;

namespace GorillaNetworking.ScheduledEvents;

public struct ScheduledEventInfo
{
	public bool isActive;

	public DateTime scheduledStart;

	public static ScheduledEventInfo None
	{
		get
		{
			ScheduledEventInfo result = default(ScheduledEventInfo);
			result.isActive = false;
			return result;
		}
	}
}
