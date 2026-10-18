using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerTypeProxy(typeof(UnsafeParallelMultiHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct UnsafeParallelMultiHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KeyValue<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	public struct Enumerator : IEnumerator<TValue>, IEnumerator, IDisposable
	{
		internal UnsafeParallelMultiHashMap<TKey, TValue> hashmap;

		internal TKey key;

		internal bool isFirst;

		private TValue value;

		private NativeParallelMultiHashMapIterator<TKey> iterator;

		public TValue Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return value;
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (isFirst)
			{
				isFirst = false;
				return hashmap.TryGetFirstValue(key, out value, out iterator);
			}
			return hashmap.TryGetNextValue(out value, ref iterator);
		}

		public void Reset()
		{
			isFirst = true;
		}

		public Enumerator GetEnumerator()
		{
			return this;
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

		public unsafe readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Buffer->keyCapacity;
			}
		}

		public unsafe void Add(TKey key, TValue item)
		{
			UnsafeParallelHashMapBase<TKey, TValue>.AddAtomicMulti(m_Buffer, key, item, m_ThreadIndex);
		}

		public unsafe void Add(TKey key, TValue item, int threadIndexOverride)
		{
			UnsafeParallelHashMapBase<TKey, TValue>.AddAtomicMulti(m_Buffer, key, item, threadIndexOverride);
		}
	}

	public struct KeyValueEnumerator : IEnumerator<KeyValue<TKey, TValue>>, IEnumerator, IDisposable
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

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ReadOnly : IEnumerable<KeyValue<TKey, TValue>>, IEnumerable
	{
		internal UnsafeParallelMultiHashMap<TKey, TValue> m_MultiHashMapData;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_MultiHashMapData.IsCreated;
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
				return m_MultiHashMapData.IsEmpty;
			}
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_MultiHashMapData.Capacity;
			}
		}

		internal ReadOnly(UnsafeParallelMultiHashMap<TKey, TValue> container)
		{
			m_MultiHashMapData = container;
		}

		public readonly int Count()
		{
			return m_MultiHashMapData.Count();
		}

		public readonly bool TryGetFirstValue(TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
		{
			return m_MultiHashMapData.TryGetFirstValue(key, out item, out it);
		}

		public readonly bool TryGetNextValue(out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
		{
			return m_MultiHashMapData.TryGetNextValue(out item, ref it);
		}

		public readonly bool ContainsKey(TKey key)
		{
			return m_MultiHashMapData.ContainsKey(key);
		}

		public readonly NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_MultiHashMapData.GetKeyArray(allocator);
		}

		public readonly NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_MultiHashMapData.GetValueArray(allocator);
		}

		public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
		{
			return m_MultiHashMapData.GetKeyValueArrays(allocator);
		}

		public unsafe KeyValueEnumerator GetEnumerator()
		{
			KeyValueEnumerator result = default(KeyValueEnumerator);
			result.m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_MultiHashMapData.m_Buffer);
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

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeParallelHashMapData* m_Buffer;

	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

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

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Buffer != null;
		}
	}

	public unsafe UnsafeParallelMultiHashMap(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_AllocatorLabel = allocator;
		UnsafeParallelHashMapData.AllocateHashMap<TKey, TValue>(capacity, capacity * 2, allocator, out m_Buffer);
		Clear();
	}

	public unsafe readonly int Count()
	{
		if (m_Buffer->allocatedIndexLength <= 0)
		{
			return 0;
		}
		return UnsafeParallelHashMapData.GetCount(m_Buffer);
	}

	public unsafe void Clear()
	{
		UnsafeParallelHashMapBase<TKey, TValue>.Clear(m_Buffer);
	}

	public unsafe void Add(TKey key, TValue item)
	{
		UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_Buffer, key, item, isMultiHashMap: true, m_AllocatorLabel);
	}

	public unsafe int Remove(TKey key)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, key, isMultiHashMap: true);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void Remove<TValueEQ>(TKey key, TValueEQ value) where TValueEQ : unmanaged, IEquatable<TValueEQ>
	{
		UnsafeParallelHashMapBase<TKey, TValueEQ>.RemoveKeyValue(m_Buffer, key, value);
	}

	public unsafe void Remove(NativeParallelMultiHashMapIterator<TKey> it)
	{
		UnsafeParallelHashMapBase<TKey, TValue>.Remove(m_Buffer, it);
	}

	public unsafe readonly bool TryGetFirstValue(TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.TryGetFirstValueAtomic(m_Buffer, key, out item, out it);
	}

	public unsafe readonly bool TryGetNextValue(out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.TryGetNextValueAtomic(m_Buffer, out item, ref it);
	}

	public readonly bool ContainsKey(TKey key)
	{
		TValue item;
		NativeParallelMultiHashMapIterator<TKey> it;
		return TryGetFirstValue(key, out item, out it);
	}

	public readonly int CountValuesForKey(TKey key)
	{
		if (!TryGetFirstValue(key, out var item, out var it))
		{
			return 0;
		}
		int num = 1;
		while (TryGetNextValue(out item, ref it))
		{
			num++;
		}
		return num;
	}

	public unsafe bool SetValue(TValue item, NativeParallelMultiHashMapIterator<TKey> it)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.SetValue(m_Buffer, ref it, ref item);
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

	public unsafe readonly NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TKey> result = CollectionHelper.CreateNativeArray<TKey>(Count(), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetKeyArray(m_Buffer, result);
		return result;
	}

	public unsafe readonly NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TValue> result = CollectionHelper.CreateNativeArray<TValue>(Count(), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetValueArray(m_Buffer, result);
		return result;
	}

	public unsafe readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		NativeKeyValueArrays<TKey, TValue> result = new NativeKeyValueArrays<TKey, TValue>(Count(), allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeParallelHashMapData.GetKeyValueArrays(m_Buffer, result);
		return result;
	}

	public Enumerator GetValuesForKey(TKey key)
	{
		Enumerator result = default(Enumerator);
		result.hashmap = this;
		result.key = key;
		result.isFirst = true;
		return result;
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_ThreadIndex = 0;
		result.m_Buffer = m_Buffer;
		return result;
	}

	public unsafe KeyValueEnumerator GetEnumerator()
	{
		KeyValueEnumerator result = default(KeyValueEnumerator);
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

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(this);
	}
}
