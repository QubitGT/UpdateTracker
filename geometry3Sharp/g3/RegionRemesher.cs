using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class RegionRemesher : Remesher
{
	[Flags]
	public enum QuickRemeshFlags
	{
		NoFlags = 0,
		PreventNormalFlips = 1
	}

	public DMesh3 BaseMesh;

	public DSubmesh3 Region;

	public IndexMap ReinsertSubToBaseMapV;

	public MeshEditor.DuplicateTriBehavior ReinsertDuplicateTriBehavior;

	private MeshConstraints bdry_constraints;

	private int[] cur_base_tris;

	public int[] CurrentBaseTriangles => cur_base_tris;

	public RegionRemesher(DMesh3 mesh, int[] regionTris)
	{
		BaseMesh = mesh;
		Region = new DSubmesh3(mesh, regionTris);
		Region.ComputeBoundaryInfo(regionTris);
		base.mesh = Region.SubMesh;
		cur_base_tris = (int[])regionTris.Clone();
		bdry_constraints = new MeshConstraints();
		MeshConstraintUtil.FixSubmeshBoundaryEdges(bdry_constraints, Region);
		SetExternalConstraints(bdry_constraints);
	}

	public RegionRemesher(DMesh3 mesh, IEnumerable<int> regionTris)
	{
		BaseMesh = mesh;
		Region = new DSubmesh3(mesh, regionTris);
		int tri_count_est = regionTris.Count();
		Region.ComputeBoundaryInfo(regionTris, tri_count_est);
		base.mesh = Region.SubMesh;
		cur_base_tris = regionTris.ToArray();
		bdry_constraints = new MeshConstraints();
		MeshConstraintUtil.FixSubmeshBoundaryEdges(bdry_constraints, Region);
		SetExternalConstraints(bdry_constraints);
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
		cur_base_tris = new_tris;
		return result;
	}

	public static RegionRemesher QuickRemesh(DMesh3 mesh, int[] tris, double minEdgeLen, double maxEdgeLen, double smoothSpeed, int rounds, IProjectionTarget target, QuickRemeshFlags flags = QuickRemeshFlags.PreventNormalFlips)
	{
		RegionRemesher regionRemesher = new RegionRemesher(mesh, tris);
		if (target != null)
		{
			regionRemesher.SetProjectionTarget(target);
		}
		regionRemesher.MinEdgeLength = minEdgeLen;
		regionRemesher.MaxEdgeLength = maxEdgeLen;
		regionRemesher.SmoothSpeedT = smoothSpeed;
		if ((flags & QuickRemeshFlags.PreventNormalFlips) != 0)
		{
			regionRemesher.PreventNormalFlips = true;
		}
		for (int i = 0; i < rounds; i++)
		{
			regionRemesher.BasicRemeshPass();
		}
		regionRemesher.BackPropropagate();
		return regionRemesher;
	}

	public static RegionRemesher QuickRemesh(DMesh3 mesh, int[] tris, double targetEdgeLen, double smoothSpeed, int rounds, IProjectionTarget target, QuickRemeshFlags flags = QuickRemeshFlags.PreventNormalFlips)
	{
		RegionRemesher regionRemesher = new RegionRemesher(mesh, tris);
		if (target != null)
		{
			regionRemesher.SetProjectionTarget(target);
		}
		regionRemesher.SetTargetEdgeLength(targetEdgeLen);
		regionRemesher.SmoothSpeedT = smoothSpeed;
		if ((flags & QuickRemeshFlags.PreventNormalFlips) != 0)
		{
			regionRemesher.PreventNormalFlips = true;
		}
		for (int i = 0; i < rounds; i++)
		{
			regionRemesher.BasicRemeshPass();
		}
		regionRemesher.BackPropropagate();
		return regionRemesher;
	}
}
