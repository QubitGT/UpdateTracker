using System;

namespace Unity.Cinemachine;

internal struct Point64
{
	public long X;

	public long Y;

	public Point64(Point64 pt)
	{
		X = pt.X;
		Y = pt.Y;
	}

	public Point64(long x, long y)
	{
		X = x;
		Y = y;
	}

	public Point64(double x, double y)
	{
		X = (long)Math.Round(x);
		Y = (long)Math.Round(y);
	}

	public Point64(PointD pt)
	{
		X = (long)Math.Round(pt.x);
		Y = (long)Math.Round(pt.y);
	}

	public Point64(Point64 pt, double scale)
	{
		X = (long)Math.Round((double)pt.X * scale);
		Y = (long)Math.Round((double)pt.Y * scale);
	}

	public Point64(PointD pt, double scale)
	{
		X = (long)Math.Round(pt.x * scale);
		Y = (long)Math.Round(pt.y * scale);
	}

	public static bool operator ==(Point64 lhs, Point64 rhs)
	{
		if (lhs.X == rhs.X)
		{
			return lhs.Y == rhs.Y;
		}
		return false;
	}

	public static bool operator !=(Point64 lhs, Point64 rhs)
	{
		if (lhs.X == rhs.X)
		{
			return lhs.Y != rhs.Y;
		}
		return true;
	}

	public static Point64 operator +(Point64 lhs, Point64 rhs)
	{
		return new Point64(lhs.X + rhs.X, lhs.Y + rhs.Y);
	}

	public static Point64 operator -(Point64 lhs, Point64 rhs)
	{
		return new Point64(lhs.X - rhs.X, lhs.Y - rhs.Y);
	}

	public override string ToString()
	{
		return $"{X},{Y} ";
	}

	public override bool Equals(object obj)
	{
		if (obj is Point64 point)
		{
			return this == point;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0;
	}
}
