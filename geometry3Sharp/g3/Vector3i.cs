using System;
using Unity.Mathematics;

namespace g3;

public struct Vector3i : IComparable<Vector3i>, IEquatable<Vector3i>
{
	public int x;

	public int y;

	public int z;

	public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

	public static readonly Vector3i One = new Vector3i(1, 1, 1);

	public static readonly Vector3i AxisX = new Vector3i(1, 0, 0);

	public static readonly Vector3i AxisY = new Vector3i(0, 1, 0);

	public static readonly Vector3i AxisZ = new Vector3i(0, 0, 1);

	public int this[int key]
	{
		get
		{
			return key switch
			{
				1 => y, 
				0 => x, 
				_ => z, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				z = value;
				break;
			}
		}
	}

	public int[] array => new int[3] { x, y, z };

	public int LengthSquared => x * x + y * y + z * z;

	public Vector3i(int f)
	{
		x = (y = (z = f));
	}

	public Vector3i(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3i(int[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
	}

	public void Set(Vector3i o)
	{
		x = o.x;
		y = o.y;
		z = o.z;
	}

	public void Set(int fX, int fY, int fZ)
	{
		x = fX;
		y = fY;
		z = fZ;
	}

	public void Add(Vector3i o)
	{
		x += o.x;
		y += o.y;
		z += o.z;
	}

	public void Subtract(Vector3i o)
	{
		x -= o.x;
		y -= o.y;
		z -= o.z;
	}

	public void Add(int s)
	{
		x += s;
		y += s;
		z += s;
	}

	public static Vector3i operator -(Vector3i v)
	{
		return new Vector3i(-v.x, -v.y, -v.z);
	}

	public static Vector3i operator *(int f, Vector3i v)
	{
		return new Vector3i(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3i operator *(Vector3i v, int f)
	{
		return new Vector3i(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3i operator /(Vector3i v, int f)
	{
		return new Vector3i(v.x / f, v.y / f, v.z / f);
	}

	public static Vector3i operator /(int f, Vector3i v)
	{
		return new Vector3i(f / v.x, f / v.y, f / v.z);
	}

	public static Vector3i operator *(Vector3i a, Vector3i b)
	{
		return new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector3i operator /(Vector3i a, Vector3i b)
	{
		return new Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	public static Vector3i operator +(Vector3i v0, Vector3i v1)
	{
		return new Vector3i(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
	}

	public static Vector3i operator +(Vector3i v0, int f)
	{
		return new Vector3i(v0.x + f, v0.y + f, v0.z + f);
	}

	public static Vector3i operator -(Vector3i v0, Vector3i v1)
	{
		return new Vector3i(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
	}

	public static Vector3i operator -(Vector3i v0, int f)
	{
		return new Vector3i(v0.x - f, v0.y - f, v0.z - f);
	}

	public static bool operator ==(Vector3i a, Vector3i b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(Vector3i a, Vector3i b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z != b.z;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector3i)obj;
	}

	public override int GetHashCode()
	{
		return ((((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode()) * 16777619) ^ z.GetHashCode();
	}

	public int CompareTo(Vector3i other)
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
		if (z != other.z)
		{
			if (z >= other.z)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Vector3i other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{x} {y} {z}";
	}

	public static implicit operator Vector3i(Index3i v)
	{
		return new Vector3i(v.a, v.b, v.c);
	}

	public static implicit operator Index3i(Vector3i v)
	{
		return new Index3i(v.x, v.y, v.z);
	}

	public static implicit operator Vector3i(int3 v)
	{
		return new Vector3i(v.x, v.y, v.z);
	}

	public static implicit operator int3(Vector3i v)
	{
		return new int3(v.x, v.y, v.z);
	}

	public static explicit operator Vector3i(Vector3f v)
	{
		return new Vector3i((int)v.x, (int)v.y, (int)v.z);
	}

	public static explicit operator Vector3f(Vector3i v)
	{
		return new Vector3f(v.x, v.y, v.z);
	}

	public static explicit operator Vector3i(Vector3d v)
	{
		return new Vector3i((int)v.x, (int)v.y, (int)v.z);
	}

	public static explicit operator Vector3d(Vector3i v)
	{
		return new Vector3d(v.x, v.y, v.z);
	}
}
