using System;
using Unity.Mathematics;

namespace g3;

public struct Vector2i : IComparable<Vector2i>, IEquatable<Vector2i>
{
	public int x;

	public int y;

	public static readonly Vector2i Zero = new Vector2i(0, 0);

	public static readonly Vector2i One = new Vector2i(1, 1);

	public static readonly Vector2i AxisX = new Vector2i(1, 0);

	public static readonly Vector2i AxisY = new Vector2i(0, 1);

	public int this[int key]
	{
		get
		{
			if (key != 0)
			{
				return y;
			}
			return x;
		}
		set
		{
			if (key == 0)
			{
				x = value;
			}
			else
			{
				y = value;
			}
		}
	}

	public int[] array => new int[2] { x, y };

	public int LengthSquared => x * x + y * y;

	public Vector2i(int f)
	{
		x = (y = f);
	}

	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2i(int[] v2)
	{
		x = v2[0];
		y = v2[1];
	}

	public void Add(int s)
	{
		x += s;
		y += s;
	}

	public static Vector2i operator -(Vector2i v)
	{
		return new Vector2i(-v.x, -v.y);
	}

	public static Vector2i operator *(int f, Vector2i v)
	{
		return new Vector2i(f * v.x, f * v.y);
	}

	public static Vector2i operator *(Vector2i v, int f)
	{
		return new Vector2i(f * v.x, f * v.y);
	}

	public static Vector2i operator /(Vector2i v, int f)
	{
		return new Vector2i(v.x / f, v.y / f);
	}

	public static Vector2i operator /(int f, Vector2i v)
	{
		return new Vector2i(f / v.x, f / v.y);
	}

	public static Vector2i operator *(Vector2i a, Vector2i b)
	{
		return new Vector2i(a.x * b.x, a.y * b.y);
	}

	public static Vector2i operator /(Vector2i a, Vector2i b)
	{
		return new Vector2i(a.x / b.x, a.y / b.y);
	}

	public static Vector2i operator +(Vector2i v0, Vector2i v1)
	{
		return new Vector2i(v0.x + v1.x, v0.y + v1.y);
	}

	public static Vector2i operator +(Vector2i v0, int f)
	{
		return new Vector2i(v0.x + f, v0.y + f);
	}

	public static Vector2i operator -(Vector2i v0, Vector2i v1)
	{
		return new Vector2i(v0.x - v1.x, v0.y - v1.y);
	}

	public static Vector2i operator -(Vector2i v0, int f)
	{
		return new Vector2i(v0.x - f, v0.y - f);
	}

	public static bool operator ==(Vector2i a, Vector2i b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Vector2i a, Vector2i b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2i)obj;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode();
	}

	public int CompareTo(Vector2i other)
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

	public bool Equals(Vector2i other)
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

	public static implicit operator Vector2i(int2 v)
	{
		return new Vector2i(v.x, v.y);
	}

	public static implicit operator int2(Vector2i v)
	{
		return new int2(v.x, v.y);
	}
}
