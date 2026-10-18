using System;

namespace Fusion.Statistics;

[Flags]
public enum RenderSimStats
{
	InPackets = 1,
	OutPackets = 2,
	RTT = 4,
	InBandwidth = 8,
	OutBandwidth = 0x10,
	Resimulations = 0x20,
	ForwardTicks = 0x40,
	InputReceiveDelta = 0x80,
	TimeResets = 0x100,
	StateReceiveDelta = 0x200,
	SimulationTimeOffset = 0x400,
	SimulationSpeed = 0x800,
	InterpolationOffset = 0x1000,
	InterpolationSpeed = 0x2000,
	InputInBandwidth = 0x4000,
	InputOutBandwidth = 0x8000,
	AverageInPacketSize = 0x10000,
	AverageOutPacketSize = 0x20000,
	InObjectUpdates = 0x40000,
	OutObjectUpdates = 0x80000,
	ObjectsAllocatedMemoryInUse = 0x100000,
	GeneralAllocatedMemoryInUse = 0x200000,
	ObjectsAllocatedMemoryFree = 0x400000,
	GeneralAllocatedMemoryFree = 0x800000,
	WordsWrittenCount = 0x1000000,
	WordsWrittenSize = 0x2000000,
	WordsReadCount = 0x4000000,
	WordsReadSize = 0x8000000
}
