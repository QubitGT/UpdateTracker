using System;

namespace Meta.XR.Audio;

[Flags]
public enum EnableFlag : uint
{
	NONE = 0u,
	SIMPLE_ROOM_MODELING = 2u,
	LATE_REVERBERATION = 3u,
	RANDOMIZE_REVERB = 4u,
	PERFORMANCE_COUNTERS = 5u
}
