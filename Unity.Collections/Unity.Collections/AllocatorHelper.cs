using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
public struct AllocatorHelper<T> : IDisposable where T : unmanaged, AllocatorManager.IAllocator
{
	private unsafe readonly T* m_allocator;

	private AllocatorManager.AllocatorHandle m_backingAllocator;

	public unsafe ref T Allocator => ref UnsafeUtility.AsRef<T>(m_allocator);

	[ExcludeFromBurstCompatTesting("CreateAllocator is unburstable")]
	public unsafe AllocatorHelper(AllocatorManager.AllocatorHandle backingAllocator, bool isGlobal = false, int globalIndex = 0)
	{
		m_allocator = (T*)UnsafeUtility.AddressOf(ref AllocatorManager.CreateAllocator<T>(backingAllocator, isGlobal, globalIndex));
		m_backingAllocator = backingAllocator;
	}

	[ExcludeFromBurstCompatTesting("DestroyAllocator is unburstable")]
	public unsafe void Dispose()
	{
		AllocatorManager.DestroyAllocator(ref UnsafeUtility.AsRef<T>(m_allocator), m_backingAllocator);
	}
}
