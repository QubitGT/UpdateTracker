using System;
using System.Collections.Generic;

namespace g3;

public class EdgeSpan
{
	public DMesh3 Mesh;

	public int[] Vertices;

	public int[] Edges;

	public int[] BowtieVertices;

	public int VertexCount => Vertices.Length;

	public int EdgeCount => Edges.Length;

	public EdgeSpan(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public EdgeSpan(DMesh3 mesh, int[] vertices, int[] edges, bool bCopyArrays)
	{
		Mesh = mesh;
		if (bCopyArrays)
		{
			Vertices = new int[vertices.Length];
			Array.Copy(vertices, Vertices, Vertices.Length);
			Edges = new int[edges.Length];
			Array.Copy(edges, Edges, Edges.Length);
		}
		else
		{
			Vertices = vertices;
			Edges = edges;
		}
	}

	public static EdgeSpan FromEdges(DMesh3 mesh, IList<int> edges)
	{
		int[] array = new int[edges.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = edges[i];
		}
		int[] array2 = new int[array.Length + 1];
		Index2i ev = mesh.GetEdgeV(array[0]);
		Index2i ev2 = ev;
		if (array.Length > 1)
		{
			for (int j = 1; j < array.Length; j++)
			{
				Index2i ev3 = mesh.GetEdgeV(array[j]);
				array2[j] = IndexUtil.find_shared_edge_v(ref ev2, ref ev3);
				ev2 = ev3;
			}
			array2[0] = IndexUtil.find_edge_other_v(ref ev, array2[1]);
			array2[^1] = IndexUtil.find_edge_other_v(ev2, array2[^2]);
		}
		else
		{
			array2[0] = ev[0];
			array2[1] = ev[1];
		}
		return new EdgeSpan(mesh, array2, array, bCopyArrays: false);
	}

	public static EdgeSpan FromVertices(DMesh3 mesh, IList<int> vertices)
	{
		int count = vertices.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = vertices[i];
		}
		int num = count - 1;
		int[] array2 = new int[num];
		for (int j = 0; j < num; j++)
		{
			array2[j] = mesh.FindEdge(array[j], array[j + 1]);
			if (array2[j] == -1)
			{
				throw new Exception("EdgeSpan.FromVertices: vertices are not connected by edge!");
			}
		}
		return new EdgeSpan(mesh, array, array2, bCopyArrays: false);
	}

	public Vector3d GetVertex(int i)
	{
		return Mesh.GetVertex(Vertices[i]);
	}

	public AxisAlignedBox3d GetBounds()
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		for (int i = 0; i < Vertices.Length; i++)
		{
			empty.Contain(Mesh.GetVertex(Vertices[i]));
		}
		return empty;
	}

	public DCurve3 ToCurve(DMesh3 sourceMesh = null)
	{
		if (sourceMesh == null)
		{
			sourceMesh = Mesh;
		}
		DCurve3 dCurve = MeshUtil.ExtractLoopV(sourceMesh, Vertices);
		dCurve.Closed = false;
		return dCurve;
	}

	public bool IsInternalSpan()
	{
		int num = Vertices.Length;
		for (int i = 0; i < num - 1; i++)
		{
			int eid = Mesh.FindEdge(Vertices[i], Vertices[i + 1]);
			if (Mesh.IsBoundaryEdge(eid))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsBoundarySpan(DMesh3 testMesh = null)
	{
		DMesh3 dMesh = ((testMesh != null) ? testMesh : Mesh);
		int num = Vertices.Length;
		for (int i = 0; i < num - 1; i++)
		{
			int eid = dMesh.FindEdge(Vertices[i], Vertices[i + 1]);
			if (!dMesh.IsBoundaryEdge(eid))
			{
				return false;
			}
		}
		return true;
	}

	public int FindNearestVertex(Vector3d v)
	{
		int result = -1;
		double num = double.MaxValue;
		int num2 = Vertices.Length;
		for (int i = 0; i < num2; i++)
		{
			Vector3d vertex = Mesh.GetVertex(Vertices[i]);
			double num3 = v.DistanceSquared(vertex);
			if (num3 < num)
			{
				num = num3;
				result = i;
			}
		}
		return result;
	}

	public int CountWithinTolerance(Vector3d v, double tol, out int last_in_tol)
	{
		last_in_tol = -1;
		int num = 0;
		int num2 = Vertices.Length;
		for (int i = 0; i < num2; i++)
		{
			Vector3d vertex = Mesh.GetVertex(Vertices[i]);
			if (v.Distance(vertex) < tol)
			{
				num++;
				last_in_tol = i;
			}
		}
		return num;
	}

	public bool IsSameSpan(EdgeSpan Spanw, bool bReverse2 = false, double tolerance = 1E-08)
	{
		throw new NotImplementedException("todo!");
	}

	public bool CheckValidity(FailMode eFailMode = FailMode.Throw)
	{
		bool is_ok = true;
		Action<bool> action = delegate(bool b)
		{
			is_ok &= b;
		};
		switch (eFailMode)
		{
		case FailMode.DebugAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.gDevAssert:
			action = delegate(bool b)
			{
				is_ok &= b;
			};
			break;
		case FailMode.Throw:
			action = delegate(bool b)
			{
				if (!b)
				{
					throw new Exception("EdgeSpan.CheckValidity: check failed");
				}
			};
			break;
		}
		action(Vertices.Length == Edges.Length + 1);
		for (int i = 0; i < Edges.Length; i++)
		{
			Index2i edgeV = Mesh.GetEdgeV(Edges[i]);
			action(Mesh.IsVertex(edgeV.a));
			action(Mesh.IsVertex(edgeV.b));
			action(Mesh.FindEdge(edgeV.a, edgeV.b) != -1);
			action(Vertices[i] == edgeV.a || Vertices[i] == edgeV.b);
			action(Vertices[i + 1] == edgeV.a || Vertices[i + 1] == edgeV.b);
		}
		for (int j = 0; j < Vertices.Length - 1; j++)
		{
			int num = Vertices[j];
			int num2 = Vertices[j + 1];
			action(Mesh.IsVertex(num));
			action(Mesh.IsVertex(num2));
			action(Mesh.FindEdge(num, num2) != -1);
			if (j >= Vertices.Length - 2)
			{
				continue;
			}
			int num3 = 0;
			int num4 = Edges[j];
			int num5 = Edges[j + 1];
			foreach (int item in Mesh.VtxEdgesItr(num2))
			{
				if (item == num4 || item == num5)
				{
					num3++;
				}
			}
			action(num3 == 2);
		}
		return true;
	}

	public static int[] VerticesToEdges(DMesh3 mesh, int[] vertex_span)
	{
		int num = vertex_span.Length;
		int[] array = new int[num - 1];
		for (int i = 0; i < num - 1; i++)
		{
			int vA = vertex_span[i];
			int vB = vertex_span[i + 1];
			array[i] = mesh.FindEdge(vA, vB);
		}
		return array;
	}
}
