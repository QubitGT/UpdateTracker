using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector3f : IComparable<Vector3f>, IEquatable<Vector3f>
{
	public float x;

	public float y;

	public float z;

	public static readonly Vector3f Zero = new Vector3f(0f, 0f, 0f);

	public static readonly Vector3f One = new Vector3f(1f, 1f, 1f);

	public static readonly Vector3f OneNormalized = new Vector3f(1f, 1f, 1f).Normalized;

	public static readonly Vector3f Invalid = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);

	public static readonly Vector3f AxisX = new Vector3f(1f, 0f, 0f);

	public static readonly Vector3f AxisY = new Vector3f(0f, 1f, 0f);

	public static readonly Vector3f AxisZ = new Vector3f(0f, 0f, 1f);

	public static readonly Vector3f MaxValue = new Vector3f(float.MaxValue, float.MaxValue, float.MaxValue);

	public static readonly Vector3f MinValue = new Vector3f(float.MinValue, float.MinValue, float.MinValue);

	public float this[int key]
	{
		get
		{
			return key switch
			{
				1 => y, 
				0 => x, 
				_ => z, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			default:
				z = value;
				break;
			}
		}
	}

	public Vector2f xy
	{
		get
		{
			return new Vector2f(x, y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	public Vector2f xz
	{
		get
		{
			return new Vector2f(x, z);
		}
		set
		{
			x = value.x;
			z = value.y;
		}
	}

	public Vector2f yz
	{
		get
		{
			return new Vector2f(y, z);
		}
		set
		{
			y = value.x;
			z = value.y;
		}
	}

	public float LengthSquared => x * x + y * y + z * z;

	public float Length => (float)Math.Sqrt(LengthSquared);

	public float LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z);

	public float Max => Math.Max(x, Math.Max(y, z));

	public float Min => Math.Min(x, Math.Min(y, z));

	public float MaxAbs => Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));

	public float MinAbs => Math.Min(Math.Abs(x), Math.Min(Math.Abs(y), Math.Abs(z)));

	public Vector3f Normalized
	{
		get
		{
			float length = Length;
			if (length > 1.1920929E-07f)
			{
				float num = 1f / length;
				return new Vector3f(x * num, y * num, z * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => Math.Abs(x * x + y * y + z * z - 1f) < 1E-06f;

	public bool IsFinite
	{
		get
		{
			float f = x + y + z;
			if (!float.IsNaN(f))
			{
				return !float.IsInfinity(f);
			}
			return false;
		}
	}

	public Vector3f(float f)
	{
		x = (y = (z = f));
	}

	public Vector3f(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3f(float[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
	}

	public Vector3f(Vector3f copy)
	{
		x = copy.x;
		y = copy.y;
		z = copy.z;
	}

	public Vector3f(double f)
	{
		x = (y = (z = (float)f));
	}

	public Vector3f(double x, double y, double z)
	{
		this.x = (float)x;
		this.y = (float)y;
		this.z = (float)z;
	}

	public Vector3f(double[] v2)
	{
		x = (float)v2[0];
		y = (float)v2[1];
		z = (float)v2[2];
	}

	public Vector3f(Vector3d copy)
	{
		x = (float)copy.x;
		y = (float)copy.y;
		z = (float)copy.z;
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
		}
		else
		{
			num = 0f;
			x = (y = (z = 0f));
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = (float)Math.Round(x, nDecimals);
		y = (float)Math.Round(y, nDecimals);
		z = (float)Math.Round(z, nDecimals);
	}

	public float Dot(Vector3f v2)
	{
		return x * v2[0] + y * v2[1] + z * v2[2];
	}

	public static float Dot(Vector3f v1, Vector3f v2)
	{
		return v1.Dot(v2);
	}

	public Vector3f Cross(Vector3f v2)
	{
		return new Vector3f(y * v2.z - z * v2.y, z * v2.x - x * v2.z, x * v2.y - y * v2.x);
	}

	public static Vector3f Cross(Vector3f v1, Vector3f v2)
	{
		return v1.Cross(v2);
	}

	public Vector3f UnitCross(Vector3f v2)
	{
		Vector3f result = new Vector3f(y * v2.z - z * v2.y, z * v2.x - x * v2.z, x * v2.y - y * v2.x);
		result.Normalize();
		return result;
	}

	public float AngleD(Vector3f v2)
	{
		return (float)(Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f)) * (180.0 / Math.PI));
	}

	public static float AngleD(Vector3f v1, Vector3f v2)
	{
		return v1.AngleD(v2);
	}

	public float AngleR(Vector3f v2)
	{
		return (float)Math.Acos(MathUtil.Clamp(Dot(v2), -1f, 1f));
	}

	public static float AngleR(Vector3f v1, Vector3f v2)
	{
		return v1.AngleR(v2);
	}

	public float DistanceSquared(Vector3f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public float Distance(Vector3f v2)
	{
		float num = v2.x - x;
		float num2 = v2.y - y;
		float num3 = v2.z - z;
		return (float)Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public void Set(Vector3f o)
	{
		x = o[0];
		y = o[1];
		z = o[2];
	}

	public void Set(float fX, float fY, float fZ)
	{
		x = fX;
		y = fY;
		z = fZ;
	}

	public void Add(Vector3f o)
	{
		x += o[0];
		y += o[1];
		z += o[2];
	}

	public void Subtract(Vector3f o)
	{
		x -= o[0];
		y -= o[1];
		z -= o[2];
	}

	public static Vector3f operator -(Vector3f v)
	{
		return new Vector3f(0f - v.x, 0f - v.y, 0f - v.z);
	}

	public static Vector3f operator *(float f, Vector3f v)
	{
		return new Vector3f(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3f operator *(Vector3f v, float f)
	{
		return new Vector3f(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3f operator /(Vector3f v, float f)
	{
		return new Vector3f(v.x / f, v.y / f, v.z / f);
	}

	public static Vector3f operator /(float f, Vector3f v)
	{
		return new Vector3f(f / v.x, f / v.y, f / v.z);
	}

	public static Vector3f operator *(Vector3f a, Vector3f b)
	{
		return new Vector3f(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector3f operator /(Vector3f a, Vector3f b)
	{
		return new Vector3f(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	public static Vector3f operator +(Vector3f v0, Vector3f v1)
	{
		return new Vector3f(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
	}

	public static Vector3f operator +(Vector3f v0, float f)
	{
		return new Vector3f(v0.x + f, v0.y + f, v0.z + f);
	}

	public static Vector3f operator -(Vector3f v0, Vector3f v1)
	{
		return new Vector3f(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
	}

	public static Vector3f operator -(Vector3f v0, float f)
	{
		return new Vector3f(v0.x - f, v0.y - f, v0.z - f);
	}

	public static bool operator ==(Vector3f a, Vector3f b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(Vector3f a, Vector3f b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z != b.z;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector3f)obj;
	}

	public override int GetHashCode()
	{
		return ((((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode()) * 16777619) ^ z.GetHashCode();
	}

	public int CompareTo(Vector3f other)
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
		return 0;
	}

	public bool Equals(Vector3f other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public bool EpsilonEqual(Vector3f v2, float epsilon)
	{
		if (Math.Abs(x - v2.x) <= epsilon && Math.Abs(y - v2.y) <= epsilon)
		{
			return Math.Abs(z - v2.z) <= epsilon;
		}
		return false;
	}

	public static Vector3f Lerp(Vector3f a, Vector3f b, float t)
	{
		float num = 1f - t;
		return new Vector3f(num * a.x + t * b.x, num * a.y + t * b.y, num * a.z + t * b.z);
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8} {z:F8}";
	}

	public string ToString(string fmt)
	{
		return $"{x.ToString(fmt)} {y.ToString(fmt)} {z.ToString(fmt)}";
	}

	public static implicit operator Vector3f(Vector3 v)
	{
		return new Vector3f(v.x, v.y, v.z);
	}

	public static implicit operator Vector3(Vector3f v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public static explicit operator Vector3f(double3 v)
	{
		return new Vector3f((float)v.x, (float)v.y, (float)v.z);
	}

	public static implicit operator double3(Vector3f v)
	{
		return new double3(v.x, v.y, v.z);
	}

	public static implicit operator Vector3f(float3 v)
	{
		return new Vector3f(v.x, v.y, v.z);
	}

	public static implicit operator float3(Vector3f v)
	{
		return new float3(v.x, v.y, v.z);
	}

	public static implicit operator Color(Vector3f v)
	{
		return new Color(v.x, v.y, v.z, 1f);
	}

	public static implicit operator Vector3f(Color c)
	{
		return new Vector3f(c.r, c.g, c.b);
	}
}
