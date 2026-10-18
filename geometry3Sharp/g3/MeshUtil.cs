using System;
using System.Collections.Generic;

namespace g3;

public static class MeshUtil
{
	public static Vector3d UniformSmooth(DMesh3 mesh, int vID, double t)
	{
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d centroid = Vector3d.Zero;
		mesh.VtxOneRingCentroid(vID, ref centroid);
		double num = 1.0 - t;
		vertex.x = num * vertex.x + t * centroid.x;
		vertex.y = num * vertex.y + t * centroid.y;
		vertex.z = num * vertex.z + t * centroid.z;
		return vertex;
	}

	public static Vector3d MeanValueSmooth(DMesh3 mesh, int vID, double t)
	{
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d vector3d = MeshWeights.MeanValueCentroid(mesh, vID);
		return (1.0 - t) * vertex + t * vector3d;
	}

	public static Vector3d CotanSmooth(DMesh3 mesh, int vID, double t)
	{
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d vector3d = MeshWeights.CotanCentroid(mesh, vID);
		return (1.0 - t) * vertex + t * vector3d;
	}

	public static void ScaleMesh(DMesh3 mesh, Frame3f f, Vector3f vScale)
	{
		foreach (int item in mesh.VertexIndices())
		{
			Vector3f v = (Vector3f)mesh.GetVertex(item);
			Vector3f v2 = f.ToFrameP(ref v) * vScale;
			Vector3d vNewPos = f.FromFrameP(ref v2);
			mesh.SetVertex(item, vNewPos);
		}
	}

	public static double OpeningAngleD(DMesh3 mesh, int eid)
	{
		Index2i edgeT = mesh.GetEdgeT(eid);
		if (edgeT[1] == -1)
		{
			return double.MaxValue;
		}
		Vector3d triNormal = mesh.GetTriNormal(edgeT[0]);
		Vector3d triNormal2 = mesh.GetTriNormal(edgeT[1]);
		return Vector3d.AngleD(triNormal, triNormal2);
	}

	public static double DiscreteGaussCurvature(DMesh3 mesh, int vid)
	{
		double num = 0.0;
		foreach (int item in mesh.VtxTrianglesItr(vid))
		{
			Index3i tri_verts = mesh.GetTriangle(item);
			int i = IndexUtil.find_tri_index(vid, ref tri_verts);
			num += mesh.GetTriInternalAngleR(item, i);
		}
		return num - Math.PI * 2.0;
	}

