using System.Collections;
using System.Collections.Generic;

namespace g3;

public class HBitArray : IEnumerable<int>, IEnumerable
{
	private struct MyBitVector32
	{
		private int bits;

		public bool this[int i]
		{
			get
			{
				return (bits & (1 << i)) != 0;
			}
			set
			{
				if (value)
				{
					bits |= 1 << i;
				}
				else
				{
					bits &= ~(1 << i);
				}
			}
		}

		public int Data => bits;
	}

	private struct Layer
	{
		public MyBitVector32[] layer_bits;
	}

	private MyBitVector32[] bits;

	private Layer[] layers;

	private int layerCount;

	private int max_index;

	private int count;

	public bool this[int i]
	{
		get
		{
			return Get(i);
		}
		set
		{
			Set(i, value);
		}
	}

	public int Count => max_index;

	public int TrueCount => count;

	public HBitArray(int maxIndex)
	{
		max_index = maxIndex;
		int num = maxIndex / 32;
		if (maxIndex % 32 != 0)
		{
			num++;
		}
		bits = new MyBitVector32[num];
		count = 0;
		layerCount = 2;
		layers = new Layer[layerCount];
		int num2 = bits.Length;
		for (int i = 0; i < layerCount; i++)
		{
			int num3 = num2 / 32;
			if (num2 % 32 != 0)
			{
				num3++;
			}
			layers[i].layer_bits = new MyBitVector32[num3];
			num2 = num3;
		}
	}

	public bool Contains(int i)
	{
		return Get(i);
	}

	public void Add(int i)
	{
		Set(i, value: true);
	}

	public void Set(int i, bool value)
	{
		int num = i / 32;
		int i2 = i - 32 * num;
		if (value)
		{
			if (!bits[num][i2])
			{
				bits[num][i2] = true;
				count++;
				for (int j = 0; j < layerCount; j++)
				{
					int num2 = num / 32;
					int i3 = num - 32 * num2;
					layers[j].layer_bits[num2][i3] = true;
					num = num2;
				}
			}
		}
		else if (bits[num][i2])
		{
			bits[num][i2] = false;
			count--;
			for (int k = 0; k < layerCount; k++)
			{
				int num3 = num / 32;
				int i4 = num - 32 * num3;
				layers[k].layer_bits[num3][i4] = false;
				num = num3;
			}
		}
	}

	public bool Get(int i)
	{
		int num = i / 32;
		int i2 = i - 32 * num;
		return bits[num][i2];
	}

	public IEnumerator<int> GetEnumerator()
	{
		int bi2;
		if (count > max_index / 3)
		{
			bi2 = 0;
			while (bi2 < bits.Length)
			{
				int d = bits[bi2].Data;
				int dmask = 1;
				int maxj = ((bi2 == bits.Length - 1) ? (max_index % 32) : 32);
				int num;
				for (int i = 0; i < maxj; i = num)
				{
					if ((d & dmask) != 0)
					{
						yield return bi2 * 32 + i;
					}
					dmask <<= 1;
					num = i + 1;
				}
				num = bi2 + 1;
				bi2 = num;
			}
			yield break;
		}
		bi2 = 0;
		while (bi2 < layers[1].layer_bits.Length)
		{
			int num;
			if (layers[1].layer_bits[bi2].Data != 0)
			{
				for (int maxj = 0; maxj < 32; maxj++)
				{
					if (!layers[1].layer_bits[bi2][maxj])
					{
						continue;
					}
					int dmask = bi2 * 32 + maxj;
					for (int d = 0; d < 32; d++)
					{
						if (!layers[0].layer_bits[dmask][d])
						{
							continue;
						}
						int i = dmask * 32 + d;
						int d2 = bits[i].Data;
						int dmask2 = 1;
						for (int j = 0; j < 32; j = num)
						{
							if ((d2 & dmask2) != 0)
							{
								yield return i * 32 + j;
							}
							dmask2 <<= 1;
							num = j + 1;
						}
					}
				}
			}
			num = bi2 + 1;
			bi2 = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
