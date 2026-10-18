using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(NativeRingQueueDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct NativeRingQueue<T> : INativeDisposable, IDisposable where T : unmanaged
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeRingQueue<T>* m_RingQueue;

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (m_RingQueue != null)
			{
				return m_RingQueue->IsCreated;
			}
			return false;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (m_RingQueue != null)
			{
				return m_RingQueue->Length == 0;
			}
			return true;
		}
	}

	public unsafe readonly int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return CollectionHelper.AssumePositive(m_RingQueue->Length);
		}
	}

	public unsafe readonly int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return CollectionHelper.AssumePositive(m_RingQueue->Capacity);
		}
	}

	public unsafe NativeRingQueue(int capacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		m_RingQueue = UnsafeRingQueue<T>.Alloc(allocator);
		*m_RingQueue = new UnsafeRingQueue<T>(capacity, allocator, options);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeRingQueue<T>.Free(m_RingQueue);
			m_RingQueue = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeRingQueueDisposeJob jobData = default(NativeRingQueueDisposeJob);
		jobData.Data = new NativeRingQueueDispose
		{
			m_QueueData = (UnsafeRingQueue<int>*)m_RingQueue
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_RingQueue = null;
		return result;
	}

	public unsafe bool TryEnqueue(T value)
	{
		return m_RingQueue->TryEnqueue(value);
	}

	public unsafe void Enqueue(T value)
	{
		m_RingQueue->Enqueue(value);
	}

	public unsafe bool TryDequeue(out T item)
	{
		return m_RingQueue->TryDequeue(out item);
	}

	public unsafe T Dequeue()
	{
		return m_RingQueue->Dequeue();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckWrite()
	{
	}
}
