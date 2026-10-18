using System;

namespace Meta.XR.Acoustics;

[Flags]
public enum EnableFlagInternal : uint
{
	NONE = 0u,
	SIMPLE_ROOM_MODELING = 2u,
	LATE_REVERBERATION = 3u,
	RANDOMIZE_REVERB = 4u,
	PERFORMANCE_COUNTERS = 5u,
	DIFFRACTION = 6u
}
