using System;
using Unity.Mathematics;

namespace g3;

public class Matrix2f
{
	public float m00;

	public float m01;

	public float m10;

	public float m11;

	public static readonly Matrix2f Identity = new Matrix2f(bIdentity: true);

	public static readonly Matrix2f Zero = new Matrix2f(bIdentity: false);

	public static readonly Matrix2f One = new Matrix2f(1f, 1f, 1f, 1f);

	public float Determinant => m00 * m11 - m01 * m10;

	public Matrix2f(bool bIdentity)
	{
		if (bIdentity)
		{
			m00 = (m11 = 1f);
			m01 = (m10 = 0f);
		}
		else
		{
			m00 = (m01 = (m10 = (m11 = 0f)));
		}
	}

	public Matrix2f(float m00, float m01, float m10, float m11)
	{
		this.m00 = m00;
		this.m01 = m01;
		this.m10 = m10;
		this.m11 = m11;
	}

	public Matrix2f(float m00, float m11)
	{
		this.m00 = m00;
		this.m11 = m11;
		m01 = (m10 = 0f);
	}

	public Matrix2f(float radians)
	{
		SetToRotationRad(radians);
	}

	public Matrix2f(Vector2f u, Vector2f v, bool columns)
	{
		if (columns)
		{
			m00 = u.x;
			m01 = v.x;
			m10 = u.y;
			m11 = v.y;
		}
		else
		{
			m00 = u.x;
			m01 = u.y;
			m10 = v.x;
			m11 = v.y;
		}
	}

	public Vector2f Row(int i)
	{
		if (i != 0)
		{
			return new Vector2f(m10, m11);
		}
		return new Vector2f(m00, m01);
	}

	public Vector2f Column(int i)
	{
		if (i != 0)
		{
			return new Vector2f(m01, m11);
		}
		return new Vector2f(m00, m10);
	}

	public Matrix2f(Vector2f u, Vector2f v)
	{
		m00 = u.x * v.x;
		m01 = u.x * v.y;
		m10 = u.y * v.x;
		m11 = u.y * v.y;
	}

	public void SetToDiagonal(float m00, float m11)
	{
		this.m00 = m00;
		this.m11 = m11;
		m01 = (m10 = 0f);
	}

	public void SetToRotationRad(float angleRad)
	{
		m11 = (m00 = (float)Math.Cos(angleRad));
		m10 = (float)Math.Sin(angleRad);
		m01 = 0f - m10;
	}

	public void SetToRotationDeg(float angleDeg)
	{
		SetToRotationRad(MathF.PI / 180f * angleDeg);
	}

	public float QForm(Vector2f u, Vector2f v)
	{
		return u.Dot(this * v);
	}

	public Matrix2f Transpose()
	{
		return new Matrix2f(m00, m10, m01, m11);
	}

	public Matrix2f Inverse(float epsilon = 0f)
	{
		float num = m00 * m11 - m10 * m01;
		if (Math.Abs(num) > epsilon)
		{
			float num2 = 1f / num;
			return new Matrix2f(m11 * num2, (0f - m01) * num2, (0f - m10) * num2, m00 * num2);
		}
		return Zero;
	}

	public Matrix2f Adjoint()
	{
		return new Matrix2f(m11, 0f - m01, 0f - m10, m00);
	}

	public float ExtractAngle()
	{
		return (float)Math.Atan2(m10, m00);
	}

	public void Orthonormalize()
	{
		float num = 1f / (float)Math.Sqrt(m00 * m00 + m10 * m10);
		m00 *= num;
		m10 *= num;
		float num2 = m00 * m01 + m10 * m11;
		m01 -= num2 * m00;
		m11 -= num2 * m10;
		num = 1f / (float)Math.Sqrt(m01 * m01 + m11 * m11);
		m01 *= num;
		m11 *= num;
	}

	public void EigenDecomposition(ref Matrix2f rot, ref Matrix2f diag)
	{
		float num = Math.Abs(m00) + Math.Abs(m11);
		if (Math.Abs(m01) + num == num)
		{
			rot.m00 = 1f;
			rot.m01 = 0f;
			rot.m10 = 0f;
			rot.m11 = 1f;
			diag.m00 = m00;
			diag.m01 = 0f;
			diag.m10 = 0f;
			diag.m11 = m11;
			return;
		}
		float num2 = m00 + m11;
		float num3 = m00 - m11;
		float num4 = (float)Math.Sqrt(num3 * num3 + 4f * m01 * m01);
		float num5 = 0.5f * (num2 - num4);
		float num6 = 0.5f * (num2 + num4);
		diag.SetToDiagonal(num5, num6);
		float num7;
		float num8;
		if ((double)num3 >= 0.0)
		{
			num7 = m01;
			num8 = num5 - m00;
		}
		else
		{
			num7 = num5 - m11;
			num8 = m01;
		}
		float num9 = 1f / (float)Math.Sqrt(num7 * num7 + num8 * num8);
		num7 *= num9;
		num8 *= num9;
		rot.m00 = num7;
		rot.m01 = 0f - num8;
		rot.m10 = num8;
		rot.m11 = num7;
	}

	public static Matrix2f operator -(Matrix2f v)
	{
		return new Matrix2f(0f - v.m00, 0f - v.m01, 0f - v.m10, 0f - v.m11);
	}

	public static Matrix2f operator +(Matrix2f a, Matrix2f o)
	{
		return new Matrix2f(a.m00 + o.m00, a.m01 + o.m01, a.m10 + o.m10, a.m11 + o.m11);
	}

	public static Matrix2f operator +(Matrix2f a, float f)
	{
		return new Matrix2f(a.m00 + f, a.m01 + f, a.m10 + f, a.m11 + f);
	}

	public static Matrix2f operator -(Matrix2f a, Matrix2f o)
	{
		return new Matrix2f(a.m00 - o.m00, a.m01 - o.m01, a.m10 - o.m10, a.m11 - o.m11);
	}

	public static Matrix2f operator -(Matrix2f a, float f)
	{
		return new Matrix2f(a.m00 - f, a.m01 - f, a.m10 - f, a.m11 - f);
	}

	public static Matrix2f operator *(Matrix2f a, float f)
	{
		return new Matrix2f(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
	}

	public static Matrix2f operator *(float f, Matrix2f a)
	{
		return new Matrix2f(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
	}

	public static Matrix2f operator /(Matrix2f a, float f)
	{
		return new Matrix2f(a.m00 / f, a.m01 / f, a.m10 / f, a.m11 / f);
	}

	public static Vector2f operator *(Matrix2f m, Vector2f v)
	{
		return new Vector2f(m.m00 * v.x + m.m01 * v.y, m.m10 * v.x + m.m11 * v.y);
	}

	public static Vector2f operator *(Vector2f v, Matrix2f m)
	{
		return new Vector2f(v.x * m.m00 + v.y * m.m10, v.x * m.m01 + v.y * m.m11);
	}

	public static implicit operator Matrix2f(float2x2 m)
	{
		return new Matrix2f(m.c0, m.c1, columns: true);
	}

	public static explicit operator float2x2(Matrix2f m)
	{
		return new float2x2(m.Column(0), m.Column(1));
	}
}
