using System;

namespace g3;

public class IntrSegment2Segment2
{
	private Segment2d segment1;

	private Segment2d segment2;

	private double intervalThresh;

	private double dotThresh = 1E-08;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d Point0;

	public Vector2d Point1;

	public double Parameter0;

	public double Parameter1;

	public Segment2d Segment1
	{
		get
		{
			return segment1;
		}
		set
		{
			segment1 = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Segment2d Segment2
	{
		get
		{
			return segment2;
		}
		set
		{
			segment2 = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public double IntervalThreshold
	{
		get
		{
			return intervalThresh;
		}
		set
		{
			intervalThresh = Math.Max(value, 0.0);
			Result = IntersectionResult.NotComputed;
		}
	}

	public double DotThreshold
	{
		get
		{
			return dotThresh;
		}
		set
		{
			dotThresh = Math.Max(value, 0.0);
			Result = IntersectionResult.NotComputed;
		}
	}

	public bool IsSimpleIntersection
	{
		get
		{
			if (Result == IntersectionResult.Intersects)
			{
				return Type == IntersectionType.Point;
			}
			return false;
		}
	}

	public IntrSegment2Segment2(Segment2d seg1, Segment2d seg2)
	{
		segment1 = seg1;
		segment2 = seg2;
	}

	public IntrSegment2Segment2 Compute()
	{
		Find();
		return this;
	}

	public bool Find()
	{
		if (Result != 0)
		{
			return Result == IntersectionResult.Intersects;
		}
		if (!segment1.Direction.IsNormalized || !segment2.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		Vector2d s = Vector2d.Zero;
		Type = IntrLine2Line2.Classify(segment1.Center, segment1.Direction, segment2.Center, segment2.Direction, dotThresh, ref s);
		if (Type == IntersectionType.Point)
		{
			if (Math.Abs(s[0]) <= segment1.Extent + intervalThresh && Math.Abs(s[1]) <= segment2.Extent + intervalThresh)
			{
				Quantity = 1;
				Point0 = segment1.Center + s[0] * segment1.Direction;
				Parameter0 = s[0];
			}
			else
			{
				Quantity = 0;
				Type = IntersectionType.Empty;
			}
		}
		else if (Type == IntersectionType.Line)
		{
			Vector2d v = segment2.Center - segment1.Center;
			double num = segment1.Direction.Dot(v);
			double v2 = num - segment2.Extent;
			double v3 = num + segment2.Extent;
			Intersector1 intersector = new Intersector1(0.0 - segment1.Extent, segment1.Extent, v2, v3);
			intersector.Find();
			Quantity = intersector.NumIntersections;
			if (Quantity == 2)
			{
				Type = IntersectionType.Segment;
				Parameter0 = intersector.GetIntersection(0);
				Point0 = segment1.Center + Parameter0 * segment1.Direction;
				Parameter1 = intersector.GetIntersection(1);
				Point1 = segment1.Center + Parameter1 * segment1.Direction;
			}
			else if (Quantity == 1)
			{
				Type = IntersectionType.Point;
				Parameter0 = intersector.GetIntersection(0);
				Point0 = segment1.Center + Parameter0 * segment1.Direction;
			}
			else
			{
				Type = IntersectionType.Empty;
			}
		}
		else
		{
			Quantity = 0;
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	private void sanity_check()
	{
		if (Quantity != 0 && Quantity != 1)
		{
			_ = Quantity;
			_ = 2;
		}
	}
}
