namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct VertexBufferChunk
{
	public GraphicsBuffer vertices;

	public int verticesStartOffset;

	public uint vertexCount;

	public uint vertexStride;

	public int baseVertex;
}
