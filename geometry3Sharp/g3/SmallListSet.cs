using System;
using System.Collections.Generic;

namespace g3;

public class SmallListSet
{
	private const int Null = -1;

	private const int BLOCKSIZE = 8;

	private const int BLOCK_LIST_OFFSET = 9;

	private DVector<int> list_heads;

	private DVector<int> block_store;

	private DVector<int> free_blocks;

	private int allocated_count;

	private DVector<int> linked_store;

	private int free_head_ptr;

	public int Size => list_heads.size;

	public string MemoryUsage => $"ListSize {list_heads.size}  Blocks Count {allocated_count} Free {free_blocks.size * 4 / 1024} Mem {block_store.size}kb  Linked Mem {linked_store.size * 4 / 1024}kb";

	public SmallListSet()
	{
		list_heads = new DVector<int>();
		linked_store = new DVector<int>();
		free_head_ptr = -1;
		block_store = new DVector<int>();
		free_blocks = new DVector<int>();
	}

	public SmallListSet(SmallListSet copy)
	{
		linked_store = new DVector<int>(copy.linked_store);
		free_head_ptr = copy.free_head_ptr;
		list_heads = new DVector<int>(copy.list_heads);
		block_store = new DVector<int>(copy.block_store);
		free_blocks = new DVector<int>(copy.free_blocks);
	}

	public void Resize(int new_size)
	{
		int size = list_heads.size;
		if (new_size > size)
		{
			list_heads.resize(new_size);
			for (int i = size; i < new_size; i++)
			{
				list_heads[i] = -1;
			}
		}
	}

	public void AllocateAt(int list_index)
	{
		if (list_index >= list_heads.size)
		{
			int i = list_heads.size;
			list_heads.insert(-1, list_index);
			for (; i < list_index; i++)
			{
				list_heads[i] = -1;
			}
		}
		else if (list_heads[list_index] != -1)
		{
			throw new Exception("SmallListSet: list at " + list_index + " is not empty!");
		}
	}

	public void Insert(int list_index, int val)
	{
		int num = list_heads[list_index];
		if (num == -1)
		{
			num = allocate_block();
			block_store[num] = 0;
			list_heads[list_index] = num;
		}
		int num2 = block_store[num];
		if (num2 < 8)
		{
			block_store[num + num2 + 1] = val;
		}
		else
		{
			int value = block_store[num + 9];
			if (free_head_ptr == -1)
			{
				int size = linked_store.size;
				linked_store.Add(val);
				linked_store.Add(value);
				block_store[num + 9] = size;
			}
			else
			{
				int num3 = free_head_ptr;
				free_head_ptr = linked_store[num3 + 1];
				linked_store[num3] = val;
				linked_store[num3 + 1] = value;
				block_store[num + 9] = num3;
			}
		}
		block_store[num]++;
	}

	public bool Remove(int list_index, int val)
	{
		int num = list_heads[list_index];
		int num2 = block_store[num];
		int num3 = num + Math.Min(num2, 8);
		for (int i = num + 1; i <= num3; i++)
		{
			if (block_store[i] == val)
			{
				for (int j = i + 1; j <= num3; j++)
				{
					block_store[j - 1] = block_store[j];
				}
				if (num2 > 8)
				{
					int num4 = block_store[num + 9];
					block_store[num + 9] = linked_store[num4 + 1];
					block_store[num3] = linked_store[num4];
					add_free_link(num4);
				}
				block_store[num]--;
				return true;
			}
		}
		if (num2 > 8 && remove_from_linked_list(num, val))
		{
			block_store[num]--;
			return true;
		}
		return false;
	}

	public void Move(int from_index, int to_index)
	{
		if (list_heads[to_index] != -1)
		{
			throw new Exception("SmallListSet.MoveTo: list at " + to_index + " is not empty!");
		}
		if (list_heads[from_index] == -1)
		{
			throw new Exception("SmallListSet.MoveTo: list at " + from_index + " is empty!");
		}
		list_heads[to_index] = list_heads[from_index];
		list_heads[from_index] = -1;
	}

	public void Clear(int list_index)
	{
		int num = list_heads[list_index];
		if (num == -1)
		{
			return;
		}
		if (block_store[num] > 8)
		{
			int num2 = block_store[num + 9];
			while (num2 != -1)
			{
				int ptr = num2;
				num2 = linked_store[num2 + 1];
				add_free_link(ptr);
			}
			block_store[num + 9] = -1;
		}
		block_store[num] = 0;
		free_blocks.push_back(num);
		list_heads[list_index] = -1;
	}

