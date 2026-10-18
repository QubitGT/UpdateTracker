using System;
using Unity.Mathematics;

namespace g3;

public struct Index3i : IComparable<Index3i>, IEquatable<Index3i>
{
	public int a;

	public int b;

	public int c;

	public static readonly Index3i Zero = new Index3i(0, 0, 0);

	public static readonly Index3i One = new Index3i(1, 1, 1);

	public static readonly Index3i Max = new Index3i(int.MaxValue, int.MaxValue, int.MaxValue);

	public static readonly Index3i Min = new Index3i(int.MinValue, int.MinValue, int.MinValue);

	public int this[int key]
	{
		get
		{
			return key switch
			{
				1 => b, 
				0 => a, 
				_ => c, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				a = value;
				break;
			case 1:
				b = value;
				break;
			default:
				c = value;
				break;
			}
		}
	}

	public int[] array => new int[3] { a, b, c };

	public int LengthSquared => a * a + b * b + c * c;

	public int Length => (int)Math.Sqrt(LengthSquared);

	public Index3i(int z)
	{
		a = (b = (c = z));
	}

	public Index3i(int ii, int jj, int kk)
	{
		a = ii;
		b = jj;
		c = kk;
	}

	public Index3i(int[] i2)
	{
		a = i2[0];
		b = i2[1];
		c = i2[2];
	}

	public Index3i(Index3i copy)
	{
		a = copy.a;
		b = copy.b;
		c = copy.b;
	}

	public Index3i(int ii, int jj, int kk, bool cycle)
	{
		a = ii;
		if (cycle)
		{
			b = kk;
			c = jj;
		}
		else
		{
			b = jj;
			c = kk;
		}
	}

	public void Set(Index3i o)
	{
		a = o[0];
		b = o[1];
		c = o[2];
	}

	public void Set(int ii, int jj, int kk)
	{
		a = ii;
		b = jj;
		c = kk;
	}

	public static Index3i operator -(Index3i v)
	{
		return new Index3i(-v.a, -v.b, -v.c);
	}

	public static Index3i operator *(int f, Index3i v)
	{
		return new Index3i(f * v.a, f * v.b, f * v.c);
	}

	public static Index3i operator *(Index3i v, int f)
	{
		return new Index3i(f * v.a, f * v.b, f * v.c);
	}

	public static Index3i operator /(Index3i v, int f)
	{
		return new Index3i(v.a / f, v.b / f, v.c / f);
	}

	public static Index3i operator *(Index3i a, Index3i b)
	{
		return new Index3i(a.a * b.a, a.b * b.b, a.c * b.c);
	}

	public static Index3i operator /(Index3i a, Index3i b)
	{
		return new Index3i(a.a / b.a, a.b / b.b, a.c / b.c);
	}

	public static Index3i operator +(Index3i v0, Index3i v1)
	{
		return new Index3i(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c);
	}

	public static Index3i operator +(Index3i v0, int f)
	{
		return new Index3i(v0.a + f, v0.b + f, v0.c + f);
	}

	public static Index3i operator -(Index3i v0, Index3i v1)
	{
		return new Index3i(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c);
	}

	public static Index3i operator -(Index3i v0, int f)
	{
		return new Index3i(v0.a - f, v0.b - f, v0.c - f);
	}

	public static bool operator ==(Index3i a, Index3i b)
	{
		if (a.a == b.a && a.b == b.b)
		{
			return a.c == b.c;
		}
		return false;
	}

	public static bool operator !=(Index3i a, Index3i b)
	{
		if (a.a == b.a && a.b == b.b)
		{
			return a.c != b.c;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Index3i)obj;
	}

	public override int GetHashCode()
	{
		return ((((0x50C5D1F ^ a.GetHashCode()) * 16777619) ^ b.GetHashCode()) * 16777619) ^ c.GetHashCode();
	}

	public int CompareTo(Index3i other)
	{
		if (a != other.a)
		{
			if (a >= other.a)
			{
				return 1;
			}
			return -1;
		}
		if (b != other.b)
		{
			if (b >= other.b)
			{
				return 1;
			}
			return -1;
		}
		if (c != other.c)
		{
			if (c >= other.c)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Index3i other)
	{
		if (a == other.a && b == other.b)
		{
			return c == other.c;
		}
		return false;
	}

	public override string ToString()
	{
		return $"[{a},{b},{c}]";
	}

	public static implicit operator Index3i(int3 v)
	{
		return new Index3i(v.x, v.y, v.z);
	}

	public static implicit operator int3(Index3i v)
	{
		return new int3(v.a, v.b, v.c);
	}
}
