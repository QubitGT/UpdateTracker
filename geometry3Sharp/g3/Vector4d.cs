using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector4d : IComparable<Vector4d>, IEquatable<Vector4d>
{
	public double x;

	public double y;

	public double z;

	public double w;

	public static readonly Vector4d Zero = new Vector4d(0.0, 0.0, 0.0, 0.0);

	public static readonly Vector4d One = new Vector4d(1.0, 1.0, 1.0, 1.0);

	public double this[int key]
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

	public double LengthSquared => x * x + y * y + z * z + w * w;

	public double Length => Math.Sqrt(LengthSquared);

	public double LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z) + Math.Abs(w);

	public Vector4d Normalized
	{
		get
		{
			double length = Length;
			if (length > 2.220446049250313E-16)
			{
				double num = 1.0 / length;
				return new Vector4d(x * num, y * num, z * num, w * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => Math.Abs(x * x + y * y + z * z + w * w - 1.0) < 1E-08;

	public bool IsFinite
	{
		get
		{
			double d = x + y + z + w;
			if (!double.IsNaN(d))
			{
				return !double.IsInfinity(d);
			}
			return false;
		}
	}

	public Vector4d(double f)
	{
		x = (y = (z = (w = f)));
	}

	public Vector4d(double x, double y, double z, double w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public Vector4d(double[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
		w = v2[3];
	}

	public Vector4d(Vector4d copy)
	{
		x = copy.x;
		y = copy.y;
		z = copy.z;
		w = copy.w;
	}

	public double Normalize(double epsilon = 2.220446049250313E-16)
	{
		double num = Length;
		if (num > epsilon)
		{
			double num2 = 1.0 / num;
			x *= num2;
			y *= num2;
			z *= num2;
			w *= num2;
		}
		else
		{
			num = 0.0;
			x = (y = (z = (w = 0.0)));
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = Math.Round(x, nDecimals);
		y = Math.Round(y, nDecimals);
		z = Math.Round(z, nDecimals);
		w = Math.Round(w, nDecimals);
	}

	public double Dot(Vector4d v2)
	{
		return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
	}

	public double Dot(ref Vector4d v2)
	{
		return x * v2.x + y * v2.y + z * v2.z + w * v2.w;
	}

	public static double Dot(Vector4d v1, Vector4d v2)
	{
		return v1.Dot(v2);
	}

	public double AngleD(Vector4d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0)) * (180.0 / Math.PI);
	}

	public static double AngleD(Vector4d v1, Vector4d v2)
	{
		return v1.AngleD(v2);
	}

	public double AngleR(Vector4d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0));
	}

	public static double AngleR(Vector4d v1, Vector4d v2)
	{
		return v1.AngleR(v2);
	}

	public double DistanceSquared(Vector4d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		double num4 = v2.w - w;
		return num * num + num2 * num2 + num3 * num3 + num4 * num4;
	}

	public double DistanceSquared(ref Vector4d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		double num4 = v2.w - w;
		return num * num + num2 * num2 + num3 * num3 + num4 * num4;
	}

	public double Distance(Vector4d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		double num4 = v2.w - w;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
	}

	public double Distance(ref Vector4d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		double num4 = v2.w - w;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
	}

	public static Vector4d operator -(Vector4d v)
	{
		return new Vector4d(0.0 - v.x, 0.0 - v.y, 0.0 - v.z, 0.0 - v.w);
	}

	public static Vector4d operator *(double f, Vector4d v)
	{
		return new Vector4d(f * v.x, f * v.y, f * v.z, f * v.w);
	}

	public static Vector4d operator *(Vector4d v, double f)
	{
		return new Vector4d(f * v.x, f * v.y, f * v.z, f * v.w);
	}

	public static Vector4d operator /(Vector4d v, double f)
	{
		return new Vector4d(v.x / f, v.y / f, v.z / f, v.w / f);
	}

	public static Vector4d operator /(double f, Vector4d v)
	{
		return new Vector4d(f / v.x, f / v.y, f / v.z, f / v.w);
	}

	public static Vector4d operator *(Vector4d a, Vector4d b)
	{
		return new Vector4d(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}

	public static Vector4d operator /(Vector4d a, Vector4d b)
	{
		return new Vector4d(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	}

	public static Vector4d operator +(Vector4d v0, Vector4d v1)
	{
		return new Vector4d(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z, v0.w + v1.w);
	}

	public static Vector4d operator +(Vector4d v0, double f)
	{
		return new Vector4d(v0.x + f, v0.y + f, v0.z + f, v0.w + f);
	}

	public static Vector4d operator -(Vector4d v0, Vector4d v1)
	{
		return new Vector4d(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z, v0.w - v1.w);
	}

	public static Vector4d operator -(Vector4d v0, double f)
	{
		return new Vector4d(v0.x - f, v0.y - f, v0.z - f, v0.w - f);
	}

	public static bool operator ==(Vector4d a, Vector4d b)
	{
		if (a.x == b.x && a.y == b.y && a.z == b.z)
		{
			return a.w == b.w;
		}
		return false;
	}

	public static bool operator !=(Vector4d a, Vector4d b)
	{
		if (a.x == b.x && a.y == b.y && a.z == b.z)
		{
			return a.w != b.w;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector4d)obj;
	}

	public override int GetHashCode()
	{
		return ((((((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode()) * 16777619) ^ z.GetHashCode()) * 16777619) ^ w.GetHashCode();
	}

	public int CompareTo(Vector4d other)
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

	public bool Equals(Vector4d other)
	{
		if (x == other.x && y == other.y && z == other.z)
		{
			return w == other.w;
		}
		return false;
	}

	public bool EpsilonEqual(Vector4d v2, double epsilon)
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

	public static implicit operator Vector4d(Vector4f v)
	{
		return new Vector4d(v.x, v.y, v.z, v.w);
	}

	public static explicit operator Vector4f(Vector4d v)
	{
		return new Vector4f((float)v.x, (float)v.y, (float)v.z, (float)v.w);
	}

	public static implicit operator Vector4d(Vector4 v)
	{
		return new Vector4d(v.x, v.y, v.z, v.w);
	}

	public static explicit operator Vector4(Vector4d v)
	{
		return new Vector4((float)v.x, (float)v.y, (float)v.z, (float)v.w);
	}

	public static implicit operator Vector4d(float4 v)
	{
		return new Vector4d(v.x, v.y, v.z, v.w);
	}

	public static explicit operator float4(Vector4d v)
	{
		return new float4((float)v.x, (float)v.y, (float)v.z, (float)v.w);
	}

	public static implicit operator Vector4d(double4 v)
	{
		return new Vector4d(v.x, v.y, v.z, v.w);
	}

	public static implicit operator double4(Vector4d v)
	{
		return new double4((float)v.x, (float)v.y, (float)v.z, (float)v.w);
	}
}
