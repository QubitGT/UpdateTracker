using System;

namespace UnityEngine.ProBuilder;

internal struct IntVec4 : IEquatable<IntVec4>
{
	public Vector4 value;

	public float x => value.x;

	public float y => value.y;

	public float z => value.z;

	public float w => value.w;

	public IntVec4(Vector4 vector)
	{
		value = vector;
	}

	public override string ToString()
	{
		return $"({x:F2}, {y:F2}, {z:F2}, {w:F2})";
	}

	public static bool operator ==(IntVec4 a, IntVec4 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(IntVec4 a, IntVec4 b)
	{
		return !(a == b);
	}

	public bool Equals(IntVec4 p)
	{
		if (round(x) == round(p.x) && round(y) == round(p.y) && round(z) == round(p.z))
		{
			return round(w) == round(p.w);
		}
		return false;
	}

	public bool Equals(Vector4 p)
	{
		if (round(x) == round(p.x) && round(y) == round(p.y) && round(z) == round(p.z))
		{
			return round(w) == round(p.w);
		}
		return false;
	}

	public override bool Equals(object b)
	{
		if (!(b is IntVec4) || !Equals((IntVec4)b))
		{
			if (b is Vector4)
			{
				return Equals((Vector4)b);
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

	public static implicit operator Vector4(IntVec4 p)
	{
		return p.value;
	}

	public static implicit operator IntVec4(Vector4 p)
	{
		return new IntVec4(p);
	}
}
