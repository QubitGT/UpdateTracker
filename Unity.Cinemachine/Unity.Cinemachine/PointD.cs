using System;

namespace Unity.Cinemachine;

internal struct PointD
{
	public double x;

	public double y;

	public PointD(PointD pt)
	{
		x = pt.x;
		y = pt.y;
	}

	public PointD(Point64 pt)
	{
		x = pt.X;
		y = pt.Y;
	}

	public PointD(PointD pt, double scale)
	{
		x = pt.x * scale;
		y = pt.y * scale;
	}

	public PointD(Point64 pt, double scale)
	{
		x = (double)pt.X * scale;
		y = (double)pt.Y * scale;
	}

	public PointD(long x, long y)
	{
		this.x = x;
		this.y = y;
	}

	public PointD(double x, double y)
	{
		this.x = x;
		this.y = y;
	}

	public override string ToString()
	{
		return $"{x:F},{y:F} ";
	}

	private static bool IsAlmostZero(double value)
	{
		return Math.Abs(value) <= 1E-15;
	}

	public static bool operator ==(PointD lhs, PointD rhs)
	{
		if (IsAlmostZero(lhs.x - rhs.x))
		{
			return IsAlmostZero(lhs.y - rhs.y);
		}
		return false;
	}

	public static bool operator !=(PointD lhs, PointD rhs)
	{
		if (IsAlmostZero(lhs.x - rhs.x))
		{
			return !IsAlmostZero(lhs.y - rhs.y);
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is PointD pointD)
		{
			return this == pointD;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0;
	}
}
