using System.Collections.Generic;
using System.Threading;

namespace g3;

public class DenseGrid3i
{
	public int[] Buffer;

	public int ni;

	public int nj;

	public int nk;

	public int size => ni * nj * nk;

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

	public int this[int i, int j, int k]
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

	public int this[Vector3i ijk]
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

	public DenseGrid3i()
	{
		ni = (nj = (nk = 0));
	}

	public DenseGrid3i(int ni, int nj, int nk, int initialValue)
	{
		resize(ni, nj, nk);
		assign(initialValue);
	}

	public void resize(int ni, int nj, int nk)
	{
		Buffer = new int[ni * nj * nk];
		this.ni = ni;
		this.nj = nj;
		this.nk = nk;
	}

	public void assign(int value)
	{
		for (int i = 0; i < Buffer.Length; i++)
		{
			Buffer[i] = value;
		}
	}

	public void increment(int i, int j, int k)
	{
		Buffer[i + ni * (j + nj * k)]++;
	}

	public void decrement(int i, int j, int k)
	{
		Buffer[i + ni * (j + nj * k)]--;
	}

	public void atomic_increment(int i, int j, int k)
	{
		Interlocked.Increment(ref Buffer[i + ni * (j + nj * k)]);
	}

	public void atomic_decrement(int i, int j, int k)
	{
		Interlocked.Decrement(ref Buffer[i + ni * (j + nj * k)]);
	}

	public void atomic_incdec(int i, int j, int k, bool decrement = false)
	{
		if (decrement)
		{
			Interlocked.Decrement(ref Buffer[i + ni * (j + nj * k)]);
		}
		else
		{
			Interlocked.Increment(ref Buffer[i + ni * (j + nj * k)]);
		}
	}

	public DenseGrid2i get_slice(int slice_i, int dimension)
	{
		DenseGrid2i denseGrid2i;
		switch (dimension)
		{
		case 0:
		{
			denseGrid2i = new DenseGrid2i(nj, nk, 0);
			for (int k = 0; k < nk; k++)
			{
				for (int l = 0; l < nj; l++)
				{
					denseGrid2i[l, k] = Buffer[slice_i + ni * (l + nj * k)];
				}
			}
			break;
		}
		case 1:
		{
			denseGrid2i = new DenseGrid2i(ni, nk, 0);
			for (int m = 0; m < nk; m++)
			{
				for (int n = 0; n < ni; n++)
				{
					denseGrid2i[n, m] = Buffer[n + ni * (slice_i + nj * m)];
				}
			}
			break;
		}
		default:
		{
			denseGrid2i = new DenseGrid2i(ni, nj, 0);
			for (int i = 0; i < nj; i++)
			{
				for (int j = 0; j < ni; j++)
				{
					denseGrid2i[j, i] = Buffer[j + ni * (i + nj * slice_i)];
				}
			}
			break;
		}
		}
		return denseGrid2i;
	}

	public Bitmap3 get_bitmap(int thresh = 0)
	{
		Bitmap3 bitmap = new Bitmap3(new Vector3i(ni, nj, nk));
		for (int i = 0; i < Buffer.Length; i++)
		{
			bitmap[i] = ((Buffer[i] > thresh) ? true : false);
		}
		return bitmap;
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
}
