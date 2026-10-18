using System;

namespace g3;

public class DistLine2Line2
{
	private Line2d line1;

	private Line2d line2;

	public double DistanceSquared = -1.0;

	public Vector2d Line1Closest;

	public Vector2d Line2Closest;

	public double Line1Parameter;

	public double Line2Parameter;

	public Line2d Line
	{
		get
		{
			return line1;
		}
		set
		{
			line1 = value;
			DistanceSquared = -1.0;
		}
	}

	public Line2d Line2
	{
		get
		{
			return line2;
		}
		set
		{
			line2 = value;
			DistanceSquared = -1.0;
		}
	}

	public DistLine2Line2(Line2d Line1, Line2d Line2)
	{
		line2 = Line2;
		line1 = Line1;
	}

	public static double MinDistance(Line2d line1, Line2d line2)
	{
		return new DistLine2Line2(line1, line2).Get();
	}

	public DistLine2Line2 Compute()
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
		Vector2d vector2d = line1.Origin - line2.Origin;
		double num = 0.0 - line1.Direction.Dot(line2.Direction);
		double num2 = vector2d.Dot(line1.Direction);
		double lengthSquared = vector2d.LengthSquared;
		double num3 = Math.Abs(1.0 - num * num);
		double num6;
		double num7;
		double num8;
		if (num3 >= 1E-08)
		{
			double num4 = 0.0 - vector2d.Dot(line2.Direction);
			double num5 = 1.0 / num3;
			num6 = (num * num4 - num2) * num5;
			num7 = (num * num2 - num4) * num5;
			num8 = 0.0;
		}
		else
		{
			num6 = 0.0 - num2;
			num7 = 0.0;
			num8 = num2 * num6 + lengthSquared;
			if (num8 < 0.0)
			{
				num8 = 0.0;
			}
		}
		Line1Parameter = num6;
		Line1Closest = line1.Origin + num6 * line1.Direction;
		Line2Parameter = num7;
		Line2Closest = line2.Origin + num7 * line2.Direction;
		DistanceSquared = num8;
		return num8;
	}
}
