using System;
using System.Collections.Generic;

namespace g3;

public class MeshLoopClosure
{
	public DMesh3 Mesh;

	public EdgeLoop InitialBorderLoop;

	public Frame3f FlatClosePlane;

	public double TargetEdgeLen;

	public int ExtrudeGroup = -1;

	public int FillGroup = -1;

	public MeshLoopClosure(DMesh3 mesh, EdgeLoop border_loop)
	{
		Mesh = mesh;
		InitialBorderLoop = border_loop;
	}

	public virtual ValidationStatus Validate()
	{
		ValidationStatus validationStatus = MeshValidation.IsBoundaryLoop(Mesh, InitialBorderLoop);
		if (validationStatus != 0)
		{
			return validationStatus;
		}
		ValidationStatus validationStatus2 = MeshValidation.HasDuplicateTriangles(Mesh);
		if (validationStatus2 != 0)
		{
			return validationStatus2;
		}
		return ValidationStatus.Ok;
	}

	public virtual bool Close()
	{
		Close_Flat();
		return true;
	}

	public void Close_Flat()
	{
		MeshQueries.EdgeLengthStats(Mesh, out var _, out var _, out var avgEdgeLen, 1000);
		double num = ((TargetEdgeLen <= 0.0) ? avgEdgeLen : TargetEdgeLen);
		cleanup_boundary(Mesh, InitialBorderLoop, avgEdgeLen, out var result_edges);
		MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh);
		int num2 = meshBoundaryLoops.FindLoopContainingEdge(result_edges[0]);
		if (num2 == -1)
		{
			num2 = meshBoundaryLoops.MaxVerticesLoopIndex;
		}
		EdgeLoop loop = meshBoundaryLoops.Loops[num2];
		int num3 = ((ExtrudeGroup == -1) ? Mesh.AllocateTriangleGroup() : ExtrudeGroup);
		int group_id = ((FillGroup == -1) ? Mesh.AllocateTriangleGroup() : FillGroup);
		MeshExtrudeLoop meshExtrudeLoop = new MeshExtrudeLoop(Mesh, loop);
		meshExtrudeLoop.PositionF = (Vector3d v, Vector3f n, int i) => FlatClosePlane.ProjectToPlane((Vector3f)v, 1);
		meshExtrudeLoop.Extrude(num3);
		MeshValidation.IsBoundaryLoop(Mesh, meshExtrudeLoop.NewLoop);
		MeshLoopSmooth meshLoopSmooth = new MeshLoopSmooth(Mesh, meshExtrudeLoop.NewLoop);
		meshLoopSmooth.ProjectF = (Vector3d v, int i) => FlatClosePlane.ProjectToPlane((Vector3f)v, 1);
		meshLoopSmooth.Alpha = 0.5;
		meshLoopSmooth.Rounds = 100;
		meshLoopSmooth.Smooth();
		SimpleHoleFiller simpleHoleFiller = new SimpleHoleFiller(Mesh, meshExtrudeLoop.NewLoop);
		simpleHoleFiller.Fill(group_id);
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
		meshFaceSelection.Select(meshExtrudeLoop.NewTriangles);
		meshFaceSelection.Select(simpleHoleFiller.NewTriangles);
		meshFaceSelection.ExpandToOneRingNeighbours();
		meshFaceSelection.ExpandToOneRingNeighbours();
		meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		int[] regionTris = meshFaceSelection.ToArray();
		FaceGroupUtil.SetGroupToGroup(Mesh, num3, 0);
		RegionRemesher regionRemesher = new RegionRemesher(Mesh, regionTris);
		DCurveProjectionTarget target = new DCurveProjectionTarget(MeshUtil.ExtractLoopV(Mesh, meshExtrudeLoop.NewLoop.Vertices));
		int[] array = (int[])meshExtrudeLoop.NewLoop.Vertices.Clone();
		regionRemesher.Region.MapVerticesToSubmesh(array);
		MeshConstraintUtil.ConstrainVtxLoopTo(regionRemesher.Constraints, regionRemesher.Mesh, array, target);
		DMeshAABBTree3 dMeshAABBTree = new DMeshAABBTree3(Mesh);
		dMeshAABBTree.Build();
		MeshProjectionTarget projectionTarget = new MeshProjectionTarget(Mesh, dMeshAABBTree);
		regionRemesher.SetProjectionTarget(projectionTarget);
		if (true)
		{
			regionRemesher.Precompute();
			regionRemesher.EnableFlips = (regionRemesher.EnableSplits = (regionRemesher.EnableCollapses = true));
			regionRemesher.MinEdgeLength = num;
			regionRemesher.MaxEdgeLength = 2.0 * num;
			regionRemesher.EnableSmoothing = true;
			regionRemesher.SmoothSpeedT = 1.0;
			for (int j = 0; j < 40; j++)
			{
				regionRemesher.BasicRemeshPass();
			}
			regionRemesher.SetProjectionTarget(null);
			regionRemesher.SmoothSpeedT = 0.25;
			for (int k = 0; k < 10; k++)
			{
				regionRemesher.BasicRemeshPass();
			}
			regionRemesher.BackPropropagate();
		}
		smooth_region(Mesh, regionRemesher.Region.BaseBorderV, 3);
	}

	public static void smooth_region(DMesh3 mesh, IEnumerable<int> vertices, int nRings)
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(mesh);
		meshFaceSelection.SelectVertexOneRings(vertices);
		for (int i = 0; i < nRings; i++)
		{
			meshFaceSelection.ExpandToOneRingNeighbours();
		}
		meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(mesh);
		meshVertexSelection.SelectTriangleVertices(meshFaceSelection.ToArray());
		MeshIterativeSmooth meshIterativeSmooth = new MeshIterativeSmooth(mesh, meshVertexSelection.ToArray(), bOwnVertices: true);
		meshIterativeSmooth.Alpha = 0.20000000298023224;
		meshIterativeSmooth.Rounds = 10;
		meshIterativeSmooth.Smooth();
	}

	public static void smooth_loop(DMesh3 mesh, EdgeLoop loop, int nRings)
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(mesh);
		meshFaceSelection.SelectVertexOneRings(loop.Vertices);
		for (int i = 0; i < nRings; i++)
		{
			meshFaceSelection.ExpandToOneRingNeighbours();
		}
		meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(mesh);
		meshVertexSelection.SelectTriangleVertices(meshFaceSelection.ToArray());
		meshVertexSelection.Deselect(loop.Vertices);
		MeshLoopSmooth meshLoopSmooth = new MeshLoopSmooth(mesh, loop);
		meshLoopSmooth.Rounds = 1;
		MeshIterativeSmooth meshIterativeSmooth = new MeshIterativeSmooth(mesh, meshVertexSelection.ToArray(), bOwnVertices: true);
		meshIterativeSmooth.Rounds = 1;
		for (int j = 0; j < 10; j++)
		{
			meshLoopSmooth.Smooth();
			meshIterativeSmooth.Smooth();
		}
	}

	public static void cleanup_boundary(DMesh3 mesh, EdgeLoop loop, double target_edge_len, out List<int> result_edges, int nRings = 3)
	{
		MeshFaceSelection meshFaceSelection = new MeshFaceSelection(mesh);
		meshFaceSelection.SelectVertexOneRings(loop.Vertices);
		for (int i = 0; i < nRings; i++)
		{
			meshFaceSelection.ExpandToOneRingNeighbours();
		}
		meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		RegionRemesher regionRemesher = new RegionRemesher(mesh, meshFaceSelection.ToArray());
		int[] array = new int[loop.EdgeCount];
		Array.Copy(loop.Edges, array, loop.EdgeCount);
		regionRemesher.Region.MapEdgesToSubmesh(array);
		MeshConstraintUtil.AddTrackedEdges(regionRemesher.Constraints, array, 100);
		regionRemesher.Precompute();
		regionRemesher.EnableFlips = (regionRemesher.EnableSplits = (regionRemesher.EnableCollapses = true));
		regionRemesher.MinEdgeLength = target_edge_len;
		regionRemesher.MaxEdgeLength = 2.0 * target_edge_len;
		regionRemesher.EnableSmoothing = true;
		regionRemesher.SmoothSpeedT = 0.10000000149011612;
		for (int j = 0; j < nRings * 3; j++)
		{
			regionRemesher.BasicRemeshPass();
		}
		List<int> edges = regionRemesher.Constraints.FindConstrainedEdgesBySetID(100);
		regionRemesher.BackPropropagate();
		result_edges = MeshIndexUtil.MapEdgesViaVertexMap(regionRemesher.ReinsertSubToBaseMapV, regionRemesher.Region.SubMesh, regionRemesher.BaseMesh, edges);
	}
}
