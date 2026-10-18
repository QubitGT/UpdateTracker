using System;
using System.Collections.Generic;

namespace g3;

public class Arrangement2d
{
	protected struct SegmentPoint
	{
		public double t;

		public int vid;
	}

	protected struct Intersection
	{
		public int eid;

		public int sidex;

		public int sidey;

		public IntrSegment2Segment2 intr;
	}

	public DGraph2 Graph;

	public PointHashGrid2d<int> PointHash;

	public double VertexSnapTol = 1E-05;

	public Arrangement2d(AxisAlignedBox2d boundsHint)
	{
		Graph = new DGraph2();
		double cellSize = boundsHint.MaxDim / 64.0;
		PointHash = new PointHashGrid2d<int>(cellSize, -1);
	}

	public void Insert(Vector2d a, Vector2d b, int gid = -1)
	{
		insert_segment(a, b, gid);
	}

	public void Insert(Segment2d segment, int gid = -1)
	{
		insert_segment(segment.P0, segment.P1, gid);
	}

	public void Insert(PolyLine2d pline, int gid = -1)
	{
		int num = pline.VertexCount - 1;
		for (int i = 0; i < num; i++)
		{
			Vector2d a = pline[i];
			Vector2d b = pline[i + 1];
			insert_segment(a, b, gid);
		}
	}

