namespace UnityEngine.Rendering.UnifiedRayTracing;

internal static class GeometryPoolConstants
{
	public const int GeoPoolPosByteSize = 12;

	private const int UvFieldSizeInDWords = 2;

	public const int GeoPoolUV0ByteSize = 8;

	public const int GeoPoolUV1ByteSize = 8;

	public const int GeoPoolNormalByteSize = 4;

	public const int GeoPoolPosByteOffset = 0;

	public const int GeoPoolUV0ByteOffset = 12;

	public const int GeoPoolUV1ByteOffset = 20;

	public const int GeoPoolNormalByteOffset = 28;

	public const int GeoPoolIndexByteSize = 4;

	public const int GeoPoolVertexByteSize = 32;
}
