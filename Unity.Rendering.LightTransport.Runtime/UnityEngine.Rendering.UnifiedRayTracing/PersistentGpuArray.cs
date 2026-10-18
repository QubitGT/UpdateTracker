using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class PersistentGpuArray<Tstruct> : IDisposable where Tstruct : struct
{
	private BlockAllocator m_SlotAllocator;

	private ComputeBuffer m_GpuBuffer;

	private NativeArray<Tstruct> m_CpuList;

	private BitArray m_Updates;

	private bool m_gpuBufferDirty = true;

	private int m_ElementCount;

	public int elementCount => m_ElementCount;

	public PersistentGpuArray(int initialSize)
	{
		m_SlotAllocator.Initialize(initialSize);
		m_GpuBuffer = new ComputeBuffer(initialSize, Marshal.SizeOf<Tstruct>());
		m_CpuList = new NativeArray<Tstruct>(initialSize, Allocator.Persistent);
		m_Updates = new BitArray(initialSize);
		m_ElementCount = 0;
	}

	public void Dispose()
	{
		m_ElementCount = 0;
		m_SlotAllocator.Dispose();
		m_GpuBuffer.Dispose();
		m_CpuList.Dispose();
	}

	public BlockAllocator.Allocation Add(Tstruct element)
	{
		m_ElementCount++;
		BlockAllocator.Allocation result = m_SlotAllocator.Allocate(1);
		if (!result.valid)
		{
			Grow();
			result = m_SlotAllocator.Allocate(1);
		}
		m_CpuList[result.block.offset] = element;
		m_Updates[result.block.offset] = true;
		m_gpuBufferDirty = true;
		return result;
	}

	public BlockAllocator.Allocation[] Add(int elementCount)
	{
		m_ElementCount += elementCount;
		BlockAllocator.Allocation allocation = m_SlotAllocator.Allocate(elementCount);
		if (!allocation.valid)
		{
			Grow();
			allocation = m_SlotAllocator.Allocate(elementCount);
		}
		return m_SlotAllocator.SplitAllocation(in allocation, elementCount);
	}

	public void Remove(BlockAllocator.Allocation allocation)
	{
		m_ElementCount--;
		m_SlotAllocator.FreeAllocation(in allocation);
	}

	public void Clear()
	{
		m_ElementCount = 0;
		int capacity = m_SlotAllocator.capacity;
		m_SlotAllocator.Dispose();
		m_SlotAllocator = default(BlockAllocator);
		m_SlotAllocator.Initialize(capacity);
		m_Updates = new BitArray(capacity);
		m_gpuBufferDirty = false;
	}

	public void Set(BlockAllocator.Allocation allocation, Tstruct element)
	{
		m_CpuList[allocation.block.offset] = element;
		m_Updates[allocation.block.offset] = true;
		m_gpuBufferDirty = true;
	}

	public Tstruct Get(BlockAllocator.Allocation allocation)
	{
		return m_CpuList[allocation.block.offset];
	}

	public void ModifyForEach(Func<Tstruct, Tstruct> lambda)
	{
		for (int i = 0; i < m_CpuList.Length; i++)
		{
			m_CpuList[i] = lambda(m_CpuList[i]);
			m_Updates[i] = true;
		}
		m_gpuBufferDirty = true;
	}

	public ComputeBuffer GetGpuBuffer(CommandBuffer cmd)
	{
		if (m_gpuBufferDirty)
		{
			int num = -1;
			for (int i = 0; i < m_Updates.Length; i++)
			{
				if (m_Updates[i])
				{
					if (num == -1)
					{
						num = i;
					}
					m_Updates[i] = false;
				}
				else if (num != -1)
				{
					int num2 = i;
					cmd.SetBufferData(m_GpuBuffer, m_CpuList, num, num, num2 - num);
					num = -1;
				}
			}
			if (num != -1)
			{
				int length = m_Updates.Length;
				cmd.SetBufferData(m_GpuBuffer, m_CpuList, num, num, length - num);
			}
			m_gpuBufferDirty = false;
		}
		return m_GpuBuffer;
	}

	private void Grow()
	{
		int capacity = m_SlotAllocator.capacity;
		m_SlotAllocator.Grow(m_SlotAllocator.capacity + 1);
		m_GpuBuffer.Dispose();
		m_GpuBuffer = new ComputeBuffer(m_SlotAllocator.capacity, Marshal.SizeOf<Tstruct>());
		NativeArray<Tstruct> cpuList = m_CpuList;
		m_CpuList = new NativeArray<Tstruct>(m_SlotAllocator.capacity, Allocator.Persistent);
		NativeArray<Tstruct>.Copy(cpuList, m_CpuList, capacity);
		cpuList.Dispose();
		BitArray updates = m_Updates;
		m_Updates = new BitArray(m_SlotAllocator.capacity);
		for (int i = 0; i < capacity; i++)
		{
			m_Updates[i] = updates[i];
		}
	}
}
