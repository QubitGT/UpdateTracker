using System;

namespace g3;

public class IntrLine2Line2
{
	private Line2d line1;

	private Line2d line2;

	private double dotThresh = 1E-08;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d Point;

	public double Segment1Parameter;

	public double Segment2Parameter;

	public Line2d Line1
	{
		get
		{
			return line1;
		}
		set
		{
			line1 = value;
			Result = IntersectionResult.NotComputed;
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

	public IntrLine2Line2(Line2d l1, Line2d l2)
	{
		line1 = l1;
		line2 = l2;
	}

	public IntrLine2Line2 Compute()
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
		if (!line1.Direction.IsNormalized || !line2.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		Vector2d s = Vector2d.Zero;
		Type = Classify(line1.Origin, line1.Direction, line2.Origin, line2.Direction, dotThresh, ref s);
		if (Type == IntersectionType.Point)
		{
			Quantity = 1;
			Point = line1.Origin + s.x * line1.Direction;
			Segment1Parameter = s.x;
			Segment2Parameter = s.y;
		}
		else if (Type == IntersectionType.Line)
		{
			Quantity = int.MaxValue;
		}
		else
		{
			Quantity = 0;
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public static IntersectionType Classify(Vector2d P0, Vector2d D0, Vector2d P1, Vector2d D1, double dotThreshold, ref Vector2d s)
	{
		dotThreshold = Math.Max(dotThreshold, 0.0);
		Vector2d vector2d = P1 - P0;
		double num = D0.DotPerp(D1);
		if (Math.Abs(num) > dotThreshold)
		{
			double num2 = 1.0 / num;
			double num3 = vector2d.DotPerp(D0);
			double num4 = vector2d.DotPerp(D1);
			s[0] = num4 * num2;
			s[1] = num3 * num2;
			return IntersectionType.Point;
		}
		vector2d.Normalize();
		if (Math.Abs(vector2d.DotPerp(D1)) <= dotThreshold)
		{
			return IntersectionType.Line;
		}
		return IntersectionType.Empty;
	}
}
