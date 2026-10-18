namespace Fusion;

public class TimeSyncConfiguration
{
	[InlineHelp]
	[Unit(Units.Seconds)]
	[RangeEx(1.0, 10.0)]
	public double SampleWindowSeconds = 1.0;

	[InlineHelp]
	[Unit(Units.Percentage)]
	[RangeEx(0.10000000149011612, 10.0)]
	public double MaxLateInputs = 1.0;

	[InlineHelp]
	[Unit(Units.Percentage)]
	[RangeEx(0.10000000149011612, 10.0)]
	public double MaxLateSnapshots = 5.0;

	[InlineHelp]
	[Unit(Units.Packets)]
	[RangeEx(0.0, 8.0)]
	public int RedundantInputs = 1;

	[InlineHelp]
	[Unit(Units.Packets)]
	[RangeEx(0.0, 8.0)]
	public int RedundantSnapshots = 1;

	internal double SampleWindowSecondsNormalized => Maths.Clamp(SampleWindowSeconds, 1.0, 10.0);

	internal double MaxLateInputsNormalized => Maths.Clamp(MaxLateInputs, 0.1, 10.0) / 100.0;

	internal double MaxLateSnapshotsNormalized => Maths.Clamp(MaxLateSnapshots, 0.1, 10.0) / 100.0;

	internal int RedundantInputsNormalized => Maths.Clamp(RedundantInputs, 0, 8);

	internal int RedundantSnapshotsNormalized => Maths.Clamp(RedundantSnapshots, 0, 8);

	private double MaxSimSpeedAdjust => 5.0;

	internal double MaxSimSpeedAdjustNormalized => Maths.Clamp(MaxSimSpeedAdjust, 1.0, 10.0) / 100.0;

	private double MaxInterpSpeedAdjust => 5.0;

	internal double MaxInterpSpeedAdjustNormalized => Maths.Clamp(MaxInterpSpeedAdjust, 1.0, 10.0) / 100.0;

	internal static TimeSyncConfiguration GetFromTickrate(TickRate.Resolved tickrate)
	{
		TimeSyncConfiguration timeSyncConfiguration = new TimeSyncConfiguration();
		switch (tickrate.ClientSend)
		{
		case 50:
		case 60:
		case 64:
		case 100:
		case 120:
		case 128:
		case 240:
		case 256:
			timeSyncConfiguration.MaxLateInputs = 1.0;
			timeSyncConfiguration.RedundantInputs = 1;
			break;
		case 30:
		case 32:
			timeSyncConfiguration.MaxLateInputs = 1.0;
			timeSyncConfiguration.RedundantInputs = 1;
			break;
		case 20:
		case 24:
			timeSyncConfiguration.MaxLateInputs = 1.0;
			timeSyncConfiguration.RedundantInputs = 1;
			break;
		case 8:
		case 10:
		case 16:
			timeSyncConfiguration.MaxLateInputs = 1.0;
			timeSyncConfiguration.RedundantInputs = 1;
			break;
		}
		switch (tickrate.ServerSend)
		{
		case 50:
		case 60:
		case 64:
		case 100:
		case 120:
		case 128:
		case 240:
		case 256:
			timeSyncConfiguration.MaxLateSnapshots = 5.0;
			timeSyncConfiguration.RedundantSnapshots = 1;
			break;
		case 30:
		case 32:
			timeSyncConfiguration.MaxLateSnapshots = 5.0;
			timeSyncConfiguration.RedundantSnapshots = 1;
			break;
		case 20:
		case 24:
			timeSyncConfiguration.MaxLateSnapshots = 5.0;
			timeSyncConfiguration.RedundantSnapshots = 1;
			break;
		case 8:
		case 10:
		case 16:
			timeSyncConfiguration.MaxLateSnapshots = 5.0;
			timeSyncConfiguration.RedundantSnapshots = 1;
			break;
		}
		return timeSyncConfiguration;
	}
}
