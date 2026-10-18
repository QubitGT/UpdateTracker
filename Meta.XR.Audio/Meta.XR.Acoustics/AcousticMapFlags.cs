using System;

namespace Meta.XR.Acoustics;

[Flags]
public enum AcousticMapFlags : uint
{
	NONE = 0u,
	STATIC_ONLY = 1u,
	NO_FLOATING = 2u,
	MAP_ONLY = 4u,
	DIFFRACTION = 8u
}
