using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafeQueue<T> : INativeDisposable, IDisposable where T : unmanaged
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeQueueBlockHeader* m_FirstBlock;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeQueueBlockHeader* m_Block;

		internal int m_Index;

		private T value;

		public T Current
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
		public unsafe bool MoveNext()
		{
			m_Index++;
			while (m_Block != null)
			{
				int numItems = m_Block->m_NumItems;
				if (m_Index < numItems)
				{
					value = UnsafeUtility.ReadArrayElement<T>(m_Block + 1, m_Index);
					return true;
				}
				m_Index -= numItems;
				m_Block = m_Block->m_NextBlock;
			}
			value = default(T);
			return false;
		}

		public unsafe void Reset()
		{
			m_Block = m_FirstBlock;
			m_Index = -1;
		}
	}

	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		[NativeDisableUnsafePtrRestriction]
		private unsafe UnsafeQueueData* m_Buffer;

		public unsafe readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Buffer != null;
			}
		}

		public unsafe readonly int Count
		{
			get
			{
				int num = 0;
				for (UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock; ptr != null; ptr = ptr->m_NextBlock)
				{
					num += ptr->m_NumItems;
				}
				return num - m_Buffer->m_CurrentRead;
			}
		}

		public readonly T this[int index]
		{
			get
			{
				TryGetValue(index, out var item);
				return item;
			}
		}

		internal unsafe ReadOnly(ref UnsafeQueue<T> data)
		{
			m_Buffer = data.m_Buffer;
		}

		public unsafe readonly bool IsEmpty()
		{
			int num = 0;
			int currentRead = m_Buffer->m_CurrentRead;
			for (UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock; ptr != null; ptr = ptr->m_NextBlock)
			{
				num += ptr->m_NumItems;
				if (num > currentRead)
				{
					return false;
				}
			}
			return num == currentRead;
		}

		private unsafe readonly bool TryGetValue(int index, out T item)
		{
			if (index >= 0)
			{
				int num = index;
				for (UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock; ptr != null; ptr = ptr->m_NextBlock)
				{
					int numItems = ptr->m_NumItems;
					if (num < numItems)
					{
						item = UnsafeUtility.ReadArrayElement<T>(ptr + 1, num);
						return true;
					}
					num -= numItems;
				}
			}
			item = default(T);
			return false;
		}

		public unsafe readonly Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_FirstBlock = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
			result.m_Block = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
			result.m_Index = -1;
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
		[Conditional("UNITY_DOTS_DEBUG")]
		private readonly void ThrowIndexOutOfRangeException(int index)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of bounds [0-{Count}].");
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelWriter
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeQueueData* m_Buffer;

		internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

		[NativeSetThreadIndex]
		internal int m_ThreadIndex;

		public unsafe void Enqueue(T value)
		{
			UnsafeQueueBlockHeader* ptr = UnsafeQueueData.AllocateWriteBlockMT<T>(m_Buffer, m_AllocatorLabel, m_ThreadIndex);
			UnsafeUtility.WriteArrayElement(ptr + 1, ptr->m_NumItems, value);
			ptr->m_NumItems++;
		}

		public unsafe void Enqueue(T value, int threadIndexOverride)
		{
			UnsafeQueueBlockHeader* ptr = UnsafeQueueData.AllocateWriteBlockMT<T>(m_Buffer, m_AllocatorLabel, threadIndexOverride);
			UnsafeUtility.WriteArrayElement(ptr + 1, ptr->m_NumItems, value);
			ptr->m_NumItems++;
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeQueueData* m_Buffer;

	[NativeDisableUnsafePtrRestriction]
	internal AllocatorManager.AllocatorHandle m_AllocatorLabel;

	public unsafe readonly int Count
	{
		get
		{
			int num = 0;
			for (UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock; ptr != null; ptr = ptr->m_NextBlock)
			{
				num += ptr->m_NumItems;
			}
			return num - m_Buffer->m_CurrentRead;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Buffer != null;
		}
	}

	public unsafe UnsafeQueue(AllocatorManager.AllocatorHandle allocator)
	{
		m_AllocatorLabel = allocator;
		UnsafeQueueData.AllocateQueue<T>(allocator, out m_Buffer);
	}

	internal unsafe static UnsafeQueue<T>* Alloc(AllocatorManager.AllocatorHandle allocator)
	{
		return (UnsafeQueue<T>*)Memory.Unmanaged.Allocate(sizeof(UnsafeQueue<T>), UnsafeUtility.AlignOf<UnsafeQueue<T>>(), allocator);
	}

	internal unsafe static void Free(UnsafeQueue<T>* data)
	{
		if (data == null)
		{
			throw new InvalidOperationException("UnsafeQueue has yet to be created or has been destroyed!");
		}
		AllocatorManager.AllocatorHandle allocatorLabel = data->m_AllocatorLabel;
		data->Dispose();
		Memory.Unmanaged.Free(data, allocatorLabel);
	}

	public unsafe readonly bool IsEmpty()
	{
		if (IsCreated)
		{
			int num = 0;
			int currentRead = m_Buffer->m_CurrentRead;
			for (UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock; ptr != null; ptr = ptr->m_NextBlock)
			{
				num += ptr->m_NumItems;
				if (num > currentRead)
				{
					return false;
				}
			}
			return num == currentRead;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe T Peek()
	{
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
		return UnsafeUtility.ReadArrayElement<T>(ptr + 1, m_Buffer->m_CurrentRead);
	}

	public unsafe void Enqueue(T value)
	{
		UnsafeQueueBlockHeader* ptr = UnsafeQueueData.AllocateWriteBlockMT<T>(m_Buffer, m_AllocatorLabel, 0);
		UnsafeUtility.WriteArrayElement(ptr + 1, ptr->m_NumItems, value);
		ptr->m_NumItems++;
	}

	public T Dequeue()
	{
		TryDequeue(out var item);
		return item;
	}

	public unsafe bool TryDequeue(out T item)
	{
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
		if (ptr != null)
		{
			int num = m_Buffer->m_CurrentRead++;
			int numItems = ptr->m_NumItems;
			item = UnsafeUtility.ReadArrayElement<T>(ptr + 1, num);
			if (num + 1 >= numItems)
			{
				m_Buffer->m_CurrentRead = 0;
				m_Buffer->m_FirstBlock = (IntPtr)ptr->m_NextBlock;
				if (m_Buffer->m_FirstBlock == IntPtr.Zero)
				{
					m_Buffer->m_LastBlock = IntPtr.Zero;
				}
				int threadIndexCount = JobsUtility.ThreadIndexCount;
				for (int i = 0; i < threadIndexCount; i++)
				{
					if (m_Buffer->GetCurrentWriteBlockTLS(i) == ptr)
					{
						m_Buffer->SetCurrentWriteBlockTLS(i, null);
					}
				}
				Memory.Unmanaged.Free(ptr, m_AllocatorLabel);
			}
			return true;
		}
		item = default(T);
		return false;
	}

	public unsafe NativeArray<T> ToArray(AllocatorManager.AllocatorHandle allocator)
	{
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
		NativeArray<T> nativeArray = CollectionHelper.CreateNativeArray<T>(Count, allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeQueueBlockHeader* ptr2 = ptr;
		byte* unsafePtr = (byte*)nativeArray.GetUnsafePtr();
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = 0;
		int num3 = m_Buffer->m_CurrentRead * num;
		int num4 = m_Buffer->m_CurrentRead;
		while (ptr2 != null)
		{
			int num5 = (ptr2->m_NumItems - num4) * num;
			UnsafeUtility.MemCpy(unsafePtr + num2, (byte*)(ptr2 + 1) + num3, num5);
			num3 = (num4 = 0);
			num2 += num5;
			ptr2 = ptr2->m_NextBlock;
		}
		return nativeArray;
	}

	public unsafe void Clear()
	{
		UnsafeQueueBlockHeader* ptr = (UnsafeQueueBlockHeader*)(void*)m_Buffer->m_FirstBlock;
		while (ptr != null)
		{
			UnsafeQueueBlockHeader* nextBlock = ptr->m_NextBlock;
			Memory.Unmanaged.Free(ptr, m_AllocatorLabel);
			ptr = nextBlock;
		}
		m_Buffer->m_FirstBlock = IntPtr.Zero;
		m_Buffer->m_LastBlock = IntPtr.Zero;
		m_Buffer->m_CurrentRead = 0;
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		for (int i = 0; i < threadIndexCount; i++)
		{
			m_Buffer->SetCurrentWriteBlockTLS(i, null);
		}
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeQueueData.DeallocateQueue(m_Buffer, m_AllocatorLabel);
			m_Buffer = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		UnsafeQueueDisposeJob jobData = default(UnsafeQueueDisposeJob);
		jobData.Data = new UnsafeQueueDispose
		{
			m_Buffer = m_Buffer,
			m_AllocatorLabel = m_AllocatorLabel
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_Buffer = null;
		return result;
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(ref this);
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		ParallelWriter result = default(ParallelWriter);
		result.m_Buffer = m_Buffer;
		result.m_AllocatorLabel = m_AllocatorLabel;
		result.m_ThreadIndex = 0;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe void CheckNotEmpty()
	{
		_ = m_Buffer->m_FirstBlock == (IntPtr)0;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void ThrowEmpty()
	{
		throw new InvalidOperationException("Trying to read from an empty queue.");
	}
}
