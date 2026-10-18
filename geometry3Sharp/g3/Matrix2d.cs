using System;
using Unity.Mathematics;

namespace g3;

public class Matrix2d
{
	public double m00;

	public double m01;

	public double m10;

	public double m11;

	public static readonly Matrix2d Identity = new Matrix2d(bIdentity: true);

	public static readonly Matrix2d Zero = new Matrix2d(bIdentity: false);

	public static readonly Matrix2d One = new Matrix2d(1.0, 1.0, 1.0, 1.0);

	public double this[int r, int c]
	{
		get
		{
			if (r != 0)
			{
				if (c != 0)
				{
					return m11;
				}
				return m10;
			}
			if (c != 0)
			{
				return m01;
			}
			return m00;
		}
	}

	public double Determinant => m00 * m11 - m01 * m10;

	public Matrix2d(bool bIdentity)
	{
		if (bIdentity)
		{
			m00 = (m11 = 1.0);
			m01 = (m10 = 0.0);
		}
		else
		{
			m00 = (m01 = (m10 = (m11 = 0.0)));
		}
	}

	public Matrix2d(double m00, double m01, double m10, double m11)
	{
		this.m00 = m00;
		this.m01 = m01;
		this.m10 = m10;
		this.m11 = m11;
	}

	public Matrix2d(double m00, double m11)
	{
		this.m00 = m00;
		this.m11 = m11;
		m01 = (m10 = 0.0);
	}

	public Matrix2d(double angle, bool bDegrees = false)
	{
		if (bDegrees)
		{
			SetToRotationDeg(angle);
		}
		else
		{
			SetToRotationRad(angle);
		}
	}

	public Matrix2d(Vector2d u, Vector2d v, bool columns)
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

	public Matrix2d(Vector2d u, Vector2d v)
	{
		m00 = u.x * v.x;
		m01 = u.x * v.y;
		m10 = u.y * v.x;
		m11 = u.y * v.y;
	}

	public void SetToDiagonal(double m00, double m11)
	{
		this.m00 = m00;
		this.m11 = m11;
		m01 = (m10 = 0.0);
	}

	public void SetToRotationRad(double angleRad)
	{
		m11 = (m00 = Math.Cos(angleRad));
		m10 = Math.Sin(angleRad);
		m01 = 0.0 - m10;
	}

	public void SetToRotationDeg(double angleDeg)
	{
		SetToRotationRad(Math.PI / 180.0 * angleDeg);
	}

	public double QForm(Vector2d u, Vector2d v)
	{
		return u.Dot(this * v);
	}

	public Matrix2d Transpose()
	{
		return new Matrix2d(m00, m10, m01, m11);
	}

	public Matrix2d Inverse(double epsilon = 0.0)
	{
		double num = m00 * m11 - m10 * m01;
		if (Math.Abs(num) > epsilon)
		{
			double num2 = 1.0 / num;
			return new Matrix2d(m11 * num2, (0.0 - m01) * num2, (0.0 - m10) * num2, m00 * num2);
		}
		return Zero;
	}

	public Matrix2d Adjoint()
	{
		return new Matrix2d(m11, 0.0 - m01, 0.0 - m10, m00);
	}

	public double ExtractAngle()
	{
		return Math.Atan2(m10, m00);
	}

	public Vector2d Row(int i)
	{
		if (i != 0)
		{
			return new Vector2d(m10, m11);
		}
		return new Vector2d(m00, m01);
	}

	public Vector2d Column(int i)
	{
		if (i != 0)
		{
			return new Vector2d(m01, m11);
		}
		return new Vector2d(m00, m10);
	}

	public void Orthonormalize()
	{
		double num = 1.0 / Math.Sqrt(m00 * m00 + m10 * m10);
		m00 *= num;
		m10 *= num;
		double num2 = m00 * m01 + m10 * m11;
		m01 -= num2 * m00;
		m11 -= num2 * m10;
		num = 1.0 / Math.Sqrt(m01 * m01 + m11 * m11);
		m01 *= num;
		m11 *= num;
	}

	public void EigenDecomposition(ref Matrix2d rot, ref Matrix2d diag)
	{
		double num = Math.Abs(m00) + Math.Abs(m11);
		if (Math.Abs(m01) + num == num)
		{
			rot.m00 = 1.0;
			rot.m01 = 0.0;
			rot.m10 = 0.0;
			rot.m11 = 1.0;
			diag.m00 = m00;
			diag.m01 = 0.0;
			diag.m10 = 0.0;
			diag.m11 = m11;
			return;
		}
		double num2 = m00 + m11;
		double num3 = m00 - m11;
		double num4 = Math.Sqrt(num3 * num3 + 4.0 * m01 * m01);
		double num5 = 0.5 * (num2 - num4);
		double num6 = 0.5 * (num2 + num4);
		diag.SetToDiagonal(num5, num6);
		double num7;
		double num8;
		if (num3 >= 0.0)
		{
			num7 = m01;
			num8 = num5 - m00;
		}
		else
		{
			num7 = num5 - m11;
			num8 = m01;
		}
		double num9 = 1.0 / Math.Sqrt(num7 * num7 + num8 * num8);
		num7 *= num9;
		num8 *= num9;
		rot.m00 = num7;
		rot.m01 = 0.0 - num8;
		rot.m10 = num8;
		rot.m11 = num7;
	}

	public static Matrix2d operator -(Matrix2d v)
	{
		return new Matrix2d(0.0 - v.m00, 0.0 - v.m01, 0.0 - v.m10, 0.0 - v.m11);
	}

	public static Matrix2d operator +(Matrix2d a, Matrix2d o)
	{
		return new Matrix2d(a.m00 + o.m00, a.m01 + o.m01, a.m10 + o.m10, a.m11 + o.m11);
	}

	public static Matrix2d operator +(Matrix2d a, double f)
	{
		return new Matrix2d(a.m00 + f, a.m01 + f, a.m10 + f, a.m11 + f);
	}

	public static Matrix2d operator -(Matrix2d a, Matrix2d o)
	{
		return new Matrix2d(a.m00 - o.m00, a.m01 - o.m01, a.m10 - o.m10, a.m11 - o.m11);
	}

	public static Matrix2d operator -(Matrix2d a, double f)
	{
		return new Matrix2d(a.m00 - f, a.m01 - f, a.m10 - f, a.m11 - f);
	}

	public static Matrix2d operator *(Matrix2d a, double f)
	{
		return new Matrix2d(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
	}

	public static Matrix2d operator *(double f, Matrix2d a)
	{
		return new Matrix2d(a.m00 * f, a.m01 * f, a.m10 * f, a.m11 * f);
	}

	public static Matrix2d operator /(Matrix2d a, double f)
	{
		return new Matrix2d(a.m00 / f, a.m01 / f, a.m10 / f, a.m11 / f);
	}

	public static Vector2d operator *(Matrix2d m, Vector2d v)
	{
		return new Vector2d(m.m00 * v.x + m.m01 * v.y, m.m10 * v.x + m.m11 * v.y);
	}

	public static Vector2d operator *(Vector2d v, Matrix2d m)
	{
		return new Vector2d(v.x * m.m00 + v.y * m.m10, v.x * m.m01 + v.y * m.m11);
	}

	public static implicit operator Matrix2d(double2x2 m)
	{
		return new Matrix2d(m.c0, m.c1, columns: true);
	}

	public static explicit operator double2x2(Matrix2d m)
	{
		return new double2x2(m.Column(0), m.Column(1));
	}
}
