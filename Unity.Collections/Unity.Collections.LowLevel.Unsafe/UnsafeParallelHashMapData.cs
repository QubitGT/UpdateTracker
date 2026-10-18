using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[StructLayout(LayoutKind.Explicit)]
[GenerateTestsForBurstCompatibility]
internal struct UnsafeParallelHashMapData
{
	[FieldOffset(0)]
	internal unsafe byte* values;

	[FieldOffset(8)]
	internal unsafe byte* keys;

	[FieldOffset(16)]
	internal unsafe byte* next;

	[FieldOffset(24)]
	internal unsafe byte* buckets;

	[FieldOffset(32)]
	internal int keyCapacity;

	[FieldOffset(36)]
	internal int bucketCapacityMask;

	[FieldOffset(40)]
	internal int allocatedIndexLength;

	private const int kFirstFreeTLSOffset = 64;

	internal const int IntsPerCacheLine = 16;

	internal unsafe int* firstFreeTLS => (int*)((byte*)UnsafeUtility.AddressOf(ref this) + 64);

	internal static int GetBucketSize(int capacity)
	{
		return capacity * 2;
	}

	internal static int GrowCapacity(int capacity)
	{
		if (capacity == 0)
		{
			return 1;
		}
		return capacity * 2;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal unsafe static void AllocateHashMap<TKey, TValue>(int length, int bucketLength, AllocatorManager.AllocatorHandle label, out UnsafeParallelHashMapData* outBuf) where TKey : unmanaged where TValue : unmanaged
	{
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		UnsafeParallelHashMapData* ptr = (UnsafeParallelHashMapData*)Memory.Unmanaged.Allocate(64 + 64 * threadIndexCount, 64, label);
		bucketLength = math.ceilpow2(bucketLength);
		ptr->keyCapacity = length;
		ptr->bucketCapacityMask = bucketLength - 1;
		int keyOffset;
		int nextOffset;
		int bucketOffset;
		int num = CalculateDataSize<TKey, TValue>(length, bucketLength, out keyOffset, out nextOffset, out bucketOffset);
		ptr->values = (byte*)Memory.Unmanaged.Allocate(num, 64, label);
		ptr->keys = ptr->values + keyOffset;
		ptr->next = ptr->values + nextOffset;
		ptr->buckets = ptr->values + bucketOffset;
		outBuf = ptr;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal unsafe static void ReallocateHashMap<TKey, TValue>(UnsafeParallelHashMapData* data, int newCapacity, int newBucketCapacity, AllocatorManager.AllocatorHandle label) where TKey : unmanaged where TValue : unmanaged
	{
		newBucketCapacity = math.ceilpow2(newBucketCapacity);
		if (data->keyCapacity == newCapacity && data->bucketCapacityMask + 1 == newBucketCapacity)
		{
			return;
		}
		int keyOffset;
		int nextOffset;
		int bucketOffset;
		byte* ptr = (byte*)Memory.Unmanaged.Allocate(CalculateDataSize<TKey, TValue>(newCapacity, newBucketCapacity, out keyOffset, out nextOffset, out bucketOffset), 64, label);
		byte* destination = ptr + keyOffset;
		byte* ptr2 = ptr + nextOffset;
		byte* ptr3 = ptr + bucketOffset;
		UnsafeUtility.MemCpy(ptr, data->values, data->keyCapacity * UnsafeUtility.SizeOf<TValue>());
		UnsafeUtility.MemCpy(destination, data->keys, data->keyCapacity * UnsafeUtility.SizeOf<TKey>());
		UnsafeUtility.MemCpy(ptr2, data->next, data->keyCapacity * UnsafeUtility.SizeOf<int>());
		for (int i = data->keyCapacity; i < newCapacity; i++)
		{
			*(int*)(ptr2 + (nint)i * (nint)4) = -1;
		}
		for (int j = 0; j < newBucketCapacity; j++)
		{
			*(int*)(ptr3 + (nint)j * (nint)4) = -1;
		}
		for (int k = 0; k <= data->bucketCapacityMask; k++)
		{
			int* ptr4 = (int*)data->buckets;
			int* ptr5 = (int*)ptr2;
			while (ptr4[k] >= 0)
			{
				int num = ptr4[k];
				ptr4[k] = ptr5[num];
				int num2 = UnsafeUtility.ReadArrayElement<TKey>(data->keys, num).GetHashCode() & (newBucketCapacity - 1);
				ptr5[num] = *(int*)(ptr3 + (nint)num2 * (nint)4);
				*(int*)(ptr3 + (nint)num2 * (nint)4) = num;
			}
		}
		Memory.Unmanaged.Free(data->values, label);
		if (data->allocatedIndexLength > data->keyCapacity)
		{
			data->allocatedIndexLength = data->keyCapacity;
		}
		data->values = ptr;
		data->keys = destination;
		data->next = ptr2;
		data->buckets = ptr3;
		data->keyCapacity = newCapacity;
		data->bucketCapacityMask = newBucketCapacity - 1;
	}

	internal unsafe static void DeallocateHashMap(UnsafeParallelHashMapData* data, AllocatorManager.AllocatorHandle allocator)
	{
		Memory.Unmanaged.Free(data->values, allocator);
		Memory.Unmanaged.Free(data, allocator);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal static int CalculateDataSize<TKey, TValue>(int length, int bucketLength, out int keyOffset, out int nextOffset, out int bucketOffset) where TKey : unmanaged where TValue : unmanaged
	{
		int num = UnsafeUtility.SizeOf<TValue>();
		int num2 = UnsafeUtility.SizeOf<TKey>();
		int num3 = UnsafeUtility.SizeOf<int>();
		int num4 = CollectionHelper.Align(num * length, 64);
		int num5 = CollectionHelper.Align(num2 * length, 64);
		int num6 = CollectionHelper.Align(num3 * length, 64);
		int num7 = CollectionHelper.Align(num3 * bucketLength, 64);
		int result = num4 + num5 + num6 + num7;
		keyOffset = num4;
		nextOffset = keyOffset + num5;
		bucketOffset = nextOffset + num6;
		return result;
	}

	internal unsafe static bool IsEmpty(UnsafeParallelHashMapData* data)
	{
		if (data->allocatedIndexLength <= 0)
		{
			return true;
		}
		int* ptr = (int*)data->buckets;
		int* ptr2 = (int*)data->next;
		int num = data->bucketCapacityMask;
		for (int i = 0; i <= num; i++)
		{
			if (ptr[i] != -1)
			{
				return false;
			}
		}
		return true;
	}

	internal unsafe static int GetCount(UnsafeParallelHashMapData* data)
	{
		if (data->allocatedIndexLength <= 0)
		{
			return 0;
		}
		int* ptr = (int*)data->next;
		int num = 0;
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		for (int i = 0; i < threadIndexCount; i++)
		{
			for (int num2 = data->firstFreeTLS[i * 16]; num2 >= 0; num2 = ptr[num2])
			{
				num++;
			}
		}
		return math.min(data->keyCapacity, data->allocatedIndexLength) - num;
	}

	internal unsafe static bool MoveNextSearch(UnsafeParallelHashMapData* data, ref int bucketIndex, ref int nextIndex, out int index)
	{
		int* ptr = (int*)data->buckets;
		int num = data->bucketCapacityMask;
		for (int i = bucketIndex; i <= num; i++)
		{
			int num2 = ptr[i];
			if (num2 != -1)
			{
				int* ptr2 = (int*)data->next;
				index = num2;
				bucketIndex = i + 1;
				nextIndex = ptr2[num2];
				return true;
			}
		}
		index = -1;
		bucketIndex = num + 1;
		nextIndex = -1;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static bool MoveNext(UnsafeParallelHashMapData* data, ref int bucketIndex, ref int nextIndex, out int index)
	{
		if (nextIndex != -1)
		{
			int* ptr = (int*)data->next;
			index = nextIndex;
			nextIndex = ptr[nextIndex];
			return true;
		}
		return MoveNextSearch(data, ref bucketIndex, ref nextIndex, out index);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe static void GetKeyArray<TKey>(UnsafeParallelHashMapData* data, NativeArray<TKey> result) where TKey : unmanaged
	{
		int* ptr = (int*)data->buckets;
		int* ptr2 = (int*)data->next;
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (; i <= data->bucketCapacityMask; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num2 = ptr[i]; num2 != -1; num2 = ptr2[num2])
			{
				result[num++] = UnsafeUtility.ReadArrayElement<TKey>(data->keys, num2);
			}
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe static void GetValueArray<TValue>(UnsafeParallelHashMapData* data, NativeArray<TValue> result) where TValue : unmanaged
	{
		int* ptr = (int*)data->buckets;
		int* ptr2 = (int*)data->next;
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (int num2 = data->bucketCapacityMask; i <= num2; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num3 = ptr[i]; num3 != -1; num3 = ptr2[num3])
			{
				result[num++] = UnsafeUtility.ReadArrayElement<TValue>(data->values, num3);
			}
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal unsafe static void GetKeyValueArrays<TKey, TValue>(UnsafeParallelHashMapData* data, NativeKeyValueArrays<TKey, TValue> result) where TKey : unmanaged where TValue : unmanaged
	{
		int* ptr = (int*)data->buckets;
		int* ptr2 = (int*)data->next;
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (int num2 = data->bucketCapacityMask; i <= num2; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num3 = ptr[i]; num3 != -1; num3 = ptr2[num3])
			{
				result.Keys[num] = UnsafeUtility.ReadArrayElement<TKey>(data->keys, num3);
				result.Values[num] = UnsafeUtility.ReadArrayElement<TValue>(data->values, num3);
				num++;
			}
		}
	}

	internal unsafe UnsafeParallelHashMapBucketData GetBucketData()
	{
		return new UnsafeParallelHashMapBucketData(values, keys, next, buckets, bucketCapacityMask);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe static void CheckHashMapReallocateDoesNotShrink(UnsafeParallelHashMapData* data, int newCapacity)
	{
		if (data->keyCapacity > newCapacity)
		{
			throw new InvalidOperationException("Shrinking a hash map is not supported");
		}
	}
}
