using System;

namespace UnityEngine.ProBuilder;

internal struct IntVec3 : IEquatable<IntVec3>
{
	public Vector3 value;

	public float x => value.x;

	public float y => value.y;

	public float z => value.z;

	public IntVec3(Vector3 vector)
	{
		value = vector;
	}

	public override string ToString()
	{
		return $"({x:F2}, {y:F2}, {z:F2})";
	}

	public static bool operator ==(IntVec3 a, IntVec3 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(IntVec3 a, IntVec3 b)
	{
		return !(a == b);
	}

	public bool Equals(IntVec3 p)
	{
		if (round(x) == round(p.x) && round(y) == round(p.y))
		{
			return round(z) == round(p.z);
		}
		return false;
	}

	public bool Equals(Vector3 p)
	{
		if (round(x) == round(p.x) && round(y) == round(p.y))
		{
			return round(z) == round(p.z);
		}
		return false;
	}

	public override bool Equals(object b)
	{
		if (!(b is IntVec3) || !Equals((IntVec3)b))
		{
			if (b is Vector3)
			{
				return Equals((Vector3)b);
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

	public static implicit operator Vector3(IntVec3 p)
	{
		return p.value;
	}

	public static implicit operator IntVec3(Vector3 p)
	{
		return new IntVec3(p);
	}
}
