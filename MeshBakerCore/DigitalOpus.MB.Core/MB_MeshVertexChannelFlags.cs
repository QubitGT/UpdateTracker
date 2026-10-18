using System;

namespace DigitalOpus.MB.Core;

[Flags]
public enum MB_MeshVertexChannelFlags
{
	none = 0,
	vertex = 1,
	normal = 2,
	tangent = 4,
	colors = 8,
	uv0 = 0x10,
	nuvsSliceIdx = 0x20,
	uv2 = 0x40,
	uv3 = 0x80,
	uv4 = 0x100,
	uv5 = 0x200,
	uv6 = 0x400,
	uv7 = 0x800,
	uv8 = 0x1000,
	blendWeight = 0x2000,
	blendIndices = 0x4000
}
