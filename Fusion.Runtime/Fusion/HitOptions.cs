using System;

namespace Fusion;

[Flags]
public enum HitOptions
{
	None = 0,
	IncludePhysX = 1,
	IncludeBox2D = 2,
	SubtickAccuracy = 4,
	IgnoreInputAuthority = 8
}
