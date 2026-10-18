using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

[Flags]
internal enum BuildFlags
{
	None = 0,
	PreferFastTrace = 1,
	PreferFastBuild = 2,
	MinimizeMemory = 4
}
