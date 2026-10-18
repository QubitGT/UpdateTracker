using System;

namespace g3;

public class DistPoint2Box2
{
	private Vector2d point;

	private Box2d box;

	public double DistanceSquared = -1.0;

	public Vector2d BoxClosest;

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

	public Box2d Box
	{
		get
		{
			return box;
		}
		set
		{
			box = value;
			DistanceSquared = -1.0;
		}
	}

	public DistPoint2Box2(Vector2d PointIn, Box2d boxIn)
	{
		point = PointIn;
		box = boxIn;
	}

	public DistPoint2Box2 Compute()
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
		Vector2d vector2d = point - box.Center;
		double num = 0.0;
		Vector2d zero = Vector2d.Zero;
		for (int i = 0; i < 2; i++)
		{
			zero[i] = vector2d.Dot(box.Axis(i));
			if (zero[i] < 0.0 - box.Extent[i])
			{
				double num2 = zero[i] + box.Extent[i];
				num += num2 * num2;
				zero[i] = 0.0 - box.Extent[i];
			}
			else if (zero[i] > box.Extent[i])
			{
				double num2 = zero[i] - box.Extent[i];
				num += num2 * num2;
				zero[i] = box.Extent[i];
			}
		}
		BoxClosest = box.Center;
		for (int i = 0; i < 2; i++)
		{
			BoxClosest += zero[i] * box.Axis(i);
		}
		DistanceSquared = num;
		return num;
	}
}
