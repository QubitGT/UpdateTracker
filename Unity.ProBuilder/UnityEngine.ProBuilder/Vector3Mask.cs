using System;

namespace UnityEngine.ProBuilder;

internal struct Vector3Mask : IEquatable<Vector3Mask>
{
	private const byte X = 1;

	private const byte Y = 2;

	private const byte Z = 4;

	public static readonly Vector3Mask XYZ = new Vector3Mask(7);

	private byte m_Mask;

	public float x
	{
		get
		{
			if ((m_Mask & 1) != 1)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public float y
	{
		get
		{
			if ((m_Mask & 2) != 2)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public float z
	{
		get
		{
			if ((m_Mask & 4) != 4)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public int active
	{
		get
		{
			int num = 0;
			if ((m_Mask & 1) > 0)
			{
				num++;
			}
			if ((m_Mask & 2) > 0)
			{
				num++;
			}
			if ((m_Mask & 4) > 0)
			{
				num++;
			}
			return num;
		}
	}

	public float this[int i]
	{
		get
		{
			if (i < 0 || i > 2)
			{
				throw new IndexOutOfRangeException();
			}
			return (float)(1 & (m_Mask >> i)) * 1f;
		}
		set
		{
			if (i < 0 || i > 2)
			{
				throw new IndexOutOfRangeException();
			}
			m_Mask &= (byte)(~(1 << i));
			m_Mask |= (byte)(((value > 0f) ? 1u : 0u) << i);
		}
	}

	public Vector3Mask(Vector3 v, float epsilon = float.Epsilon)
	{
		m_Mask = 0;
		if (Mathf.Abs(v.x) > epsilon)
		{
			m_Mask |= 1;
		}
		if (Mathf.Abs(v.y) > epsilon)
		{
			m_Mask |= 2;
		}
		if (Mathf.Abs(v.z) > epsilon)
		{
			m_Mask |= 4;
		}
	}

	public Vector3Mask(byte mask)
	{
		m_Mask = mask;
	}

	public override string ToString()
	{
		return $"{{{x}, {y}, {z}}}";
	}

	public static implicit operator Vector3(Vector3Mask mask)
	{
		return new Vector3(mask.x, mask.y, mask.z);
	}

	public static explicit operator Vector3Mask(Vector3 v)
	{
		return new Vector3Mask(v);
	}

	public static Vector3Mask operator |(Vector3Mask left, Vector3Mask right)
	{
		return new Vector3Mask((byte)(left.m_Mask | right.m_Mask));
	}

	public static Vector3Mask operator &(Vector3Mask left, Vector3Mask right)
	{
		return new Vector3Mask((byte)(left.m_Mask & right.m_Mask));
	}

	public static Vector3Mask operator ^(Vector3Mask left, Vector3Mask right)
	{
		return new Vector3Mask((byte)(left.m_Mask ^ right.m_Mask));
	}

	public static Vector3 operator *(Vector3Mask mask, float value)
	{
		return new Vector3(mask.x * value, mask.y * value, mask.z * value);
	}

	public static Vector3 operator *(Vector3Mask mask, Vector3 value)
	{
		return new Vector3(mask.x * value.x, mask.y * value.y, mask.z * value.z);
	}

	public static Vector3 operator *(Quaternion rotation, Vector3Mask mask)
	{
		int num = mask.active;
		if (num > 2)
		{
			return mask;
		}
		Vector3 vector = (rotation * (Vector3)mask).Abs();
		if (num > 1)
		{
			return new Vector3((vector.x > vector.y || vector.x > vector.z) ? 1 : 0, (vector.y > vector.x || vector.y > vector.z) ? 1 : 0, (vector.z > vector.x || vector.z > vector.y) ? 1 : 0);
		}
		return new Vector3((vector.x > vector.y && vector.x > vector.z) ? 1 : 0, (vector.y > vector.z && vector.y > vector.x) ? 1 : 0, (vector.z > vector.x && vector.z > vector.y) ? 1 : 0);
	}

	public static bool operator ==(Vector3Mask left, Vector3Mask right)
	{
		return left.m_Mask == right.m_Mask;
	}

	public static bool operator !=(Vector3Mask left, Vector3Mask right)
	{
		return !(left == right);
	}

	public bool Equals(Vector3Mask other)
	{
		return m_Mask == other.m_Mask;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Vector3Mask)
		{
			return Equals((Vector3Mask)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_Mask.GetHashCode();
	}
}
