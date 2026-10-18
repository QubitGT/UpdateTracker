namespace UnityEngine.ProBuilder;

internal struct Vector2Mask
{
	private const byte X = 1;

	private const byte Y = 2;

	public static readonly Vector2Mask XY = new Vector2Mask(3);

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

	public Vector2Mask(Vector3 v, float epsilon = float.Epsilon)
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
	}

	public Vector2Mask(byte mask)
	{
		m_Mask = mask;
	}

	public static implicit operator Vector2(Vector2Mask mask)
	{
		return new Vector2(mask.x, mask.y);
	}

	public static implicit operator Vector2Mask(Vector2 v)
	{
		return new Vector2Mask(v);
	}

	public static Vector2Mask operator |(Vector2Mask left, Vector2Mask right)
	{
		return new Vector2Mask((byte)(left.m_Mask | right.m_Mask));
	}

	public static Vector2Mask operator &(Vector2Mask left, Vector2Mask right)
	{
		return new Vector2Mask((byte)(left.m_Mask & right.m_Mask));
	}

	public static Vector2Mask operator ^(Vector2Mask left, Vector2Mask right)
	{
		return new Vector2Mask((byte)(left.m_Mask ^ right.m_Mask));
	}

	public static Vector2 operator *(Vector2Mask mask, float value)
	{
		return new Vector2(mask.x * value, mask.y * value);
	}
}