	public int Count(int list_index)
	{
		int num = list_heads[list_index];
		if (num != -1)
		{
			return block_store[num];
		}
		return 0;
	}

	public bool Contains(int list_index, int val)
	{
		int num = list_heads[list_index];
		if (num != -1)
		{
			int num2 = block_store[num];
			if (num2 < 8)
			{
				int num3 = num + num2;
				for (int i = num + 1; i <= num3; i++)
				{
					if (block_store[i] == val)
					{
						return true;
					}
				}
			}
			else
			{
				int num4 = num + 8;
				for (int j = num + 1; j <= num4; j++)
				{
					if (block_store[j] == val)
					{
						return true;
					}
				}
				for (int num5 = block_store[num + 9]; num5 != -1; num5 = linked_store[num5 + 1])
				{
					if (linked_store[num5] == val)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public int First(int list_index)
	{
		int num = list_heads[list_index];
		return block_store[num + 1];
	}

	public IEnumerable<int> ValueItr(int list_index)
	{
		int block_ptr = list_heads[list_index];
		if (block_ptr == -1)
		{
			yield break;
		}
		int num = block_store[block_ptr];
		int iEnd;
		if (num < 8)
		{
			iEnd = block_ptr + num;
			int i = block_ptr + 1;
			while (i <= iEnd)
			{
				yield return block_store[i];
				int num2 = i + 1;
				i = num2;
			}
			yield break;
		}
		iEnd = block_ptr + 8;
		int k = block_ptr + 1;
		while (k <= iEnd)
		{
			yield return block_store[k];
			int num2 = k + 1;
			k = num2;
		}
		for (int i = block_store[block_ptr + 9]; i != -1; i = linked_store[i + 1])
		{
			yield return linked_store[i];
		}
	}

	public int Find(int list_index, Func<int, bool> findF, int invalidValue = -1)
	{
		int num = list_heads[list_index];
		if (num != -1)
		{
			int num2 = block_store[num];
			if (num2 < 8)
			{
				int num3 = num + num2;
				for (int i = num + 1; i <= num3; i++)
				{
					int num4 = block_store[i];
					if (findF(num4))
					{
						return num4;
					}
				}
			}
			else
			{
				int num5 = num + 8;
				for (int j = num + 1; j <= num5; j++)
				{
					int num6 = block_store[j];
					if (findF(num6))
					{
						return num6;
					}
				}
				for (int num7 = block_store[num + 9]; num7 != -1; num7 = linked_store[num7 + 1])
				{
					int num8 = linked_store[num7];
					if (findF(num8))
					{
						return num8;
					}
				}
			}
		}
		return invalidValue;
	}

	public bool Replace(int list_index, Func<int, bool> findF, int new_value)
	{
		int num = list_heads[list_index];
		if (num != -1)
		{
			int num2 = block_store[num];
			if (num2 < 8)
			{
				int num3 = num + num2;
				for (int i = num + 1; i <= num3; i++)
				{
					int arg = block_store[i];
					if (findF(arg))
					{
						block_store[i] = new_value;
						return true;
					}
				}
			}
			else
			{
				int num4 = num + 8;
				for (int j = num + 1; j <= num4; j++)
				{
					int arg2 = block_store[j];
					if (findF(arg2))
					{
						block_store[j] = new_value;
						return true;
					}
				}
				for (int num5 = block_store[num + 9]; num5 != -1; num5 = linked_store[num5 + 1])
				{
					int arg3 = linked_store[num5];
					if (findF(arg3))
					{
						linked_store[num5] = new_value;
						return true;
					}
				}
			}
		}
		return false;
	}

	protected int allocate_block()
	{
		int size = free_blocks.size;
		if (size > 0)
		{
			int result = free_blocks[size - 1];
			free_blocks.pop_back();
			return result;
		}
		int size2 = block_store.size;
		block_store.insert(-1, size2 + 9);
		block_store[size2] = 0;
		allocated_count++;
		return size2;
	}

	private void add_free_link(int ptr)
	{
		linked_store[ptr + 1] = free_head_ptr;
		free_head_ptr = ptr;
	}

	private bool remove_from_linked_list(int block_ptr, int val)
	{
		int num = block_store[block_ptr + 9];
		int num2 = -1;
		while (num != -1)
		{
			if (linked_store[num] == val)
			{
				int value = linked_store[num + 1];
				if (num2 == -1)
				{
					block_store[block_ptr + 9] = value;
				}
				else
				{
					linked_store[num2 + 1] = value;
				}
				add_free_link(num);
				return true;
			}
			num2 = num;
			num = linked_store[num + 1];
		}
		return false;
	}
}
