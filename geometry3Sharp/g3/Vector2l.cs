using System;
using Unity.Mathematics;

namespace g3;

public struct Vector2l : IComparable<Vector2l>, IEquatable<Vector2l>
{
	public long x;

	public long y;

	public static readonly Vector2l Zero = new Vector2l(0L, 0L);

	public static readonly Vector2l One = new Vector2l(1L, 1L);

	public static readonly Vector2l AxisX = new Vector2l(1L, 0L);

	public static readonly Vector2l AxisY = new Vector2l(0L, 1L);

	public long this[long key]
	{
		get
		{
			if (key != 0L)
			{
				return y;
			}
			return x;
		}
		set
		{
			if (key == 0L)
			{
				x = value;
			}
			else
			{
				y = value;
			}
		}
	}

	public long[] array => new long[2] { x, y };

	public Vector2l(long f)
	{
		x = (y = f);
	}

	public Vector2l(long x, long y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2l(long[] v2)
	{
		x = v2[0];
		y = v2[1];
	}

	public void Add(long s)
	{
		x += s;
		y += s;
	}

	public static Vector2l operator -(Vector2l v)
	{
		return new Vector2l(-v.x, -v.y);
	}

	public static Vector2l operator *(long f, Vector2l v)
	{
		return new Vector2l(f * v.x, f * v.y);
	}

	public static Vector2l operator *(Vector2l v, long f)
	{
		return new Vector2l(f * v.x, f * v.y);
	}

	public static Vector2l operator /(Vector2l v, long f)
	{
		return new Vector2l(v.x / f, v.y / f);
	}

	public static Vector2l operator /(long f, Vector2l v)
	{
		return new Vector2l(f / v.x, f / v.y);
	}

	public static Vector2l operator *(Vector2l a, Vector2l b)
	{
		return new Vector2l(a.x * b.x, a.y * b.y);
	}

	public static Vector2l operator /(Vector2l a, Vector2l b)
	{
		return new Vector2l(a.x / b.x, a.y / b.y);
	}

	public static Vector2l operator +(Vector2l v0, Vector2l v1)
	{
		return new Vector2l(v0.x + v1.x, v0.y + v1.y);
	}

	public static Vector2l operator +(Vector2l v0, long f)
	{
		return new Vector2l(v0.x + f, v0.y + f);
	}

	public static Vector2l operator -(Vector2l v0, Vector2l v1)
	{
		return new Vector2l(v0.x - v1.x, v0.y - v1.y);
	}

	public static Vector2l operator -(Vector2l v0, long f)
	{
		return new Vector2l(v0.x - f, v0.y - f);
	}

	public static bool operator ==(Vector2l a, Vector2l b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Vector2l a, Vector2l b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2l)obj;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode();
	}

	public int CompareTo(Vector2l other)
	{
		if (x != other.x)
		{
			if (x >= other.x)
			{
				return 1;
			}
			return -1;
		}
		if (y != other.y)
		{
			if (y >= other.y)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Vector2l other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{x} {y}";
	}

	public static implicit operator Vector2l(int2 v)
	{
		return new Vector2l(v.x, v.y);
	}

	public static explicit operator int2(Vector2l v)
	{
		return new int2((int)v.x, (int)v.y);
	}
}
