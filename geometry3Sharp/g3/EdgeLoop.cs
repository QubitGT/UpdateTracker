using System;
using System.Collections.Generic;

namespace g3;

public class EdgeLoop
{
	public DMesh3 Mesh;

	public int[] Vertices;

	public int[] Edges;

	public int[] BowtieVertices;

	public int VertexCount => Vertices.Length;

	public int EdgeCount => Edges.Length;

	public EdgeLoop(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public EdgeLoop(DMesh3 mesh, int[] vertices, int[] edges, bool bCopyArrays)
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

	public EdgeLoop(EdgeLoop copy)
	{
		Mesh = copy.Mesh;
		Vertices = new int[copy.Vertices.Length];
		Array.Copy(copy.Vertices, Vertices, Vertices.Length);
		Edges = new int[copy.Edges.Length];
		Array.Copy(copy.Edges, Edges, Edges.Length);
		if (copy.BowtieVertices != null)
		{
			BowtieVertices = new int[copy.BowtieVertices.Length];
			Array.Copy(copy.BowtieVertices, BowtieVertices, BowtieVertices.Length);
		}
	}

	public static EdgeLoop FromEdges(DMesh3 mesh, IList<int> edges)
	{
		int[] array = new int[edges.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = edges[i];
		}
		int[] array2 = new int[array.Length];
		Index2i ev = mesh.GetEdgeV(array[0]);
		Index2i ev2 = ev;
		for (int j = 1; j < array.Length; j++)
		{
			Index2i ev3 = mesh.GetEdgeV(array[j % array.Length]);
			array2[j] = IndexUtil.find_shared_edge_v(ref ev2, ref ev3);
			ev2 = ev3;
		}
		array2[0] = IndexUtil.find_edge_other_v(ref ev, array2[1]);
		return new EdgeLoop(mesh, array2, array, bCopyArrays: false);
	}

	public static EdgeLoop FromVertices(DMesh3 mesh, IList<int> vertices)
	{
		int count = vertices.Count;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = vertices[i];
		}
		int num = count;
		int[] array2 = new int[num];
		for (int j = 0; j < num; j++)
		{
			array2[j] = mesh.FindEdge(array[j], array[(j + 1) % num]);
			if (array2[j] == -1)
			{
				throw new Exception("EdgeLoop.FromVertices: vertices are not connected by edge!");
			}
		}
		return new EdgeLoop(mesh, array, array2, bCopyArrays: false);
	}

	public static EdgeLoop FromVertices(DMesh3 mesh, IList<int> vertices, bool bAutoOrient = true)
	{
		int[] array = new int[vertices.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = vertices[i];
		}
		if (bAutoOrient)
		{
			int num = array[0];
			int num2 = array[1];
			int num3 = mesh.FindEdge(num, num2);
			if (mesh.IsBoundaryEdge(num3))
			{
				Index2i orientedBoundaryEdgeV = mesh.GetOrientedBoundaryEdgeV(num3);
				if (orientedBoundaryEdgeV.a == num2 && orientedBoundaryEdgeV.b == num)
				{
					Array.Reverse(array);
				}
			}
		}
		int[] array2 = new int[array.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			int vA = array[j];
			int vB = array[(j + 1) % array.Length];
			array2[j] = mesh.FindEdge(vA, vB);
			if (array2[j] == -1)
			{
				throw new Exception("EdgeLoop.FromVertices: invalid edge [" + vA + "," + vB + "]");
			}
		}
		return new EdgeLoop(mesh, array, array2, bCopyArrays: false);
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
		dCurve.Closed = true;
		return dCurve;
	}

	public bool CorrectOrientation()
	{
		int num = Vertices[0];
		int num2 = Vertices[1];
		int num3 = Mesh.FindEdge(num, num2);
		if (Mesh.IsBoundaryEdge(num3))
		{
			Index2i orientedBoundaryEdgeV = Mesh.GetOrientedBoundaryEdgeV(num3);
			if (orientedBoundaryEdgeV.a == num2 && orientedBoundaryEdgeV.b == num)
			{
				Reverse();
				return true;
			}
		}
		return false;
	}

	public void Reverse()
	{
		Array.Reverse(Vertices);
		Array.Reverse(Edges);
	}

