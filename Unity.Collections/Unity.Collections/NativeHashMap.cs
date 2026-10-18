using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerTypeProxy(typeof(NativeHashMapDebuggerTypeProxy<, >))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct NativeHashMap<TKey, TValue> : INativeDisposable, IDisposable, IEnumerable<KVPair<TKey, TValue>>, IEnumerable where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct Enumerator : IEnumerator<KVPair<TKey, TValue>>, IEnumerator, IDisposable
	{
		[NativeDisableUnsafePtrRestriction]
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

	[NativeContainer]
	[NativeContainerIsReadOnly]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public struct ReadOnly : IEnumerable<KVPair<TKey, TValue>>, IEnumerable
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe HashMapHelper<TKey>* m_Data;

		public unsafe readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (m_Data != null)
				{
					return m_Data->IsCreated;
				}
				return false;
			}
		}

		public unsafe readonly bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (!IsCreated)
				{
					return true;
				}
				return m_Data->IsEmpty;
			}
		}

		public unsafe readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data->Count;
			}
		}

		public unsafe readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data->Capacity;
			}
		}

		public unsafe readonly TValue this[TKey key]
		{
			get
			{
				m_Data->TryGetValue<TValue>(key, out var item);
				return item;
			}
		}

		internal unsafe ReadOnly(ref NativeHashMap<TKey, TValue> data)
		{
			m_Data = data.m_Data;
		}

		public unsafe readonly bool TryGetValue(TKey key, out TValue item)
		{
			return m_Data->TryGetValue<TValue>(key, out item);
		}

		public unsafe readonly bool ContainsKey(TKey key)
		{
			return -1 != m_Data->Find(key);
		}

		public unsafe readonly NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data->GetKeyArray(allocator);
		}

		public unsafe readonly NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data->GetValueArray<TValue>(allocator);
		}

		public unsafe readonly NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data->GetKeyValueArrays<TValue>(allocator);
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new HashMapHelper<TKey>.Enumerator(m_Data);
			return result;
		}

		IEnumerator<KVPair<TKey, TValue>> IEnumerable<KVPair<TKey, TValue>>.GetEnumerator()
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

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private readonly void ThrowKeyNotPresent(TKey key)
		{
			throw new ArgumentException($"Key: {key} is not present.");
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe HashMapHelper<TKey>* m_Data;

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (m_Data != null)
			{
				return m_Data->IsCreated;
			}
			return false;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (!IsCreated)
			{
				return true;
			}
			return m_Data->IsEmpty;
		}
	}

	public unsafe readonly int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data->Count;
		}
	}

	public unsafe int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_Data->Capacity;
		}
		set
		{
			m_Data->Resize(value);
		}
	}

	public unsafe TValue this[TKey key]
	{
		get
		{
			m_Data->TryGetValue<TValue>(key, out var item);
			return item;
		}
		set
		{
			int num = m_Data->Find(key);
			if (-1 == num)
			{
				TryAdd(key, value);
			}
			else
			{
				UnsafeUtility.WriteArrayElement(m_Data->Ptr, num, value);
			}
		}
	}

	public unsafe NativeHashMap(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = HashMapHelper<TKey>.Alloc(initialCapacity, sizeof(TValue), 256, allocator);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			HashMapHelper<TKey>.Free(m_Data);
			m_Data = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeHashMapDisposeJob jobData = default(NativeHashMapDisposeJob);
		jobData.Data = new NativeHashMapDispose
		{
			m_HashMapData = (UnsafeHashMap<int, int>*)m_Data
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_Data = null;
		return result;
	}

	public unsafe void Clear()
	{
		m_Data->Clear();
	}

	public unsafe bool TryAdd(TKey key, TValue item)
	{
		int num = m_Data->TryAdd(in key);
		if (-1 != num)
		{
			UnsafeUtility.WriteArrayElement(m_Data->Ptr, num, item);
			return true;
		}
		return false;
	}

	public void Add(TKey key, TValue item)
	{
		TryAdd(key, item);
	}

	public unsafe bool Remove(TKey key)
	{
		return -1 != m_Data->TryRemove(key);
	}

	public unsafe bool TryGetValue(TKey key, out TValue item)
	{
		return m_Data->TryGetValue<TValue>(key, out item);
	}

	public unsafe bool ContainsKey(TKey key)
	{
		return -1 != m_Data->Find(key);
	}

	public unsafe void TrimExcess()
	{
		m_Data->TrimExcess();
	}

	public unsafe NativeArray<TKey> GetKeyArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data->GetKeyArray(allocator);
	}

	public unsafe NativeArray<TValue> GetValueArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data->GetValueArray<TValue>(allocator);
	}

	public unsafe NativeKeyValueArrays<TKey, TValue> GetKeyValueArrays(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data->GetKeyValueArrays<TValue>(allocator);
	}

	public unsafe Enumerator GetEnumerator()
	{
		Enumerator result = default(Enumerator);
		result.m_Enumerator = new HashMapHelper<TKey>.Enumerator(m_Data);
		return result;
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
		return new ReadOnly(ref this);
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
		throw new ArgumentException($"Key: {key} is not present.");
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowKeyAlreadyAdded(TKey key)
	{
		throw new ArgumentException($"An item with the same key has already been added: {key}");
	}
}
