using System.Collections;
using System.Collections.Generic;

namespace g3;

public class IndexFlagSet : IEnumerable<int>, IEnumerable
{
	private BitArray bits;

	private HashSet<int> hash;

	private int count;

	public int Count
	{
		get
		{
			if (bits != null)
			{
				return count;
			}
			return hash.Count;
		}
	}

	public bool this[int key]
	{
		get
		{
			if (bits == null)
			{
				return hash.Contains(key);
			}
			return bits[key];
		}
		set
		{
			if (bits != null)
			{
				if (bits[key] != value)
				{
					bits[key] = value;
					if (!value)
					{
						count--;
					}
					else
					{
						count++;
					}
				}
			}
			else if (value)
			{
				hash.Add(key);
			}
			else if (!value && hash.Contains(key))
			{
				hash.Remove(key);
			}
		}
	}

	public IndexFlagSet(bool bForceSparse, int MaxIndex = -1)
	{
		if (bForceSparse)
		{
			hash = new HashSet<int>();
		}
		else
		{
			bits = new BitArray(MaxIndex);
		}
		count = 0;
	}

	public IndexFlagSet(int MaxIndex, int SubsetCountEst)
	{
		bool num = MaxIndex < 128000;
		float num2 = (float)SubsetCountEst / (float)MaxIndex;
		float num3 = 0.05f;
		if (num || num2 > num3)
		{
			bits = new BitArray(MaxIndex);
		}
		else
		{
			hash = new HashSet<int>();
		}
		count = 0;
	}

	public bool Contains(int i)
	{
		return this[i];
	}

	public void Add(int i)
	{
		this[i] = true;
	}

	public IEnumerator<int> GetEnumerator()
	{
		if (bits != null)
		{
			int i = 0;
			while (i < bits.Length)
			{
				if (bits[i])
				{
					yield return i;
				}
				int num = i + 1;
				i = num;
			}
			yield break;
		}
		foreach (int item in hash)
		{
			yield return item;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
