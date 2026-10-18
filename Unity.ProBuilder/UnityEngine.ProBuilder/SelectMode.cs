using System;

namespace UnityEngine.ProBuilder;

[Flags]
public enum SelectMode
{
	None = 0,
	Vertex = 1,
	Edge = 2,
	Face = 4,
	TextureVertex = 8,
	TextureEdge = 0x10,
	TextureFace = 0x20,
	InputTool = 0x40,
	Any = 0xFF
}
