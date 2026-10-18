namespace Fusion;

internal struct TimelinePoint
{
	public Tick Snapshot;

	public Tick Tick;

	public double Time;

	public TimelinePoint(Tick snapshot, Tick tick, double tickDeltaDouble)
	{
		Snapshot = snapshot;
		Tick = tick;
		Time = (double)(int)tick * tickDeltaDouble;
	}
}
