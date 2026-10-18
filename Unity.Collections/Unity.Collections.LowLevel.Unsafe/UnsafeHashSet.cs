using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerTypeProxy(typeof(UnsafeHashSetDebuggerTypeProxy<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafeHashSet<T> : INativeDisposable, IDisposable, IEnumerable<T>, IEnumerable where T : unmanaged, IEquatable<T>
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		internal HashMapHelper<T>.Enumerator m_Enumerator;

		public unsafe T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.m_Data->Keys[m_Enumerator.m_Index];
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
		internal HashMapHelper<T> m_Data;

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

		internal ReadOnly(ref HashMapHelper<T> data)
		{
			m_Data = data;
		}

		public readonly bool Contains(T item)
		{
			return -1 != m_Data.Find(item);
		}

		public readonly NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
		{
			return m_Data.GetKeyArray(allocator);
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			fixed (HashMapHelper<T>* data = &m_Data)
			{
				Enumerator result = default(Enumerator);
				result.m_Enumerator = new HashMapHelper<T>.Enumerator(data);
				return result;
			}
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

	internal HashMapHelper<T> m_Data;

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (IsCreated)
			{
				return m_Data.IsEmpty;
			}
			return true;
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

	public readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data.IsCreated;
		}
	}

	public UnsafeHashSet(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = default(HashMapHelper<T>);
		m_Data.Init(initialCapacity, 0, 256, allocator);
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
		m_Data.Ptr = null;
		return result;
	}

	public void Clear()
	{
		m_Data.Clear();
	}

	public bool Add(T item)
	{
		return -1 != m_Data.TryAdd(in item);
	}

	public bool Remove(T item)
	{
		return -1 != m_Data.TryRemove(item);
	}

	public bool Contains(T item)
	{
		return -1 != m_Data.Find(item);
	}

	public void TrimExcess()
	{
		m_Data.TrimExcess();
	}

	public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Data.GetKeyArray(allocator);
	}

	public unsafe Enumerator GetEnumerator()
	{
		fixed (HashMapHelper<T>* data = &m_Data)
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = new HashMapHelper<T>.Enumerator(data);
			return result;
		}
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
		return new ReadOnly(ref m_Data);
	}
}
