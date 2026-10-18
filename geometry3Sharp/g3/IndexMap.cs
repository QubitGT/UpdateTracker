using System.Collections.Generic;

namespace g3;

public class IndexMap : IIndexMap
{
	public readonly int InvalidIndex = int.MinValue;

	private int[] dense_map;

	private Dictionary<int, int> sparse_map;

	private int MaxIndex;

	public int this[int index]
	{
		get
		{
			if (dense_map != null)
			{
				return dense_map[index];
			}
			if (sparse_map.TryGetValue(index, out var value))
			{
				return value;
			}
			return InvalidIndex;
		}
		set
		{
			if (dense_map != null)
			{
				dense_map[index] = value;
			}
			else
			{
				sparse_map[index] = value;
			}
		}
	}

	public IndexMap(bool bForceSparse, int MaxIndex = -1)
	{
		if (bForceSparse)
		{
			sparse_map = new Dictionary<int, int>();
		}
		else
		{
			dense_map = new int[MaxIndex];
		}
		this.MaxIndex = MaxIndex;
		SetToInvalid();
	}

	public IndexMap(int[] use_dense_map, int MaxIndex = -1)
	{
		dense_map = use_dense_map;
		this.MaxIndex = MaxIndex;
	}

	public IndexMap(int MaxIndex, int SubsetCountEst)
	{
		bool num = MaxIndex < 32000;
		float num2 = (float)SubsetCountEst / (float)MaxIndex;
		float num3 = 0.1f;
		if (num || num2 > num3)
		{
			dense_map = new int[MaxIndex];
		}
		else
		{
			sparse_map = new Dictionary<int, int>();
		}
		this.MaxIndex = MaxIndex;
		SetToInvalid();
	}

	public void SetToInvalid()
	{
		if (dense_map != null)
		{
			for (int i = 0; i < dense_map.Length; i++)
			{
				dense_map[i] = InvalidIndex;
			}
		}
	}

	public bool Contains(int index)
	{
		if (MaxIndex > 0 && index >= MaxIndex)
		{
			return false;
		}
		if (dense_map != null)
		{
			return dense_map[index] != InvalidIndex;
		}
		return sparse_map.ContainsKey(index);
	}
}
