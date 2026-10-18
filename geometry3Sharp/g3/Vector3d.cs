using System;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector3d : IComparable<Vector3d>, IEquatable<Vector3d>
{
	public double x;

	public double y;

	public double z;

	public static readonly Vector3d Zero = new Vector3d(0.0, 0.0, 0.0);

	public static readonly Vector3d One = new Vector3d(1.0, 1.0, 1.0);

	public static readonly Vector3d AxisX = new Vector3d(1.0, 0.0, 0.0);

	public static readonly Vector3d AxisY = new Vector3d(0.0, 1.0, 0.0);

	public static readonly Vector3d AxisZ = new Vector3d(0.0, 0.0, 1.0);

	public static readonly Vector3d MaxValue = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);

	public static readonly Vector3d MinValue = new Vector3d(double.MinValue, double.MinValue, double.MinValue);

	public double this[int key]
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

	public Vector2d xy
	{
		get
		{
			return new Vector2d(x, y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	public Vector2d xz
	{
		get
		{
			return new Vector2d(x, z);
		}
		set
		{
			x = value.x;
			z = value.y;
		}
	}

	public Vector2d yz
	{
		get
		{
			return new Vector2d(y, z);
		}
		set
		{
			y = value.x;
			z = value.y;
		}
	}

	public double LengthSquared => x * x + y * y + z * z;

	public double Length => Math.Sqrt(LengthSquared);

	public double LengthL1 => Math.Abs(x) + Math.Abs(y) + Math.Abs(z);

	public double Max => Math.Max(x, Math.Max(y, z));

	public double Min => Math.Min(x, Math.Min(y, z));

	public double MaxAbs => Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));

	public double MinAbs => Math.Min(Math.Abs(x), Math.Min(Math.Abs(y), Math.Abs(z)));

	public Vector3d Abs => new Vector3d(Math.Abs(x), Math.Abs(y), Math.Abs(z));

	public Vector3d Normalized
	{
		get
		{
			double length = Length;
			if (length > 2.220446049250313E-16)
			{
				double num = 1.0 / length;
				return new Vector3d(x * num, y * num, z * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => Math.Abs(x * x + y * y + z * z - 1.0) < 1E-08;

	public bool IsFinite
	{
		get
		{
			double d = x + y + z;
			if (!double.IsNaN(d))
			{
				return !double.IsInfinity(d);
			}
			return false;
		}
	}

	public Vector3d(double f)
	{
		x = (y = (z = f));
	}

	public Vector3d(double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3d(double[] v2)
	{
		x = v2[0];
		y = v2[1];
		z = v2[2];
	}

	public Vector3d(Vector3d copy)
	{
		x = copy.x;
		y = copy.y;
		z = copy.z;
	}

	public Vector3d(Vector3f copy)
	{
		x = copy.x;
		y = copy.y;
		z = copy.z;
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
		}
		else
		{
			num = 0.0;
			x = (y = (z = 0.0));
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = Math.Round(x, nDecimals);
		y = Math.Round(y, nDecimals);
		z = Math.Round(z, nDecimals);
	}

	public double Dot(Vector3d v2)
	{
		return x * v2.x + y * v2.y + z * v2.z;
	}

	public double Dot(ref Vector3d v2)
	{
		return x * v2.x + y * v2.y + z * v2.z;
	}

	public static double Dot(Vector3d v1, Vector3d v2)
	{
		return v1.Dot(ref v2);
	}

	public Vector3d Cross(Vector3d v2)
	{
		return new Vector3d(y * v2.z - z * v2.y, z * v2.x - x * v2.z, x * v2.y - y * v2.x);
	}

	public Vector3d Cross(ref Vector3d v2)
	{
		return new Vector3d(y * v2.z - z * v2.y, z * v2.x - x * v2.z, x * v2.y - y * v2.x);
	}

	public static Vector3d Cross(Vector3d v1, Vector3d v2)
	{
		return v1.Cross(ref v2);
	}

	public Vector3d UnitCross(ref Vector3d v2)
	{
		Vector3d result = new Vector3d(y * v2.z - z * v2.y, z * v2.x - x * v2.z, x * v2.y - y * v2.x);
		result.Normalize();
		return result;
	}

	public Vector3d UnitCross(Vector3d v2)
	{
		return UnitCross(ref v2);
	}

	public double AngleD(Vector3d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0)) * (180.0 / Math.PI);
	}

	public static double AngleD(Vector3d v1, Vector3d v2)
	{
		return v1.AngleD(v2);
	}

	public double AngleR(Vector3d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0));
	}

	public static double AngleR(Vector3d v1, Vector3d v2)
	{
		return v1.AngleR(v2);
	}

	public double DistanceSquared(Vector3d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public double DistanceSquared(ref Vector3d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public double Distance(Vector3d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public double Distance(ref Vector3d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		double num3 = v2.z - z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public void Set(Vector3d o)
	{
		x = o.x;
		y = o.y;
		z = o.z;
	}

	public void Set(double fX, double fY, double fZ)
	{
		x = fX;
		y = fY;
		z = fZ;
	}

	public void Add(Vector3d o)
	{
		x += o.x;
		y += o.y;
		z += o.z;
	}

	public void Subtract(Vector3d o)
	{
		x -= o.x;
		y -= o.y;
		z -= o.z;
	}

	public static Vector3d operator -(Vector3d v)
	{
		return new Vector3d(0.0 - v.x, 0.0 - v.y, 0.0 - v.z);
	}

	public static Vector3d operator *(double f, Vector3d v)
	{
		return new Vector3d(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3d operator *(Vector3d v, double f)
	{
		return new Vector3d(f * v.x, f * v.y, f * v.z);
	}

	public static Vector3d operator /(Vector3d v, double f)
	{
		return new Vector3d(v.x / f, v.y / f, v.z / f);
	}

	public static Vector3d operator /(double f, Vector3d v)
	{
		return new Vector3d(f / v.x, f / v.y, f / v.z);
	}

	public static Vector3d operator *(Vector3d a, Vector3d b)
	{
		return new Vector3d(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static Vector3d operator /(Vector3d a, Vector3d b)
	{
		return new Vector3d(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	public static Vector3d operator +(Vector3d v0, Vector3d v1)
	{
		return new Vector3d(v0.x + v1.x, v0.y + v1.y, v0.z + v1.z);
	}

	public static Vector3d operator +(Vector3d v0, double f)
	{
		return new Vector3d(v0.x + f, v0.y + f, v0.z + f);
	}

	public static Vector3d operator -(Vector3d v0, Vector3d v1)
	{
		return new Vector3d(v0.x - v1.x, v0.y - v1.y, v0.z - v1.z);
	}

	public static Vector3d operator -(Vector3d v0, double f)
	{
		return new Vector3d(v0.x - f, v0.y - f, v0.z - f);
	}

	public static bool operator ==(Vector3d a, Vector3d b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(Vector3d a, Vector3d b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z != b.z;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector3d)obj;
	}

	public override int GetHashCode()
	{
		return ((((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode()) * 16777619) ^ z.GetHashCode();
	}

	public int CompareTo(Vector3d other)
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

	public bool Equals(Vector3d other)
	{
		if (x == other.x && y == other.y)
		{
			return z == other.z;
		}
		return false;
	}

	public bool EpsilonEqual(Vector3d v2, double epsilon)
	{
		if (Math.Abs(x - v2.x) <= epsilon && Math.Abs(y - v2.y) <= epsilon)
		{
			return Math.Abs(z - v2.z) <= epsilon;
		}
		return false;
	}

	public static Vector3d Lerp(Vector3d a, Vector3d b, double t)
	{
		double num = 1.0 - t;
		return new Vector3d(num * a.x + t * b.x, num * a.y + t * b.y, num * a.z + t * b.z);
	}

	public static Vector3d Lerp(ref Vector3d a, ref Vector3d b, double t)
	{
		double num = 1.0 - t;
		return new Vector3d(num * a.x + t * b.x, num * a.y + t * b.y, num * a.z + t * b.z);
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8} {z:F8}";
	}

	public string ToString(string fmt)
	{
		return $"{x.ToString(fmt)} {y.ToString(fmt)} {z.ToString(fmt)}";
	}

	public static implicit operator Vector3d(Vector3f v)
	{
		return new Vector3d(v.x, v.y, v.z);
	}

	public static explicit operator Vector3f(Vector3d v)
	{
		return new Vector3f((float)v.x, (float)v.y, (float)v.z);
	}

	public static implicit operator Vector3d(Vector3 v)
	{
		return new Vector3d(v.x, v.y, v.z);
	}

	public static explicit operator Vector3(Vector3d v)
	{
		return new Vector3((float)v.x, (float)v.y, (float)v.z);
	}

	public static implicit operator Vector3d(float3 v)
	{
		return new Vector3d(v.x, v.y, v.z);
	}

	public static explicit operator float3(Vector3d v)
	{
		return new float3((float)v.x, (float)v.y, (float)v.z);
	}

	public static implicit operator Vector3d(double3 v)
	{
		return new Vector3d(v.x, v.y, v.z);
	}

	public static implicit operator double3(Vector3d v)
	{
		return new double3(v.x, v.y, v.z);
	}

	public static double Orthonormalize(ref Vector3d u, ref Vector3d v, ref Vector3d w)
	{
		double num = u.Normalize();
		double num2 = u.Dot(v);
		v -= num2 * u;
		double num3 = v.Normalize();
		if (num3 < num)
		{
			num = num3;
		}
		double num4 = v.Dot(w);
		num2 = u.Dot(w);
		w -= num2 * u + num4 * v;
		num3 = w.Normalize();
		if (num3 < num)
		{
			num = num3;
		}
		return num;
	}

	public static void GenerateComplementBasis(ref Vector3d u, ref Vector3d v, Vector3d w)
	{
		if (Math.Abs(w.x) >= Math.Abs(w.y))
		{
			double num = MathUtil.InvSqrt(w.x * w.x + w.z * w.z);
			u.x = (0.0 - w.z) * num;
			u.y = 0.0;
			u.z = w.x * num;
			v.x = w.y * u.z;
			v.y = w.z * u.x - w.x * u.z;
			v.z = (0.0 - w.y) * u.x;
		}
		else
		{
			double num = MathUtil.InvSqrt(w.y * w.y + w.z * w.z);
			u.x = 0.0;
			u.y = w.z * num;
			u.z = (0.0 - w.y) * num;
			v.x = w.y * u.z - w.z * u.y;
			v.y = (0.0 - w.x) * u.z;
			v.z = w.x * u.y;
		}
	}

	public static double ComputeOrthogonalComplement(int numInputs, Vector3d v0, ref Vector3d v1, ref Vector3d v2)
	{
		if (numInputs == 1)
		{
			if (Math.Abs(v0[0]) > Math.Abs(v0[1]))
			{
				v1 = new Vector3d(0.0 - v0[2], 0.0, v0[0]);
			}
			else
			{
				v1 = new Vector3d(0.0, v0[2], 0.0 - v0[1]);
			}
			numInputs = 2;
		}
		if (numInputs == 2)
		{
			v2 = Cross(v0, v1);
			return Orthonormalize(ref v0, ref v1, ref v2);
		}
		return 0.0;
	}

	public static void MakePerpVectors(ref Vector3d n, out Vector3d b1, out Vector3d b2)
	{
		if (n.z < 0.0)
		{
			double num = 1.0 / (1.0 - n.z);
			double num2 = n.x * n.y * num;
			b1.x = 1.0 - n.x * n.x * num;
			b1.y = 0.0 - num2;
			b1.z = n.x;
			b2.x = num2;
			b2.y = n.y * n.y * num - 1.0;
			b2.z = 0.0 - n.y;
		}
		else
		{
			double num3 = 1.0 / (1.0 + n.z);
			double num4 = (0.0 - n.x) * n.y * num3;
			b1.x = 1.0 - n.x * n.x * num3;
			b1.y = num4;
			b1.z = 0.0 - n.x;
			b2.x = num4;
			b2.y = 1.0 - n.y * n.y * num3;
			b2.z = 0.0 - n.y;
		}
	}
}
