using System;

namespace UnityEngine.ProBuilder;

public enum MeshSyncState
{
	Null,
	[Obsolete("InstanceIDMismatch is no longer used. Mesh references are not tracked by Instance ID.")]
	InstanceIDMismatch,
	Lightmap,
	InSync,
	NeedsRebuild
}
