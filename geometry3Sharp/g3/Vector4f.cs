using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector4f : IComparable<Vector4f>, IEquatable<Vector4f>
{
	public float x;

	public float y;

	public float z;

	public float w;

	public static readonly Vector4f Zero = new Vector4f(0f, 0f, 0f, 0f);

	public static readonly Vector4f One = new Vector4f(1f, 1f, 1f, 1f);

	public float this[int key]
	{
		get
		{
			if (key >= 2)
			{
				if (key != 2)
				{
					return w;
				}
				return z;
			}
			if (key != 0)
			{
				return y;
			}
			return x;
		}
		set
		{
			if (key < 2)
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
			else if (key == 2)
			{
				z = value;
			}
			else
			{
				w = value;
			}
		}
	}

	public float LengthSquared => x * x + y * y + z * z + w * w;

	public float Length => (float)Math.Sqrt(LengthSquared);

	public float LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z) + Math.Abs(w);

	public Vector4f Normalized
	{
		get
		{
			float length = Length;
			if ((double)length > 2.220446049250313E-16)
			{
				float num = 1f / length;
				return new Vector4f(x * num, y * num, z * num, w * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => (double)Math.Abs(x * x + y * y + z * z + w * w - 1f) < 1E-08;

	public bool IsFinite
	{
		get
		{
			float f = x + y + z + w;
			if (!float.IsNaN(f))
			{
				return !float.IsInfinity(f);
			}
			return false;
		}
	}

	public Vector4f(float f)
	{
		x = (y = (z = (w = f)));
	}

	public Vector4f(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public Vector4f(float[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
		w = v2[3];
	}

	public Vector4f(Vector4f copy)
	{
		x = copy.x;
		y = copy.y;
		z = copy.z;
		w = copy.w;
	}

	public float Normalize(float epsilon = 1.1920929E-07f)
	{
		float num = Length;
		if (num > epsilon)
		{
			float num2 = 1f / num;
			x *= num2;
			y *= num2;
			z *= num2;
			w *= num2;
		}
		else
		{
			num = 0f;
			x = (y = (z = (w = 0f)));
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = (float)Math.Round(x, nDecimals);
		y = (float)Math.Round(y, nDecimals);
		z = (float)Math.Round(z, nDecimals);
		w = (float)Math.Round(w, nDecimals);
	}

	public float Dot(Vector4f v2)
	{
		return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
	}

	public float Dot(ref Vector4f v2)
	{
		return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
	}

	public static float Dot(Vector4f v1, Vector4f v2)
	{
		return v1.Dot(v2);
	}

	public float AngleD(Vector4f v2)
	{
		return (float)Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f)) * 57.29578f;
	}

	public static float AngleD(Vector4f v1, Vector4f v2)
	{
		return v1.AngleD(v2);
	}

	public float AngleR(Vector4f v2)
	{
		return (float)Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f));
	}

	public static float AngleR(Vector4f v1, Vector4f v2)
	{
		return v1.AngleR(v2);
	}

	public float DistanceSquared(Vector4f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		float num4 = v2.w - w;
		return num * num + num2 * num2 + num3 * num3 + num4 * num4;
	}

	public float DistanceSquared(ref Vector4f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		float num4 = v2.w - w;
		return num * num + num2 * num2 + num3 * num3 + num4 * num4;
	}

	public float Distance(Vector4f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		float num4 = v2.w - w;
		return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
	}

	public float Distance(ref Vector4f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		float num4 = v2.w - w;
		return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
	}

	public static Vector4f operator -(Vector4f v)
	{
		return new Vector4f(0f - v.x, 0f - v.y, 0f - v.z, 0f - v.w);
	}

	public static Vector4f operator *(float f, Vector4f v)
	{
		return new Vector4f(f * v.x, f * v.y, f * v.z, f * v.w);
	}

	public static Vector4f operator *(Vector4f v, float f)
	{
		return new Vector4f(f * v.x, f * v.y, f * v.z, f * v.w);
	}

	public static Vector4f operator /(Vector4f v, float f)
	{
		return new Vector4f(v.x / f, v.y / f, v.z / f, v.w / f);
	}

	public static Vector4f operator /(float f, Vector4f v)
	{
		return new Vector4f(f / v.x, f / v.y, f / v.z, f / v.w);
	}

	public static Vector4f operator *(Vector4f a, Vector4f b)
	{
		return new Vector4f(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}

	public static Vector4f operator /(Vector4f a, Vector4f b)
	{
		return new Vector4f(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	}

	public static Vector4f operator +(Vector4f v0, Vector4f v1)
	{
		return new Vector4f(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
	}

	public static Vector4f operator +(Vector4f v0, float f)
	{
		return new Vector4f(v0.x + f, v0.y + f, v0.z + f, v0.w + f);
	}

	public static Vector4f operator -(Vector4f v0, Vector4f v1)
	{
		return new Vector4f(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
	}

	public static Vector4f operator -(Vector4f v0, float f)
	{
		return new Vector4f(v0.x - f, v0.y - f, v0.z - f, v0.w - f);
	}

	public static bool operator ==(Vector4f a, Vector4f b)
	{
		if (a.x == b.x && a.y == b.y && a.z == b.z)
		{
			return a.w == b.w;
		}
		return false;
	}

	public static bool operator !=(Vector4f a, Vector4f b)
	{
		if (a.x == b.x && a.y == b.y && a.z == b.z)
		{
			return a.w != b.w;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector4f)obj;
	}

	public override int GetHashCode()
	{
		return ((((((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode()) * 16777619) ^ z.GetHashCode()) * 16777619) ^ w.GetHashCode();
	}

	public int CompareTo(Vector4f other)
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
		if (z != other.z)
		{
			if (!(z < other.z))
			{
				return 1;
			}
			return -1;
		}
		if (w != other.w)
		{
			if (!(w < other.w))
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public bool Equals(Vector4f other)
	{
		if (x == other.x && y == other.y && z == other.z)
		{
			return w == other.w;
		}
		return false;
	}

	public bool EpsilonEqual(Vector4f v2, float epsilon)
	{
		if (Math.Abs(x - v2.x) <= epsilon && Math.Abs(y - v2.y) <= epsilon && Math.Abs(z - v2.z) <= epsilon)
		{
			return Math.Abs(w - v2.w) <= epsilon;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8} {z:F8} {w:F8}";
	}

	public string ToString(string fmt)
	{
		return $"{x.ToString(fmt)} {y.ToString(fmt)} {z.ToString(fmt)} {w.ToString(fmt)}";
	}

	public static implicit operator Vector4f(Vector4 v)
	{
		return new Vector4f(v.x, v.y, v.z, v.w);
	}

	public static implicit operator Vector4(Vector4f v)
	{
		return new Vector4(v.x, v.y, v.z, v.w);
	}

	public static implicit operator Color(Vector4f v)
	{
		return new Color(v.x, v.y, v.z, v.w);
	}

	public static implicit operator Vector4f(Color c)
	{
		return new Vector4f(c.r, c.g, c.b, c.a);
	}

	public static implicit operator Vector4f(float4 v)
	{
		return new Vector4f(v.x, v.y, v.z, v.w);
	}

	public static implicit operator float4(Vector4f v)
	{
		return new Vector4(v.x, v.y, v.z, v.w);
	}

	public static implicit operator double4(Vector4f v)
	{
		return new double4(v.x, v.y, v.z, v.w);
	}

	public static explicit operator Vector4f(double4 v)
	{
		return new Vector4f((float)v.x, (float)v.y, (float)v.z, (float)v.w);
	}
}
