using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class DGraph3 : DGraph
{
	public static readonly Vector3d InvalidVertex = new Vector3d(double.MaxValue, 0.0, 0.0);

	private DVector<double> vertices;

	private DVector<float> colors;

	private AxisAlignedBox3d cached_bounds;

	private int cached_bounds_timestamp = -1;

	public bool HasVertexColors => colors != null;

	public AxisAlignedBox3d CachedBounds
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

	public DGraph3()
	{
		vertices = new DVector<double>();
	}

	public DGraph3(DGraph3 copy)
	{
		vertices = new DVector<double>();
		AppendGraph(copy);
	}

	public Vector3d GetVertex(int vID)
	{
		int num = 3 * vID;
		return new Vector3d(vertices[num], vertices[num + 1], vertices[num + 2]);
	}

	public void SetVertex(int vID, Vector3d vNewPos)
	{
		if (vertices_refcount.isValid(vID))
		{
			int num = 3 * vID;
			vertices[num] = vNewPos.x;
			vertices[num + 1] = vNewPos.y;
			vertices[num + 2] = vNewPos.z;
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

	public bool GetEdgeV(int eID, ref Vector3d a, ref Vector3d b)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 3 * edges[3 * eID];
			a.x = vertices[num];
			a.y = vertices[num + 1];
			a.z = vertices[num + 2];
			int num2 = 3 * edges[3 * eID + 1];
			b.x = vertices[num2];
			b.y = vertices[num2 + 1];
			b.z = vertices[num2 + 2];
			return true;
		}
		return false;
	}

	public Segment3d GetEdgeSegment(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 3 * edges[3 * eID];
			int num2 = 3 * edges[3 * eID + 1];
			return new Segment3d(new Vector3d(vertices[num], vertices[num + 1], vertices[num + 2]), new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]));
		}
		throw new Exception("DGraph3.GetEdgeSegment: invalid segment with id " + eID);
	}

	public Vector3d GetEdgeCenter(int eID)
	{
		if (edges_refcount.isValid(eID))
		{
			int num = 3 * edges[3 * eID];
			int num2 = 3 * edges[3 * eID + 1];
			return new Vector3d((vertices[num] + vertices[num2]) * 0.5, (vertices[num + 1] + vertices[num2 + 1]) * 0.5, (vertices[num + 2] + vertices[num2 + 2]) * 0.5);
		}
		throw new Exception("DGraph3.GetEdgeCenter: invalid segment with id " + eID);
	}

	public IEnumerable<Segment3d> Segments()
	{
		foreach (int item in edges_refcount)
		{
			yield return GetEdgeSegment(item);
		}
	}

	public int AppendVertex(Vector3d v)
	{
		return AppendVertex(v, Vector3f.One);
	}

	public int AppendVertex(Vector3d v, Vector3f c)
	{
		int num = append_vertex_internal();
		int num2 = 3 * num;
		vertices.insert(v[2], num2 + 2);
		vertices.insert(v[1], num2 + 1);
		vertices.insert(v[0], num2);
		if (colors != null)
		{
			colors.insert(c.z, num2 + 2);
			colors.insert(c.y, num2 + 1);
			colors.insert(c.x, num2);
		}
		return num;
	}

	public void AppendGraph(DGraph3 graph, int gid = -1)
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

	public IEnumerable<Vector3d> Vertices()
	{
		foreach (int item in vertices_refcount)
		{
			int num2 = 3 * item;
			yield return new Vector3d(vertices[num2], vertices[num2 + 1], vertices[num2 + 2]);
		}
	}

	public AxisAlignedBox3d GetBounds()
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		IEnumerator enumerator = vertices_refcount.GetEnumerator();
		try
		{
			if (enumerator.MoveNext())
			{
				int num4 = (int)enumerator.Current;
				num = vertices[3 * num4];
				num2 = vertices[3 * num4 + 1];
				num3 = vertices[3 * num4 + 2];
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
		double num5 = num;
		double num6 = num;
		double num7 = num2;
		double num8 = num2;
		double num9 = num3;
		double num10 = num3;
		foreach (int item in vertices_refcount)
		{
			int num12 = 3 * item;
			num = vertices[num12];
			num2 = vertices[num12 + 1];
			num3 = vertices[num12 + 2];
			if (num < num5)
			{
				num5 = num;
			}
			else if (num > num6)
			{
				num6 = num;
			}
			if (num2 < num7)
			{
				num7 = num2;
			}
			else if (num2 > num8)
			{
				num8 = num2;
			}
			if (num3 < num9)
			{
				num9 = num3;
			}
			else if (num3 > num10)
			{
				num10 = num3;
			}
		}
		return new AxisAlignedBox3d(num5, num7, num9, num6, num8, num10);
	}

	protected override int append_new_split_vertex(int a, int b)
	{
		Vector3d v = 0.5 * (GetVertex(a) + GetVertex(b));
		Vector3f c = (HasVertexColors ? (0.5f * (GetVertexColor(a) + GetVertexColor(b))) : Vector3f.One);
		return AppendVertex(v, c);
	}

	protected override void subclass_validity_checks(Action<bool> CheckOrFailF)
	{
		foreach (int item in VertexIndices())
		{
			Vector3d vertex = GetVertex(item);
			CheckOrFailF(!double.IsNaN(vertex.LengthSquared));
			CheckOrFailF(!double.IsInfinity(vertex.LengthSquared));
		}
	}
}
