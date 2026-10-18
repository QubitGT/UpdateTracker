using System;

namespace Fusion;

[Flags]
internal enum NetworkObjectRuntimeFlags
{
	None = 0,
	HadAwake = 1,
	IsDestroyed = 2,
	IsNested = 4,
	NotAwakeWhenAttaching = 0x2000,
	ClearMask = 0xFFF0000,
	InSimulation = 0x10000,
	PreexistingObject = 0x20000,
	AttachOptionLocalSpawn = 0x100000,
	Spawned = 0x800000,
	OwnsNestedObjects = 0x1000000,
	HasMainNetworkTRSP = 0x4000000
}
