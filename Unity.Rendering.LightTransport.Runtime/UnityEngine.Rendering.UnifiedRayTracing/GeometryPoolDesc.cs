namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct GeometryPoolDesc
{
	public int vertexPoolByteSize;

	public int indexPoolByteSize;

	public int meshChunkTablesByteSize;

	public static GeometryPoolDesc NewDefault()
	{
		GeometryPoolDesc result = default(GeometryPoolDesc);
		result.vertexPoolByteSize = 268435456;
		result.indexPoolByteSize = 33554432;
		result.meshChunkTablesByteSize = 4194304;
		return result;
	}
}
