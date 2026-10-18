namespace UnityEngine.Rendering;

internal struct RenderersBatchersContextDesc
{
	public InstanceNumInfo instanceNumInfo;

	public bool supportDitheringCrossFade;

	public bool enableBoundingSpheresInstanceData;

	public float smallMeshScreenPercentage;

	public bool enableCullerDebugStats;

	public static RenderersBatchersContextDesc NewDefault()
	{
		RenderersBatchersContextDesc result = default(RenderersBatchersContextDesc);
		result.instanceNumInfo = new InstanceNumInfo(1024, 32);
		return result;
	}
}
