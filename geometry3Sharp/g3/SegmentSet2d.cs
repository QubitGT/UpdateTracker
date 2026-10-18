using System.Collections.Generic;

namespace g3;

public class SegmentSet2d
{
	private List<Segment2d> Segments;

	public SegmentSet2d()
	{
		Segments = new List<Segment2d>();
	}

	public SegmentSet2d(GeneralPolygon2d poly)
	{
		Segments = new List<Segment2d>(poly.Outer.SegmentItr());
		foreach (Polygon2d hole in poly.Holes)
		{
			Segments.AddRange(hole.SegmentItr());
		}
	}

	public SegmentSet2d(List<GeneralPolygon2d> polys)
	{
		Segments = new List<Segment2d>();
		foreach (GeneralPolygon2d poly in polys)
		{
			Segments.AddRange(poly.Outer.SegmentItr());
			foreach (Polygon2d hole in poly.Holes)
			{
				Segments.AddRange(hole.SegmentItr());
			}
		}
	}

	public IntrSegment2Segment2 FindAnyIntersection(Segment2d seg, out int iSegment)
	{
		int count = Segments.Count;
		for (iSegment = 0; iSegment < count; iSegment++)
		{
			IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(seg, Segments[iSegment]);
			if (intrSegment2Segment.Find())
			{
				return intrSegment2Segment;
			}
		}
		return null;
	}

	public void FindAllIntersections(Segment2d seg, List<double> segmentTs, List<int> indices = null, List<IntrSegment2Segment2> tests = null, bool bOnlySimple = true)
	{
		int count = Segments.Count;
		for (int i = 0; i < count; i++)
		{
			IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(seg, Segments[i])
			{
				IntervalThreshold = 1E-08
			};
			if (!intrSegment2Segment.Find() || (bOnlySimple && !intrSegment2Segment.IsSimpleIntersection))
			{
				continue;
			}
			tests?.Add(intrSegment2Segment);
			indices?.Add(i);
			if (segmentTs != null)
			{
				segmentTs.Add(intrSegment2Segment.Parameter0);
				if (!intrSegment2Segment.IsSimpleIntersection)
				{
					segmentTs.Add(intrSegment2Segment.Parameter1);
				}
			}
		}
	}
}
