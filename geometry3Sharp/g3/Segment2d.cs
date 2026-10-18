using System;

namespace g3;

public struct Segment2d : IParametricCurve2d
{
	public Vector2d Center;

	public Vector2d Direction;

	public double Extent;

	public Vector2d P0
	{
		get
		{
			return Center - Extent * Direction;
		}
		set
		{
			update_from_endpoints(value, P1);
		}
	}

	public Vector2d P1
	{
		get
		{
			return Center + Extent * Direction;
		}
		set
		{
			update_from_endpoints(P0, value);
		}
	}

	public double Length => 2.0 * Extent;

	public bool IsClosed => false;

	public double ParamLength => 1.0;

	public bool HasArcLength => true;

	public double ArcLength => 2.0 * Extent;

	public bool IsTransformable => true;

	public Segment2d(Vector2d p0, Vector2d p1)
	{
		Center = 0.5 * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5 * Direction.Normalize();
	}

	public Segment2d(Vector2d center, Vector2d direction, double extent)
	{
		Center = center;
		Direction = direction;
		Extent = extent;
	}

	public Vector2d Endpoint(int i)
	{
		if (i != 0)
		{
			return Center + Extent * Direction;
		}
		return Center - Extent * Direction;
	}

	public Vector2d PointAt(double d)
	{
		return Center + d * Direction;
	}

	public Vector2d PointBetween(double t)
	{
		return Center + (2.0 * t - 1.0) * Extent * Direction;
	}

	public double DistanceSquared(Vector2d p)
	{
		double num = (p - Center).Dot(Direction);
		if (num >= Extent)
		{
			return P1.DistanceSquared(p);
		}
		if (num <= 0.0 - Extent)
		{
			return P0.DistanceSquared(p);
		}
		return (Center + num * Direction).DistanceSquared(p);
	}

	public double DistanceSquared(Vector2d p, out double t)
	{
		t = (p - Center).Dot(Direction);
		if (t >= Extent)
		{
			t = Extent;
			return P1.DistanceSquared(p);
		}
		if (t <= 0.0 - Extent)
		{
			t = 0.0 - Extent;
			return P0.DistanceSquared(p);
		}
		return (Center + t * Direction).DistanceSquared(p);
	}

	public Vector2d NearestPoint(Vector2d p)
	{
		double num = (p - Center).Dot(Direction);
		if (num >= Extent)
		{
			return P1;
		}
		if (num <= 0.0 - Extent)
		{
			return P0;
		}
		return Center + num * Direction;
	}

	public double Project(Vector2d p)
	{
		return (p - Center).Dot(Direction);
	}

	private void update_from_endpoints(Vector2d p0, Vector2d p1)
	{
		Center = 0.5 * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5 * Direction.Normalize();
	}

	public int WhichSide(Vector2d test, double tol = 0.0)
	{
		Vector2d vector2d = Center + Extent * Direction;
		Vector2d vector2d2 = Center - Extent * Direction;
		double num = test.x - vector2d.x;
		double num2 = test.y - vector2d.y;
		double num3 = vector2d2.x - vector2d.x;
		double num4 = vector2d2.y - vector2d.y;
		double num5 = num * num4 - num3 * num2;
		if (!(num5 > tol))
		{
			if (!(num5 < 0.0 - tol))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public Vector2d SampleT(double t)
	{
		return Center + (2.0 * t - 1.0) * Extent * Direction;
	}

	public Vector2d TangentT(double t)
	{
		return Direction;
	}

	public Vector2d SampleArcLength(double a)
	{
		return P0 + a * Direction;
	}

	public void Reverse()
	{
		update_from_endpoints(P1, P0);
	}

	public IParametricCurve2d Clone()
	{
		return new Segment2d(Center, Direction, Extent);
	}

	public void Transform(ITransform2 xform)
	{
		Center = xform.TransformP(Center);
		Direction = xform.TransformN(Direction);
		Extent = xform.TransformScalar(Extent);
	}

	public static double FastDistanceSquared(ref Vector2d a, ref Vector2d b, ref Vector2d pt)
	{
		double num = b.x - a.x;
		double num2 = b.y - a.y;
		double num3 = num * num + num2 * num2;
		double num4 = pt.x - a.x;
		double num5 = pt.y - a.y;
		if (num3 < 1E-13)
		{
			return num4 * num4 + num5 * num5;
		}
		double num6 = num4 * num + num5 * num2;
		if (num6 <= 0.0)
		{
			return num4 * num4 + num5 * num5;
		}
		if (num6 >= num3)
		{
			num4 = pt.x - b.x;
			num5 = pt.y - b.y;
			return num4 * num4 + num5 * num5;
		}
		num4 = pt.x - (a.x + num6 * num / num3);
		num5 = pt.y - (a.y + num6 * num2 / num3);
		return num4 * num4 + num5 * num5;
	}

	public static int WhichSide(ref Vector2d a, ref Vector2d b, ref Vector2d test, double tol = 0.0)
	{
		double num = test.x - a.x;
		double num2 = test.y - a.y;
		double num3 = b.x - a.x;
		double num4 = b.y - a.y;
		double num5 = num * num4 - num3 * num2;
		if (!(num5 > tol))
		{
			if (!(num5 < 0.0 - tol))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public bool Intersects(ref Segment2d seg2, double dotThresh = double.Epsilon, double intervalThresh = 0.0)
	{
		Vector2d vector2d = seg2.Center - Center;
		double num = Direction.DotPerp(seg2.Direction);
		if (Math.Abs(num) > dotThresh)
		{
			double num2 = 1.0 / num;
			double num3 = vector2d.DotPerp(Direction);
			double value = vector2d.DotPerp(seg2.Direction) * num2;
			double value2 = num3 * num2;
			if (Math.Abs(value) <= Extent + intervalThresh)
			{
				return Math.Abs(value2) <= seg2.Extent + intervalThresh;
			}
			return false;
		}
		vector2d.Normalize();
		if (Math.Abs(vector2d.DotPerp(seg2.Direction)) <= dotThresh)
		{
			vector2d = seg2.Center - Center;
			double num4 = Direction.Dot(vector2d);
			double x = num4 - seg2.Extent;
			double y = num4 + seg2.Extent;
			if (new Interval1d(0.0 - Extent, Extent).Overlaps(new Interval1d(x, y)))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool Intersects(Segment2d seg2, double dotThresh = double.Epsilon, double intervalThresh = 0.0)
	{
		return Intersects(ref seg2, dotThresh, intervalThresh);
	}

	public bool BiEquals(Segment2d seg)
	{
		if (seg.Center == Center)
		{
			return seg.Extent == Extent;
		}
		return false;
	}
}
