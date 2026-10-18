using System;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal struct BlockAllocator : IDisposable
{
	public struct Block
	{
		public int offset;

		public int count;

		public static readonly Block Invalid = new Block
		{
			offset = 0,
			count = 0
		};
	}

	public struct Allocation
	{
		public int handle;

		public Block block;

		public static readonly Allocation Invalid = new Allocation
		{
			handle = -1
		};

		public readonly bool valid => handle != -1;
	}

	private int m_FreeElementCount;

	private int m_MaxElementCount;

	private NativeList<Block> m_freeBlocks;

	private NativeList<Block> m_usedBlocks;

	private NativeList<int> m_freeSlots;

	public int freeElementsCount => m_FreeElementCount;

	public int freeBlocks => m_freeBlocks.Length;

	public int capacity => m_MaxElementCount;

	public int allocatedSize => m_MaxElementCount - m_FreeElementCount;

	public void Initialize(int maxElementCounts)
	{
		m_MaxElementCount = maxElementCounts;
		m_FreeElementCount = maxElementCounts;
		if (!m_freeBlocks.IsCreated)
		{
			m_freeBlocks = new NativeList<Block>(Allocator.Persistent);
		}
		else
		{
			m_freeBlocks.Clear();
		}
		ref NativeList<Block> reference = ref m_freeBlocks;
		Block value = new Block
		{
			offset = 0,
			count = m_FreeElementCount
		};
		reference.Add(in value);
		if (!m_usedBlocks.IsCreated)
		{
			m_usedBlocks = new NativeList<Block>(Allocator.Persistent);
		}
		else
		{
			m_usedBlocks.Clear();
		}
		if (!m_freeSlots.IsCreated)
		{
			m_freeSlots = new NativeList<int>(Allocator.Persistent);
		}
		else
		{
			m_freeSlots.Clear();
		}
	}

	private int CalculateGeometricGrowthCapacity(int desiredNewCapacity, int maxAllowedNewCapacity)
	{
		int num = capacity;
		if (num > maxAllowedNewCapacity - num / 2)
		{
			return maxAllowedNewCapacity;
		}
		int num2 = num + num / 2;
		if (num2 < desiredNewCapacity)
		{
			return desiredNewCapacity;
		}
		return num2;
	}

	public int Grow(int newDesiredCapacity, int maxAllowedCapacity = int.MaxValue)
	{
		int num = CalculateGeometricGrowthCapacity(newDesiredCapacity, maxAllowedCapacity);
		int maxElementCount = m_MaxElementCount;
		int num2 = num - maxElementCount;
		m_FreeElementCount += num2;
		m_MaxElementCount = num;
		int num3 = m_freeBlocks.Length;
		ref NativeList<Block> reference = ref m_freeBlocks;
		Block value = new Block
		{
			offset = maxElementCount,
			count = num2
		};
		reference.Add(in value);
		while (num3 != -1)
		{
			num3 = MergeBlockFrontBack(num3);
		}
		return m_MaxElementCount;
	}

	public Allocation GrowAndAllocate(int elementCounts, out int oldCapacity, out int newCapacity)
	{
		return GrowAndAllocate(elementCounts, int.MaxValue, out oldCapacity, out newCapacity);
	}

	public Allocation GrowAndAllocate(int elementCounts, int maxAllowedCapacity, out int oldCapacity, out int newCapacity)
	{
		oldCapacity = capacity;
		int num = (m_freeBlocks.IsEmpty ? elementCounts : math.max(elementCounts - m_freeBlocks[m_freeBlocks.Length - 1].count, 0));
		if (maxAllowedCapacity < capacity || maxAllowedCapacity - capacity < num)
		{
			newCapacity = capacity;
			return Allocation.Invalid;
		}
		newCapacity = ((num > 0) ? Grow(capacity + num, maxAllowedCapacity) : capacity);
		return Allocate(elementCounts);
	}

	public void Dispose()
	{
		m_MaxElementCount = 0;
		m_FreeElementCount = 0;
		if (m_freeBlocks.IsCreated)
		{
			m_freeBlocks.Dispose();
		}
		if (m_usedBlocks.IsCreated)
		{
			m_usedBlocks.Dispose();
		}
		if (m_freeSlots.IsCreated)
		{
			m_freeSlots.Dispose();
		}
	}

	public Allocation Allocate(int elementCounts)
	{
		if (elementCounts > m_FreeElementCount || m_freeBlocks.IsEmpty)
		{
			return Allocation.Invalid;
		}
		int num = -1;
		int num2 = 0;
		for (int i = 0; i < m_freeBlocks.Length; i++)
		{
			Block block = m_freeBlocks[i];
			if (elementCounts <= block.count && (num == -1 || block.count < num2))
			{
				num2 = block.count;
				num = i;
			}
		}
		if (num == -1)
		{
			return Allocation.Invalid;
		}
		Block value = m_freeBlocks[num];
		Block value2 = value;
		value2.offset += elementCounts;
		value2.count -= elementCounts;
		value.count = elementCounts;
		if (value2.count > 0)
		{
			m_freeBlocks[num] = value2;
		}
		else
		{
			m_freeBlocks.RemoveAtSwapBack(num);
		}
		int num3;
		if (m_freeSlots.IsEmpty)
		{
			num3 = m_usedBlocks.Length;
			m_usedBlocks.Add(in value);
		}
		else
		{
			num3 = m_freeSlots[m_freeSlots.Length - 1];
			m_freeSlots.RemoveAtSwapBack(m_freeSlots.Length - 1);
			m_usedBlocks[num3] = value;
		}
		m_FreeElementCount -= elementCounts;
		Allocation result = default(Allocation);
		result.handle = num3;
		result.block = value;
		return result;
	}

	private int MergeBlockFrontBack(int freeBlockId)
	{
		Block block = m_freeBlocks[freeBlockId];
		for (int i = 0; i < m_freeBlocks.Length; i++)
		{
			if (i == freeBlockId)
			{
				continue;
			}
			Block value = m_freeBlocks[i];
			bool flag = false;
			if (block.offset == value.offset + value.count)
			{
				value.count += block.count;
				flag = true;
			}
			else if (value.offset == block.offset + block.count)
			{
				value.offset = block.offset;
				value.count += block.count;
				flag = true;
			}
			if (flag)
			{
				m_freeBlocks[i] = value;
				m_freeBlocks.RemoveAtSwapBack(freeBlockId);
				if (i != m_freeBlocks.Length)
				{
					return i;
				}
				return freeBlockId;
			}
		}
		return -1;
	}

	public void FreeAllocation(in Allocation allocation)
	{
		m_freeSlots.Add(in allocation.handle);
		m_usedBlocks[allocation.handle] = Block.Invalid;
		int num = m_freeBlocks.Length;
		m_freeBlocks.Add(in allocation.block);
		while (num != -1)
		{
			num = MergeBlockFrontBack(num);
		}
		m_FreeElementCount += allocation.block.count;
	}

	public Allocation[] SplitAllocation(in Allocation allocation, int count)
	{
		Allocation[] array = new Allocation[count];
		int num = allocation.block.count / count;
		Block block = default(Block);
		block.offset = allocation.block.offset;
		block.count = num;
		Block block2 = block;
		m_usedBlocks[allocation.handle] = block2;
		array[0] = new Allocation
		{
			handle = allocation.handle,
			block = block2
		};
		for (int i = 1; i < count; i++)
		{
			block = default(Block);
			block.offset = allocation.block.offset + i * num;
			block.count = num;
			Block value = block;
			int num2;
			if (m_freeSlots.IsEmpty)
			{
				num2 = m_usedBlocks.Length;
				m_usedBlocks.Add(in value);
			}
			else
			{
				num2 = m_freeSlots[m_freeSlots.Length - 1];
				m_freeSlots.RemoveAtSwapBack(m_freeSlots.Length - 1);
				m_usedBlocks[num2] = value;
			}
			array[i] = new Allocation
			{
				handle = num2,
				block = value
			};
		}
		return array;
	}
}
