using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
internal struct HashMapHelper<TKey> where TKey : unmanaged, IEquatable<TKey>
{
	internal struct Enumerator
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe HashMapHelper<TKey>* m_Data;

		internal int m_Index;

		internal int m_BucketIndex;

		internal int m_NextIndex;

		internal unsafe Enumerator(HashMapHelper<TKey>* data)
		{
			m_Data = data;
			m_Index = -1;
			m_BucketIndex = 0;
			m_NextIndex = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe bool MoveNext()
		{
			return m_Data->MoveNext(ref m_BucketIndex, ref m_NextIndex, out m_Index);
		}

		internal void Reset()
		{
			m_Index = -1;
			m_BucketIndex = 0;
			m_NextIndex = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe KVPair<TKey, TValue> GetCurrent<TValue>() where TValue : unmanaged
		{
			KVPair<TKey, TValue> result = default(KVPair<TKey, TValue>);
			result.m_Data = m_Data;
			result.m_Index = m_Index;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal unsafe TKey GetCurrentKey()
		{
			if (m_Index != -1)
			{
				return m_Data->Keys[m_Index];
			}
			return default(TKey);
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe byte* Ptr;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe TKey* Keys;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe int* Next;

	[NativeDisableUnsafePtrRestriction]
	internal unsafe int* Buckets;

	internal int Count;

	internal int Capacity;

	internal int Log2MinGrowth;

	internal int BucketCapacity;

	internal int AllocatedIndex;

	internal int FirstFreeIdx;

	internal int SizeOfTValue;

	internal AllocatorManager.AllocatorHandle Allocator;

	internal const int kMinimumCapacity = 256;

	internal unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Ptr != null;
		}
	}

	internal readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (IsCreated)
			{
				return Count == 0;
			}
			return true;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int CalcCapacityCeilPow2(int capacity)
	{
		capacity = math.max(math.max(1, Count), capacity);
		return math.ceilpow2(math.max(capacity, 1 << Log2MinGrowth));
	}

	internal static int GetBucketSize(int capacity)
	{
		return capacity * 2;
	}

	internal unsafe void Clear()
	{
		UnsafeUtility.MemSet(Buckets, byte.MaxValue, BucketCapacity * 4);
		UnsafeUtility.MemSet(Next, byte.MaxValue, Capacity * 4);
		Count = 0;
		FirstFreeIdx = -1;
		AllocatedIndex = 0;
	}

	internal unsafe void Init(int capacity, int sizeOfValueT, int minGrowth, AllocatorManager.AllocatorHandle allocator)
	{
		Count = 0;
		Log2MinGrowth = (byte)(32 - math.lzcnt(math.max(1, minGrowth) - 1));
		capacity = CalcCapacityCeilPow2(capacity);
		Capacity = capacity;
		BucketCapacity = GetBucketSize(capacity);
		Allocator = allocator;
		SizeOfTValue = sizeOfValueT;
		int outKeyOffset;
		int outNextOffset;
		int outBucketOffset;
		int num = CalculateDataSize(capacity, BucketCapacity, sizeOfValueT, out outKeyOffset, out outNextOffset, out outBucketOffset);
		Ptr = (byte*)Memory.Unmanaged.Allocate(num, 64, allocator);
		Keys = (TKey*)(Ptr + outKeyOffset);
		Next = (int*)(Ptr + outNextOffset);
		Buckets = (int*)(Ptr + outBucketOffset);
		Clear();
	}

	internal unsafe void Dispose()
	{
		Memory.Unmanaged.Free(Ptr, Allocator);
		Ptr = null;
		Keys = null;
		Next = null;
		Buckets = null;
		Count = 0;
		BucketCapacity = 0;
	}

	internal unsafe static HashMapHelper<TKey>* Alloc(int capacity, int sizeOfValueT, int minGrowth, AllocatorManager.AllocatorHandle allocator)
	{
		HashMapHelper<TKey>* ptr = (HashMapHelper<TKey>*)Memory.Unmanaged.Allocate(sizeof(HashMapHelper<TKey>), UnsafeUtility.AlignOf<HashMapHelper<TKey>>(), allocator);
		ptr->Init(capacity, sizeOfValueT, minGrowth, allocator);
		return ptr;
	}

	internal unsafe static void Free(HashMapHelper<TKey>* data)
	{
		if (data == null)
		{
			throw new InvalidOperationException("Hash based container has yet to be created or has been destroyed!");
		}
		data->Dispose();
		Memory.Unmanaged.Free(data, data->Allocator);
	}

	internal void Resize(int newCapacity)
	{
		newCapacity = math.max(newCapacity, Count);
		int num = math.ceilpow2(GetBucketSize(newCapacity));
		if (Capacity != newCapacity || BucketCapacity != num)
		{
			ResizeExact(newCapacity, num);
		}
	}

	internal unsafe void ResizeExact(int newCapacity, int newBucketCapacity)
	{
		int outKeyOffset;
		int outNextOffset;
		int outBucketOffset;
		int num = CalculateDataSize(newCapacity, newBucketCapacity, SizeOfTValue, out outKeyOffset, out outNextOffset, out outBucketOffset);
		byte* ptr = Ptr;
		TKey* keys = Keys;
		int* next = Next;
		int* buckets = Buckets;
		int bucketCapacity = BucketCapacity;
		Ptr = (byte*)Memory.Unmanaged.Allocate(num, 64, Allocator);
		Keys = (TKey*)(Ptr + outKeyOffset);
		Next = (int*)(Ptr + outNextOffset);
		Buckets = (int*)(Ptr + outBucketOffset);
		Capacity = newCapacity;
		BucketCapacity = newBucketCapacity;
		Clear();
		int i = 0;
		for (int num2 = bucketCapacity; i < num2; i++)
		{
			for (int num3 = buckets[i]; num3 != -1; num3 = next[num3])
			{
				int num4 = TryAdd(in keys[num3]);
				UnsafeUtility.MemCpy(Ptr + SizeOfTValue * num4, ptr + SizeOfTValue * num3, SizeOfTValue);
			}
		}
		Memory.Unmanaged.Free(ptr, Allocator);
	}

	internal void TrimExcess()
	{
		int num = CalcCapacityCeilPow2(Count);
		ResizeExact(num, GetBucketSize(num));
	}

	internal unsafe static int CalculateDataSize(int capacity, int bucketCapacity, int sizeOfTValue, out int outKeyOffset, out int outNextOffset, out int outBucketOffset)
	{
		int num = 4;
		int num2 = sizeOfTValue * capacity;
		int num3 = sizeof(TKey) * capacity;
		int num4 = num * capacity;
		int num5 = num * bucketCapacity;
		int result = num2 + num3 + num4 + num5;
		outKeyOffset = num2;
		outNextOffset = outKeyOffset + num3;
		outBucketOffset = outNextOffset + num4;
		return result;
	}

	internal unsafe readonly int GetCount()
	{
		if (AllocatedIndex <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int num2 = FirstFreeIdx; num2 >= 0; num2 = Next[num2])
		{
			num++;
		}
		return math.min(Capacity, AllocatedIndex) - num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetBucket(in TKey key)
	{
		return (int)((uint)key.GetHashCode() & (BucketCapacity - 1));
	}

	internal unsafe int TryAdd(in TKey key)
	{
		if (-1 == Find(key))
		{
			if (AllocatedIndex >= Capacity && FirstFreeIdx < 0)
			{
				int newCapacity = CalcCapacityCeilPow2(Capacity + (1 << Log2MinGrowth));
				Resize(newCapacity);
			}
			int num = FirstFreeIdx;
			if (num >= 0)
			{
				FirstFreeIdx = Next[num];
			}
			else
			{
				num = AllocatedIndex++;
			}
			UnsafeUtility.WriteArrayElement(Keys, num, key);
			int bucket = GetBucket(in key);
			Next[num] = Buckets[bucket];
			Buckets[bucket] = num;
			Count++;
			return num;
		}
		return -1;
	}

	internal unsafe int Find(TKey key)
	{
		if (AllocatedIndex > 0)
		{
			int bucket = GetBucket(in key);
			int num = Buckets[bucket];
			if ((uint)num < (uint)Capacity)
			{
				int* next = Next;
				while (!UnsafeUtility.ReadArrayElement<TKey>(Keys, num).Equals(key))
				{
					num = next[num];
					if ((uint)num >= (uint)Capacity)
					{
						return -1;
					}
				}
				return num;
			}
		}
		return -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe bool TryGetValue<TValue>(TKey key, out TValue item) where TValue : unmanaged
	{
		int num = Find(key);
		if (-1 != num)
		{
			item = UnsafeUtility.ReadArrayElement<TValue>(Ptr, num);
			return true;
		}
		item = default(TValue);
		return false;
	}

	internal unsafe int TryRemove(TKey key)
	{
		if (Capacity != 0)
		{
			int num = 0;
			int bucket = GetBucket(in key);
			int num2 = -1;
			int num3 = Buckets[bucket];
			while (num3 >= 0 && num3 < Capacity)
			{
				if (UnsafeUtility.ReadArrayElement<TKey>(Keys, num3).Equals(key))
				{
					num++;
					if (num2 < 0)
					{
						Buckets[bucket] = Next[num3];
					}
					else
					{
						Next[num2] = Next[num3];
					}
					int num4 = Next[num3];
					Next[num3] = FirstFreeIdx;
					FirstFreeIdx = num3;
					num3 = num4;
					break;
				}
				num2 = num3;
				num3 = Next[num3];
			}
			Count -= num;
			if (num == 0)
			{
				return -1;
			}
			return num;
		}
		return -1;
	}

	internal unsafe bool MoveNextSearch(ref int bucketIndex, ref int nextIndex, out int index)
	{
		int i = bucketIndex;
		for (int bucketCapacity = BucketCapacity; i < bucketCapacity; i++)
		{
			int num = Buckets[i];
			if (num != -1)
			{
				index = num;
				bucketIndex = i + 1;
				nextIndex = Next[num];
				return true;
			}
		}
		index = -1;
		bucketIndex = BucketCapacity;
		nextIndex = -1;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe bool MoveNext(ref int bucketIndex, ref int nextIndex, out int index)
	{
		if (nextIndex != -1)
		{
			index = nextIndex;
			nextIndex = Next[nextIndex];
			return true;
		}
		return MoveNextSearch(ref bucketIndex, ref nextIndex, out index);
	}

	internal unsafe NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TKey> result = CollectionHelper.CreateNativeArray<TKey>(Count, allocator, NativeArrayOptions.UninitializedMemory);
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (int bucketCapacity = BucketCapacity; i < bucketCapacity; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num2 = Buckets[i]; num2 != -1; num2 = Next[num2])
			{
				result[num++] = UnsafeUtility.ReadArrayElement<TKey>(Keys, num2);
			}
		}
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe NativeArray<TValue> GetValueArray<TValue>(AllocatorManager.AllocatorHandle allocator) where TValue : unmanaged
	{
		NativeArray<TValue> result = CollectionHelper.CreateNativeArray<TValue>(Count, allocator, NativeArrayOptions.UninitializedMemory);
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (int bucketCapacity = BucketCapacity; i < bucketCapacity; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num2 = Buckets[i]; num2 != -1; num2 = Next[num2])
			{
				result[num++] = UnsafeUtility.ReadArrayElement<TValue>(Ptr, num2);
			}
		}
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays<TValue>(AllocatorManager.AllocatorHandle allocator) where TValue : unmanaged
	{
		NativeKeyValueArrays<TKey, TValue> result = new NativeKeyValueArrays<TKey, TValue>(Count, allocator, NativeArrayOptions.UninitializedMemory);
		int i = 0;
		int num = 0;
		int length = result.Length;
		for (int bucketCapacity = BucketCapacity; i < bucketCapacity; i++)
		{
			if (num >= length)
			{
				break;
			}
			for (int num2 = Buckets[i]; num2 != -1; num2 = Next[num2])
			{
				result.Keys[num] = UnsafeUtility.ReadArrayElement<TKey>(Keys, num2);
				result.Values[num] = UnsafeUtility.ReadArrayElement<TValue>(Ptr, num2);
				num++;
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckIndexOutOfBounds(int idx)
	{
		if ((uint)idx >= (uint)Capacity)
		{
			throw new InvalidOperationException($"Internal HashMap error. idx {idx}");
		}
	}
}
