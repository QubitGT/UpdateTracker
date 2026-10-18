namespace g3;

public struct Line3f
{
	public Vector3f Origin;

	public Vector3f Direction;

	public Line3f(Vector3f origin, Vector3f direction)
	{
		Origin = origin;
		Direction = direction;
	}

	public Vector3f PointAt(float d)
	{
		return Origin + d * Direction;
	}

	public float Project(Vector3f p)
	{
		return (p - Origin).Dot(Direction);
	}

	public float DistanceSquared(Vector3f p)
	{
		float num = (p - Origin).Dot(Direction);
		return (Origin + num * Direction - p).LengthSquared;
	}

	public Vector3f ClosestPoint(Vector3f p)
	{
		float num = (p - Origin).Dot(Direction);
		return Origin + num * Direction;
	}
}
