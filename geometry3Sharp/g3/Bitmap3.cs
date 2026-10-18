using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class Bitmap3 : IBinaryVoxelGrid, IGridElement3, IFixedGrid3
{
	public BitArray Bits;

	private Vector3i dimensions;

	private int row_size;

	private int slab_size;

	private SpinLock bit_lock;

	public Vector3i Dimensions => dimensions;

	public AxisAlignedBox3i GridBounds => new AxisAlignedBox3i(Vector3i.Zero, Dimensions);

	public bool this[int i]
	{
		get
		{
			return Bits[i];
		}
		set
		{
			Bits[i] = value;
		}
	}

	public bool this[Vector3i idx]
	{
		get
		{
			int index = idx.z * slab_size + idx.y * row_size + idx.x;
			return Bits[index];
		}
		set
		{
			int index = idx.z * slab_size + idx.y * row_size + idx.x;
			Bits[index] = value;
		}
	}

	public Bitmap3(Vector3i dims)
	{
		int length = dims.x * dims.y * dims.z;
		Bits = new BitArray(length);
		dimensions = dims;
		row_size = dims.x;
		slab_size = dims.x * dims.y;
	}

	public void Set(Vector3i idx, bool val)
	{
		int index = idx.z * slab_size + idx.y * row_size + idx.x;
		Bits[index] = val;
	}

	public void SafeSet(Vector3i idx, bool val)
	{
		bool lockTaken = false;
		bit_lock.Enter(ref lockTaken);
		int index = idx.z * slab_size + idx.y * row_size + idx.x;
		Bits[index] = val;
		bit_lock.Exit();
	}

	public bool Get(Vector3i idx)
	{
		int index = idx.z * slab_size + idx.y * row_size + idx.x;
		return Bits[index];
	}

	public Vector3i ToIndex(int i)
	{
		int num = i / slab_size;
		i -= num * slab_size;
		int num2 = i / row_size;
		i -= num2 * row_size;
		return new Vector3i(i, num2, num);
	}

	public int ToLinear(Vector3i idx)
	{
		return idx.z * slab_size + idx.y * row_size + idx.x;
	}

	public IEnumerable<Vector3i> Indices()
	{
		int z = 0;
		while (z < Dimensions.z)
		{
			int num;
			for (int y = 0; y < Dimensions.y; y = num)
			{
				for (int x = 0; x < Dimensions.x; x = num)
				{
					yield return new Vector3i(x, y, z);
					num = x + 1;
				}
				num = y + 1;
			}
			num = z + 1;
			z = num;
		}
	}

	public IEnumerable<Vector3i> NonZeros()
	{
		int i = 0;
		while (i < Bits.Count)
		{
			if (Bits[i])
			{
				yield return ToIndex(i);
			}
			int num = i + 1;
			i = num;
		}
	}

	public void Filter(int nMinNbrs)
	{
		AxisAlignedBox3i gridBounds = GridBounds;
		gridBounds.Max -= Vector3i.One;
		for (int i = 0; i < Bits.Length; i++)
		{
			if (!Bits[i])
			{
				continue;
			}
			Vector3i vector3i = ToIndex(i);
			int num = 0;
			for (int j = 0; j < 6; j++)
			{
				if (num > nMinNbrs)
				{
					break;
				}
				Vector3i vector3i2 = vector3i + gIndices.GridOffsets6[j];
				if (gridBounds.Contains(vector3i2) && Get(vector3i2))
				{
					num++;
				}
			}
			if (num <= nMinNbrs)
			{
				Bits[i] = false;
			}
		}
	}

	public virtual IGridElement3 CreateNewGridElement(bool bCopy)
	{
		Bitmap3 result = new Bitmap3(Dimensions);
		if (bCopy)
		{
			throw new NotImplementedException();
		}
		return result;
	}
}
