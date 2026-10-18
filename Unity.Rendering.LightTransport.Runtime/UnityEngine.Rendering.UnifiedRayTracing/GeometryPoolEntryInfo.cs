namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct GeometryPoolEntryInfo
{
	public bool valid;

	public uint refCount;

	public static GeometryPoolEntryInfo NewDefault()
	{
		GeometryPoolEntryInfo result = default(GeometryPoolEntryInfo);
		result.valid = false;
		result.refCount = 0u;
		return result;
	}
}
