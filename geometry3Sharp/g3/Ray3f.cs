using UnityEngine;

namespace g3;

public struct Ray3f
{
	public Vector3f Origin;

	public Vector3f Direction;

	public Ray3f(Vector3f origin, Vector3f direction, bool bIsNormalized = false)
	{
		Origin = origin;
		Direction = direction;
		if (!bIsNormalized && !Direction.IsNormalized)
		{
			Direction.Normalize();
		}
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

	public static implicit operator Ray3f(Ray r)
	{
		return new Ray3f(r.origin, r.direction);
	}

	public static implicit operator Ray(Ray3f r)
	{
		return new Ray(r.Origin, r.Direction);
	}
}
