namespace g3;

public struct Segment3f
{
	public Vector3f Center;

	public Vector3f Direction;

	public float Extent;

	public Vector3f P0
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

	public Vector3f P1
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

	public float Length => 2f * Extent;

	public Segment3f(Vector3f p0, Vector3f p1)
	{
		Center = 0.5f * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5f * Direction.Normalize();
	}

	public Segment3f(Vector3f center, Vector3f direction, float extent)
	{
		Center = center;
		Direction = direction;
		Extent = extent;
	}

	public void SetEndpoints(Vector3f p0, Vector3f p1)
	{
		update_from_endpoints(p0, p1);
	}

	public Vector3f PointAt(float d)
	{
		return Center + d * Direction;
	}

	public Vector3f PointBetween(float t)
	{
		return Center + (2f * t - 1f) * Extent * Direction;
	}

	public float DistanceSquared(Vector3f p)
	{
		float num = (p - Center).Dot(Direction);
		if (num >= Extent)
		{
			return P1.DistanceSquared(p);
		}
		if (num <= 0f - Extent)
		{
			return P0.DistanceSquared(p);
		}
		return (Center + num * Direction - p).LengthSquared;
	}

	public Vector3f NearestPoint(Vector3f p)
	{
		float num = (p - Center).Dot(Direction);
		if (num >= Extent)
		{
			return P1;
		}
		if (num <= 0f - Extent)
		{
			return P0;
		}
		return Center + num * Direction;
	}

	public float Project(Vector3f p)
	{
		return (p - Center).Dot(Direction);
	}

	private void update_from_endpoints(Vector3f p0, Vector3f p1)
	{
		Center = 0.5f * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5f * Direction.Normalize();
	}
}
