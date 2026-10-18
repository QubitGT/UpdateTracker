using System;

namespace g3;

public class IntrLine2Segment2
{
	private Line2d line;

	private Segment2d segment;

	private double intervalThresh;

	private double dotThresh = 1E-08;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d Point;

	public double Parameter;

	public Line2d Line
	{
		get
		{
			return line;
		}
		set
		{
			line = value;
			Result = IntersectionResult.NotComputed;
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

	public IntrLine2Segment2(Line2d line, Segment2d seg)
	{
		this.line = line;
		segment = seg;
	}

	public IntrLine2Segment2 Compute()
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
		if (!line.Direction.IsNormalized || !segment.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		Vector2d s = Vector2d.Zero;
		Type = IntrLine2Line2.Classify(line.Origin, line.Direction, segment.Center, segment.Direction, dotThresh, ref s);
		if (Type == IntersectionType.Point)
		{
			if (Math.Abs(s[1]) <= segment.Extent + intervalThresh)
			{
				Quantity = 1;
				Point = line.Origin + s[0] * line.Direction;
				Parameter = s[0];
			}
			else
			{
				Quantity = 0;
				Type = IntersectionType.Empty;
			}
		}
		else if (Type == IntersectionType.Line)
		{
			Type = IntersectionType.Segment;
			Quantity = int.MaxValue;
		}
		else
		{
			Quantity = 0;
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}
}
