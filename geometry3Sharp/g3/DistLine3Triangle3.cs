using System;

namespace g3;

public class DistLine3Triangle3
{
	private Line3d line;

	private Triangle3d triangle;

	public double DistanceSquared = -1.0;

	public Vector3d LineClosest;

	public double LineParam;

	public Vector3d TriangleClosest;

	public Vector3d TriangleBaryCoords;

	public Line3d Line
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

	public DistLine3Triangle3(Line3d LineIn, Triangle3d TriangleIn)
	{
		triangle = TriangleIn;
		line = LineIn;
	}

	public DistLine3Triangle3 Compute()
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
		Vector3d vector3d = triangle.V1 - triangle.V0;
		Vector3d vector3d2 = triangle.V2 - triangle.V0;
		if (Math.Abs(vector3d.UnitCross(vector3d2).Dot(line.Direction)) > 1E-08)
		{
			Vector3d v = line.Origin - triangle.V0;
			Vector3d u = Vector3d.Zero;
			Vector3d v2 = Vector3d.Zero;
			Vector3d.GenerateComplementBasis(ref u, ref v2, line.Direction);
			double num = u.Dot(vector3d);
			double num2 = u.Dot(vector3d2);
			double num3 = u.Dot(v);
			double num4 = v2.Dot(vector3d);
			double num5 = v2.Dot(vector3d2);
			double num6 = v2.Dot(v);
			double num7 = 1.0 / (num * num5 - num2 * num4);
			double num8 = (num5 * num3 - num2 * num6) * num7;
			double num9 = (num * num6 - num4 * num3) * num7;
			double num10 = 1.0 - num8 - num9;
			if (num10 >= 0.0 && num8 >= 0.0 && num9 >= 0.0)
			{
				double num11 = line.Direction.Dot(vector3d);
				double num12 = line.Direction.Dot(vector3d2);
				double num13 = line.Direction.Dot(v);
				LineParam = num8 * num11 + num9 * num12 - num13;
				TriangleBaryCoords = new Vector3d(num10, num8, num9);
				LineClosest = line.Origin + LineParam * line.Direction;
				TriangleClosest = triangle.V0 + num8 * vector3d + num9 * vector3d2;
				DistanceSquared = 0.0;
				return 0.0;
			}
		}
		double num14 = double.MaxValue;
		int num15 = 2;
		int num16 = 0;
		while (num16 < 3)
		{
			Segment3d segmentIn = new Segment3d(triangle[num15], triangle[num16]);
			DistLine3Segment3 distLine3Segment = new DistLine3Segment3(line, segmentIn);
			double squared = distLine3Segment.GetSquared();
			if (squared < num14)
			{
				LineClosest = distLine3Segment.LineClosest;
				TriangleClosest = distLine3Segment.SegmentClosest;
				num14 = squared;
				LineParam = distLine3Segment.LineParameter;
				double num17 = distLine3Segment.SegmentParameter / segmentIn.Extent;
				TriangleBaryCoords = Vector3d.Zero;
				TriangleBaryCoords[num15] = 0.5 * (1.0 - num17);
				TriangleBaryCoords[num16] = 1.0 - TriangleBaryCoords[num15];
				TriangleBaryCoords[3 - num15 - num16] = 0.0;
			}
			num15 = num16++;
		}
		DistanceSquared = num14;
		return DistanceSquared;
	}
}
