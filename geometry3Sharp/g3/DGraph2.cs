using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class DGraph2 : DGraph
{
	public static readonly Vector2d InvalidVertex = new Vector2d(double.MaxValue, 0.0);

	private DVector<double> vertices;

	private DVector<float> colors;

	private AxisAlignedBox2d cached_bounds;

	private int cached_bounds_timestamp = -1;

	public bool HasVertexColors => colors != null;

	public AxisAlignedBox2d CachedBounds
	{
		get
		{
			if (cached_bounds_timestamp != base.Timestamp)
			{
				cached_bounds = GetBounds();
				cached_bounds_timestamp = base.Timestamp;
			}
			return cached_bounds;
		}
	}

	public DGraph2()
	{
		vertices = new DVector<double>();
	}

	public DGraph2(DGraph2 copy)
	{
		vertices = new DVector<double>();
		AppendGraph(copy);
	}

	public Vector2d GetVertex(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return InvalidVertex;
		}
		return new Vector2d(vertices[2 * vID], vertices[2 * vID + 1]);
	}

	public void SetVertex(int vID, Vector2d vNewPos)
	{
		if (vertices_refcount.isValid(vID))
		{
			int num = 2 * vID;
			vertices[num] = vNewPos.x;
			vertices[num + 1] = vNewPos.y;
			updateTimeStamp(bShapeChange: true);
		}
	}

	public Vector3f GetVertexColor(int vID)
	{
		if (colors == null)
		{
			return Vector3f.One;
		}
		int num = 3 * vID;
		return new Vector3f(colors[num], colors[num + 1], colors[num + 2]);
	}

	public void SetVertexColor(int vID, Vector3f vNewColor)
	{
		if (HasVertexColors)
		{
			int num = 3 * vID;
			colors[num] = vNewColor.x;
			colors[num + 1] = vNewColor.y;
			colors[num + 2] = vNewColor.z;
			updateTimeStamp(bShapeChange: false);
		}
	}

	public bool GetEdgeV(int eID, ref Vector2d a, ref Vector2d b)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 2 * edges[3 * eID];
			a.x = vertices[num];
			a.y = vertices[num + 1];
			int num2 = 2 * edges[3 * eID + 1];
			b.x = vertices[num2];
			b.y = vertices[num2 + 1];
			return true;
		}
		return false;
	}

	public Segment2d GetEdgeSegment(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 2 * edges[3 * eID];
			int num2 = 2 * edges[3 * eID + 1];
			return new Segment2d(new Vector2d(vertices[num], vertices[num + 1]), new Vector2d(vertices[num2], vertices[num2 + 1]));
		}
		throw new Exception("DGraph2.GetEdgeSegment: invalid segment with id " + eID);
	}

	public Vector2d GetEdgeCenter(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 2 * edges[3 * eID];
			int num2 = 2 * edges[3 * eID + 1];
			return new Vector2d((vertices[num] + vertices[num2]) * 0.5, (vertices[num + 1] + vertices[num2 + 1]) * 0.5);
		}
		throw new Exception("DGraph2.GetEdgeCenter: invalid segment with id " + eID);
	}

	public int AppendVertex(Vector2d v)
	{
		return AppendVertex(v, Vector3f.One);
	}

	public int AppendVertex(Vector2d v, Vector3f c)
	{
		int num = append_vertex_internal();
		int num2 = 2 * num;
		vertices.insert(v[1], num2 + 1);
		vertices.insert(v[0], num2);
		if (colors != null)
		{
			num2 = 3 * num;
			colors.insert(c.z, num2 + 2);
			colors.insert(c.y, num2 + 1);
			colors.insert(c.x, num2);
		}
		return num;
	}

	public void AppendPolygon(Polygon2d poly, int gid = -1)
	{
		int v = -1;
		int num = -1;
		int vertexCount = poly.VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			int num2 = AppendVertex(poly[i]);
			if (num == -1)
			{
				v = num2;
			}
			else
			{
				AppendEdge(num, num2, gid);
			}
			num = num2;
		}
		AppendEdge(num, v, gid);
	}

	public void AppendPolygon(GeneralPolygon2d poly, int gid = -1)
	{
		AppendPolygon(poly.Outer, gid);
		foreach (Polygon2d hole in poly.Holes)
		{
			AppendPolygon(hole, gid);
		}
	}

	public void AppendPolyline(PolyLine2d poly, int gid = -1)
	{
		int v = -1;
		int vertexCount = poly.VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			int num = AppendVertex(poly[i]);
			if (i > 0)
			{
				AppendEdge(v, num, gid);
			}
			v = num;
		}
	}

	public void AppendGraph(DGraph2 graph, int gid = -1)
	{
		int[] array = new int[graph.MaxVertexID];
		foreach (int item in graph.VertexIndices())
		{
			array[item] = AppendVertex(graph.GetVertex(item));
		}
		foreach (int item2 in graph.EdgeIndices())
		{
			Index2i edgeV = graph.GetEdgeV(item2);
			int gid2 = ((gid == -1) ? graph.GetEdgeGroup(item2) : gid);
			AppendEdge(array[edgeV.a], array[edgeV.b], gid2);
		}
	}

	public void EnableVertexColors(Vector3f initial_color)
	{
		if (!HasVertexColors)
		{
			colors = new DVector<float>();
			int maxVertexID = base.MaxVertexID;
			colors.resize(3 * maxVertexID);
			for (int i = 0; i < maxVertexID; i++)
			{
				int num = 3 * i;
				colors[num] = initial_color.x;
				colors[num + 1] = initial_color.y;
				colors[num + 2] = initial_color.z;
			}
		}
	}

	public void DiscardVertexColors()
	{
		colors = null;
	}

	public IEnumerable<Vector2d> Vertices()
	{
		foreach (int item in vertices_refcount)
		{
			int num2 = 2 * item;
			yield return new Vector2d(vertices[num2], vertices[num2 + 1]);
		}
	}

	public int[] SortedVtxEdges(int vID)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return null;
		}
		List<int> list = vertex_edges[vID];
		int count = list.Count;
		int[] array = new int[count];
		double[] array2 = new double[count];
		Vector2d vector2d = new Vector2d(vertices[2 * vID], vertices[2 * vID + 1]);
		for (int i = 0; i < count; i++)
		{
			int num = edge_other_v(list[i], vID);
			double x = vertices[2 * num] - vector2d.x;
			double y = vertices[2 * num + 1] - vector2d.y;
			array2[i] = MathUtil.Atan2Positive(y, x);
			array[i] = list[i];
		}
		Array.Sort(array2, array);
		return array;
	}

	public AxisAlignedBox2d GetBounds()
	{
		double num = 0.0;
		double num2 = 0.0;
		IEnumerator enumerator = vertices_refcount.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				int num3 = (int)enumerator.Current;
				num = vertices[2 * num3];
				num2 = vertices[2 * num3 + 1];
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		double num4 = num;
		double num5 = num;
		double num6 = num2;
		double num7 = num2;
		foreach (int item in vertices_refcount)
		{
			num = vertices[2 * item];
			num2 = vertices[2 * item + 1];
			if (num < num4)
			{
				num4 = num;
			}
			else if (num > num5)
			{
				num5 = num;
			}
			if (num2 < num6)
			{
				num6 = num2;
			}
			else if (num2 > num7)
			{
				num7 = num2;
			}
		}
		return new AxisAlignedBox2d(num4, num6, num5, num7);
	}

	public double OpeningAngle(int vID, double invalidValue = double.MaxValue)
	{
		if (!vertices_refcount.isValid(vID))
		{
			return invalidValue;
		}
		List<int> list = vertex_edges[vID];
		if (list.Count != 2)
		{
			return invalidValue;
		}
		int num = edge_other_v(list[0], vID);
		int num2 = edge_other_v(list[1], vID);
		Vector2d vector2d = new Vector2d(vertices[2 * vID], vertices[2 * vID + 1]);
		Vector2d v = new Vector2d(vertices[2 * num], vertices[2 * num + 1]);
		Vector2d v2 = new Vector2d(vertices[2 * num2], vertices[2 * num2 + 1]);
		v -= vector2d;
		if (v.Normalize() == 0.0)
		{
			return invalidValue;
		}
		v2 -= vector2d;
		if (v2.Normalize() == 0.0)
		{
			return invalidValue;
		}
		return Vector2d.AngleD(v, v2);
	}

	protected override int append_new_split_vertex(int a, int b)
	{
		Vector2d v = 0.5 * (GetVertex(a) + GetVertex(b));
		Vector3f c = (HasVertexColors ? (0.5f * (GetVertexColor(a) + GetVertexColor(b))) : Vector3f.One);
		return AppendVertex(v, c);
	}

	protected override void subclass_validity_checks(Action<bool> CheckOrFailF)
	{
		foreach (int item in VertexIndices())
		{
			Vector2d vertex = GetVertex(item);
			CheckOrFailF(!double.IsNaN(vertex.LengthSquared));
			CheckOrFailF(!double.IsInfinity(vertex.LengthSquared));
		}
	}
}