	public static bool CheckIfCollapseCreatesFlip(DMesh3 mesh, int edgeID, Vector3d newv)
	{
		Index4i edge = mesh.GetEdge(edgeID);
		int c = edge.c;
		int d = edge.d;
		for (int i = 0; i < 2; i++)
		{
			int num = edge[i];
			int num2 = edge[(i + 1) % 2];
			foreach (int item in mesh.VtxTrianglesItr(num))
			{
				if (item == c || item == d)
				{
					continue;
				}
				Index3i triangle = mesh.GetTriangle(item);
				if (triangle.a == num2 || triangle.b == num2 || triangle.c == num2)
				{
					return true;
				}
				Vector3d vertex = mesh.GetVertex(triangle.a);
				Vector3d vertex2 = mesh.GetVertex(triangle.b);
				Vector3d vertex3 = mesh.GetVertex(triangle.c);
				Vector3d vector3d = (vertex2 - vertex).Cross(vertex3 - vertex);
				double num3 = 0.0;
				if (triangle.a == num)
				{
					Vector3d v = (vertex2 - newv).Cross(vertex3 - newv);
					num3 = vector3d.Dot(v);
				}
				else if (triangle.b == num)
				{
					Vector3d v2 = (newv - vertex).Cross(vertex3 - vertex);
					num3 = vector3d.Dot(v2);
				}
				else
				{
					if (triangle.c != num)
					{
						throw new Exception("should never be here!");
					}
					Vector3d v3 = (vertex2 - vertex).Cross(newv - vertex);
					num3 = vector3d.Dot(v3);
				}
				if (num3 <= 0.0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckIfEdgeFlipCreatesFlip(DMesh3 mesh, int eID, double flip_dot_tol = 0.0)
	{
		Index4i edge = mesh.GetEdge(eID);
		Index2i edgeOpposingV = mesh.GetEdgeOpposingV(eID);
		int a = edge.a;
		int b = edge.b;
		int a2 = edgeOpposingV.a;
		int b2 = edgeOpposingV.b;
		int c = edge.c;
		Vector3d v = mesh.GetVertex(a2);
		Vector3d v2 = mesh.GetVertex(b2);
		Index3i tri_verts = mesh.GetTriangle(c);
		int a3 = a;
		int b3 = b;
		IndexUtil.orient_tri_edge(ref a3, ref b3, ref tri_verts);
		Vector3d v3 = mesh.GetVertex(a3);
		Vector3d v4 = mesh.GetVertex(b3);
		Vector3d n = MathUtil.FastNormalDirection(ref v3, ref v4, ref v);
		Vector3d n2 = MathUtil.FastNormalDirection(ref v4, ref v3, ref v2);
		Vector3d n3 = MathUtil.FastNormalDirection(ref v, ref v2, ref v4);
		if (edge_flip_metric(ref n, ref n3, flip_dot_tol) <= flip_dot_tol || edge_flip_metric(ref n2, ref n3, flip_dot_tol) <= flip_dot_tol)
		{
			return true;
		}
		Vector3d n4 = MathUtil.FastNormalDirection(ref v2, ref v, ref v3);
		if (edge_flip_metric(ref n, ref n4, flip_dot_tol) <= flip_dot_tol || edge_flip_metric(ref n2, ref n4, flip_dot_tol) <= flip_dot_tol)
		{
			return true;
		}
		return false;
	}

	private static double edge_flip_metric(ref Vector3d n0, ref Vector3d n1, double flip_dot_tol)
	{
		if (flip_dot_tol != 0.0)
		{
			return n0.Normalized.Dot(n1.Normalized);
		}
		return n0.Dot(n1);
	}

	public static void GetEdgeFlipTris(DMesh3 mesh, int eID, out Index3i orig_t0, out Index3i orig_t1, out Index3i flip_t0, out Index3i flip_t1)
	{
		Index4i edge = mesh.GetEdge(eID);
		Index2i edgeOpposingV = mesh.GetEdgeOpposingV(eID);
		int a = edge.a;
		int b = edge.b;
		int a2 = edgeOpposingV.a;
		int b2 = edgeOpposingV.b;
		int c = edge.c;
		Index3i tri_verts = mesh.GetTriangle(c);
		int a3 = a;
		int b3 = b;
		IndexUtil.orient_tri_edge(ref a3, ref b3, ref tri_verts);
		orig_t0 = new Index3i(a3, b3, a2);
		orig_t1 = new Index3i(b3, a3, b2);
		flip_t0 = new Index3i(a2, b2, b3);
		flip_t1 = new Index3i(b2, a2, a3);
	}

	public static void GetEdgeFlipNormals(DMesh3 mesh, int eID, out Vector3d n1, out Vector3d n2, out Vector3d on1, out Vector3d on2)
	{
		Index4i edge = mesh.GetEdge(eID);
		Index2i edgeOpposingV = mesh.GetEdgeOpposingV(eID);
		int a = edge.a;
		int b = edge.b;
		int a2 = edgeOpposingV.a;
		int b2 = edgeOpposingV.b;
		int c = edge.c;
		Vector3d v = mesh.GetVertex(a2);
		Vector3d v2 = mesh.GetVertex(b2);
		Index3i tri_verts = mesh.GetTriangle(c);
		int a3 = a;
		int b3 = b;
		IndexUtil.orient_tri_edge(ref a3, ref b3, ref tri_verts);
		Vector3d v3 = mesh.GetVertex(a3);
		Vector3d v4 = mesh.GetVertex(b3);
		n1 = MathUtil.Normal(ref v3, ref v4, ref v);
		n2 = MathUtil.Normal(ref v4, ref v3, ref v2);
		on1 = MathUtil.Normal(ref v, ref v2, ref v4);
		on2 = MathUtil.Normal(ref v2, ref v, ref v3);
	}

	public static DCurve3 ExtractLoopV(IMesh mesh, IEnumerable<int> vertices)
	{
		DCurve3 dCurve = new DCurve3();
		foreach (int vertex in vertices)
		{
			dCurve.AppendVertex(mesh.GetVertex(vertex));
		}
		dCurve.Closed = true;
		return dCurve;
	}

	public static DCurve3 ExtractLoopV(IMesh mesh, int[] vertices)
	{
		DCurve3 dCurve = new DCurve3();
		for (int i = 0; i < vertices.Length; i++)
		{
			dCurve.AppendVertex(mesh.GetVertex(vertices[i]));
		}
		dCurve.Closed = true;
		return dCurve;
	}
}
