using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class RegionOperator
{
	public DMesh3 BaseMesh;

	public DSubmesh3 Region;

	public IndexMap ReinsertSubToBaseMapV;

	public IndexMap ReinsertSubToBaseMapT;

	public MeshEditor.DuplicateTriBehavior ReinsertDuplicateTriBehavior;

	private int[] cur_base_tris;

	public int[] CurrentBaseTriangles => cur_base_tris;

	public RegionOperator(DMesh3 mesh, int[] regionTris, Action<DSubmesh3> submeshConfigF = null)
	{
		BaseMesh = mesh;
		Region = new DSubmesh3(mesh);
		submeshConfigF?.Invoke(Region);
		Region.Compute(regionTris);
		Region.ComputeBoundaryInfo(regionTris);
		cur_base_tris = (int[])regionTris.Clone();
	}

	public RegionOperator(DMesh3 mesh, IEnumerable<int> regionTris, Action<DSubmesh3> submeshConfigF = null)
	{
		BaseMesh = mesh;
		Region = new DSubmesh3(mesh);
		submeshConfigF?.Invoke(Region);
		Region.Compute(regionTris);
		int tri_count_est = regionTris.Count();
		Region.ComputeBoundaryInfo(regionTris, tri_count_est);
		cur_base_tris = regionTris.ToArray();
	}

	public HashSet<int> CurrentBaseInteriorVertices()
	{
		HashSet<int> hashSet = new HashSet<int>();
		IndexHashSet baseBorderV = Region.BaseBorderV;
		int[] array = cur_base_tris;
		foreach (int tID in array)
		{
			Index3i triangle = BaseMesh.GetTriangle(tID);
			if (!baseBorderV[triangle.a])
			{
				hashSet.Add(triangle.a);
			}
			if (!baseBorderV[triangle.b])
			{
				hashSet.Add(triangle.b);
			}
			if (!baseBorderV[triangle.c])
			{
				hashSet.Add(triangle.c);
			}
		}
		return hashSet;
	}

	public void RepairPossibleNonManifoldEdges()
	{
		int maxEdgeID = Region.SubMesh.MaxEdgeID;
		List<int> list = new List<int>();
		for (int i = 0; i < maxEdgeID; i++)
		{
			if (!Region.SubMesh.IsEdge(i) || Region.SubMesh.IsBoundaryEdge(i))
			{
				continue;
			}
			Index2i edgeV = Region.SubMesh.GetEdgeV(i);
			if (Region.SubMesh.IsBoundaryVertex(edgeV.a) && Region.SubMesh.IsBoundaryVertex(edgeV.b))
			{
				int num = Region.MapVertexToBaseMesh(edgeV.a);
				int num2 = Region.MapVertexToBaseMesh(edgeV.b);
				if (num != -1 && num2 != -1 && Region.BaseMesh.FindEdge(num, num2) != -1)
				{
					list.Add(i);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Region.SubMesh.SplitEdge(list[j], out var _);
		}
	}

	public void SetSubmeshGroupID(int gid)
	{
		FaceGroupUtil.SetGroupID(Region.SubMesh, gid);
	}

	public bool BackPropropagate(bool bAllowSubmeshRepairs = true)
	{
		if (bAllowSubmeshRepairs)
		{
			RepairPossibleNonManifoldEdges();
		}
		MeshEditor meshEditor = new MeshEditor(BaseMesh);
		meshEditor.RemoveTriangles(cur_base_tris, bRemoveIsolatedVerts: true);
		int[] new_tris = new int[Region.SubMesh.TriangleCount];
		ReinsertSubToBaseMapV = null;
		bool result = meshEditor.ReinsertSubmesh(Region, ref new_tris, out ReinsertSubToBaseMapV, ReinsertDuplicateTriBehavior);
		int maxTriangleID = Region.SubMesh.MaxTriangleID;
		ReinsertSubToBaseMapT = new IndexMap(bForceSparse: false, maxTriangleID);
		int num = 0;
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (Region.SubMesh.IsTriangle(i))
			{
				ReinsertSubToBaseMapT[i] = new_tris[num++];
			}
		}
		cur_base_tris = new_tris;
		return result;
	}

	public bool BackPropropagateVertices(bool bRecomputeBoundaryNormals = false)
	{
		bool flag = Region.SubMesh.HasVertexNormals && Region.BaseMesh.HasVertexNormals;
		foreach (int item in Region.SubMesh.VertexIndices())
		{
			int vID = Region.SubToBaseV[item];
			Vector3d vertex = Region.SubMesh.GetVertex(item);
			Region.BaseMesh.SetVertex(vID, vertex);
			if (flag)
			{
				Region.BaseMesh.SetVertexNormal(vID, Region.SubMesh.GetVertexNormal(item));
			}
		}
		if (bRecomputeBoundaryNormals)
		{
			foreach (int item2 in Region.BaseBorderV)
			{
				Vector3d vector3d = MeshNormals.QuickCompute(Region.BaseMesh, item2);
				Region.BaseMesh.SetVertexNormal(item2, (Vector3f)vector3d);
			}
		}
		return true;
	}
}
