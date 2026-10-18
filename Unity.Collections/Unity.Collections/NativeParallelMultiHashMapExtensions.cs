using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class NativeParallelMultiHashMapExtensions
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int),
		typeof(AllocatorManager.AllocatorHandle)
	})]
	internal static void Initialize<TKey, TValue, U>(this ref NativeParallelMultiHashMap<TKey, TValue> container, int capacity, ref U allocator) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged where U : unmanaged, AllocatorManager.IAllocator
	{
		container.m_MultiHashMapData = new UnsafeParallelMultiHashMap<TKey, TValue>(capacity, allocator.Handle);
	}
}
