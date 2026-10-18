using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace g3;

public struct Vector2d : IComparable<Vector2d>, IEquatable<Vector2d>
{
	public struct Information
	{
		public int mDimension;

		public Vector2d mMin;

		public Vector2d mMax;

		public double mMaxRange;

		public Vector2d mOrigin;

		public Vector2d mDirection0;

		public Vector2d mDirection1;

		public Vector3i mExtreme;

		public bool mExtremeCCW;
	}

	public double x;

	public double y;

	public static readonly Vector2d Zero = new Vector2d(0f, 0f);

	public static readonly Vector2d One = new Vector2d(1f, 1f);

	public static readonly Vector2d AxisX = new Vector2d(1f, 0f);

	public static readonly Vector2d AxisY = new Vector2d(0f, 1f);

	public static readonly Vector2d MaxValue = new Vector2d(double.MaxValue, double.MaxValue);

	public static readonly Vector2d MinValue = new Vector2d(double.MinValue, double.MinValue);

	public double this[int key]
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

	public double LengthSquared => x * x + y * y;

	public double Length => Math.Sqrt(LengthSquared);

	public Vector2d Normalized
	{
		get
		{
			double length = Length;
			if (length > 2.220446049250313E-16)
			{
				double num = 1.0 / length;
				return new Vector2d(x * num, y * num);
			}
			return Zero;
		}
	}

	public bool IsNormalized => Math.Abs(x * x + y * y - 1.0) < 1E-08;

	public bool IsFinite
	{
		get
		{
			double d = x + y;
			if (!double.IsNaN(d))
			{
				return !double.IsInfinity(d);
			}
			return false;
		}
	}

	public Vector2d Perp => new Vector2d(y, 0.0 - x);

	public Vector2d UnitPerp => new Vector2d(y, 0.0 - x).Normalized;

	public Vector2d(double f)
	{
		x = (y = f);
	}

	public Vector2d(double x, double y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2d(double[] v2)
	{
		x = v2[0];
		y = v2[1];
	}

	public Vector2d(float f)
	{
		x = (y = f);
	}

	public Vector2d(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2d(float[] v2)
	{
		x = v2[0];
		y = v2[1];
	}

	public Vector2d(Vector2d copy)
	{
		x = copy.x;
		y = copy.y;
	}

	public Vector2d(Vector2f copy)
	{
		x = copy.x;
		y = copy.y;
	}

	public static Vector2d FromAngleRad(double angle)
	{
		return new Vector2d(Math.Cos(angle), Math.Sin(angle));
	}

	public static Vector2d FromAngleDeg(double angle)
	{
		angle *= Math.PI / 180.0;
		return new Vector2d(Math.Cos(angle), Math.Sin(angle));
	}

	public double Normalize(double epsilon = 2.220446049250313E-16)
	{
		double num = Length;
		if (num > epsilon)
		{
			double num2 = 1.0 / num;
			x *= num2;
			y *= num2;
		}
		else
		{
			num = 0.0;
			x = (y = 0.0);
		}
		return num;
	}

	public void Round(int nDecimals)
	{
		x = Math.Round(x, nDecimals);
		y = Math.Round(y, nDecimals);
	}

	public double Dot(Vector2d v2)
	{
		return x * v2.x + y * v2.y;
	}

	public double Cross(Vector2d v2)
	{
		return x * v2.y - y * v2.x;
	}

	public double DotPerp(Vector2d v2)
	{
		return x * v2.y - y * v2.x;
	}

	public double AngleD(Vector2d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0)) * (180.0 / Math.PI);
	}

	public static double AngleD(Vector2d v1, Vector2d v2)
	{
		return v1.AngleD(v2);
	}

	public double AngleR(Vector2d v2)
	{
		return Math.Acos(MathUtil.Clamp(Dot(v2), -1.0, 1.0));
	}

	public static double AngleR(Vector2d v1, Vector2d v2)
	{
		return v1.AngleR(v2);
	}

	public double DistanceSquared(Vector2d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		return num * num + num2 * num2;
	}

	public double Distance(Vector2d v2)
	{
		double num = v2.x - x;
		double num2 = v2.y - y;
		return Math.Sqrt(num * num + num2 * num2);
	}

	public void Set(Vector2d o)
	{
		x = o.x;
		y = o.y;
	}

	public void Set(double fX, double fY)
	{
		x = fX;
		y = fY;
	}

	public void Add(Vector2d o)
	{
		x += o.x;
		y += o.y;
	}

	public void Subtract(Vector2d o)
	{
		x -= o.x;
		y -= o.y;
	}

	public static Vector2d operator -(Vector2d v)
	{
		return new Vector2d(0.0 - v.x, 0.0 - v.y);
	}

	public static Vector2d operator +(Vector2d a, Vector2d o)
	{
		return new Vector2d(a.x + o.x, a.y + o.y);
	}

	public static Vector2d operator +(Vector2d a, double f)
	{
		return new Vector2d(a.x + f, a.y + f);
	}

	public static Vector2d operator -(Vector2d a, Vector2d o)
	{
		return new Vector2d(a.x - o.x, a.y - o.y);
	}

	public static Vector2d operator -(Vector2d a, double f)
	{
		return new Vector2d(a.x - f, a.y - f);
	}

	public static Vector2d operator *(Vector2d a, double f)
	{
		return new Vector2d(a.x * f, a.y * f);
	}

