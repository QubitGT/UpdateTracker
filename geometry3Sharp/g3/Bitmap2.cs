using System.Collections;
using System.Collections.Generic;

namespace g3;

public class Bitmap2
{
	public BitArray Bits;

	private Vector2i dimensions;

	private int row_size;

	public Vector2i Dimensions => dimensions;

	public AxisAlignedBox2i GridBounds => new AxisAlignedBox2i(Vector2i.Zero, Dimensions);

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

	public bool this[int r, int c]
	{
		get
		{
			return Bits[r * row_size + c];
		}
		set
		{
			Bits[r * row_size + c] = value;
		}
	}

	public bool this[Vector2i idx]
	{
		get
		{
			int index = idx.y * row_size + idx.x;
			return Bits[index];
		}
		set
		{
			int index = idx.y * row_size + idx.x;
			Bits[index] = value;
		}
	}

	public Bitmap2(Vector2i dims)
	{
		Resize(dims);
	}

	public Bitmap2(int Width, int Height)
	{
		Resize(new Vector2i(Width, Height));
	}

	public void Resize(Vector2i dims)
	{
		int length = dims.x * dims.y;
		Bits = new BitArray(length);
		dimensions = dims;
		row_size = dims.x;
	}

	public void Set(Vector2i idx, bool val)
	{
		int index = idx.y * row_size + idx.x;
		Bits[index] = val;
	}

	public bool Get(Vector2i idx)
	{
		int index = idx.y * row_size + idx.x;
		return Bits[index];
	}

	public Vector2i ToIndex(int i)
	{
		int num = i / row_size;
		i -= num * row_size;
		return new Vector2i(i, num);
	}

	public int ToLinear(Vector2i idx)
	{
		return idx.y * row_size + idx.x;
	}

	public IEnumerable<Vector2i> Indices()
	{
		int y = 0;
		while (y < Dimensions.y)
		{
			int num;
			for (int x = 0; x < Dimensions.x; x = num)
			{
				yield return new Vector2i(x, y);
				num = x + 1;
			}
			num = y + 1;
			y = num;
		}
	}

	public IEnumerable<Vector2i> NonZeros()
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
}
