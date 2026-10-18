using System;

namespace Meta.XR.Acoustics;

[Flags]
public enum MeshFlags : uint
{
	NONE = 0u,
	ENABLE_SIMPLIFICATION = 1u,
	ENABLE_DIFFRACTION = 2u
}
