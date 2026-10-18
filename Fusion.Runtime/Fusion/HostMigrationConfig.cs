using System;

namespace Fusion;

[Serializable]
public class HostMigrationConfig
{
	[InlineHelp]
	public bool EnableAutoUpdate;

	[InlineHelp]
	[Unit(Units.Seconds)]
	[RangeEx(10.0, 60.0)]
	public int UpdateDelay = 10;
}
