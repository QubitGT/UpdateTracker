using System;

namespace Pathfinding;

public struct Int2 : IEquatable<Int2>
{
	public int x;

	public int y;

	public long sqrMagnitudeLong => (long)x * (long)x + (long)y * (long)y;

	public Int2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static Int2 operator -(Int2 lhs)
	{
		lhs.x = -lhs.x;
		lhs.y = -lhs.y;
		return lhs;
	}

	public static Int2 operator +(Int2 a, Int2 b)
	{
		return new Int2(a.x + b.x, a.y + b.y);
	}

	public static Int2 operator -(Int2 a, Int2 b)
	{
		return new Int2(a.x - b.x, a.y - b.y);
	}

	public static bool operator ==(Int2 a, Int2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Int2 a, Int2 b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public static long DotLong(Int2 a, Int2 b)
	{
		return (long)a.x * (long)b.x + (long)a.y * (long)b.y;
	}

	public override bool Equals(object o)
	{
		if (o == null)
		{
			return false;
		}
		Int2 @int = (Int2)o;
		if (x == @int.x)
		{
			return y == @int.y;
		}
		return false;
	}

	public bool Equals(Int2 other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x * 49157 + y * 98317;
	}

	public static Int2 Min(Int2 a, Int2 b)
	{
		return new Int2(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
	}

	public static Int2 Max(Int2 a, Int2 b)
	{
		return new Int2(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
	}

	public static Int2 FromInt3XZ(Int3 o)
	{
		return new Int2(o.x, o.z);
	}

	public static Int3 ToInt3XZ(Int2 o)
	{
		return new Int3(o.x, 0, o.y);
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
