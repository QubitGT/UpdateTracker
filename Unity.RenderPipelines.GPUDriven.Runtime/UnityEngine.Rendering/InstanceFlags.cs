using System;

namespace UnityEngine.Rendering;

[Flags]
internal enum InstanceFlags : byte
{
	None = 0,
	AffectsLightmaps = 1,
	IsShadowsOff = 2,
	IsShadowsOnly = 4,
	HasMeshLod = 8,
	SmallMeshCulling = 0x10
}
