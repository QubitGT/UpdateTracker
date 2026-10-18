namespace UnityEngine.Rendering;

internal struct IndirectBufferAllocInfo
{
	public int drawAllocIndex;

	public int drawCount;

	public int instanceAllocIndex;

	public int instanceCount;

	public bool IsEmpty()
	{
		return drawCount == 0;
	}

	public bool IsWithinLimits(in IndirectBufferLimits limits)
	{
		if (drawAllocIndex + drawCount <= limits.maxDrawCount)
		{
			return instanceAllocIndex + instanceCount <= limits.maxInstanceCount;
		}
		return false;
	}

	public int GetExtraDrawInfoSlotIndex()
	{
		return drawAllocIndex + drawCount;
	}
}
