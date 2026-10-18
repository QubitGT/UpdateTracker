namespace g3;

public struct Line2f
{
	public Vector2f Origin;

	public Vector2f Direction;

	public Line2f(Vector2f origin, Vector2f direction)
	{
		Origin = origin;
		Direction = direction;
	}

	public Vector2f PointAt(float d)
	{
		return Origin + d * Direction;
	}

	public float Project(Vector2f p)
	{
		return (p - Origin).Dot(Direction);
	}

	public float DistanceSquared(Vector2f p)
	{
		float num = (p - Origin).Dot(Direction);
		return (Origin + num * Direction - p).LengthSquared;
	}
}
