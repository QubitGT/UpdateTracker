namespace g3;

public struct Line3d
{
	public Vector3d Origin;

	public Vector3d Direction;

	public Line3d(Vector3d origin, Vector3d direction)
	{
		Origin = origin;
		Direction = direction;
	}

	public Vector3d PointAt(double d)
	{
		return Origin + d * Direction;
	}

	public double Project(Vector3d p)
	{
		return (p - Origin).Dot(Direction);
	}

	public double DistanceSquared(Vector3d p)
	{
		double num = (p - Origin).Dot(Direction);
		return (Origin + num * Direction - p).LengthSquared;
	}

	public Vector3d ClosestPoint(Vector3d p)
	{
		double num = (p - Origin).Dot(Direction);
		return Origin + num * Direction;
	}

	public static implicit operator Line3d(Line3f v)
	{
		return new Line3d(v.Origin, v.Direction);
	}

	public static explicit operator Line3f(Line3d v)
	{
		return new Line3f((Vector3f)v.Origin, (Vector3f)v.Direction);
	}
}
