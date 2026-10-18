namespace g3;

public struct Segment3d : IParametricCurve3d
{
	public Vector3d Center;

	public Vector3d Direction;

	public double Extent;

	public Vector3d P0
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

	public Vector3d P1
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

	public Segment3d(Vector3d p0, Vector3d p1)
	{
		Center = 0.5 * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5 * Direction.Normalize();
	}

	public Segment3d(Vector3d center, Vector3d direction, double extent)
	{
		Center = center;
		Direction = direction;
		Extent = extent;
	}

	public void SetEndpoints(Vector3d p0, Vector3d p1)
	{
		update_from_endpoints(p0, p1);
	}

	public Vector3d PointAt(double d)
	{
		return Center + d * Direction;
	}

	public Vector3d PointBetween(double t)
	{
		return Center + (2.0 * t - 1.0) * Extent * Direction;
	}

	public double DistanceSquared(Vector3d p)
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
		return (Center + num * Direction - p).LengthSquared;
	}

	public double DistanceSquared(Vector3d p, out double t)
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
		return (Center + t * Direction - p).LengthSquared;
	}

	public Vector3d NearestPoint(Vector3d p)
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

	public double Project(Vector3d p)
	{
		return (p - Center).Dot(Direction);
	}

	private void update_from_endpoints(Vector3d p0, Vector3d p1)
	{
		Center = 0.5 * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5 * Direction.Normalize();
	}

	public static implicit operator Segment3d(Segment3f v)
	{
		return new Segment3d(v.Center, v.Direction, v.Extent);
	}

	public static explicit operator Segment3f(Segment3d v)
	{
		return new Segment3f((Vector3f)v.Center, (Vector3f)v.Direction, (float)v.Extent);
	}

	public Vector3d SampleT(double t)
	{
		return Center + (2.0 * t - 1.0) * Extent * Direction;
	}

	public Vector3d TangentT(double t)
	{
		return Direction;
	}

	public Vector3d SampleArcLength(double a)
	{
		return P0 + a * Direction;
	}

	public void Reverse()
	{
		update_from_endpoints(P1, P0);
	}

	public IParametricCurve3d Clone()
	{
		return new Segment3d(Center, Direction, Extent);
	}
}
