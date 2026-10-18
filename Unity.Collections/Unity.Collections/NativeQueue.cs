using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct NativeQueue<T> : INativeDisposable, IDisposable where T : unmanaged
{
	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		internal UnsafeQueue<T>.Enumerator m_Enumerator;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Enumerator.Current;
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
	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		private UnsafeQueue<T>.ReadOnly m_ReadOnly;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_ReadOnly.IsCreated;
			}
		}

		public readonly int Count => m_ReadOnly.Count;

		public readonly T this[int index] => m_ReadOnly[index];

		internal unsafe ReadOnly(ref NativeQueue<T> data)
		{
			m_ReadOnly = new UnsafeQueue<T>.ReadOnly(ref *data.m_Queue);
		}

		public readonly bool IsEmpty()
		{
			return m_ReadOnly.IsEmpty();
		}

		public readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Enumerator = m_ReadOnly.GetEnumerator();
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

	[NativeContainer]
	[NativeContainerIsAtomicWriteOnly]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelWriter
	{
		internal UnsafeQueue<T>.ParallelWriter unsafeWriter;

		public void Enqueue(T value)
		{
			unsafeWriter.Enqueue(value);
		}

		public void Enqueue(T value, int threadIndexOverride)
		{
			unsafeWriter.Enqueue(value, threadIndexOverride);
		}
	}

	[NativeDisableUnsafePtrRestriction]
	private unsafe UnsafeQueue<T>* m_Queue;

	public unsafe readonly int Count => m_Queue->Count;

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (m_Queue != null)
			{
				return m_Queue->IsCreated;
			}
			return false;
		}
	}

	public unsafe NativeQueue(AllocatorManager.AllocatorHandle allocator)
	{
		m_Queue = UnsafeQueue<T>.Alloc(allocator);
		*m_Queue = new UnsafeQueue<T>(allocator);
	}

	public unsafe readonly bool IsEmpty()
	{
		if (IsCreated)
		{
			return m_Queue->IsEmpty();
		}
		return true;
	}

	public unsafe T Peek()
	{
		return m_Queue->Peek();
	}

	public unsafe void Enqueue(T value)
	{
		m_Queue->Enqueue(value);
	}

	public unsafe T Dequeue()
	{
		return m_Queue->Dequeue();
	}

	public unsafe bool TryDequeue(out T item)
	{
		return m_Queue->TryDequeue(out item);
	}

	public unsafe NativeArray<T> ToArray(AllocatorManager.AllocatorHandle allocator)
	{
		return m_Queue->ToArray(allocator);
	}

	public unsafe void Clear()
	{
		m_Queue->Clear();
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeQueue<T>.Free(m_Queue);
			m_Queue = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeQueueDisposeJob jobData = default(NativeQueueDisposeJob);
		jobData.Data = new NativeQueueDispose
		{
			m_QueueData = (UnsafeQueue<int>*)m_Queue
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_Queue = null;
		return result;
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(ref this);
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.unsafeWriter = m_Queue->AsParallelWriter();
		return result;
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
