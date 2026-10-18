namespace Fusion;

internal struct TimeAdjustment
{
	public Tick Tick;

	public double Total;

	public TimeAdjustment(Tick tick, double total)
	{
		Tick = tick;
		Total = total;
	}

	public override string ToString()
	{
		return $"({Tick}, {Total})";
	}
}
