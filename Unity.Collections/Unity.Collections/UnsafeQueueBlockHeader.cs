namespace Unity.Collections;

internal struct UnsafeQueueBlockHeader
{
	public unsafe UnsafeQueueBlockHeader* m_NextBlock;

	public int m_NumItems;
}
