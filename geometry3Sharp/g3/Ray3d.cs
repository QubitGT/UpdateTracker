using UnityEngine;

namespace g3;

public struct Ray3d
{
	public Vector3d Origin;

	public Vector3d Direction;

	public Ray3d(Vector3d origin, Vector3d direction, bool bIsNormalized = false)
	{
		Origin = origin;
		Direction = direction;
		if (!bIsNormalized && !Direction.IsNormalized)
		{
			Direction.Normalize();
		}
	}

	public Ray3d(Vector3f origin, Vector3f direction)
	{
		Origin = origin;
		Direction = direction;
		Direction.Normalize();
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
		if (num < 0.0)
		{
			return Origin.DistanceSquared(p);
		}
		return (Origin + num * Direction - p).LengthSquared;
	}

	public Vector3d ClosestPoint(Vector3d p)
	{
		double num = (p - Origin).Dot(Direction);
		if (num < 0.0)
		{
			return Origin;
		}
		return Origin + num * Direction;
	}

	public static implicit operator Ray3d(Ray3f v)
	{
		return new Ray3d(v.Origin, ((Vector3d)v.Direction).Normalized);
	}

	public static explicit operator Ray3f(Ray3d v)
	{
		return new Ray3f((Vector3f)v.Origin, ((Vector3f)v.Direction).Normalized);
	}

	public static implicit operator Ray3d(Ray r)
	{
		return new Ray3d(r.origin, ((Vector3d)r.direction).Normalized);
	}

	public static explicit operator Ray(Ray3d r)
	{
		return new Ray((Vector3)r.Origin, ((Vector3)r.Direction).normalized);
	}
}
