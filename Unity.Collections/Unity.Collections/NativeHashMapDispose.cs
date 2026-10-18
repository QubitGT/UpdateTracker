using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeHashMapDispose
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeHashMap<int, int>* m_HashMapData;

	internal AllocatorManager.AllocatorHandle m_Allocator;

	internal unsafe void Dispose()
	{
		HashMapHelper<int>* hashMapData = (HashMapHelper<int>*)m_HashMapData;
		HashMapHelper<int>.Free(hashMapData);
	}
}
