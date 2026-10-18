using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerTypeProxy(typeof(NativeParallelMultiHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct NativeParallelMultiHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KeyValue<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	[NativeContainer]
	[NativeContainerIsAtomicWriteOnly]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ParallelWriter
	{
		internal UnsafeParallelMultiHashMap<TKey, TValue>.ParallelWriter m_Writer;

		public int m_ThreadIndex => m_Writer.m_ThreadIndex;

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Writer.Capacity;
			}
		}

		public void Add(TKey key, TValue item)
		{
			m_Writer.Add(key, item);
		}

		public void Add(TKey key, TValue item, int threadIndexOverride)
		{
			m_Writer.Add(key, item, threadIndexOverride);
		}
	}

	public struct Enumerator : IEnumerator<TValue>, IEnumerator, IDisposable
	{
		internal NativeParallelMultiHashMap<TKey, TValue> hashmap;

		internal TKey key;

		internal byte isFirst;

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
			if (isFirst == 1)
			{
				isFirst = 0;
				return hashmap.TryGetFirstValue(key, out value, out iterator);
			}
			return hashmap.TryGetNextValue(out value, ref iterator);
		}

		public void Reset()
		{
			isFirst = 1;
		}

		public Enumerator GetEnumerator()
		{
			return this;
		}
	}

	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct KeyValueEnumerator : IEnumerator<KeyValue<TKey, TValue>>, IEnumerator, IDisposable
	{
		internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

		public readonly KeyValue<TKey, TValue> Current
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

	[NativeContainer]
	[NativeContainerIsReadOnly]
	[DebuggerTypeProxy(typeof(NativeParallelHashMapDebuggerTypeProxy<, >))]
	[DebuggerDisplay("Count = {m_HashMapData.Count()}, Capacity = {m_HashMapData.Capacity}, IsCreated = {m_HashMapData.IsCreated}, IsEmpty = {IsEmpty}")]
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

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private readonly void CheckRead()
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private readonly void ThrowKeyNotPresent(TKey key)
		{
			throw new ArgumentException($"Key: {key} is not present in the NativeParallelHashMap.");
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

	internal UnsafeParallelMultiHashMap<TKey, TValue> m_MultiHashMapData;

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_MultiHashMapData.IsEmpty;
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_MultiHashMapData.Capacity;
		}
		set
		{
			m_MultiHashMapData.Capacity = value;
		}
	}

	public readonly bool IsCreated => m_MultiHashMapData.IsCreated;

	public NativeParallelMultiHashMap(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		this = default(NativeParallelMultiHashMap<TKey, TValue>);
		Initialize(capacity, ref allocator);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal void Initialize<U>(int capacity, ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		m_MultiHashMapData = new UnsafeParallelMultiHashMap<TKey, TValue>(capacity, allocator.Handle);
	}

	public readonly int Count()
	{
		return m_MultiHashMapData.Count();
	}

	public void Clear()
	{
		m_MultiHashMapData.Clear();
	}

	public void Add(TKey key, TValue item)
	{
		m_MultiHashMapData.Add(key, item);
	}

	public int Remove(TKey key)
	{
		return m_MultiHashMapData.Remove(key);
	}

	public void Remove(NativeParallelMultiHashMapIterator<TKey> it)
	{
		m_MultiHashMapData.Remove(it);
	}

	public bool TryGetFirstValue(TKey key, out TValue item, out NativeParallelMultiHashMapIterator<TKey> it)
	{
		return m_MultiHashMapData.TryGetFirstValue(key, out item, out it);
	}

	public bool TryGetNextValue(out TValue item, ref NativeParallelMultiHashMapIterator<TKey> it)
	{
		return m_MultiHashMapData.TryGetNextValue(out item, ref it);
	}

	public bool ContainsKey(TKey key)
	{
		TValue item;
		NativeParallelMultiHashMapIterator<TKey> it;
		return TryGetFirstValue(key, out item, out it);
	}

	public int CountValuesForKey(TKey key)
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

	public bool SetValue(TValue item, NativeParallelMultiHashMapIterator<TKey> it)
	{
		return m_MultiHashMapData.SetValue(item, it);
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			m_MultiHashMapData.Dispose();
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		UnsafeParallelHashMapDataDisposeJob jobData = default(UnsafeParallelHashMapDataDisposeJob);
		jobData.Data = new UnsafeParallelHashMapDataDispose
		{
			m_Buffer = m_MultiHashMapData.m_Buffer,
			m_AllocatorLabel = m_MultiHashMapData.m_AllocatorLabel
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_MultiHashMapData.m_Buffer = null;
		return result;
	}

	public NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_MultiHashMapData.GetKeyArray(allocator);
	}

	public NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_MultiHashMapData.GetValueArray(allocator);
	}

	public NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		return m_MultiHashMapData.GetKeyValueArrays(allocator);
	}

	public ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_Writer = m_MultiHashMapData.AsParallelWriter();
		return result;
	}

	public Enumerator GetValuesForKey(TKey key)
	{
		Enumerator result = default(Enumerator);
		result.hashmap = this;
		result.key = key;
		result.isFirst = 1;
		return result;
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

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(m_MultiHashMapData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckWrite()
	{
	}
}
