using System;

namespace Meta.XR.Acoustics;

[Flags]
public enum AcousticMapStatus : uint
{
	EMPTY = 0u,
	MAPPED = 1u,
	READY = 3u
}
