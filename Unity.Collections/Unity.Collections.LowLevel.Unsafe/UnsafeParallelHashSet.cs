using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerTypeProxy(typeof(UnsafeParallelHashSetDebuggerTypeProxy<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafeParallelHashSet<T> : INativeDisposable, IDisposable, IEnumerable<T>, IEnumerable where T : unmanaged, IEquatable<T>
{
	[NativeContainerIsAtomicWriteOnly]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelWriter
	{
		internal UnsafeParallelHashMap<T, bool>.ParallelWriter m_Data;

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Data.Capacity;
			}
		}

		public bool Add(T item)
		{
			return m_Data.TryAdd(item, item: false);
		}

		public bool Add(T item, int threadIndexOverride)
		{
			return m_Data.TryAdd(item, item: false, threadIndexOverride);
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		internal UnsafeParallelHashMapDataEnumerator m_Enumerator;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.GetCurrentKey<T>();
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

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		internal UnsafeParallelHashMap<T, bool> m_Data;

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
				if (m_Data.IsCreated)
				{
					return m_Data.IsEmpty;
				}
				return true;
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

		internal ReadOnly(ref UnsafeParallelHashSet<T> data)
		{
			m_Data = data.m_Data;
		}

		public readonly int Count()
		{
			return m_Data.Count();
		}

		public readonly bool Contains(T item)
		{
			return m_Data.ContainsKey(item);
		}

		public readonly NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data.GetKeyArray(allocator);
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Data.m_Buffer);
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
	}

	internal UnsafeParallelHashMap<T, bool> m_Data;

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.IsEmpty;
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
			m_Data.Capacity = value;
		}
	}

	public readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.IsCreated;
		}
	}

	public UnsafeParallelHashSet(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = new UnsafeParallelHashMap<T, bool>(capacity, allocator);
	}

	public int Count()
	{
		return m_Data.Count();
	}

	public void Dispose()
	{
		m_Data.Dispose();
	}

	public JobHandle Dispose(JobHandle inputDeps)
	{
		return m_Data.Dispose(inputDeps);
	}

	public void Clear()
	{
		m_Data.Clear();
	}

	public bool Add(T item)
	{
		return m_Data.TryAdd(item, item: false);
	}

	public bool Remove(T item)
	{
		return m_Data.Remove(item);
	}

	public bool Contains(T item)
	{
		return m_Data.ContainsKey(item);
	}

	public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data.GetKeyArray(allocator);
	}

	public ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_Data = m_Data.AsParallelWriter();
		return result;
	}

	public unsafe Enumerator GetEnumerator()
	{
		Enumerator result = default(Enumerator);
		result.m_Enumerator = new UnsafeParallelHashMapDataEnumerator(m_Data.m_Buffer);
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
}
