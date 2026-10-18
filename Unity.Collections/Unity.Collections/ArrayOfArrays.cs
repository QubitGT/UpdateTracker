using System;
using System.Diagnostics;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

internal struct ArrayOfArrays<T> : IDisposable where T : unmanaged
{
	private AllocatorManager.AllocatorHandle m_backingAllocatorHandle;

	private int m_lengthInElements;

	private int m_capacityInElements;

	private int m_log2BlockSizeInElements;

	private int m_blocks;

	private unsafe IntPtr* m_block;

	private int BlockSizeInElements => 1 << m_log2BlockSizeInElements;

	private unsafe int BlockSizeInBytes => BlockSizeInElements * sizeof(T);

	private int BlockMask => BlockSizeInElements - 1;

	public int Length => m_lengthInElements;

	public int Capacity => m_capacityInElements;

	public unsafe ref T this[int elementIndex]
	{
		get
		{
			int num = BlockIndexOfElement(elementIndex);
			IntPtr intPtr = m_block[num];
			int num2 = elementIndex & BlockMask;
			T* ptr = (T*)(void*)intPtr;
			return ref ptr[num2];
		}
	}

	public unsafe ArrayOfArrays(int capacityInElements, AllocatorManager.AllocatorHandle backingAllocatorHandle, int log2BlockSizeInElements = 12)
	{
		this = default(ArrayOfArrays<T>);
		m_backingAllocatorHandle = backingAllocatorHandle;
		m_lengthInElements = 0;
		m_capacityInElements = capacityInElements;
		m_log2BlockSizeInElements = log2BlockSizeInElements;
		m_blocks = capacityInElements + BlockMask >> m_log2BlockSizeInElements;
		m_block = (IntPtr*)Memory.Unmanaged.Allocate(sizeof(IntPtr) * m_blocks, 16, m_backingAllocatorHandle);
		UnsafeUtility.MemSet(m_block, 0, sizeof(IntPtr) * m_blocks);
	}

	public unsafe void LockfreeAdd(T t)
	{
		int elementIndex = Interlocked.Increment(ref m_lengthInElements) - 1;
		int i = BlockIndexOfElement(elementIndex);
		if (m_block[i] == IntPtr.Zero)
		{
			void* ptr = Memory.Unmanaged.Allocate(BlockSizeInBytes, 16, m_backingAllocatorHandle);
			int num;
			for (num = math.min(m_blocks, i + 4); i < num && !(IntPtr.Zero == Interlocked.CompareExchange(ref m_block[i], (IntPtr)ptr, IntPtr.Zero)); i++)
			{
			}
			if (i == num)
			{
				Memory.Unmanaged.Free(ptr, m_backingAllocatorHandle);
			}
		}
		this[elementIndex] = t;
	}

	public void Rewind()
	{
		m_lengthInElements = 0;
	}

	public unsafe void Clear()
	{
		Rewind();
		for (int i = 0; i < m_blocks; i++)
		{
			if (m_block[i] != IntPtr.Zero)
			{
				Memory.Unmanaged.Free((void*)m_block[i], m_backingAllocatorHandle);
				m_block[i] = IntPtr.Zero;
			}
		}
	}

	public unsafe void Dispose()
	{
		Clear();
		Memory.Unmanaged.Free(m_block, m_backingAllocatorHandle);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckElementIndex(int elementIndex)
	{
		if (elementIndex >= m_lengthInElements)
		{
			throw new ArgumentException($"Element index {elementIndex} must be less than length in elements {m_lengthInElements}.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckBlockIndex(int blockIndex)
	{
		if (blockIndex >= m_blocks)
		{
			throw new ArgumentException($"Block index {blockIndex} must be less than number of blocks {m_blocks}.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe void CheckBlockIsNotNull(int blockIndex)
	{
		if (m_block[blockIndex] == IntPtr.Zero)
		{
			throw new ArgumentException($"Block index {blockIndex} is a null pointer.");
		}
	}

	public void RemoveAtSwapBack(int elementIndex)
	{
		this[elementIndex] = this[Length - 1];
		m_lengthInElements--;
	}

	private int BlockIndexOfElement(int elementIndex)
	{
		return elementIndex >> m_log2BlockSizeInElements;
	}

	public unsafe void TrimExcess()
	{
		for (int i = BlockIndexOfElement(m_lengthInElements + BlockMask); i < m_blocks; i++)
		{
			if (m_block[i] != IntPtr.Zero)
			{
				Memory.Unmanaged.Free((void*)m_block[i], m_backingAllocatorHandle);
				m_block[i] = IntPtr.Zero;
			}
		}
	}
}
