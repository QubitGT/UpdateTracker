using System;

namespace g3;

public class DistSegment3Triangle3
{
	private Segment3d segment;

	private Triangle3d triangle;

	public double DistanceSquared = -1.0;

	public Vector3d SegmentClosest;

	public double SegmentParam;

	public Vector3d TriangleClosest;

	public Vector3d TriangleBaryCoords;

	public Segment3d Segment
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

	public Triangle3d Triangle
	{
		get
		{
			return triangle;
		}
		set
		{
			triangle = value;
			DistanceSquared = -1.0;
		}
	}

	public DistSegment3Triangle3(Segment3d SegmentIn, Triangle3d TriangleIn)
	{
		triangle = TriangleIn;
		segment = SegmentIn;
	}

	public DistSegment3Triangle3 Compute()
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
		DistLine3Triangle3 distLine3Triangle = new DistLine3Triangle3(new Line3d(segment.Center, segment.Direction), triangle);
		double squared = distLine3Triangle.GetSquared();
		SegmentParam = distLine3Triangle.LineParam;
		if (SegmentParam >= 0.0 - segment.Extent)
		{
			if (SegmentParam <= segment.Extent)
			{
				SegmentClosest = distLine3Triangle.LineClosest;
				TriangleClosest = distLine3Triangle.TriangleClosest;
				TriangleBaryCoords = distLine3Triangle.TriangleBaryCoords;
			}
			else
			{
				SegmentClosest = segment.P1;
				DistPoint3Triangle3 distPoint3Triangle = new DistPoint3Triangle3(SegmentClosest, triangle);
				squared = distPoint3Triangle.GetSquared();
				TriangleClosest = distPoint3Triangle.TriangleClosest;
				SegmentParam = segment.Extent;
				TriangleBaryCoords = distPoint3Triangle.TriangleBaryCoords;
			}
		}
		else
		{
			SegmentClosest = segment.P0;
			DistPoint3Triangle3 distPoint3Triangle2 = new DistPoint3Triangle3(SegmentClosest, triangle);
			squared = distPoint3Triangle2.GetSquared();
			TriangleClosest = distPoint3Triangle2.TriangleClosest;
			SegmentParam = 0.0 - segment.Extent;
			TriangleBaryCoords = distPoint3Triangle2.TriangleBaryCoords;
		}
		DistanceSquared = squared;
		return DistanceSquared;
	}
}
