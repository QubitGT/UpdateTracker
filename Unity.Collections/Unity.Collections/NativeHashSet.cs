using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerTypeProxy(typeof(NativeHashSetDebuggerTypeProxy<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct NativeHashSet<T> : INativeDisposable, IDisposable, IEnumerable<T>, IEnumerable where T : unmanaged, IEquatable<T>
{
	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[NativeDisableUnsafePtrRestriction]
		internal HashMapHelper<T>.Enumerator m_Enumerator;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.GetCurrentKey();
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
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe HashMapHelper<T>* m_Data;

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

		internal unsafe ReadOnly(ref NativeHashSet<T> data)
		{
			m_Data = data.m_Data;
		}

		public unsafe readonly bool Contains(T item)
		{
			return -1 != m_Data->Find(item);
		}

		public unsafe readonly NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data->GetKeyArray(allocator);
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new HashMapHelper<T>.Enumerator(m_Data);
			return result;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe HashMapHelper<T>* m_Data;

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

	public unsafe NativeHashSet(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = HashMapHelper<T>.Alloc(initialCapacity, 0, 256, allocator);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			HashMapHelper<T>.Free(m_Data);
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

	public unsafe bool Add(T item)
	{
		return -1 != m_Data->TryAdd(in item);
	}

	public unsafe bool Remove(T item)
	{
		return -1 != m_Data->TryRemove(item);
	}

	public unsafe bool Contains(T item)
	{
		return -1 != m_Data->Find(item);
	}

	public unsafe void TrimExcess()
	{
		m_Data->TrimExcess();
	}

	public unsafe NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data->GetKeyArray(allocator);
	}

	public unsafe Enumerator GetEnumerator()
	{
		Enumerator result = default(Enumerator);
		result.m_Enumerator = new HashMapHelper<T>.Enumerator(m_Data);
		return result;
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
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
}
