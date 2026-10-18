using System;

namespace g3;

[Flags]
public enum MeshComponents
{
	None = 0,
	VertexNormals = 1,
	VertexColors = 2,
	VertexUVs = 4,
	FaceGroups = 8,
	All = 0xF
}
