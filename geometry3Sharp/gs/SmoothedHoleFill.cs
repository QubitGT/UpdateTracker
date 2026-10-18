using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class SmoothedHoleFill
{
	public DMesh3 Mesh;

	public Vector3d OffsetDirection = Vector3d.Zero;

	public double OffsetDistance;

	public double TargetEdgeLength = 2.5;

	public double SmoothAlpha = 1.0;

	public int InitialRemeshPasses = 20;

	public bool RemeshBeforeSmooth = true;

	public bool RemeshAfterSmooth = true;

	public Action<Remesher, bool> ConfigureRemesherF;

	public bool EnableLaplacianSmooth = true;

	public int SmoothSolveIterations = 1;

	public bool ConstrainToHoleInterior;

	public EdgeLoop FillLoop;

	public List<int> BorderHintTris;

	public int[] FillTriangles;

	public int[] FillVertices;

	public SmoothedHoleFill(DMesh3 mesh, EdgeLoop fillLoop = null)
	{
		Mesh = mesh;
		FillLoop = fillLoop;
	}

	public bool Apply()
	{
		EdgeLoop edgeLoop = null;
		if (FillLoop == null)
		{
			MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh);
			if (meshBoundaryLoops.Count == 0)
			{
				return false;
			}
			if (BorderHintTris != null)
			{
				edgeLoop = select_loop_tris_hint(meshBoundaryLoops);
			}
			if (edgeLoop == null && meshBoundaryLoops.MaxVerticesLoopIndex >= 0)
			{
				edgeLoop = meshBoundaryLoops[meshBoundaryLoops.MaxVerticesLoopIndex];
			}
		}
		else
		{
			edgeLoop = FillLoop;
		}
		if (edgeLoop == null)
		{
			return false;
		}
		SimpleHoleFiller simpleHoleFiller = new SimpleHoleFiller(Mesh, edgeLoop);
		if (!simpleHoleFiller.Fill())
		{
			return false;
		}
		if (edgeLoop.Vertices.Length <= 3)
		{
			FillTriangles = simpleHoleFiller.NewTriangles;
			FillVertices = new int[0];
			return true;
		}
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
		meshFaceSelection.Select(simpleHoleFiller.NewTriangles);
		if (OffsetDistance > 0.0)
		{
			MeshExtrudeFaces meshExtrudeFaces = new MeshExtrudeFaces(Mesh, meshFaceSelection);
			meshExtrudeFaces.ExtrudedPositionF = (Vector3d v, Vector3f n, int vid) => v + OffsetDistance * OffsetDirection;
			if (!meshExtrudeFaces.Extrude())
			{
				return false;
			}
			meshFaceSelection.Select(meshExtrudeFaces.JoinTriangles);
		}
		if (!ConstrainToHoleInterior)
		{
			meshFaceSelection.ExpandToOneRingNeighbours(2);
			meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		}
		if (RemeshBeforeSmooth)
		{
			RegionRemesher regionRemesher = new RegionRemesher(Mesh, meshFaceSelection);
			regionRemesher.SetTargetEdgeLength(TargetEdgeLength);
			regionRemesher.EnableSmoothing = SmoothAlpha > 0.0;
			regionRemesher.SmoothSpeedT = SmoothAlpha;
			if (ConfigureRemesherF != null)
			{
				ConfigureRemesherF(regionRemesher, arg2: true);
			}
			for (int i = 0; i < InitialRemeshPasses; i++)
			{
				regionRemesher.BasicRemeshPass();
			}
			regionRemesher.BackPropropagate();
			meshFaceSelection = new MeshFaceSelection(Mesh);
			meshFaceSelection.Select(regionRemesher.CurrentBaseTriangles);
			if (!ConstrainToHoleInterior)
			{
				meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
			}
		}
		if (ConstrainToHoleInterior)
		{
			for (int j = 0; j < SmoothSolveIterations; j++)
			{
				smooth_and_remesh_preserve(meshFaceSelection, j == SmoothSolveIterations - 1);
				meshFaceSelection = new MeshFaceSelection(Mesh);
				meshFaceSelection.Select(FillTriangles);
			}
		}
		else
		{
			smooth_and_remesh(meshFaceSelection);
			meshFaceSelection = new MeshFaceSelection(Mesh);
			meshFaceSelection.Select(FillTriangles);
		}
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(Mesh);
		meshVertexSelection.SelectInteriorVertices(meshFaceSelection);
		FillVertices = meshVertexSelection.ToArray();
		return true;
	}

	private void smooth_and_remesh_preserve(MeshFaceSelection tris, bool bFinal)
	{
		if (EnableLaplacianSmooth)
		{
			LaplacianMeshSmoother.RegionSmooth(Mesh, tris, 2, 2, bPreserveExteriorRings: true);
		}
		if (RemeshAfterSmooth)
		{
			MeshProjectionTarget projectionTarget = (bFinal ? MeshProjectionTarget.Auto(Mesh, tris) : null);
			RegionRemesher regionRemesher = new RegionRemesher(Mesh, tris);
			regionRemesher.SetTargetEdgeLength(TargetEdgeLength);
			regionRemesher.SmoothSpeedT = 1.0;
			regionRemesher.SetProjectionTarget(projectionTarget);
			if (ConfigureRemesherF != null)
			{
				ConfigureRemesherF(regionRemesher, arg2: false);
			}
			for (int i = 0; i < 10; i++)
			{
				regionRemesher.BasicRemeshPass();
			}
			regionRemesher.BackPropropagate();
			FillTriangles = regionRemesher.CurrentBaseTriangles;
		}
		else
		{
			FillTriangles = tris.ToArray();
		}
	}

	private void smooth_and_remesh(MeshFaceSelection tris)
	{
		if (EnableLaplacianSmooth)
		{
			LaplacianMeshSmoother.RegionSmooth(Mesh, tris, 2, 2, bPreserveExteriorRings: false);
		}
		if (RemeshAfterSmooth)
		{
			tris.ExpandToOneRingNeighbours(2);
			tris.LocalOptimize(bClipFins: true, bFillEars: true);
			MeshProjectionTarget projectionTarget = MeshProjectionTarget.Auto(Mesh, tris);
			RegionRemesher regionRemesher = new RegionRemesher(Mesh, tris);
			regionRemesher.SetTargetEdgeLength(TargetEdgeLength);
			regionRemesher.SmoothSpeedT = 1.0;
			regionRemesher.SetProjectionTarget(projectionTarget);
			if (ConfigureRemesherF != null)
			{
				ConfigureRemesherF(regionRemesher, arg2: false);
			}
			for (int i = 0; i < 10; i++)
			{
				regionRemesher.BasicRemeshPass();
			}
			regionRemesher.BackPropropagate();
			FillTriangles = regionRemesher.CurrentBaseTriangles;
		}
		else
		{
			FillTriangles = tris.ToArray();
		}
	}

	private EdgeLoop select_loop_tris_hint(MeshBoundaryLoops loops)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int borderHintTri in BorderHintTris)
		{
			if (!Mesh.IsTriangle(borderHintTri))
			{
				continue;
			}
			Index3i triEdges = Mesh.GetTriEdges(borderHintTri);
			for (int i = 0; i < 3; i++)
			{
				if (Mesh.IsBoundaryEdge(triEdges[i]))
				{
					hashSet.Add(triEdges[i]);
				}
			}
		}
		int count = loops.Count;
		int num = -1;
		int num2 = 0;
		for (int j = 0; j < count; j++)
		{
			int num3 = 0;
			int[] edges = loops[j].Edges;
			foreach (int item in edges)
			{
				if (hashSet.Contains(item))
				{
					num3++;
				}
			}
			if (num3 > num2)
			{
				num = j;
				num2 = num3;
			}
		}
		if (num == -1)
		{
			return null;
		}
		return loops[num];
	}
}
