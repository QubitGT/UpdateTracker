using System;

namespace Fusion.Statistics;

[Flags]
internal enum NetworkObjectStat
{
	InBandwidth = 1,
	OutBandwidth = 2,
	InPackets = 4,
	OutPackets = 8,
	AverageInPacketSize = 0x10,
	AverageOutPacketSize = 0x20
}
