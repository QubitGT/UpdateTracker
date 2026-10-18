using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(UnsafeRingQueueDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafeRingQueue<T> : INativeDisposable, IDisposable where T : unmanaged
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe T* Ptr;

	public AllocatorManager.AllocatorHandle Allocator;

	internal readonly int m_Capacity;

	internal int m_Filled;

	internal int m_Write;

	internal int m_Read;

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Filled == 0;
		}
	}

	public readonly int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Filled;
		}
	}

	public readonly int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Capacity;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Ptr != null;
		}
	}

	public unsafe UnsafeRingQueue(T* ptr, int capacity)
	{
		Ptr = ptr;
		Allocator = AllocatorManager.None;
		m_Capacity = capacity;
		m_Filled = 0;
		m_Write = 0;
		m_Read = 0;
	}

	public unsafe UnsafeRingQueue(int capacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		Allocator = allocator;
		m_Capacity = capacity;
		m_Filled = 0;
		m_Write = 0;
		m_Read = 0;
		int num = capacity * UnsafeUtility.SizeOf<T>();
		Ptr = (T*)Memory.Unmanaged.Allocate(num, 16, allocator);
		if (options == NativeArrayOptions.ClearMemory)
		{
			UnsafeUtility.MemClear(Ptr, num);
		}
	}

	internal unsafe static UnsafeRingQueue<T>* Alloc(AllocatorManager.AllocatorHandle allocator)
	{
		return (UnsafeRingQueue<T>*)Memory.Unmanaged.Allocate(sizeof(UnsafeRingQueue<T>), UnsafeUtility.AlignOf<UnsafeRingQueue<T>>(), allocator);
	}

	internal unsafe static void Free(UnsafeRingQueue<T>* data)
	{
		if (data == null)
		{
			throw new InvalidOperationException("UnsafeRingQueue has yet to be created or has been destroyed!");
		}
		AllocatorManager.AllocatorHandle allocator = data->Allocator;
		data->Dispose();
		Memory.Unmanaged.Free(data, allocator);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			if (CollectionHelper.ShouldDeallocate(Allocator))
			{
				Memory.Unmanaged.Free(Ptr, Allocator);
				Allocator = AllocatorManager.Invalid;
			}
			Ptr = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		if (CollectionHelper.ShouldDeallocate(Allocator))
		{
			UnsafeDisposeJob jobData = default(UnsafeDisposeJob);
			jobData.Ptr = Ptr;
			jobData.Allocator = Allocator;
			JobHandle result = jobData.Schedule(inputDeps);
			Ptr = null;
			Allocator = AllocatorManager.Invalid;
			return result;
		}
		Ptr = null;
		return inputDeps;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe bool TryEnqueueInternal(T value)
	{
		if (m_Filled == m_Capacity)
		{
			return false;
		}
		Ptr[m_Write] = value;
		m_Write++;
		if (m_Write == m_Capacity)
		{
			m_Write = 0;
		}
		m_Filled++;
		return true;
	}

	public bool TryEnqueue(T value)
	{
		return TryEnqueueInternal(value);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void ThrowQueueFull()
	{
		throw new InvalidOperationException("Trying to enqueue into full queue.");
	}

	public void Enqueue(T value)
	{
		TryEnqueueInternal(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe bool TryDequeueInternal(out T item)
	{
		item = Ptr[m_Read];
		if (m_Filled == 0)
		{
			return false;
		}
		m_Read++;
		if (m_Read == m_Capacity)
		{
			m_Read = 0;
		}
		m_Filled--;
		return true;
	}

	public bool TryDequeue(out T item)
	{
		return TryDequeueInternal(out item);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void ThrowQueueEmpty()
	{
		throw new InvalidOperationException("Trying to dequeue from an empty queue");
	}

	public T Dequeue()
	{
		TryDequeueInternal(out var item);
		return item;
	}
}
