using System;

namespace g3;

public class DistPoint2Circle2
{
	private Vector2d point;

	private Circle2d circle;

	public double DistanceSquared = -1.0;

	public Vector2d CircleClosest;

	public bool AllCirclePointsEquidistant;

	public Vector2d Point
	{
		get
		{
			return point;
		}
		set
		{
			point = value;
			DistanceSquared = -1.0;
		}
	}

	public Circle2d Circle
	{
		get
		{
			return circle;
		}
		set
		{
			circle = value;
			DistanceSquared = -1.0;
		}
	}

	public DistPoint2Circle2(Vector2d PointIn, Circle2d circleIn)
	{
		point = PointIn;
		circle = circleIn;
	}

	public DistPoint2Circle2 Compute()
	{
		GetSquared();
		return this;
	}

	public double Get()
	{
		return Math.Sqrt(GetSquared());
	}

	public double GetSquared()
	{
		if (DistanceSquared >= 0.0)
		{
			return DistanceSquared;
		}
		Vector2d vector2d = point - circle.Center;
		double length = vector2d.Length;
		if (length > 2.220446049250313E-16)
		{
			CircleClosest = circle.Center + circle.Radius * vector2d / length;
			AllCirclePointsEquidistant = false;
		}
		else
		{
			CircleClosest = circle.Center + circle.Radius;
			AllCirclePointsEquidistant = true;
		}
		Vector2d v = point - CircleClosest;
		double num = v.Dot(v);
		if (num < 0.0)
		{
			num = 0.0;
		}
		DistanceSquared = num;
		return num;
	}
}
