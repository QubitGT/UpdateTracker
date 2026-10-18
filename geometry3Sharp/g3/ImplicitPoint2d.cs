using System;

namespace g3;

public class ImplicitPoint2d : ImplicitField2d
{
	private Vector2f m_vCenter;

	private float m_radius;

	public AxisAlignedBox2f Bounds => new AxisAlignedBox2f(LowX, LowY, HighX, HighY);

	public float LowX => m_vCenter.x - radius;

	public float LowY => m_vCenter.y - radius;

	public float HighX => m_vCenter.x + radius;

	public float HighY => m_vCenter.y + radius;

	public float radius
	{
		get
		{
			return m_radius;
		}
		set
		{
			m_radius = value;
		}
	}

	public float x
	{
		get
		{
			return m_vCenter.x;
		}
		set
		{
			m_vCenter.x = value;
		}
	}

	public float y
	{
		get
		{
			return m_vCenter.y;
		}
		set
		{
			m_vCenter.y = value;
		}
	}

	public Vector2f Center
	{
		get
		{
			return m_vCenter;
		}
		set
		{
			m_vCenter = value;
		}
	}

	public ImplicitPoint2d(float x, float y)
	{
		m_vCenter = new Vector2f(x, y);
		m_radius = 1f;
	}

	public ImplicitPoint2d(float x, float y, float radius)
	{
		m_vCenter = new Vector2f(x, y);
		m_radius = radius;
	}

	public float Value(float fX, float fY)
	{
		float num = fX - m_vCenter.x;
		float num2 = fY - m_vCenter.y;
		float num3 = num * num + num2 * num2;
		num3 /= m_radius * m_radius;
		num3 = 1f - num3;
		if (num3 < 0f)
		{
			return 0f;
		}
		return num3 * num3 * num3;
	}

	public void Gradient(float fX, float fY, ref float fGX, ref float fGY)
	{
		float num = fX - m_vCenter.x;
		float num2 = fY - m_vCenter.y;
		float num3 = num * num + num2 * num2;
		float num4 = 1f - num3;
		if (num4 < 0f)
		{
			fGX = (fGY = 0f);
			return;
		}
		float num5 = (float)Math.Sqrt(num3);
		float num6 = -6f * num5 * num4 * num4;
		num6 /= num5;
		fGX = num * num6;
		fGY = num2 * num6;
	}

	public bool InBounds(float x, float y)
	{
		if (x >= LowX && x <= HighX && x >= LowY)
		{
			return x <= HighY;
		}
		return false;
	}
}
