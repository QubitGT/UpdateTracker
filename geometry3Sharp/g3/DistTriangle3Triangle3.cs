using System;

namespace g3;

public class DistTriangle3Triangle3
{
	private Triangle3d triangle0;

	private Triangle3d triangle1;

	public double DistanceSquared = -1.0;

	public Vector3d Triangle0Closest;

	public Vector3d Triangle0BaryCoords;

	public Vector3d Triangle1Closest;

	public Vector3d Triangle1BaryCoords;

	public Triangle3d Triangle0
	{
		get
		{
			return triangle0;
		}
		set
		{
			triangle0 = value;
			DistanceSquared = -1.0;
		}
	}

	public Triangle3d Triangle1
	{
		get
		{
			return triangle1;
		}
		set
		{
			triangle1 = value;
			DistanceSquared = -1.0;
		}
	}

	public DistTriangle3Triangle3(Triangle3d Triangle0in, Triangle3d Triangle1in)
	{
		triangle0 = Triangle0in;
		triangle1 = Triangle1in;
	}

	public DistTriangle3Triangle3 Compute()
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
		double num = double.MaxValue;
		Segment3d segmentIn = default(Segment3d);
		int num2 = 2;
		int num3 = 0;
		while (num3 < 3)
		{
			segmentIn.SetEndpoints(triangle0[num2], triangle0[num3]);
			DistSegment3Triangle3 distSegment3Triangle = new DistSegment3Triangle3(segmentIn, triangle1);
			double squared = distSegment3Triangle.GetSquared();
			if (squared < num)
			{
				Triangle0Closest = distSegment3Triangle.SegmentClosest;
				Triangle1Closest = distSegment3Triangle.TriangleClosest;
				num = squared;
				double num4 = distSegment3Triangle.SegmentParam / segmentIn.Extent;
				Triangle0BaryCoords = Vector3d.Zero;
				Triangle0BaryCoords[num2] = 0.5 * (1.0 - num4);
				Triangle0BaryCoords[num3] = 1.0 - Triangle0BaryCoords[num2];
				Triangle0BaryCoords[3 - num2 - num3] = 0.0;
				Triangle1BaryCoords = distSegment3Triangle.TriangleBaryCoords;
				if (num <= 1E-08)
				{
					DistanceSquared = 0.0;
					return 0.0;
				}
			}
			num2 = num3++;
		}
		num2 = 2;
		num3 = 0;
		while (num3 < 3)
		{
			segmentIn.SetEndpoints(triangle1[num2], triangle1[num3]);
			DistSegment3Triangle3 distSegment3Triangle2 = new DistSegment3Triangle3(segmentIn, triangle0);
			double squared = distSegment3Triangle2.GetSquared();
			if (squared < num)
			{
				Triangle0Closest = distSegment3Triangle2.SegmentClosest;
				Triangle1Closest = distSegment3Triangle2.TriangleClosest;
				num = squared;
				double num4 = distSegment3Triangle2.SegmentParam / segmentIn.Extent;
				Triangle1BaryCoords = Vector3d.Zero;
				Triangle1BaryCoords[num2] = 0.5 * (1.0 - num4);
				Triangle1BaryCoords[num3] = 1.0 - Triangle1BaryCoords[num2];
				Triangle1BaryCoords[3 - num2 - num3] = 0.0;
				Triangle0BaryCoords = distSegment3Triangle2.TriangleBaryCoords;
				if (num <= 1E-08)
				{
					DistanceSquared = 0.0;
					return 0.0;
				}
			}
			num2 = num3++;
		}
		DistanceSquared = num;
		return DistanceSquared;
	}
}
