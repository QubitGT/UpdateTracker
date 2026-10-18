using System;

namespace Meta.XR.Acoustics;

[Flags]
public enum ObjectFlags : uint
{
	EMPTY = 0u,
	ENABLED = 1u,
	STATIC = 2u
}
