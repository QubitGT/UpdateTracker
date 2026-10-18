using System;
using Unity.Mathematics;

namespace g3;

public struct Index4i
{
	public int a;

	public int b;

	public int c;

	public int d;

	public static readonly Index4i Zero = new Index4i(0, 0, 0, 0);

	public static readonly Index4i One = new Index4i(1, 1, 1, 1);

	public static readonly Index4i Max = new Index4i(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

	public int this[int key]
	{
		get
		{
			return key switch
			{
				2 => c, 
				1 => b, 
				0 => a, 
				_ => d, 
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
			case 2:
				c = value;
				break;
			default:
				d = value;
				break;
			}
		}
	}

	public int[] array => new int[4] { a, b, c, d };

	public int LengthSquared => a * a + b * b + c * c + d * d;

	public int Length => (int)Math.Sqrt(LengthSquared);

	public Index4i(int z)
	{
		a = (b = (c = (d = z)));
	}

	public Index4i(int aa, int bb, int cc, int dd)
	{
		a = aa;
		b = bb;
		c = cc;
		d = dd;
	}

	public Index4i(int[] i2)
	{
		a = i2[0];
		b = i2[1];
		c = i2[2];
		d = i2[3];
	}

	public Index4i(Index4i copy)
	{
		a = copy.a;
		b = copy.b;
		c = copy.b;
		d = copy.d;
	}

	public void Set(Index4i o)
	{
		a = o[0];
		b = o[1];
		c = o[2];
		d = o[3];
	}

	public void Set(int aa, int bb, int cc, int dd)
	{
		a = aa;
		b = bb;
		c = cc;
		d = dd;
	}

	public bool Contains(int val)
	{
		if (a != val && b != val && c != val)
		{
			return d == val;
		}
		return true;
	}

	public void Sort()
	{
		if (d < c)
		{
			int num = d;
			d = c;
			c = num;
		}
		if (c < b)
		{
			int num = c;
			c = b;
			b = num;
		}
		if (b < a)
		{
			int num = b;
			b = a;
			a = num;
		}
		if (b > c)
		{
			int num = c;
			c = b;
			b = num;
		}
		if (c > d)
		{
			int num = d;
			d = c;
			c = num;
		}
		if (b > c)
		{
			int num = c;
			c = b;
			b = num;
		}
	}

	public static Index4i operator -(Index4i v)
	{
		return new Index4i(-v.a, -v.b, -v.c, -v.d);
	}

	public static Index4i operator *(int f, Index4i v)
	{
		return new Index4i(f * v.a, f * v.b, f * v.c, f * v.d);
	}

	public static Index4i operator *(Index4i v, int f)
	{
		return new Index4i(f * v.a, f * v.b, f * v.c, f * v.d);
	}

	public static Index4i operator /(Index4i v, int f)
	{
		return new Index4i(v.a / f, v.b / f, v.c / f, v.d / f);
	}

	public static Index4i operator *(Index4i a, Index4i b)
	{
		return new Index4i(a.a * b.a, a.b * b.b, a.c * b.c, a.d * b.d);
	}

	public static Index4i operator /(Index4i a, Index4i b)
	{
		return new Index4i(a.a / b.a, a.b / b.b, a.c / b.c, a.d / b.d);
	}

	public static Index4i operator +(Index4i v0, Index4i v1)
	{
		return new Index4i(v0.a + v1.a, v0.b + v1.b, v0.c + v1.c, v0.d + v1.d);
	}

	public static Index4i operator +(Index4i v0, int f)
	{
		return new Index4i(v0.a + f, v0.b + f, v0.c + f, v0.d + f);
	}

	public static Index4i operator -(Index4i v0, Index4i v1)
	{
		return new Index4i(v0.a - v1.a, v0.b - v1.b, v0.c - v1.c, v0.d - v1.d);
	}

	public static Index4i operator -(Index4i v0, int f)
	{
		return new Index4i(v0.a - f, v0.b - f, v0.c - f, v0.d - f);
	}

	public static bool operator ==(Index4i a, Index4i b)
	{
		if (a.a == b.a && a.b == b.b && a.c == b.c)
		{
			return a.d == b.d;
		}
		return false;
	}

	public static bool operator !=(Index4i a, Index4i b)
	{
		if (a.a == b.a && a.b == b.b && a.c == b.c)
		{
			return a.d != b.d;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Index4i)obj;
	}

	public override int GetHashCode()
	{
		return ((((((0x50C5D1F ^ a.GetHashCode()) * 16777619) ^ b.GetHashCode()) * 16777619) ^ c.GetHashCode()) * 16777619) ^ d.GetHashCode();
	}

	public int CompareTo(Index4i other)
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
		if (d != other.d)
		{
			if (d >= other.d)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Index4i other)
	{
		if (a == other.a && b == other.b && c == other.c)
		{
			return d == other.d;
		}
		return false;
	}

	public override string ToString()
	{
		return $"[{a},{b},{c},{d}]";
	}

	public static implicit operator Index4i(int4 v)
	{
		return new Index4i(v.x, v.y, v.z, v.w);
	}

	public static implicit operator int4(Index4i v)
	{
		return new int4(v.a, v.b, v.c, v.d);
	}
}
