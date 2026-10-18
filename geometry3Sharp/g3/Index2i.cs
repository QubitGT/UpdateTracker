using System;
using Unity.Mathematics;

namespace g3;

public struct Index2i : IComparable<Index2i>, IEquatable<Index2i>
{
	public int a;

	public int b;

	public static readonly Index2i Zero = new Index2i(0, 0);

	public static readonly Index2i One = new Index2i(1, 1);

	public static readonly Index2i Max = new Index2i(int.MaxValue, int.MaxValue);

	public static readonly Index2i Min = new Index2i(int.MinValue, int.MinValue);

	public int this[int key]
	{
		get
		{
			if (key != 0)
			{
				return b;
			}
			return a;
		}
		set
		{
			if (key == 0)
			{
				a = value;
			}
			else
			{
				b = value;
			}
		}
	}

	public int[] array => new int[2] { a, b };

	public int LengthSquared => a * a + b * b;

	public int Length => (int)Math.Sqrt(LengthSquared);

	public Index2i(int z)
	{
		a = (b = z);
	}

	public Index2i(int ii, int jj)
	{
		a = ii;
		b = jj;
	}

	public Index2i(int[] i2)
	{
		a = i2[0];
		b = i2[1];
	}

	public Index2i(Index2i copy)
	{
		a = copy.a;
		b = copy.b;
	}

	public void Set(Index2i o)
	{
		a = o[0];
		b = o[1];
	}

	public void Set(int ii, int jj)
	{
		a = ii;
		b = jj;
	}

	public static Index2i operator -(Index2i v)
	{
		return new Index2i(-v.a, -v.b);
	}

	public static Index2i operator *(int f, Index2i v)
	{
		return new Index2i(f * v.a, f * v.b);
	}

	public static Index2i operator *(Index2i v, int f)
	{
		return new Index2i(f * v.a, f * v.b);
	}

	public static Index2i operator /(Index2i v, int f)
	{
		return new Index2i(v.a / f, v.b / f);
	}

	public static Index2i operator *(Index2i a, Index2i b)
	{
		return new Index2i(a.a * b.a, a.b * b.b);
	}

	public static Index2i operator /(Index2i a, Index2i b)
	{
		return new Index2i(a.a / b.a, a.b / b.b);
	}

	public static Index2i operator +(Index2i v0, Index2i v1)
	{
		return new Index2i(v0.a + v1.a, v0.b + v1.b);
	}

	public static Index2i operator +(Index2i v0, int f)
	{
		return new Index2i(v0.a + f, v0.b + f);
	}

	public static Index2i operator -(Index2i v0, Index2i v1)
	{
		return new Index2i(v0.a - v1.a, v0.b - v1.b);
	}

	public static Index2i operator -(Index2i v0, int f)
	{
		return new Index2i(v0.a - f, v0.b - f);
	}

	public static bool operator ==(Index2i a, Index2i b)
	{
		if (a.a == b.a)
		{
			return a.b == b.b;
		}
		return false;
	}

	public static bool operator !=(Index2i a, Index2i b)
	{
		if (a.a == b.a)
		{
			return a.b != b.b;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Index2i)obj;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ a.GetHashCode()) * 16777619) ^ b.GetHashCode();
	}

	public int CompareTo(Index2i other)
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
		return 0;
	}

	public bool Equals(Index2i other)
	{
		if (a == other.a)
		{
			return b == other.b;
		}
		return false;
	}

	public override string ToString()
	{
		return $"[{a},{b}]";
	}

	public static implicit operator Index2i(int2 v)
	{
		return new Index2i(v.x, v.y);
	}

	public static implicit operator int2(Index2i v)
	{
		return new int2(v.a, v.b);
	}
}
