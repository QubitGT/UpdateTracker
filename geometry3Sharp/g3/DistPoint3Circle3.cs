using System;

namespace g3;

public class DistPoint3Circle3
{
	private Vector3d point;

	private Circle3d circle;

	public double DistanceSquared = -1.0;

	public Vector3d CircleClosest;

	public bool AllCirclePointsEquidistant;

	public Vector3d Point
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

	public Circle3d Circle
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

	public DistPoint3Circle3(Vector3d PointIn, Circle3d circleIn)
	{
		point = PointIn;
		circle = circleIn;
	}

	public DistPoint3Circle3 Compute()
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
		Vector3d vector3d = point - circle.Center;
		Vector3d vector3d2 = vector3d - circle.Normal.Dot(vector3d) * circle.Normal;
		double length = vector3d2.Length;
		if (length > 2.220446049250313E-16)
		{
			CircleClosest = circle.Center + circle.Radius * vector3d2 / length;
			AllCirclePointsEquidistant = false;
		}
		else
		{
			CircleClosest = circle.Center + circle.Radius * circle.PlaneX;
			AllCirclePointsEquidistant = true;
		}
		Vector3d v = point - CircleClosest;
		double num = v.Dot(v);
		if (num < 0.0)
		{
			num = 0.0;
		}
		DistanceSquared = num;
		return num;
	}
}