	public void Insert(Polygon2d poly, int gid = -1)
	{
		int vertexCount = poly.VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			Vector2d a = poly[i];
			Vector2d b = poly[(i + 1) % vertexCount];
			insert_segment(a, b, gid);
		}
	}

	public void ConnectOpenBoundaries(double distThresh)
	{
		int maxVertexID = Graph.MaxVertexID;
		for (int i = 0; i < maxVertexID; i++)
		{
			if (Graph.IsBoundaryVertex(i))
			{
				Vector2d vertex = Graph.GetVertex(i);
				int num = find_nearest_boundary_vertex(vertex, distThresh, i);
				if (num != -1)
				{
					Vector2d vertex2 = Graph.GetVertex(num);
					Insert(vertex, vertex2);
				}
			}
		}
	}

	protected bool insert_segment(ref Vector2d a, ref Vector2d b, int gid = -1, double tol = 0.0)
	{
		int num = find_existing_vertex(a);
		int num2 = find_existing_vertex(b);
		if (num == num2 && num >= 0)
		{
			return false;
		}
		List<Intersection> list = new List<Intersection>();
		find_intersecting_edges(ref a, ref b, list, tol);
		int count = list.Count;
		List<SegmentPoint> list2 = new List<SegmentPoint>();
		Segment2d segment2d = new Segment2d(a, b);
		for (int i = 0; i < count; i++)
		{
			Intersection intersection = list[i];
			int eid = intersection.eid;
			double parameter = intersection.intr.Parameter0;
			double parameter2 = intersection.intr.Parameter1;
			int num3 = -1;
			if (intersection.intr.Type == IntersectionType.Point || intersection.intr.Type == IntersectionType.Segment)
			{
				Index2i index2i = split_segment_at_t(eid, parameter, VertexSnapTol);
				num3 = index2i.b;
				Vector2d vertex = Graph.GetVertex(index2i.a);
				list2.Add(new SegmentPoint
				{
					t = segment2d.Project(vertex),
					vid = index2i.a
				});
			}
			if (intersection.intr.Type == IntersectionType.Segment)
			{
				if (num3 == -1)
				{
					Index2i index2i2 = split_segment_at_t(eid, parameter2, VertexSnapTol);
					Vector2d vertex2 = Graph.GetVertex(index2i2.a);
					list2.Add(new SegmentPoint
					{
						t = segment2d.Project(vertex2),
						vid = index2i2.a
					});
				}
				else
				{
					Segment2d edgeSegment = Graph.GetEdgeSegment(num3);
					Vector2d p = intersection.intr.Segment1.PointAt(parameter2);
					double t = edgeSegment.Project(p);
					Index2i index2i3 = split_segment_at_t(num3, t, VertexSnapTol);
					Vector2d vertex3 = Graph.GetVertex(index2i3.a);
					list2.Add(new SegmentPoint
					{
						t = segment2d.Project(vertex3),
						vid = index2i3.a
					});
				}
			}
		}
		if (num == -1)
		{
			num = find_existing_vertex(a);
		}
		if (num == -1)
		{
			num = Graph.AppendVertex(a);
			PointHash.InsertPointUnsafe(num, a);
		}
		if (num2 == -1)
		{
			num2 = find_existing_vertex(b);
		}
		if (num2 == -1)
		{
			num2 = Graph.AppendVertex(b);
			PointHash.InsertPointUnsafe(num2, b);
		}
		list2.Add(new SegmentPoint
		{
			t = segment2d.Project(a),
			vid = num
		});
		list2.Add(new SegmentPoint
		{
			t = segment2d.Project(b),
			vid = num2
		});
		list2.Sort((SegmentPoint pa, SegmentPoint pb) => (!(pa.t < pb.t)) ? ((pa.t > pb.t) ? 1 : 0) : (-1));
		for (int j = 0; j < list2.Count - 1; j++)
		{
			int vid = list2[j].vid;
			int vid2 = list2[j + 1].vid;
			if (vid != vid2)
			{
				if (Math.Abs(list2[j].t - list2[j + 1].t) < 1.1920928955078125E-07)
				{
					Console.WriteLine("insert_segment: different points with same t??");
				}
				if (Graph.FindEdge(vid, vid2) == -1)
				{
					Graph.AppendEdge(vid, vid2, gid);
				}
			}
		}
		return true;
	}

	protected bool insert_segment(Vector2d a, Vector2d b, int gid = -1, double tol = 0.0)
	{
		return insert_segment(ref a, ref b, gid, tol);
	}

	protected Index2i split_segment_at_t(int eid, double t, double tol)
	{
		Index2i edgeV = Graph.GetEdgeV(eid);
		Segment2d segment2d = new Segment2d(Graph.GetVertex(edgeV.a), Graph.GetVertex(edgeV.b));
		int num = -1;
		int jj = -1;
		if (t < 0.0 - (segment2d.Extent - tol))
		{
			num = edgeV.a;
		}
		else if (t > segment2d.Extent - tol)
		{
			num = edgeV.b;
		}
		else
		{
			if (Graph.SplitEdge(eid, out var split) != 0)
			{
				throw new Exception("insert_into_segment: edge split failed?");
			}
			num = split.vNew;
			jj = split.eNewBN;
			Vector2d vector2d = segment2d.PointAt(t);
			Graph.SetVertex(num, vector2d);
			PointHash.InsertPointUnsafe(split.vNew, vector2d);
		}
		return new Index2i(num, jj);
	}

	protected int find_existing_vertex(Vector2d pt)
	{
		return find_nearest_vertex(pt, VertexSnapTol);
	}

	protected int find_nearest_vertex(Vector2d pt, double searchRadius, int ignore_vid = -1)
	{
		KeyValuePair<int, double> keyValuePair = ((ignore_vid == -1) ? PointHash.FindNearestInRadius(pt, searchRadius, (int b) => pt.DistanceSquared(Graph.GetVertex(b))) : PointHash.FindNearestInRadius(pt, searchRadius, (int b) => pt.DistanceSquared(Graph.GetVertex(b)), (int vid) => vid == ignore_vid));
		if (keyValuePair.Key == PointHash.InvalidValue)
		{
			return -1;
		}
		return keyValuePair.Key;
	}

	protected int find_nearest_boundary_vertex(Vector2d pt, double searchRadius, int ignore_vid = -1)
	{
		KeyValuePair<int, double> keyValuePair = PointHash.FindNearestInRadius(pt, searchRadius, (int b) => pt.Distance(Graph.GetVertex(b)), (int vid) => !Graph.IsBoundaryVertex(vid) || vid == ignore_vid);
		if (keyValuePair.Key == PointHash.InvalidValue)
		{
			return -1;
		}
		return keyValuePair.Key;
	}

	protected bool find_intersecting_edges(ref Vector2d a, ref Vector2d b, List<Intersection> hits, double tol = 0.0)
	{
		int num = 0;
		Vector2d a2 = Vector2d.Zero;
		Vector2d b2 = Vector2d.Zero;
		foreach (int item in Graph.EdgeIndices())
		{
			Graph.GetEdgeV(item, ref a2, ref b2);
			int num2 = Segment2d.WhichSide(ref a, ref b, ref a2, tol);
			int num3 = Segment2d.WhichSide(ref a, ref b, ref b2, tol);
			if (num2 != num3 || num2 == 0)
			{
				IntrSegment2Segment2 intrSegment2Segment = new IntrSegment2Segment2(new Segment2d(a2, b2), new Segment2d(a, b));
				if (intrSegment2Segment.Find())
				{
					hits.Add(new Intersection
					{
						eid = item,
						sidex = num2,
						sidey = num3,
						intr = intrSegment2Segment
					});
					num++;
				}
			}
		}
		return num > 0;
	}
}
