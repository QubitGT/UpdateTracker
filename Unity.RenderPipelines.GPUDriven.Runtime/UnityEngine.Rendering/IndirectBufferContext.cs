using Unity.Jobs;

namespace UnityEngine.Rendering;

internal struct IndirectBufferContext
{
	public enum BufferState
	{
		Pending,
		Zeroed,
		NoOcclusionTest,
		AllInstancesOcclusionTested,
		OccludedInstancesReTested
	}

	public JobHandle cullingJobHandle;

	public BufferState bufferState;

	public int occluderVersion;

	public int subviewMask;

	public IndirectBufferContext(JobHandle cullingJobHandle)
	{
		this.cullingJobHandle = cullingJobHandle;
		bufferState = BufferState.Pending;
		occluderVersion = 0;
		subviewMask = 0;
	}

	public bool Matches(BufferState bufferState, int occluderVersion, int subviewMask)
	{
		if (this.bufferState == bufferState && this.occluderVersion == occluderVersion)
		{
			return this.subviewMask == subviewMask;
		}
		return false;
	}
}
