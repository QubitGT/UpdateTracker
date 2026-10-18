using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon;

public class ByteArraySlicePool
{
	private int minStackIndex = 7;

	internal readonly Stack<ByteArraySlice>[] poolTiers = new Stack<ByteArraySlice>[32];

	private int allocationCounter;

	public int MinStackIndex
	{
		get
		{
			return minStackIndex;
		}
		set
		{
			minStackIndex = ((value <= 0) ? 1 : ((value < 31) ? value : 31));
		}
	}

	public int AllocationCounter => allocationCounter;

	public ByteArraySlicePool()
	{
		lock (poolTiers)
		{
			poolTiers[0] = new Stack<ByteArraySlice>();
		}
	}

	public ByteArraySlice Acquire(byte[] buffer, int offset = 0, int count = 0)
	{
		ByteArraySlice byteArraySlice;
		lock (poolTiers)
		{
			lock (poolTiers[0])
			{
				byteArraySlice = PopOrCreate(poolTiers[0], 0);
			}
		}
		byteArraySlice.Buffer = buffer;
		byteArraySlice.Offset = offset;
		byteArraySlice.Count = count;
		return byteArraySlice;
	}

	public ByteArraySlice Acquire(int minByteCount)
	{
		if (minByteCount < 0)
		{
			throw new Exception(typeof(ByteArraySlice).Name + " requires a positive minByteCount.");
		}
		int i = minStackIndex;
		if (minByteCount > 0)
		{
			for (int num = minByteCount - 1; i < 32 && num >> i != 0; i++)
			{
			}
		}
		lock (poolTiers)
		{
			Stack<ByteArraySlice> stack = poolTiers[i];
			if (stack == null)
			{
				stack = new Stack<ByteArraySlice>();
				poolTiers[i] = stack;
			}
			lock (stack)
			{
				return PopOrCreate(stack, i);
			}
		}
	}

	private ByteArraySlice PopOrCreate(Stack<ByteArraySlice> stack, int stackIndex)
	{
		lock (stack)
		{
			if (stack.Count > 0)
			{
				return stack.Pop();
			}
		}
		ByteArraySlice result = new ByteArraySlice(this, stackIndex);
		allocationCounter++;
		return result;
	}

	internal bool Release(ByteArraySlice slice, int stackIndex)
	{
		if (slice == null || stackIndex < 0)
		{
			return false;
		}
		if (stackIndex == 0)
		{
			slice.Buffer = null;
		}
		lock (poolTiers)
		{
			lock (poolTiers[stackIndex])
			{
				poolTiers[stackIndex].Push(slice);
			}
		}
		return true;
	}

	public void ClearPools(int lower = 0, int upper = int.MaxValue)
	{
		int num = minStackIndex;
		for (int i = 0; i < 32; i++)
		{
			int num2 = 1 << i;
			if (num2 < lower || num2 > upper)
			{
				continue;
			}
			lock (poolTiers)
			{
				if (poolTiers[i] != null)
				{
					lock (poolTiers[i])
					{
						poolTiers[i].Clear();
					}
				}
			}
		}
	}
}
