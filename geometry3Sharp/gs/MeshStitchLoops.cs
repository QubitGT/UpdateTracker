using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class MeshStitchLoops
{
	private struct span
	{
		public Interval1i span0;

		public Interval1i span1;
	}

	public DMesh3 Mesh;

	public EdgeLoop Loop0;

	public EdgeLoop Loop1;

	public bool TrustLoopOrientations = true;

	public SetGroupBehavior Group = SetGroupBehavior.AutoGenerate;

	private List<span> spans = new List<span>();

	public MeshStitchLoops(DMesh3 mesh, EdgeLoop l0, EdgeLoop l1)
	{
		Mesh = mesh;
		Loop0 = new EdgeLoop(l0);
		Loop1 = new EdgeLoop(l1);
		span item = new span
		{
			span0 = new Interval1i(0, 0),
			span1 = new Interval1i(0, 0)
		};
		spans.Add(item);
	}

	public void AddKnownCorrespondences(int[] verts0, int[] verts1)
	{
		int num = verts0.Length;
		if (num != verts1.Length)
		{
			throw new Exception("MeshStitchLoops.AddKnownCorrespondence: lengths not the same!");
		}
		List<Index2i> list = new List<Index2i>();
		for (int i = 0; i < num; i++)
		{
			int ii = Loop0.FindVertexIndex(verts0[i]);
			int jj = Loop1.FindVertexIndex(verts1[i]);
			list.Add(new Index2i(ii, jj));
		}
		list.Sort((Index2i pair1, Index2i pair2) => pair1.a.CompareTo(pair2.a));
		List<span> list2 = new List<span>();
		for (int j = 0; j < list.Count; j++)
		{
			Index2i index2i = list[j];
			Index2i index2i2 = list[(j + 1) % list.Count];
			span span = default(span);
			span.span0 = new Interval1i(index2i.a, index2i2.a);
			span.span1 = new Interval1i(index2i.b, index2i2.b);
			span item = span;
			list2.Add(item);
		}
		spans = list2;
	}

	public bool Stitch()
	{
		if (spans.Count == 1)
		{
			throw new Exception("MeshStitchLoops.Stitch: blind stitching not supported yet...");
		}
		int groupID = Group.GetGroupID(Mesh);
		bool result = true;
		int count = spans.Count;
		for (int i = 0; i < count; i++)
		{
			span s = spans[i];
			if (!stitch_span_simple(s, groupID))
			{
				result = false;
			}
		}
		return result;
	}

	private bool stitch_span_simple(span s, int gid)
	{
		bool result = true;
		int num = Loop0.Vertices.Length;
		int num2 = Loop1.Vertices.Length;
		int num3 = s.span0.a;
		int b = s.span0.b;
		int num4 = s.span1.a;
		int b2 = s.span1.b;
		while (num3 != b && num4 != b2)
		{
			int num5 = (num3 + 1) % num;
			int num6 = (num4 + 1) % num2;
			int b3 = Loop0.Vertices[num3];
			int num7 = Loop0.Vertices[num5];
			int num8 = Loop1.Vertices[num4];
			int b4 = Loop1.Vertices[num6];
			if (!add_triangle(num7, b3, num8, gid))
			{
				result = false;
			}
			if (!add_triangle(num8, b4, num7, gid))
			{
				result = false;
			}
			num3 = num5;
			num4 = num6;
		}
		int c = Loop1.Vertices[num4];
		while (num3 != b)
		{
			int num9 = (num3 + 1) % num;
			int b5 = Loop0.Vertices[num3];
			int a = Loop0.Vertices[num9];
			if (!add_triangle(a, b5, c, gid))
			{
				result = false;
			}
			num3 = num9;
		}
		int c2 = Loop0.Vertices[num3];
		while (num4 != b2)
		{
			int num10 = (num4 + 1) % num2;
			int a2 = Loop1.Vertices[num4];
			int b6 = Loop1.Vertices[num10];
			if (!add_triangle(a2, b6, c2, gid))
			{
				result = false;
			}
			num4 = num10;
		}
		return result;
	}

	private bool add_triangle(int a, int b, int c, int gid)
	{
		int num = -1;
		if (!TrustLoopOrientations)
		{
			int eID = Mesh.FindEdge(a, b);
			Index2i orientedBoundaryEdgeV = Mesh.GetOrientedBoundaryEdgeV(eID);
			num = Mesh.AppendTriangle(orientedBoundaryEdgeV.b, orientedBoundaryEdgeV.a, c, gid);
		}
		else
		{
			num = Mesh.AppendTriangle(a, b, c, gid);
		}
		return num >= 0;
	}
}
