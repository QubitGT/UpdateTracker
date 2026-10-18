using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
internal struct NativeReferenceDispose
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe void* m_Data;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe void Dispose()
	{
		Memory.Unmanaged.Free(m_Data, m_AllocatorLabel);
	}
}
