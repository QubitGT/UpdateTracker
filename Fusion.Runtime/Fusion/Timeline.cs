using System;

namespace Fusion;

internal class Timeline
{
	public RingBuffer<TimelinePoint> Points;

	public InterpolationParams Params;

	public bool IsEmpty => Points.IsEmpty;

	public Timeline(int capacity)
	{
		Points = new RingBuffer<TimelinePoint>(capacity);
		Params = default(InterpolationParams);
	}

	public void Clear()
	{
		Points.Clear();
		Params = default(InterpolationParams);
	}

	public void AddPoint(TimelinePoint point, double tickDeltaDouble, bool allowInactiveHandling = true)
	{
		if (allowInactiveHandling && !Points.IsEmpty)
		{
			TimelinePoint timelinePoint = Points.Back();
			double num = point.Time - timelinePoint.Time;
			if (num >= 0.25)
			{
				TimelinePoint item = timelinePoint;
				item.Time = (double)((int)point.Tick - 1) * tickDeltaDouble;
				Points.PushBack(item);
			}
		}
		Points.PushBack(point);
	}

	public InterpolationParams GetInterpolationParams(double time)
	{
		InterpolationParams result = default(InterpolationParams);
		result.Time = time;
		if (Points.IsEmpty)
		{
			return result;
		}
		try
		{
			double time2 = Points.Front().Time;
			double time3 = Points.Back().Time;
			if (time <= time2)
			{
				result.From = Points.Front().Snapshot;
				result.To = Points.Front().Snapshot;
				result.Alpha = 0f;
				result.Status = Status.Behind;
			}
			else if (time >= time3)
			{
				result.From = Points.Back().Snapshot;
				result.To = Points.Back().Snapshot;
				result.Alpha = 0f;
				result.Status = Status.Ahead;
			}
			else
			{
				for (int i = 0; i < Points.Count - 1; i++)
				{
					double time4 = Points[i].Time;
					double time5 = Points[i + 1].Time;
					if (time >= time4 && time < time5)
					{
						result.From = Points[i].Snapshot;
						result.To = Points[i + 1].Snapshot;
						result.Alpha = (float)Maths.Clamp01((time - time4) / (time5 - time4));
						result.Status = Status.Good;
						break;
					}
				}
			}
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
		return result;
	}

	public void UpdateInterpolationParams(double time)
	{
		Params = GetInterpolationParams(time);
	}
}
