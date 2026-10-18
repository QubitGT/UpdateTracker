namespace g3;

public class IntrSegment2Triangle2
{
	private Segment2d segment;

	private Triangle2d triangle;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector2d Point0;

	public Vector2d Point1;

	public double Param0;

	public double Param1;

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

	public Triangle2d Triangle
	{
		get
		{
			return triangle;
		}
		set
		{
			triangle = value;
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

	public IntrSegment2Triangle2(Segment2d s, Triangle2d t)
	{
		segment = s;
		triangle = t;
	}

	public IntrSegment2Triangle2 Compute()
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
		if (!segment.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		Vector3d dist = Vector3d.Zero;
		Vector3i sign = Vector3i.Zero;
		int positive = 0;
		int negative = 0;
		int zero = 0;
		IntrLine2Triangle2.TriangleLineRelations(segment.Center, segment.Direction, triangle, ref dist, ref sign, ref positive, ref negative, ref zero);
		if (positive == 3 || negative == 3)
		{
			Quantity = 0;
			Type = IntersectionType.Empty;
		}
		else
		{
			Vector2d param = Vector2d.Zero;
			IntrLine2Triangle2.GetInterval(segment.Center, segment.Direction, triangle, dist, sign, ref param);
			Intersector1 intersector = new Intersector1(param[0], param[1], 0.0 - segment.Extent, segment.Extent);
			intersector.Find();
			Quantity = intersector.NumIntersections;
			if (Quantity == 2)
			{
				Type = IntersectionType.Segment;
				Param0 = intersector.GetIntersection(0);
				Point0 = segment.Center + Param0 * segment.Direction;
				Param1 = intersector.GetIntersection(1);
				Point1 = segment.Center + Param1 * segment.Direction;
			}
			else if (Quantity == 1)
			{
				Type = IntersectionType.Point;
				Param0 = intersector.GetIntersection(0);
				Point0 = segment.Center + Param0 * segment.Direction;
			}
			else
			{
				Type = IntersectionType.Empty;
			}
		}
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}
}