	public static Vector2d operator *(double f, Vector2d a)
	{
		return new Vector2d(a.x * f, a.y * f);
	}

	public static Vector2d operator /(Vector2d v, double f)
	{
		return new Vector2d(v.x / f, v.y / f);
	}

	public static Vector2d operator /(double f, Vector2d v)
	{
		return new Vector2d(f / v.x, f / v.y);
	}

	public static Vector2d operator *(Vector2d a, Vector2d b)
	{
		return new Vector2d(a.x * b.x, a.y * b.y);
	}

	public static Vector2d operator /(Vector2d a, Vector2d b)
	{
		return new Vector2d(a.x / b.x, a.y / b.y);
	}

	public static bool operator ==(Vector2d a, Vector2d b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Vector2d a, Vector2d b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return this == (Vector2d)obj;
	}

	public override int GetHashCode()
	{
		return ((0x50C5D1F ^ x.GetHashCode()) * 16777619) ^ y.GetHashCode();
	}

	public int CompareTo(Vector2d other)
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

	public bool Equals(Vector2d other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public bool EpsilonEqual(Vector2d v2, double epsilon)
	{
		if (Math.Abs(x - v2.x) <= epsilon)
		{
			return Math.Abs(y - v2.y) <= epsilon;
		}
		return false;
	}

	public static Vector2d Lerp(Vector2d a, Vector2d b, double t)
	{
		double num = 1.0 - t;
		return new Vector2d(num * a.x + t * b.x, num * a.y + t * b.y);
	}

	public static Vector2d Lerp(ref Vector2d a, ref Vector2d b, double t)
	{
		double num = 1.0 - t;
		return new Vector2d(num * a.x + t * b.x, num * a.y + t * b.y);
	}

	public override string ToString()
	{
		return $"{x:F8} {y:F8}";
	}

	public static implicit operator Vector2d(Vector2f v)
	{
		return new Vector2d(v.x, v.y);
	}

	public static explicit operator Vector2f(Vector2d v)
	{
		return new Vector2f((float)v.x, (float)v.y);
	}

	public static explicit operator float2(Vector2d v)
	{
		return new float2((float)v.x, (float)v.y);
	}

	public static implicit operator Vector2d(float2 v)
	{
		return new Vector2d(v.x, v.y);
	}

	public static implicit operator Vector2d(double2 v)
	{
		return new Vector2d(v.x, v.y);
	}

	public static implicit operator double2(Vector2d v)
	{
		return new double2(v.x, v.y);
	}

	public static implicit operator Vector2d(Vector2 v)
	{
		return new Vector2d(v.x, v.y);
	}

	public static explicit operator Vector2(Vector2d v)
	{
		return new Vector2((float)v.x, (float)v.y);
	}

	public static void GetInformation(IList<Vector2d> points, double epsilon, out Information info)
	{
		info = default(Information);
		int count = points.Count;
		if (count == 0 || points == null || epsilon <= 0.0)
		{
			return;
		}
		info.mExtremeCCW = false;
		Vector2i zero = Vector2i.Zero;
		Vector2i zero2 = Vector2i.Zero;
		for (int i = 0; i < 2; i++)
		{
			info.mMin[i] = points[0][i];
			info.mMax[i] = info.mMin[i];
			zero[i] = 0;
			zero2[i] = 0;
		}
		for (int j = 1; j < count; j++)
		{
			for (int i = 0; i < 2; i++)
			{
				if (points[j][i] < info.mMin[i])
				{
					info.mMin[i] = points[j][i];
					zero[i] = j;
				}
				else if (points[j][i] > info.mMax[i])
				{
					info.mMax[i] = points[j][i];
					zero2[i] = j;
				}
			}
		}
		info.mMaxRange = info.mMax[0] - info.mMin[0];
		info.mExtreme[0] = zero[0];
		info.mExtreme[1] = zero2[0];
		double num = info.mMax[1] - info.mMin[1];
		if (num > info.mMaxRange)
		{
			info.mMaxRange = num;
			info.mExtreme[0] = zero[1];
			info.mExtreme[1] = zero2[1];
		}
		info.mOrigin = points[info.mExtreme[0]];
		if (info.mMaxRange < epsilon)
		{
			info.mDimension = 0;
			info.mDirection0 = Zero;
			info.mDirection1 = Zero;
			for (int i = 0; i < 2; i++)
			{
				info.mExtreme[i + 1] = info.mExtreme[0];
			}
			return;
		}
		info.mDirection0 = points[info.mExtreme[1]] - info.mOrigin;
		info.mDirection0.Normalize();
		info.mDirection1 = -info.mDirection0.Perp;
		double num2 = 0.0;
		double num3 = 0.0;
		info.mExtreme[2] = info.mExtreme[0];
		for (int j = 0; j < count; j++)
		{
			Vector2d v = points[j] - info.mOrigin;
			double value = info.mDirection1.Dot(v);
			double num4 = Math.Sign(value);
			value = Math.Abs(value);
			if (value > num2)
			{
				num2 = value;
				num3 = num4;
				info.mExtreme[2] = j;
			}
		}
		if (num2 < epsilon * info.mMaxRange)
		{
			info.mDimension = 1;
			info.mExtreme[2] = info.mExtreme[1];
		}
		else
		{
			info.mDimension = 2;
			info.mExtremeCCW = num3 > 0.0;
		}
	}
}
