using System;

namespace Fusion;

[Flags]
public enum TraceChannels
{
	Global = 1,
	Stun = 2,
	Object = 4,
	Network = 8,
	Prefab = 0x10,
	SceneInfo = 0x20,
	SceneManager = 0x40,
	SimulationMessage = 0x80,
	HostMigration = 0x100,
	Encryption = 0x200,
	DummyTraffic = 0x400,
	Realtime = 0x800,
	MemoryTrack = 0x1000,
	Snapshots = 0x2000,
	Time = 0x4000
}
