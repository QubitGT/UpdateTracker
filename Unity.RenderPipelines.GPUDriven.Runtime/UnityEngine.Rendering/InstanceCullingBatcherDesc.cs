namespace UnityEngine.Rendering;

internal struct InstanceCullingBatcherDesc
{
	public OnCullingCompleteCallback onCompleteCallback;

	public static InstanceCullingBatcherDesc NewDefault()
	{
		InstanceCullingBatcherDesc result = default(InstanceCullingBatcherDesc);
		result.onCompleteCallback = null;
		return result;
	}
}
