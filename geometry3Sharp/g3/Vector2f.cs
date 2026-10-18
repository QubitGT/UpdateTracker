using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector2f : IComparable<Vector2f>, IEquatable<Vector2f>
{
	public float x;

	public float y;

	public static readonly Vector2f Zero = new Vector2f(0f, 0f);

	public static readonly Vector2f One = new Vector2f(1f, 1f);

	public static readonly Vector2f AxisX = new Vector2f(1f, 0f);

	public static readonly Vector2f AxisY = new Vector2f(0f, 1f);

	public static readonly Vector2f MaxValue = new Vector2f(float.MaxValue, float.MaxValue);

	public static readonly Vector2f MinValue = new Vector2f(float.MinValue, float.MinValue);

	public float this[int key]
	{
		get
		{
			if (key != 0)
			{
				return y;
			}
			return x;
		}
		set
		{
			if (key == 0)
			{
				x = value;
			}
			else
			{
				y = value;
			}
		}
	}

	public float LengthSquared => x * x + y * y;

	public float Length => (float)Math.Sqrt(LengthSquared);

	public Vector2f Normalized
	{
		get
		{
			float length = Length;
			if (length > 1.1920929E-07f)
			{
				float num = 1f / length;
				return new Vector2f(x * num, y * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => Math.Abs(x * x + y * y - 1f) < 1E-06f;

	public bool IsFinite
	{
		get
		{
			float f = x + y;
			if (!float.IsNaN(f))
			{
				return !float.IsInfinity(f);
			}
			return false;
		}
	}

	public Vector2f Perp => new Vector2f(y, 0f - x);

	public Vector2f UnitPerp => new Vector2f(y, 0f - x).Normalized;

	public Vector2f(float f)
	{
		x = (y = f);
	}

	public Vector2f(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2f(float[] v2)
	{
		x = v2[0];
		y = v2[1];
	}

	public Vector2f(double f)
	{
		x = (y = (float)f);
	}

	public Vector2f(double x, double y)
	{
		this.x = (float)x;
		this.y = (float)y;
	}

	public Vector2f(double[] v2)
	{
		x = (float)v2[0];
		y = (float)v2[1];
	}

	public Vector2f(Vector2f copy)
	{
		x = copy[0];
		y = copy[1];
	}

	public Vector2f(Vector2d copy)
	{
		x = (float)copy[0];
		y = (float)copy[1];
	}

	public float Normalize(float epsilon = 1.1920929E-07f)
	{
		float num = Length;
		if (num > epsilon)
		{
			float num2 = 1f / num;
			x *= num2;
			y *= num2;
		}
		else
		{
			num = 0f;
			x = (y = 0f);
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = (float)Math.Round(x, nDecimals);
		y = (float)Math.Round(y, nDecimals);
	}

	public float Dot(Vector2f v2)
	{
		return x * v2.x + y * v2.y;
	}

	public float Cross(Vector2f v2)
	{
		return x * v2.y - y * v2.x;
	}

	public float DotPerp(Vector2f v2)
	{
		return x * v2.y - y * v2.x;
	}

	public float AngleD(Vector2f v2)
	{
		return (float)(Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f)) * (180.0 / Math.PI));
	}

	public static float AngleD(Vector2f v1, Vector2f v2)
	{
		return v1.AngleD(v2);
	}

	public float AngleR(Vector2f v2)
	{
		return (float)Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f));
	}

	public static float AngleR(Vector2f v1, Vector2f v2)
	{
		return v1.AngleR(v2);
	}

	public float DistanceSquared(Vector2f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		return num * num + num2 * num2;
	}

	public float Distance(Vector2f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		return (float)Math.Sqrt(num * num + num2 * num2);
	}

	public void Set(Vector2f o)
	{
		x = o.x;
		y = o.y;
	}

	public void Set(float fX, float fY)
	{
		x = fX;
		y = fY;
	}

	public void Add(Vector2f o)
	{
		x += o.x;
		y += o.y;
	}

	public void Subtract(Vector2f o)
	{
		x -= o.x;
		y -= o.y;
	}

	public static Vector2f operator -(Vector2f v)
	{
		return new Vector2f(0f - v.x, 0f - v.y);
	}

	public static Vector2f operator +(Vector2f a, Vector2f o)
	{
		return new Vector2f(a.x + o.x, a.y + o.y);
	}

	public static Vector2f operator +(Vector2f a, float f)
	{
		return new Vector2f(a.x + f, a.y + f);
	}

	public static Vector2f operator -(Vector2f a, Vector2f o)
	{
		return new Vector2f(a.x - o.x, a.y - o.y);
	}

	public static Vector2f operator -(Vector2f a, float f)
	{
		return new Vector2f(a.x - f, a.y - f);
	}

	public static Vector2f operator *(Vector2f a, float f)
	{
		return new Vector2f(a.x * f, a.y * f);
	}

	public static Vector2f operator *(float f, Vector2f a)
	{
		return new Vector2f(a.x * f, a.y * f);
	}

	public static Vector2f operator /(Vector2f v, float f)
	{
		return new Vector2f(v.x / f, v.y / f);
	}

	public static Vector2f operator /(float f, Vector2f v)
	{
		return new Vector2f(f / v.x, f / v.y);
	}

	public static Vector2f operator *(Vector2f a, Vector2f b)
	{
		return new Vector2f(a.x * b.x, a.y * b.y);
	}

	public static Vector2f operator /(Vector2f a, Vector2f b)
	{
		return new Vector2f(a.x / b.x, a.y / b.y);
	}

	public static bool operator ==(Vector2f a, Vector2f b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Vector2f a, Vector2f b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2f)obj;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode();
	}

	public int CompareTo(Vector2f other)
	{
		if (x != other.x)
		{
			if (!(x < other.x))
			{
				return 1;
			}
			return -1;
		}
		if (y != other.y)
		{
			if (!(y < other.y))
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Vector2f other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public bool EpsilonEqual(Vector2f v2, float epsilon)
	{
		if (Math.Abs(x - v2.x) <= epsilon)
		{
			return Math.Abs(y - v2.y) <= epsilon;
		}
		return false;
	}

	public static Vector2f Lerp(Vector2f a, Vector2f b, float t)
	{
		float num = 1f - t;
		return new Vector2f(num * a.x + t * b.x, num * a.y + t * b.y);
	}

	public static Vector2f Lerp(ref Vector2f a, ref Vector2f b, float t)
	{
		float num = 1f - t;
		return new Vector2f(num * a.x + t * b.x, num * a.y + t * b.y);
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8}";
	}

	public static implicit operator Vector2f(Vector2 v)
	{
		return new Vector2f(v.x, v.y);
	}

	public static implicit operator Vector2(Vector2f v)
	{
		return new Vector2(v.x, v.y);
	}

	public static implicit operator Vector2f(float2 v)
	{
		return new Vector2f(v.x, v.y);
	}

	public static implicit operator float2(Vector2f v)
	{
		return new float2(v.x, v.y);
	}

	public static explicit operator Vector2f(double2 v)
	{
		return new Vector2f((float)v.x, (float)v.y);
	}

	public static implicit operator double2(Vector2f v)
	{
		return new double2(v.x, v.y);
	}
}
