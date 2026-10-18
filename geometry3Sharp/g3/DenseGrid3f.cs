using System;
using System.Collections.Generic;

namespace g3;

public class DenseGrid3f
{
	public float[] Buffer;

	public int ni;

	public int nj;

	public int nk;

	public int size => ni * nj * nk;

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

	public float this[int i, int j, int k]
	{
		get
		{
			return Buffer[i + ni * (j + nj * k)];
		}
		set
		{
			Buffer[i + ni * (j + nj * k)] = value;
		}
	}

	public float this[Vector3i ijk]
	{
		get
		{
			return Buffer[ijk.x + ni * (ijk.y + nj * ijk.z)];
		}
		set
		{
			Buffer[ijk.x + ni * (ijk.y + nj * ijk.z)] = value;
		}
	}

	public AxisAlignedBox3i Bounds => new AxisAlignedBox3i(0, 0, 0, ni, nj, nk);

	public AxisAlignedBox3i BoundsInclusive => new AxisAlignedBox3i(0, 0, 0, ni - 1, nj - 1, nk - 1);

	public DenseGrid3f()
	{
		ni = (nj = (nk = 0));
	}

	public DenseGrid3f(int ni, int nj, int nk, float initialValue)
	{
		resize(ni, nj, nk);
		assign(initialValue);
	}

	public DenseGrid3f(DenseGrid3f copy)
	{
		Buffer = new float[copy.Buffer.Length];
		Array.Copy(copy.Buffer, Buffer, Buffer.Length);
		ni = copy.ni;
		nj = copy.nj;
		nk = copy.nk;
	}

	public void swap(DenseGrid3f g2)
	{
		float[] buffer = g2.Buffer;
		g2.Buffer = Buffer;
		Buffer = buffer;
	}

	public void resize(int ni, int nj, int nk)
	{
		Buffer = new float[ni * nj * nk];
		this.ni = ni;
		this.nj = nj;
		this.nk = nk;
	}

	public void assign(float value)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = value;
		}
	}

	public void set_min(ref Vector3i ijk, float f)
	{
		int num = ijk.x + ni * (ijk.y + nj * ijk.z);
		if (f < Buffer[num])
		{
			Buffer[num] = f;
		}
	}

	public void set_max(ref Vector3i ijk, float f)
	{
		int num = ijk.x + ni * (ijk.y + nj * ijk.z);
		if (f > Buffer[num])
		{
			Buffer[num] = f;
		}
	}

	public void get_x_pair(int i0, int j, int k, out float a, out float b)
	{
		int num = ni * (j + nj * k);
		a = Buffer[num + i0];
		b = Buffer[num + i0 + 1];
	}

	public void get_x_pair(int i0, int j, int k, out double a, out double b)
	{
		int num = ni * (j + nj * k);
		a = Buffer[num + i0];
		b = Buffer[num + i0 + 1];
	}

	public void apply(Func<float, float> f)
	{
		for (int i = 0; i < nk; i++)
		{
			for (int j = 0; j < nj; j++)
			{
				for (int k = 0; k < ni; k++)
				{
					int num = k + ni * (j + nj * i);
					Buffer[num] = f(Buffer[num]);
				}
			}
		}
	}

	public DenseGrid2f get_slice(int slice_i, int dimension)
	{
		DenseGrid2f denseGrid2f;
		switch (dimension)
		{
		case 0:
		{
			denseGrid2f = new DenseGrid2f(nj, nk, 0f);
			for (int k = 0; k < nk; k++)
			{
				for (int l = 0; l < nj; l++)
				{
					denseGrid2f[l, k] = Buffer[slice_i + ni * (l + nj * k)];
				}
			}
			break;
		}
		case 1:
		{
			denseGrid2f = new DenseGrid2f(ni, nk, 0f);
			for (int m = 0; m < nk; m++)
			{
				for (int n = 0; n < ni; n++)
				{
					denseGrid2f[n, m] = Buffer[n + ni * (slice_i + nj * m)];
				}
			}
			break;
		}
		default:
		{
			denseGrid2f = new DenseGrid2f(ni, nj, 0f);
			for (int i = 0; i < nj; i++)
			{
				for (int j = 0; j < ni; j++)
				{
					denseGrid2f[j, i] = Buffer[j + ni * (i + nj * slice_i)];
				}
			}
			break;
		}
		}
		return denseGrid2f;
	}

	public void set_slice(DenseGrid2f slice, int slice_i, int dimension)
	{
		switch (dimension)
		{
		case 0:
		{
			for (int i = 0; i < nk; i++)
			{
				for (int j = 0; j < nj; j++)
				{
					Buffer[slice_i + ni * (j + nj * i)] = slice[j, i];
				}
			}
			return;
		}
		case 1:
		{
			for (int k = 0; k < nk; k++)
			{
				for (int l = 0; l < ni; l++)
				{
					Buffer[l + ni * (slice_i + nj * k)] = slice[l, k];
				}
			}
			return;
		}
		}
		for (int m = 0; m < nj; m++)
		{
			for (int n = 0; n < ni; n++)
			{
				Buffer[n + ni * (m + nj * slice_i)] = slice[n, m];
			}
		}
	}

	public IEnumerable<Vector3i> Indices()
	{
		int z = 0;
		while (z < nk)
		{
			int num;
			for (int y = 0; y < nj; y = num)
			{
				for (int x = 0; x < ni; x = num)
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

	public IEnumerable<Vector3i> InsetIndices(int border_width)
	{
		int stopy = nj - border_width;
		int stopx = ni - border_width;
		int z = border_width;
		while (z < nk - border_width)
		{
			int num;
			for (int y = border_width; y < stopy; y = num)
			{
				for (int x = border_width; x < stopx; x = num)
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

	public Vector3i to_index(int idx)
	{
		int x = idx % ni;
		int y = idx / ni % nj;
		int z = idx / (ni * nj);
		return new Vector3i(x, y, z);
	}

	public int to_linear(int i, int j, int k)
	{
		return i + ni * (j + nj * k);
	}

	public int to_linear(ref Vector3i ijk)
	{
		return ijk.x + ni * (ijk.y + nj * ijk.z);
	}

	public int to_linear(Vector3i ijk)
	{
		return ijk.x + ni * (ijk.y + nj * ijk.z);
	}
}
