using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeBitArrayDispose
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeBitArray* m_BitArrayData;

	public AllocatorManager.AllocatorHandle m_Allocator;

	public unsafe void Dispose()
	{
		UnsafeBitArray.Free(m_BitArrayData, m_Allocator);
	}
}
