using System;

namespace UnityEngine.ProBuilder;

public struct Normal : IEquatable<Normal>
{
	public Vector3 normal { get; set; }

	public Vector4 tangent { get; set; }

	public Vector3 bitangent { get; set; }

	public override bool Equals(object obj)
	{
		if (obj is Normal)
		{
			return Equals((Normal)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((VectorHash.GetHashCode(normal) * 397) ^ VectorHash.GetHashCode(tangent)) * 397) ^ VectorHash.GetHashCode(bitangent);
	}

	public bool Equals(Normal other)
	{
		if (normal.Approx3(other.normal) && Math.Approx3(tangent, other.tangent))
		{
			return bitangent.Approx3(other.bitangent);
		}
		return false;
	}

	public static bool operator ==(Normal a, Normal b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Normal a, Normal b)
	{
		return !(a == b);
	}
}
