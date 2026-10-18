using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerDisplay("Count = {m_HashMapData.Count()}, Capacity = {m_HashMapData.Capacity}, IsCreated = {m_HashMapData.IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(NativeParallelHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct NativeParallelHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KeyValue<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
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
		private readonly void CheckRead()
		{
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

	[NativeContainer]
	[NativeContainerIsAtomicWriteOnly]
	[DebuggerDisplay("Capacity = {m_Writer.Capacity}")]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ParallelWriter
	{
		internal UnsafeParallelHashMap<TKey, TValue>.ParallelWriter m_Writer;

		public int ThreadIndex => m_Writer.m_ThreadIndex;

		[Obsolete("'m_ThreadIndex' has been deprecated; use 'ThreadIndex' instead. (UnityUpgradable) -> ThreadIndex")]
		public int m_ThreadIndex => m_Writer.m_ThreadIndex;

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Writer.Capacity;
			}
		}

		public bool TryAdd(TKey key, TValue item)
		{
			return m_Writer.TryAdd(key, item);
		}

		public bool TryAdd(TKey key, TValue item, int threadIndexOverride)
		{
			return m_Writer.TryAdd(key, item, threadIndexOverride);
		}
	}

	[NativeContainer]
	[NativeContainerIsReadOnly]
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

	internal UnsafeParallelHashMap<TKey, TValue> m_HashMapData;

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

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_HashMapData.Capacity;
		}
		set
		{
			m_HashMapData.Capacity = value;
		}
	}

	public TValue this[TKey key]
	{
		get
		{
			if (m_HashMapData.TryGetValue(key, out var item))
			{
				return item;
			}
			return default(TValue);
		}
		set
		{
			m_HashMapData[key] = value;
		}
	}

	public readonly bool IsCreated => m_HashMapData.IsCreated;

	public NativeParallelHashMap(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_HashMapData = new UnsafeParallelHashMap<TKey, TValue>(capacity, allocator);
	}

	public int Count()
	{
		return m_HashMapData.Count();
	}

	public void Clear()
	{
		m_HashMapData.Clear();
	}

	public unsafe bool TryAdd(TKey key, TValue item)
	{
		return UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_HashMapData.m_Buffer, key, item, isMultiHashMap: false, m_HashMapData.m_AllocatorLabel);
	}

	public unsafe void Add(TKey key, TValue item)
	{
		UnsafeParallelHashMapBase<TKey, TValue>.TryAdd(m_HashMapData.m_Buffer, key, item, isMultiHashMap: false, m_HashMapData.m_AllocatorLabel);
	}

	public bool Remove(TKey key)
	{
		return m_HashMapData.Remove(key);
	}

	public bool TryGetValue(TKey key, out TValue item)
	{
		return m_HashMapData.TryGetValue(key, out item);
	}

	public bool ContainsKey(TKey key)
	{
		return m_HashMapData.ContainsKey(key);
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			m_HashMapData.Dispose();
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
			m_Buffer = m_HashMapData.m_Buffer,
			m_AllocatorLabel = m_HashMapData.m_AllocatorLabel
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_HashMapData.m_Buffer = null;
		return result;
	}

	public NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_HashMapData.GetKeyArray(allocator);
	}

	public NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_HashMapData.GetValueArray(allocator);
	}

	public NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		return m_HashMapData.GetKeyValueArrays(allocator);
	}

	public ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_Writer = m_HashMapData.AsParallelWriter();
		return result;
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(m_HashMapData);
	}

	public unsafe Enumerator GetEnumerator()
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

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowKeyNotPresent(TKey key)
	{
		throw new ArgumentException($"Key: {key} is not present in the NativeParallelHashMap.");
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowKeyAlreadyAdded(TKey key)
	{
		throw new ArgumentException("An item with the same key has already been added", "key");
	}
}
