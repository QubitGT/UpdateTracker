using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class NativeParallelHashMapExtensions
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static int Unique<T>(this NativeArray<T> array) where T : unmanaged, IEquatable<T>
	{
		if (array.Length == 0)
		{
			return 0;
		}
		int num = 0;
		int length = array.Length;
		int num2 = num;
		while (++num != length)
		{
			if (!array[num2].Equals(array[num]))
			{
				array[++num2] = array[num];
			}
		}
		return ++num2;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static (NativeArray<TKey>, int) GetUniqueKeyArray<TKey, TValue>(this UnsafeParallelMultiHashMap<TKey, TValue> container, AllocatorManager.AllocatorHandle allocator) where TKey : unmanaged, IEquatable<TKey>, IComparable<TKey> where TValue : unmanaged
	{
		NativeArray<TKey> keyArray = container.GetKeyArray(allocator);
		keyArray.Sort();
		int item = keyArray.Unique();
		return (keyArray, item);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static (NativeArray<TKey>, int) GetUniqueKeyArray<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue> container, AllocatorManager.AllocatorHandle allocator) where TKey : unmanaged, IEquatable<TKey>, IComparable<TKey> where TValue : unmanaged
	{
		NativeArray<TKey> keyArray = container.GetKeyArray(allocator);
		keyArray.Sort();
		int item = keyArray.Unique();
		return (keyArray, item);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static UnsafeParallelHashMapBucketData GetUnsafeBucketData<TKey, TValue>(this NativeParallelHashMap<TKey, TValue> container) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		return container.m_HashMapData.m_Buffer->GetBucketData();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static UnsafeParallelHashMapBucketData GetUnsafeBucketData<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue> container) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		return container.m_MultiHashMapData.m_Buffer->GetBucketData();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static void Remove<TKey, TValue>(this NativeParallelMultiHashMap<TKey, TValue> container, TKey key, TValue value) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged, IEquatable<TValue>
	{
		container.m_MultiHashMapData.Remove(key, value);
	}
}
