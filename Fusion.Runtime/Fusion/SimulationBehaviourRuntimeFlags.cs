using System;

namespace Fusion;

[Flags]
internal enum SimulationBehaviourRuntimeFlags
{
	IsGlobal = 1,
	InSimulation = 2,
	PendingRemoval = 4,
	IsUnityDestroyed = 8,
	IsUnityDisabled = 0x10,
	SkipNextUpdate = 0x20,
	ClearMask = 0x27
}
