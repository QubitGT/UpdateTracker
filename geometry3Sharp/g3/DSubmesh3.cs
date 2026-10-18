using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class DSubmesh3
{
	public DMesh3 BaseMesh;

	public DMesh3 SubMesh;

	public MeshComponents WantComponents = MeshComponents.All;

	public bool ComputeTriMaps;

	public int OverrideGroupID = -1;

	public IndexFlagSet BaseSubmeshV;

	public IndexMap BaseToSubV;

	public DVector<int> SubToBaseV;

	public IndexMap BaseToSubT;

	public DVector<int> SubToBaseT;

	public IndexHashSet BaseBorderE;

	public IndexHashSet BaseBoundaryE;

	public IndexHashSet BaseBorderV;

	public DSubmesh3(DMesh3 mesh, int[] subTriangles)
	{
		BaseMesh = mesh;
		compute(subTriangles, subTriangles.Length);
	}

	public DSubmesh3(DMesh3 mesh, IEnumerable<int> subTriangles, int nTriEstimate = 0)
	{
		BaseMesh = mesh;
		compute(subTriangles, nTriEstimate);
	}

	public DSubmesh3(DMesh3 mesh)
	{
		BaseMesh = mesh;
	}

	public void Compute(int[] subTriangles)
	{
		compute(subTriangles, subTriangles.Length);
	}

	public void Compute(IEnumerable<int> subTriangles, int nTriEstimate = 0)
	{
		compute(subTriangles, nTriEstimate);
	}

	public int MapVertexToSubmesh(int base_vID)
	{
		return BaseToSubV[base_vID];
	}

	public int MapVertexToBaseMesh(int sub_vID)
	{
		if (sub_vID < SubToBaseV.Length)
		{
			return SubToBaseV[sub_vID];
		}
		return -1;
	}

	public Index2i MapVerticesToSubmesh(Index2i v)
	{
		return new Index2i(BaseToSubV[v.a], BaseToSubV[v.b]);
	}

	public Index2i MapVerticesToBaseMesh(Index2i v)
	{
		return new Index2i(MapVertexToBaseMesh(v.a), MapVertexToBaseMesh(v.b));
	}

	public void MapVerticesToSubmesh(int[] vertices)
	{
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = BaseToSubV[vertices[i]];
		}
	}

	public int MapEdgeToSubmesh(int base_eid)
	{
		Index2i edgeV = BaseMesh.GetEdgeV(base_eid);
		Index2i index2i = MapVerticesToSubmesh(edgeV);
		return SubMesh.FindEdge(index2i.a, index2i.b);
	}

	public void MapEdgesToSubmesh(int[] edges)
	{
		for (int i = 0; i < edges.Length; i++)
		{
			edges[i] = MapEdgeToSubmesh(edges[i]);
		}
	}

	public int MapEdgeToBaseMesh(int sub_eid)
	{
		Index2i edgeV = SubMesh.GetEdgeV(sub_eid);
		Index2i index2i = MapVerticesToBaseMesh(edgeV);
		return BaseMesh.FindEdge(index2i.a, index2i.b);
	}

	public int MapTriangleToSubmesh(int base_tID)
	{
		if (!ComputeTriMaps)
		{
			throw new InvalidOperationException("DSubmesh3.MapTriangleToSubmesh: must set ComputeTriMaps = true!");
		}
		return BaseToSubT[base_tID];
	}

	public int MapTriangleToBaseMesh(int sub_tID)
	{
		if (!ComputeTriMaps)
		{
			throw new InvalidOperationException("DSubmesh3.MapTriangleToBaseMesh: must set ComputeTriMaps = true!");
		}
		if (sub_tID < SubToBaseT.Length)
		{
			return SubToBaseT[sub_tID];
		}
		return -1;
	}

	public void MapTrianglesToSubmesh(int[] triangles)
	{
		if (!ComputeTriMaps)
		{
			throw new InvalidOperationException("DSubmesh3.MapTrianglesToSubmesh: must set ComputeTriMaps = true!");
		}
		for (int i = 0; i < triangles.Length; i++)
		{
			triangles[i] = BaseToSubT[triangles[i]];
		}
	}

	public void ComputeBoundaryInfo(int[] subTriangles)
	{
		ComputeBoundaryInfo(subTriangles, subTriangles.Length);
	}

	public void ComputeBoundaryInfo(IEnumerable<int> triangles, int tri_count_est)
	{
		IndexFlagSet indexFlagSet = new IndexFlagSet(BaseMesh.MaxTriangleID, tri_count_est);
		foreach (int triangle in triangles)
		{
			indexFlagSet[triangle] = true;
		}
		BaseBorderV = new IndexHashSet();
		BaseBorderE = new IndexHashSet();
		BaseBoundaryE = new IndexHashSet();
		foreach (int triangle2 in triangles)
		{
			Index3i triEdges = BaseMesh.GetTriEdges(triangle2);
			for (int i = 0; i < 3; i++)
			{
				int num = triEdges[i];
				Index2i edgeT = BaseMesh.GetEdgeT(num);
				if (edgeT.b == -1)
				{
					BaseBoundaryE[num] = true;
				}
				else if (indexFlagSet[edgeT.a] != indexFlagSet[edgeT.b])
				{
					BaseBorderE[num] = true;
					Index2i edgeV = BaseMesh.GetEdgeV(num);
					BaseBorderV[edgeV.a] = true;
					BaseBorderV[edgeV.b] = true;
				}
			}
		}
	}

	private void compute(IEnumerable<int> triangles, int tri_count_est)
	{
		int subsetCountEst = tri_count_est / 2;
		SubMesh = new DMesh3(BaseMesh.Components & WantComponents);
		BaseSubmeshV = new IndexFlagSet(BaseMesh.MaxVertexID, subsetCountEst);
		BaseToSubV = new IndexMap(BaseMesh.MaxVertexID, subsetCountEst);
		SubToBaseV = new DVector<int>();
		if (ComputeTriMaps)
		{
			BaseToSubT = new IndexMap(BaseMesh.MaxTriangleID, tri_count_est);
			SubToBaseT = new DVector<int>();
		}
		foreach (int triangle2 in triangles)
		{
			if (!BaseMesh.IsTriangle(triangle2))
			{
				throw new Exception("DSubmesh3.compute: triangle " + triangle2 + " does not exist in BaseMesh!");
			}
			Index3i triangle = BaseMesh.GetTriangle(triangle2);
			Index3i zero = Index3i.Zero;
			int gid = BaseMesh.GetTriangleGroup(triangle2);
			for (int i = 0; i < 3; i++)
			{
				int num = triangle[i];
				int num2 = -1;
				if (!BaseSubmeshV[num])
				{
					num2 = SubMesh.AppendVertex(BaseMesh, num);
					BaseSubmeshV[num] = true;
					BaseToSubV[num] = num2;
					SubToBaseV.insert(num, num2);
				}
				else
				{
					num2 = BaseToSubV[num];
				}
				zero[i] = num2;
			}
			if (OverrideGroupID >= 0)
			{
				gid = OverrideGroupID;
			}
			int num3 = SubMesh.AppendTriangle(zero, gid);
			if (ComputeTriMaps)
			{
				BaseToSubT[triangle2] = num3;
				SubToBaseT.insert(triangle2, num3);
			}
		}
	}

	public static DMesh3 QuickSubmesh(DMesh3 mesh, int[] triangles)
	{
		return new DSubmesh3(mesh, triangles).SubMesh;
	}

	public static DMesh3 QuickSubmesh(DMesh3 mesh, IEnumerable<int> triangles)
	{
		return QuickSubmesh(mesh, triangles.ToArray());
	}
}
