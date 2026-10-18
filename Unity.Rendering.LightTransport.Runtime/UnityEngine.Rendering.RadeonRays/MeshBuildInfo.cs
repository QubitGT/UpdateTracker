namespace UnityEngine.Rendering.RadeonRays;

internal struct MeshBuildInfo
{
	public GraphicsBuffer vertices;

	public int verticesStartOffset;

	public uint vertexCount;

	public uint vertexStride;

	public int baseVertex;

	public GraphicsBuffer triangleIndices;

	public int indicesStartOffset;

	public int baseIndex;

	public IndexFormat indexFormat;

	public uint triangleCount;
}