	public bool IsInternalLoop()
	{
		int num = Vertices.Length;
		for (int i = 0; i < num; i++)
		{
			int eid = Mesh.FindEdge(Vertices[i], Vertices[(i + 1) % num]);
			if (Mesh.IsBoundaryEdge(eid))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsBoundaryLoop(DMesh3 testMesh = null)
	{
		DMesh3 dMesh = ((testMesh != null) ? testMesh : Mesh);
		int num = Vertices.Length;
		for (int i = 0; i < num; i++)
		{
			int eid = dMesh.FindEdge(Vertices[i], Vertices[(i + 1) % num]);
			if (!dMesh.IsBoundaryEdge(eid))
			{
				return false;
			}
		}
		return true;
	}

	public int FindVertexIndex(int vID)
	{
		int num = Vertices.Length;
		for (int i = 0; i < num; i++)
		{
			if (Vertices[i] == vID)
			{
				return i;
			}
		}
		return -1;
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

	public bool IsSameLoop(EdgeLoop Loop2, bool bReverse2 = false, double tolerance = 1E-08)
	{
		int num = Vertices.Length;
		int num2 = Loop2.Vertices.Length;
		if (num != num2)
		{
			return false;
		}
		DMesh3 mesh = Loop2.Mesh;
		int num3 = 0;
		int last_in_tol = -1;
		bool flag = false;
		while (!flag && num3 < num)
		{
			Vector3d vertex = Mesh.GetVertex(num3);
			if (Loop2.CountWithinTolerance(vertex, tolerance, out last_in_tol) == 1)
			{
				flag = true;
			}
			else
			{
				num3++;
			}
		}
		if (!flag)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			int num4 = (num3 + i) % num;
			int num5 = (bReverse2 ? MathUtil.WrapSignedIndex(last_in_tol - i, num2) : ((last_in_tol + i) % num2));
			Vector3d vertex2 = Mesh.GetVertex(Vertices[num4]);
			Vector3d vertex3 = mesh.GetVertex(Loop2.Vertices[num5]);
			if (vertex2.Distance(vertex3) > tolerance)
			{
				return false;
			}
		}
		return true;
	}

	public int[] GetVertexSpan(int starti, int count, int[] span, bool reverse = false)
	{
		int num = Vertices.Length;
		if (starti < 0 || starti >= num || count > num - 1)
		{
			return null;
		}
		if (reverse)
		{
			for (int i = 0; i < count; i++)
			{
				span[count - i - 1] = Vertices[(starti + i) % num];
			}
		}
		else
		{
			for (int j = 0; j < count; j++)
			{
				span[j] = Vertices[(starti + j) % num];
			}
		}
		return span;
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
					throw new Exception("EdgeLoop.CheckValidity: check failed");
				}
			};
			break;
		}
		action(Vertices.Length == Edges.Length);
		for (int i = 0; i < Edges.Length; i++)
		{
			Index2i edgeV = Mesh.GetEdgeV(Edges[i]);
			action(Mesh.IsVertex(edgeV.a));
			action(Mesh.IsVertex(edgeV.b));
			action(Mesh.FindEdge(edgeV.a, edgeV.b) != -1);
			action(Vertices[i] == edgeV.a || Vertices[i] == edgeV.b);
			action(Vertices[(i + 1) % Edges.Length] == edgeV.a || Vertices[(i + 1) % Edges.Length] == edgeV.b);
		}
		for (int j = 0; j < Vertices.Length; j++)
		{
			int num = Vertices[j];
			int num2 = Vertices[(j + 1) % Vertices.Length];
			action(Mesh.IsVertex(num));
			action(Mesh.IsVertex(num2));
			action(Mesh.FindEdge(num, num2) != -1);
			int num3 = 0;
			int num4 = Edges[j];
			int num5 = Edges[(j + 1) % Vertices.Length];
			foreach (int item in Mesh.VtxEdgesItr(num2))
			{
				if (item == num4 || item == num5)
				{
					num3++;
				}
			}
			action(num3 == 2);
		}
		return is_ok;
	}

	public static int[] VertexLoopToEdgeLoop(DMesh3 mesh, int[] vertex_loop)
	{
		int num = vertex_loop.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			int vA = vertex_loop[i];
			int vB = vertex_loop[(i + 1) % num];
			array[i] = mesh.FindEdge(vA, vB);
		}
		return array;
	}
}
