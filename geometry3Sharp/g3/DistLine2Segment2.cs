using System;

namespace g3;

public class DistLine2Segment2
{
	private Line2d line;

	private Segment2d segment;

	public double DistanceSquared = -1.0;

	public Vector2d LineClosest;

	public double LineParameter;

	public Vector2d SegmentClosest;

	public double SegmentParameter;

	public Line2d Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
			DistanceSquared = -1.0;
		}
	}

	public Segment2d Segment
	{
		get
		{
			return segment;
		}
		set
		{
			segment = value;
			DistanceSquared = -1.0;
		}
	}

	public DistLine2Segment2(Line2d LineIn, Segment2d SegmentIn)
	{
		segment = SegmentIn;
		line = LineIn;
	}

	public static double MinDistance(Line2d line, Segment2d segment)
	{
		return new DistLine2Segment2(line, segment).Get();
	}

	public DistLine2Segment2 Compute()
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
		Vector2d vector2d = line.Origin - segment.Center;
		double num = 0.0 - line.Direction.Dot(segment.Direction);
		double num2 = vector2d.Dot(line.Direction);
		double lengthSquared = vector2d.LengthSquared;
		double num3 = Math.Abs(1.0 - num * num);
		double num8;
		double num5;
		double num9;
		if (num3 >= 1E-08)
		{
			double num4 = 0.0 - vector2d.Dot(segment.Direction);
			num5 = num * num2 - num4;
			double num6 = segment.Extent * num3;
			if (num5 >= 0.0 - num6)
			{
				if (num5 <= num6)
				{
					double num7 = 1.0 / num3;
					num8 = (num * num4 - num2) * num7;
					num5 *= num7;
					num9 = 0.0;
				}
				else
				{
					num5 = segment.Extent;
					num8 = 0.0 - (num * num5 + num2);
					num9 = (0.0 - num8) * num8 + num5 * (num5 + 2.0 * num4) + lengthSquared;
				}
			}
			else
			{
				num5 = 0.0 - segment.Extent;
				num8 = 0.0 - (num * num5 + num2);
				num9 = (0.0 - num8) * num8 + num5 * (num5 + 2.0 * num4) + lengthSquared;
			}
		}
		else
		{
			num5 = 0.0;
			num8 = 0.0 - num2;
			num9 = num2 * num8 + lengthSquared;
		}
		LineParameter = num8;
		LineClosest = line.Origin + num8 * line.Direction;
		SegmentParameter = num5;
		SegmentClosest = segment.Center + num5 * segment.Direction;
		if (num9 < 0.0)
		{
			num9 = 0.0;
		}
		DistanceSquared = num9;
		return num9;
	}
}
