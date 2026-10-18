using System;
using System.Collections.Generic;

namespace g3;

public class DenseGrid2f
{
	public float[] Buffer;

	public int ni;

	public int nj;

	public int size => ni * nj;

	public float this[int i]
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

	public float this[int i, int j]
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

	public float this[Vector2i ijk]
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

	public AxisAlignedBox2i Bounds => new AxisAlignedBox2i(0, 0, ni, nj);

	public DenseGrid2f()
	{
		ni = (nj = 0);
	}

	public DenseGrid2f(int ni, int nj, float initialValue)
	{
		resize(ni, nj);
		assign(initialValue);
	}

	public DenseGrid2f(DenseGrid2f copy)
	{
		Buffer = new float[copy.Buffer.Length];
		Array.Copy(copy.Buffer, Buffer, Buffer.Length);
		ni = copy.ni;
		nj = copy.nj;
	}

	public void swap(DenseGrid2f g2)
	{
		float[] buffer = g2.Buffer;
		g2.Buffer = Buffer;
		Buffer = buffer;
	}

	public void resize(int ni, int nj)
	{
		Buffer = new float[ni * nj];
		this.ni = ni;
		this.nj = nj;
	}

	public void assign(float value)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = value;
		}
	}

	public void assign_border(float value, int rings)
	{
		for (int i = 0; i < rings; i++)
		{
			int num = nj - 1 - i;
			for (int j = 0; j < ni; j++)
			{
				Buffer[j + ni * i] = value;
				Buffer[j + ni * num] = value;
			}
		}
		int num2 = nj - 1 - rings;
		for (int k = rings; k < num2; k++)
		{
			for (int l = 0; l < rings; l++)
			{
				Buffer[l + ni * k] = value;
				Buffer[ni - 1 - l + ni * k] = value;
			}
		}
	}

	public void clear()
	{
		Array.Clear(Buffer, 0, Buffer.Length);
	}

	public void copy(DenseGrid2f copy)
	{
		Array.Copy(copy.Buffer, Buffer, Buffer.Length);
	}

	public void get_x_pair(int i0, int j, out double a, out double b)
	{
		int num = ni * j;
		a = Buffer[num + i0];
		b = Buffer[num + i0 + 1];
	}

	public void apply(Func<float, float> f)
	{
		for (int i = 0; i < nj; i++)
		{
			for (int j = 0; j < ni; j++)
			{
				int num = j + ni * i;
				Buffer[num] = f(Buffer[num]);
			}
		}
	}

	public void set_min(DenseGrid2f grid2)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = Math.Min(Buffer[i], grid2.Buffer[i]);
		}
	}

	public void set_max(DenseGrid2f grid2)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = Math.Max(Buffer[i], grid2.Buffer[i]);
		}
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
