namespace g3;

public struct Segment2f
{
	public Vector2f Center;

	public Vector2f Direction;

	public float Extent;

	public Vector2f P0
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

	public Vector2f P1
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

	public Segment2f(Vector2f p0, Vector2f p1)
	{
		Center = 0.5f * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5f * Direction.Normalize();
	}

	public Segment2f(Vector2f center, Vector2f direction, float extent)
	{
		Center = center;
		Direction = direction;
		Extent = extent;
	}

	public Vector2f PointAt(float d)
	{
		return Center + d * Direction;
	}

	public Vector2f PointBetween(float t)
	{
		return Center + (2f * t - 1f) * Extent * Direction;
	}

	public float DistanceSquared(Vector2f p)
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

	public Vector2f NearestPoint(Vector2f p)
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

	public float Project(Vector2f p)
	{
		return (p - Center).Dot(Direction);
	}

	private void update_from_endpoints(Vector2f p0, Vector2f p1)
	{
		Center = 0.5f * (p0 + p1);
		Direction = p1 - p0;
		Extent = 0.5f * Direction.Normalize();
	}

	public static float FastDistanceSquared(ref Vector2f a, ref Vector2f b, ref Vector2f pt)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		float num3 = num * num + num2 * num2;
		float num4 = pt.x - a.x;
		float num5 = pt.y - a.y;
		if ((double)num3 < 1E-07)
		{
			return num4 * num4 + num5 * num5;
		}
		float num6 = num4 * num + num5 * num2;
		if (num6 <= 0f)
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

	public bool BiEquals(Segment2d seg)
	{
		if (seg.Center == Center)
		{
			return seg.Extent == (double)Extent;
		}
		return false;
	}
}
