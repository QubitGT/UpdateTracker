using System;

namespace Fusion;

[Flags]
internal enum NetworkObjectDestroyFlags
{
	None = 0,
	DestroyedByEngine = 1,
	DestroyState = 2,
	DestroyedByReplicator = 4,
	DestroyedByDespawn = 8
}
