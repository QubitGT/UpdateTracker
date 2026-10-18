using System;

namespace UnityEngine.ProBuilder;

internal struct IntVec2 : IEquatable<IntVec2>
{
	public Vector2 value;

	public float x => value.x;

	public float y => value.y;

	public IntVec2(Vector2 vector)
	{
		value = vector;
	}

	public override string ToString()
	{
		return $"({x:F2}, {y:F2})";
	}

	public static bool operator ==(IntVec2 a, IntVec2 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(IntVec2 a, IntVec2 b)
	{
		return !(a == b);
	}

	public bool Equals(IntVec2 p)
	{
		if (round(x) == round(p.x))
		{
			return round(y) == round(p.y);
		}
		return false;
	}

	public bool Equals(Vector2 p)
	{
		if (round(x) == round(p.x))
		{
			return round(y) == round(p.y);
		}
		return false;
	}

	public override bool Equals(object b)
	{
		if (!(b is IntVec2) || !Equals((IntVec2)b))
		{
			if (b is Vector2)
			{
				return Equals((Vector2)b);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return VectorHash.GetHashCode(value);
	}

	private static int round(float v)
	{
		return Convert.ToInt32(v * 1000f);
	}

	public static implicit operator Vector2(IntVec2 p)
	{
		return p.value;
	}

	public static implicit operator IntVec2(Vector2 p)
	{
		return new IntVec2(p);
	}
}
