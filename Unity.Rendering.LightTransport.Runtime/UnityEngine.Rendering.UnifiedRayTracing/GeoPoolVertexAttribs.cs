using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

[Flags]
internal enum GeoPoolVertexAttribs
{
	Position = 1,
	Normal = 2,
	Uv0 = 4,
	Uv1 = 8
}
