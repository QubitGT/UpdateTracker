using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerDisplay("Count = {Count()}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(UnsafeParallelHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct UnsafeParallelHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KeyValue<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	[DebuggerDisplay("Count = {m_HashMapData.Count()}, Capacity = {m_HashMapData.Capacity}, IsCreated = {m_HashMapData.IsCreated}, IsEmpty = {IsEmpty}")]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ReadOnly : IEnumerable<KeyValue<TKey, TValue>>, IEnumerable
	{
		internal UnsafeParallelHashMap<TKey, TValue> m_HashMapData;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_HashMapData.IsCreated;
			}
		}

		public readonly bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!IsCreated)
				{
					return true;
				}
				return m_HashMapData.IsEmpty;
			}
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_HashMapData.Capacity;
			}
		}

		public readonly TValue this[TKey key]
		{
			get
			{
				if (m_HashMapData.TryGetValue(key, out var item))
				{
					return item;
				}
				return default(TValue);
			}
		}

		internal ReadOnly(UnsafeParallelHashMap<TKey, TValue> hashMapData)
		{
			m_HashMapData = hashMapData;
		}

		public readonly int Count()
		{
			return m_HashMapData.Count();
		}

		public readonly bool TryGetValue(TKey key, out TValue item)
		{
			return m_HashMapData.TryGetValue(key, out item);
		}

		public readonly bool ContainsKey(TKey key)
		{
			return m_HashMapData.ContainsKey(key);
		}

		public readonly NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_HashMapData.GetKeyArray(allocator);
		}

		public readonly NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_HashMapData.GetValueArray(allocator);
		}

		public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
		{
			return m_HashMapData.GetKeyValueArrays(allocator);
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private readonly void ThrowKeyNotPresent(TKey key)
		{
			throw new ArgumentException($"Key: {key} is not present in the NativeParallelHashMap.");
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_HashMapData.m_Buffer);
			return result;
		}

		IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ParallelWriter
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeParallelHashMapData* m_Buffer;

		[NativeSetThreadIndex]
		internal int m_ThreadIndex;

		public int ThreadIndex => m_ThreadIndex;

		public unsafe readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Buffer->keyCapacity;
			}
		}

		public unsafe bool TryAdd(TKey key, TValue item)
		{
			return UnsafeParallelHashMapBase<TKey, TValue>.TryAddAtomic(m_Buffer, key, item, m_ThreadIndex);
		}

		public unsafe bool TryAdd(TKey key, TValue item, int threadIndexOverride)
		{
			return UnsafeParallelHashMapBase<TKey, TValue>.TryAddAtomic(m_Buffer, key, item, threadIndexOverride);
		}
	}

	public struct Enumerator : IEnumerator<KeyValue<TKey, TValue>>, IEnumerator, IDisposable
	{
		internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

		public KeyValue<TKey, TValue> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.GetCurrent<TKey, TValue>();
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return m_Enumerator.MoveNext();
		}

		public void Reset()
		{
			m_Enumerator.Reset();
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeParallelHashMapData* m_Buffer;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Buffer != null;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (IsCreated)
			{
				return UnsafeParallelHashMapData.IsEmpty(m_Buffer);
			}
			return true;
		}
	}

	public unsafe int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_Buffer->keyCapacity;
		}
		set
		{
			UnsafeParallelHashMapData.ReallocateHashMap<TKey, TValue>(m_Buffer, value, UnsafeParallelHashMapData.GetBucketSize(value), m_AllocatorLabel);
		}
	}

	public unsafe TValue this[TKey key]
	{
		get
		{
			TryGetValue(key, out var item);
			return item;
		}
		set
		{
			if (UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out var _, out var it))
			{
				UnsafeParallelHashMapBase<TKey, TValue>.SetValue(m_Buffer, ref it, ref value);
			}
			else
			{
				UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, value, isMultiHashMap: false, m_AllocatorLabel);
			}
		}
	}

	public unsafe UnsafeParallelHashMap(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_AllocatorLabel = allocator;
		UnsafeParallelHashMapData.AllocateHashMap<TKey, TValue>(capacity, capacity * 2, allocator, out m_Buffer);
		Clear();
	}

	public unsafe readonly int Count()
	{
		return UnsafeParallelHashMapData.GetCount(m_Buffer);
	}

	public unsafe void Clear()
	{
		UnsafeParallelHashMapBase<TKey, TValue>.Clear(m_Buffer);
	}

	public unsafe bool TryAdd(TKey key, TValue item)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, isMultiHashMap: false, m_AllocatorLabel);
	}

	public unsafe void Add(TKey key, TValue item)
	{
		UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, isMultiHashMap: false, m_AllocatorLabel);
	}

	public unsafe bool Remove(TKey key)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, key, isMultiHashMap: false) != 0;
	}

	public unsafe bool TryGetValue(TKey key, out TValue item)
	{
		NativeParallelMultiHashMapIterator<TKey> it;
		return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out item, out it);
	}

	public unsafe bool ContainsKey(TKey key)
	{
		TValue item;
		NativeParallelMultiHashMapIterator<TKey> it;
		return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out item, out it);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeParallelHashMapData.DeallocateHashMap(m_Buffer, m_AllocatorLabel);
			m_Buffer = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		UnsafeParallelHashMapDisposeJob jobData = default(UnsafeParallelHashMapDisposeJob);
		jobData.Data = m_Buffer;
		jobData.Allocator = m_AllocatorLabel;
		JobHandle result = jobData.Schedule(inputDeps);
		m_Buffer = null;
		return result;
	}

	public unsafe NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TKey> result = CollectionHelper.CreateNativeArray<TKey>(UnsafeParallelHashMapData.GetCount(m_Buffer), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetKeyArray(m_Buffer, result);
		return result;
	}

	public unsafe NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TValue> result = CollectionHelper.CreateNativeArray<TValue>(UnsafeParallelHashMapData.GetCount(m_Buffer), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetValueArray(m_Buffer, result);
		return result;
	}

	public unsafe NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		NativeKeyValueArrays<TKey, TValue> result = new NativeKeyValueArrays<TKey, TValue>(UnsafeParallelHashMapData.GetCount(m_Buffer), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetKeyValueArrays(m_Buffer, result);
		return result;
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_ThreadIndex = 0;
		result.m_Buffer = m_Buffer;
		return result;
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(this);
	}

	public unsafe Enumerator GetEnumerator()
	{
		Enumerator result = default(Enumerator);
		result.m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Buffer);
		return result;
	}

	IEnumerator<KeyValue<TKey, TValue>> IEnumerable<KeyValue<TKey, TValue>>.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
