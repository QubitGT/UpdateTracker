using System;

namespace UnityEngine.ProBuilder.Csg;

[Flags]
internal enum VertexAttributes
{
	Position = 1,
	Texture0 = 2,
	Texture1 = 4,
	Lightmap = 4,
	Texture2 = 8,
	Texture3 = 0x10,
	Color = 0x20,
	Normal = 0x40,
	Tangent = 0x80,
	All = 0xFF
}
