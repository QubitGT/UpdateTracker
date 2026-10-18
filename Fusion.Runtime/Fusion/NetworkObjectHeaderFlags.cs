using System;

namespace Fusion;

[Flags]
public enum NetworkObjectHeaderFlags
{
	GlobalObjectInterest = 1,
	DestroyWhenStateAuthorityLeaves = 2,
	SpawnedByClient = 4,
	AllowStateAuthorityOverride = 0x10,
	Struct = 0x20,
	StructArray = 0x80,
	DontDestroyOnLoad = 0x40,
	HasMainNetworkTRSP = 8,
	AreaOfInterest = 0x100
}
