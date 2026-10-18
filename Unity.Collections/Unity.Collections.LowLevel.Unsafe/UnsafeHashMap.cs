using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerTypeProxy(typeof(UnsafeHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct UnsafeHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KVPair<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	public struct Enumerator : IEnumerator<KVPair<TKey, TValue>>, IEnumerator, IDisposable
	{
		internal HashMapHelper<TKey>.Enumerator m_Enumerator;

		public KVPair<TKey, TValue> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.GetCurrent<TValue>();
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
	public struct ReadOnly : IEnumerable<KVPair<TKey, TValue>>, IEnumerable
	{
		[NativeDisableUnsafePtrRestriction]
		internal HashMapHelper<TKey> m_Data;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data.IsCreated;
			}
		}

		public readonly bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data.IsEmpty;
			}
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data.Count;
			}
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data.Capacity;
			}
		}

		public readonly TValue this[TKey key]
		{
			get
			{
				m_Data.TryGetValue<TValue>(key, out var item);
				return item;
			}
		}

		internal ReadOnly(ref HashMapHelper<TKey> data)
		{
			m_Data = data;
		}

		public readonly bool TryGetValue(TKey key, out TValue item)
		{
			return m_Data.TryGetValue<TValue>(key, out item);
		}

		public readonly bool ContainsKey(TKey key)
		{
			return -1 != m_Data.Find(key);
		}

		public readonly NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data.GetKeyArray(allocator);
		}

		public readonly NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data.GetValueArray<TValue>(allocator);
		}

		public readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data.GetKeyValueArrays<TValue>(allocator);
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			fixed (HashMapHelper<TKey>* data = &m_Data)
			{
				Enumerator result = default(Enumerator);
				result.m_Enumerator = new HashMapHelper<TKey>.Enumerator(data);
				return result;
			}
		}

		IEnumerator<KVPair<TKey, TValue>> IEnumerable<KVPair<TKey, TValue>>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal HashMapHelper<TKey> m_Data;

	public readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.IsCreated;
		}
	}

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.IsEmpty;
		}
	}

	public readonly int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.Count;
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_Data.Capacity;
		}
		set
		{
			m_Data.Resize(value);
		}
	}

	public unsafe TValue this[TKey key]
	{
		get
		{
			m_Data.TryGetValue<TValue>(key, out var item);
			return item;
		}
		set
		{
			int num = m_Data.Find(key);
			if (-1 != num)
			{
				UnsafeUtility.WriteArrayElement(m_Data.Ptr, num, value);
			}
			else
			{
				TryAdd(key, value);
			}
		}
	}

	public unsafe UnsafeHashMap(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = default(HashMapHelper<TKey>);
		m_Data.Init(initialCapacity, sizeof(TValue), 256, allocator);
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			m_Data.Dispose();
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		UnsafeDisposeJob jobData = default(UnsafeDisposeJob);
		jobData.Ptr = m_Data.Ptr;
		jobData.Allocator = m_Data.Allocator;
		JobHandle result = jobData.Schedule(inputDeps);
		m_Data = default(HashMapHelper<TKey>);
		return result;
	}

	public void Clear()
	{
		m_Data.Clear();
	}

	public unsafe bool TryAdd(TKey key, TValue item)
	{
		int num = m_Data.TryAdd(in key);
		if (-1 != num)
		{
			UnsafeUtility.WriteArrayElement(m_Data.Ptr, num, item);
			return true;
		}
		return false;
	}

	public void Add(TKey key, TValue item)
	{
		TryAdd(key, item);
	}

	public bool Remove(TKey key)
	{
		return -1 != m_Data.TryRemove(key);
	}

	public bool TryGetValue(TKey key, out TValue item)
	{
		return m_Data.TryGetValue<TValue>(key, out item);
	}

	public bool ContainsKey(TKey key)
	{
		return -1 != m_Data.Find(key);
	}

	public void TrimExcess()
	{
		m_Data.TrimExcess();
	}

	public NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data.GetKeyArray(allocator);
	}

	public NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data.GetValueArray<TValue>(allocator);
	}

	public NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data.GetKeyValueArrays<TValue>(allocator);
	}

	public unsafe Enumerator GetEnumerator()
	{
		fixed (HashMapHelper<TKey>* data = &m_Data)
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new HashMapHelper<TKey>.Enumerator(data);
			return result;
		}
	}

	IEnumerator<KVPair<TKey, TValue>> IEnumerable<KVPair<TKey, TValue>>.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(ref m_Data);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowKeyNotPresent(TKey key)
	{
		throw new ArgumentException($"Key: {key} is not present.");
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowKeyAlreadyAdded(TKey key)
	{
		throw new ArgumentException($"An item with the same key has already been added: {key}");
	}
}
