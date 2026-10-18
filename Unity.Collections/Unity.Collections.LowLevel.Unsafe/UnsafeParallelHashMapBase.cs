using System;
using System.Diagnostics;
using System.Threading;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
internal struct UnsafeParallelHashMapBase<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	private const int SentinelRefilling = -2;

	private const int SentinelSwapInProgress = -3;

	internal unsafe static void Clear(UnsafeParallelHashMapData* data)
	{
		UnsafeUtility.MemSet(data->buckets, byte.MaxValue, (data->bucketCapacityMask + 1) * 4);
		UnsafeUtility.MemSet(data->next, byte.MaxValue, data->keyCapacity * 4);
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		for (int i = 0; i < threadIndexCount; i++)
		{
			data->firstFreeTLS[i * 16] = -1;
		}
		data->allocatedIndexLength = 0;
	}

	internal unsafe static int AllocEntry(UnsafeParallelHashMapData* data, int threadIndex)
	{
		int* next = (int*)data->next;
		int num;
		while (true)
		{
			num = Volatile.Read(ref data->firstFreeTLS[threadIndex * 16]);
			if (num == -3)
			{
				continue;
			}
			if (num < 0)
			{
				Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * 16], -2);
				if (data->allocatedIndexLength < data->keyCapacity)
				{
					num = Interlocked.Add(ref data->allocatedIndexLength, 16) - 16;
					if (num < data->keyCapacity - 1)
					{
						int num2 = math.min(16, data->keyCapacity - num);
						for (int i = 1; i < num2; i++)
						{
							next[num + i] = num + i + 1;
						}
						next[num + num2 - 1] = -1;
						next[num] = -1;
						Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * 16], num + 1);
						return num;
					}
					if (num == data->keyCapacity - 1)
					{
						Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * 16], -1);
						return num;
					}
				}
				Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * 16], -1);
				int threadIndexCount = JobsUtility.ThreadIndexCount;
				bool flag = true;
				while (flag)
				{
					flag = false;
					for (int num3 = (threadIndex + 1) % threadIndexCount; num3 != threadIndex; num3 = (num3 + 1) % threadIndexCount)
					{
						do
						{
							num = Volatile.Read(ref data->firstFreeTLS[num3 * 16]);
						}
						while (num == -3 || (num >= 0 && Interlocked.CompareExchange(ref data->firstFreeTLS[num3 * 16], -3, num) != num));
						if (num == -2)
						{
							flag = true;
						}
						else if (num >= 0)
						{
							Interlocked.Exchange(ref data->firstFreeTLS[num3 * 16], next[num]);
							next[num] = -1;
							return num;
						}
					}
				}
			}
			if (Interlocked.CompareExchange(ref data->firstFreeTLS[threadIndex * 16], -3, num) == num)
			{
				break;
			}
		}
		Interlocked.Exchange(ref data->firstFreeTLS[threadIndex * 16], next[num]);
		next[num] = -1;
		return num;
	}

	internal unsafe static void FreeEntry(UnsafeParallelHashMapData* data, int idx, int threadIndex)
	{
		int* next = (int*)data->next;
		int num = -1;
		while (true)
		{
			num = Volatile.Read(ref data->firstFreeTLS[threadIndex * 16]);
			if (num != -3)
			{
				next[idx] = num;
				if (Interlocked.CompareExchange(ref data->firstFreeTLS[threadIndex * 16], idx, num) == num)
				{
					break;
				}
			}
		}
	}

	internal unsafe static bool TryAddAtomic(UnsafeParallelHashMapData* data, TKey key, TValue item, int threadIndex)
	{
		if (TryGetFirstValueAtomic(data, key, out var item2, out var it))
		{
			return false;
		}
		int num = AllocEntry(data, threadIndex);
		UnsafeUtility.WriteArrayElement(data->keys, num, key);
		UnsafeUtility.WriteArrayElement(data->values, num, item);
		int num2 = key.GetHashCode() & data->bucketCapacityMask;
		int* buckets = (int*)data->buckets;
		if (Interlocked.CompareExchange(ref buckets[num2], num, -1) != -1)
		{
			int* next = (int*)data->next;
			int num3 = -1;
			do
			{
				num3 = (next[num] = buckets[num2]);
				if (TryGetFirstValueAtomic(data, key, out item2, out it))
				{
					FreeEntry(data, num, threadIndex);
					return false;
				}
			}
			while (Interlocked.CompareExchange(ref buckets[num2], num, num3) != num3);
		}
		return true;
	}

	internal unsafe static void AddAtomicMulti(UnsafeParallelHashMapData* data, TKey key, TValue item, int threadIndex)
	{
		int num = AllocEntry(data, threadIndex);
		UnsafeUtility.WriteArrayElement(data->keys, num, key);
		UnsafeUtility.WriteArrayElement(data->values, num, item);
		int num2 = key.GetHashCode() & data->bucketCapacityMask;
		int* buckets = (int*)data->buckets;
		int* next = (int*)data->next;
		int num3;
		do
		{
			num3 = (next[num] = buckets[num2]);
		}
		while (Interlocked.CompareExchange(ref buckets[num2], num, num3) != num3);
	}

	internal unsafe static bool TryAdd(UnsafeParallelHashMapData* data, TKey key, TValue item, bool isMultiHashMap, AllocatorManager.AllocatorHandle allocation)
	{
		if (isMultiHashMap || !TryGetFirstValueAtomic(data, key, out var _, out var _))
		{
			int num;
			int* next;
			if (data->allocatedIndexLength >= data->keyCapacity && *data->firstFreeTLS < 0)
			{
				int threadIndexCount = JobsUtility.ThreadIndexCount;
				for (int i = 1; i < threadIndexCount; i++)
				{
					if (data->firstFreeTLS[i * 16] >= 0)
					{
						num = data->firstFreeTLS[i * 16];
						next = (int*)data->next;
						data->firstFreeTLS[i * 16] = next[num];
						next[num] = -1;
						*data->firstFreeTLS = num;
						break;
					}
				}
				if (*data->firstFreeTLS < 0)
				{
					int num2 = UnsafeParallelHashMapData.GrowCapacity(data->keyCapacity);
					UnsafeParallelHashMapData.ReallocateHashMap<TKey, TValue>(data, num2, UnsafeParallelHashMapData.GetBucketSize(num2), allocation);
				}
			}
			num = *data->firstFreeTLS;
			if (num >= 0)
			{
				*data->firstFreeTLS = *(int*)(data->next + (nint)num * (nint)4);
			}
			else
			{
				num = data->allocatedIndexLength++;
			}
			UnsafeUtility.WriteArrayElement(data->keys, num, key);
			UnsafeUtility.WriteArrayElement(data->values, num, item);
			int num3 = key.GetHashCode() & data->bucketCapacityMask;
			int* buckets = (int*)data->buckets;
			next = (int*)data->next;
			next[num] = buckets[num3];
			buckets[num3] = num;
			return true;
		}
		return false;
	}

	internal unsafe static int Remove(UnsafeParallelHashMapData* data, TKey key, bool isMultiHashMap)
	{
		if (data->keyCapacity == 0)
		{
			return 0;
		}
		int num = 0;
		int* buckets = (int*)data->buckets;
		int* next = (int*)data->next;
		int num2 = key.GetHashCode() & data->bucketCapacityMask;
		int num3 = -1;
		int num4 = buckets[num2];
		while (num4 >= 0 && num4 < data->keyCapacity)
		{
			if (UnsafeUtility.ReadArrayElement<TKey>(data->keys, num4).Equals(key))
			{
				num++;
				if (num3 < 0)
				{
					buckets[num2] = next[num4];
				}
				else
				{
					next[num3] = next[num4];
				}
				int num5 = next[num4];
				next[num4] = *data->firstFreeTLS;
				*data->firstFreeTLS = num4;
				num4 = num5;
				if (!isMultiHashMap)
				{
					break;
				}
			}
			else
			{
				num3 = num4;
				num4 = next[num4];
			}
		}
		return num;
	}

	internal unsafe static void Remove(UnsafeParallelHashMapData* data, NativeParallelMultiHashMapIterator<TKey> it)
	{
		int* buckets = (int*)data->buckets;
		int* next = (int*)data->next;
		int num = it.key.GetHashCode() & data->bucketCapacityMask;
		int num2 = buckets[num];
		if (num2 == it.EntryIndex)
		{
			buckets[num] = next[num2];
		}
		else
		{
			while (num2 >= 0 && next[num2] != it.EntryIndex)
			{
				num2 = next[num2];
			}
			_ = 0;
			next[num2] = next[it.EntryIndex];
		}
		next[it.EntryIndex] = *data->firstFreeTLS;
		*data->firstFreeTLS = it.EntryIndex;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe static void RemoveKeyValue<TValueEQ>(UnsafeParallelHashMapData* data, TKey key, TValueEQ value) where TValueEQ : unmanaged, IEquatable<TValueEQ>
	{
		if (data->keyCapacity == 0)
		{
			return;
		}
		int* buckets = (int*)data->buckets;
		uint keyCapacity = (uint)data->keyCapacity;
		int* ptr = buckets + (key.GetHashCode() & data->bucketCapacityMask);
		int num = *ptr;
		if ((uint)num >= keyCapacity)
		{
			return;
		}
		int* next = (int*)data->next;
		byte* keys = data->keys;
		byte* values = data->values;
		int* firstFreeTLS = data->firstFreeTLS;
		do
		{
			if (UnsafeUtility.ReadArrayElement<TKey>(keys, num).Equals(key) && UnsafeUtility.ReadArrayElement<TValueEQ>(values, num).Equals(value))
			{
				int num2 = next[num];
				next[num] = *firstFreeTLS;
				*firstFreeTLS = num;
				num = (*ptr = num2);
			}
			else
			{
				ptr = next + num;
				num = *ptr;
			}
		}
		while ((uint)num < keyCapacity);
	}

	internal unsafe static bool TryGetFirstValueAtomic(UnsafeParallelHashMapData* data, TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
	{
		it.key = key;
		if (data->allocatedIndexLength <= 0)
		{
			it.EntryIndex = (it.NextEntryIndex = -1);
			item = default(TValue);
			return false;
		}
		int* buckets = (int*)data->buckets;
		int num = key.GetHashCode() & data->bucketCapacityMask;
		it.EntryIndex = (it.NextEntryIndex = buckets[num]);
		return TryGetNextValueAtomic(data, out item, ref it);
	}

	internal unsafe static bool TryGetNextValueAtomic(UnsafeParallelHashMapData* data, out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
	{
		int num = it.NextEntryIndex;
		it.NextEntryIndex = -1;
		it.EntryIndex = -1;
		item = default(TValue);
		if (num < 0 || num >= data->keyCapacity)
		{
			return false;
		}
		int* next = (int*)data->next;
		while (!UnsafeUtility.ReadArrayElement<TKey>(data->keys, num).Equals(it.key))
		{
			num = next[num];
			if (num < 0 || num >= data->keyCapacity)
			{
				return false;
			}
		}
		it.NextEntryIndex = next[num];
		it.EntryIndex = num;
		item = UnsafeUtility.ReadArrayElement<TValue>(data->values, num);
		return true;
	}

	internal unsafe static bool SetValue(UnsafeParallelHashMapData* data, ref NativeParallelMultiHashMapIterator<TKey> it, ref TValue item)
	{
		int entryIndex = it.EntryIndex;
		if (entryIndex < 0 || entryIndex >= data->keyCapacity)
		{
			return false;
		}
		UnsafeUtility.WriteArrayElement(data->values, entryIndex, item);
		return true;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckOutOfCapacity(int idx, int keyCapacity)
	{
		if (idx >= keyCapacity)
		{
			throw new InvalidOperationException($"nextPtr idx {idx} beyond capacity {keyCapacity}");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe static void CheckIndexOutOfBounds(UnsafeParallelHashMapData* data, int idx)
	{
		if (idx < 0 || idx >= data->keyCapacity)
		{
			throw new InvalidOperationException("Internal HashMap error");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void ThrowFull()
	{
		throw new InvalidOperationException("HashMap is full");
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void ThrowInvalidIterator()
	{
		throw new InvalidOperationException("Invalid iterator passed to HashMap remove");
	}
}
