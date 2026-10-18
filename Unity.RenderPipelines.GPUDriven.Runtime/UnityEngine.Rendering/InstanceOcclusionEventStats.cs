namespace UnityEngine.Rendering;

internal struct InstanceOcclusionEventStats
{
	public int viewInstanceID;

	public InstanceOcclusionEventType eventType;

	public int occluderVersion;

	public int subviewMask;

	public OcclusionTest occlusionTest;

	public int visibleInstances;

	public int culledInstances;

	public int visiblePrimitives;

	public int culledPrimitives;
}
