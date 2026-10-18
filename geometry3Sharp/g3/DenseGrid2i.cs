using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class DenseGrid2i
{
	public int[] Buffer;

	public int ni;

	public int nj;

	public int size => ni * nj;

	public int this[int i]
	{
		get
		{
			return Buffer[i];
		}
		set
		{
			Buffer[i] = value;
		}
	}

	public int this[int i, int j]
	{
		get
		{
			return Buffer[i + ni * j];
		}
		set
		{
			Buffer[i + ni * j] = value;
		}
	}

	public int this[Vector2i ijk]
	{
		get
		{
			return Buffer[ijk.x + ni * ijk.y];
		}
		set
		{
			Buffer[ijk.x + ni * ijk.y] = value;
		}
	}

	public DenseGrid2i()
	{
		ni = (nj = 0);
	}

	public DenseGrid2i(int ni, int nj, int initialValue)
	{
		resize(ni, nj);
		assign(initialValue);
	}

	public DenseGrid2i(DenseGrid2i copy)
	{
		resize(copy.ni, copy.nj);
		Array.Copy(copy.Buffer, Buffer, Buffer.Length);
	}

	public void resize(int ni, int nj)
	{
		Buffer = new int[ni * nj];
		this.ni = ni;
		this.nj = nj;
	}

	public void clear()
	{
		Array.Clear(Buffer, 0, Buffer.Length);
	}

	public void copy(DenseGrid2i copy)
	{
		Array.Copy(copy.Buffer, Buffer, Buffer.Length);
	}

	public void assign(int value)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = value;
		}
	}

	public void increment(int i, int j)
	{
		Buffer[i + ni * j]++;
	}

	public void decrement(int i, int j)
	{
		Buffer[i + ni * j]--;
	}

	public void atomic_increment(int i, int j)
	{
		Interlocked.Increment(ref Buffer[i + ni * j]);
	}

	public void atomic_decrement(int i, int j)
	{
		Interlocked.Decrement(ref Buffer[i + ni * j]);
	}

	public void atomic_incdec(int i, int j, bool decrement = false)
	{
		if (decrement)
		{
			Interlocked.Decrement(ref Buffer[i + ni * j]);
		}
		else
		{
			Interlocked.Increment(ref Buffer[i + ni * j]);
		}
	}

	public int sum()
	{
		int num = 0;
		for (int i = 0; i < Buffer.Length; i++)
		{
			num += Buffer[i];
		}
		return num;
	}

	public IEnumerable<Vector2i> Indices()
	{
		int y = 0;
		while (y < nj)
		{
			int num;
			for (int x = 0; x < ni; x = num)
			{
				yield return new Vector2i(x, y);
				num = x + 1;
			}
			num = y + 1;
			y = num;
		}
	}

	public IEnumerable<Vector2i> InsetIndices(int border_width)
	{
		int stopy = nj - border_width;
		int stopx = ni - border_width;
		int y = border_width;
		while (y < stopy)
		{
			int num;
			for (int x = border_width; x < stopx; x = num)
			{
				yield return new Vector2i(x, y);
				num = x + 1;
			}
			num = y + 1;
			y = num;
		}
	}
}
