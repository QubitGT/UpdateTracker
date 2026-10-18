using System.Collections;

namespace g3;

public class RefCountVector : IEnumerable
{
	public static readonly short invalid = -1;

	private DVector<short> ref_counts;

	private DVector<int> free_indices;

	private int used_count;

	public DVector<short> RawRefCounts => ref_counts;

	public bool empty => used_count == 0;

	public int count => used_count;

	public int max_index => ref_counts.size;

	public bool is_dense => free_indices.Length == 0;

	public string UsageStats => $"RefCountSize {ref_counts.size}  FreeSize {free_indices.size} FreeMem {free_indices.MemoryUsageBytes / 1024}kb";

	public RefCountVector()
	{
		ref_counts = new DVector<short>();
		free_indices = new DVector<int>();
		used_count = 0;
	}

	public RefCountVector(RefCountVector copy)
	{
		ref_counts = new DVector<short>(copy.ref_counts);
		free_indices = new DVector<int>(copy.free_indices);
		used_count = copy.used_count;
	}

	public RefCountVector(short[] raw_ref_counts, bool build_free_list = false)
	{
		ref_counts = new DVector<short>(raw_ref_counts);
		free_indices = new DVector<int>();
		used_count = 0;
		if (build_free_list)
		{
			rebuild_free_list();
		}
	}

	public bool isValid(int index)
	{
		if (index >= 0 && index < ref_counts.size)
		{
			return ref_counts[index] > 0;
		}
		return false;
	}

	public bool isValidUnsafe(int index)
	{
		return ref_counts[index] > 0;
	}

	public int refCount(int index)
	{
		int num = ref_counts[index];
		if (num != invalid)
		{
			return num;
		}
		return 0;
	}

	public int rawRefCount(int index)
	{
		return ref_counts[index];
	}

	public int allocate()
	{
		used_count++;
		if (free_indices.empty)
		{
			ref_counts.push_back(1);
			return ref_counts.size - 1;
		}
		int back = invalid;
		while (back == invalid && !free_indices.empty)
		{
			back = free_indices.back;
			free_indices.pop_back();
		}
		if (back != invalid)
		{
			ref_counts[back] = 1;
			return back;
		}
		ref_counts.push_back(1);
		return ref_counts.size - 1;
	}

	public int increment(int index, short increment = 1)
	{
		ref_counts[index] += increment;
		return ref_counts[index];
	}

	public void decrement(int index, short decrement = 1)
	{
		ref_counts[index] -= decrement;
		if (ref_counts[index] == 0)
		{
			free_indices.push_back(index);
			ref_counts[index] = invalid;
			used_count--;
		}
	}

	public bool allocate_at(int index)
	{
		if (index >= ref_counts.size)
		{
			for (int i = ref_counts.size; i < index; i++)
			{
				ref_counts.push_back(invalid);
				free_indices.push_back(i);
			}
			ref_counts.push_back(1);
			used_count++;
			return true;
		}
		if (ref_counts[index] > 0)
		{
			return false;
		}
		int size = free_indices.size;
		for (int j = 0; j < size; j++)
		{
			if (free_indices[j] == index)
			{
				free_indices[j] = invalid;
				ref_counts[index] = 1;
				used_count++;
				return true;
			}
		}
		return false;
	}

	public bool allocate_at_unsafe(int index)
	{
		if (index >= ref_counts.size)
		{
			for (int i = ref_counts.size; i < index; i++)
			{
				ref_counts.push_back(invalid);
			}
			ref_counts.push_back(1);
			used_count++;
			return true;
		}
		if (ref_counts[index] > 0)
		{
			return false;
		}
		ref_counts[index] = 1;
		used_count++;
		return true;
	}

	public void set_Unsafe(int index, short count)
	{
		ref_counts[index] = count;
	}

	public void rebuild_free_list()
	{
		free_indices = new DVector<int>();
		used_count = 0;
		int length = ref_counts.Length;
		for (int i = 0; i < length; i++)
		{
			if (ref_counts[i] > 0)
			{
				used_count++;
			}
			else
			{
				free_indices.Add(i);
			}
		}
	}

	public void trim(int maxIndex)
	{
		free_indices = new DVector<int>();
		ref_counts.resize(maxIndex);
		used_count = maxIndex;
	}

	public IEnumerator GetEnumerator()
	{
		int nIndex = 0;
		int nLast;
		for (nLast = max_index; nIndex != nLast && ref_counts[nIndex] <= 0; nIndex++)
		{
		}
		while (nIndex != nLast)
		{
			yield return nIndex;
			if (nIndex != nLast)
			{
				nIndex++;
			}
			for (; nIndex != nLast && ref_counts[nIndex] <= 0; nIndex++)
			{
			}
		}
	}

	public string debug_print()
	{
		string text = $"size {ref_counts.size} used {used_count} free_size {free_indices.size}\n";
		for (int i = 0; i < ref_counts.size; i++)
		{
			text += $"{i}:{ref_counts[i]} ";
		}
		text += "\nfree:\n";
		for (int j = 0; j < free_indices.size; j++)
		{
			text = text + free_indices[j] + " ";
		}
		return text;
	}
}
