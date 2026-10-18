using System;

namespace g3;

public class MeshTrimLoop
{
	public DMesh3 Mesh;

	public DMeshAABBTree3 Spatial;

	public DCurve3 TrimLine;

	public int RemeshBorderRings = 2;

	public double SmoothingAlpha = 1.0;

	public double TargetEdgeLength;

	public int RemeshRounds = 20;

	private int seed_tri = -1;

	private Vector3d seed_pt = Vector3d.MaxValue;

	public MeshTrimLoop(DMesh3 mesh, DCurve3 trimline, int tSeedTID, DMeshAABBTree3 spatial = null)
	{
		if (spatial != null && spatial.Mesh == mesh)
		{
			throw new ArgumentException("MeshTrimLoop: input spatial DS must have its own copy of mesh");
		}
		Mesh = mesh;
		TrimLine = new DCurve3(trimline);
		if (spatial != null)
		{
			Spatial = spatial;
		}
		seed_tri = tSeedTID;
	}

	public MeshTrimLoop(DMesh3 mesh, DCurve3 trimline, Vector3d vSeedPt, DMeshAABBTree3 spatial = null)
	{
		if (spatial != null && spatial.Mesh == mesh)
		{
			throw new ArgumentException("MeshTrimLoop: input spatial DS must have its own copy of mesh");
		}
		Mesh = mesh;
		TrimLine = new DCurve3(trimline);
		if (spatial != null)
		{
			Spatial = spatial;
		}
		seed_pt = vSeedPt;
	}

	public virtual ValidationStatus Validate()
	{
		return ValidationStatus.Ok;
	}

	public virtual bool Trim()
	{
		if (Spatial == null)
		{
			Spatial = new DMeshAABBTree3(new DMesh3(Mesh, bCompact: false, MeshComponents.None));
			Spatial.Build();
		}
		if (seed_tri == -1)
		{
			seed_tri = Spatial.FindNearestTriangle(seed_pt);
		}
		MeshFaceSelection meshFaceSelection = new MeshFacesFromLoop(Mesh, TrimLine, Spatial, seed_tri).ToSelection();
		meshFaceSelection.LocalOptimize(bClipFins: true, bFillEars: true);
		MeshEditor meshEditor = new MeshEditor(Mesh);
		meshEditor.RemoveTriangles(meshFaceSelection, bRemoveIsolatedVerts: true);
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(Mesh);
		meshConnectedComponents.FindConnectedT();
		if (meshConnectedComponents.Count > 1)
		{
			int largestByCount = meshConnectedComponents.LargestByCount;
			for (int i = 0; i < meshConnectedComponents.Count; i++)
			{
				if (i != largestByCount)
				{
					meshEditor.RemoveTriangles(meshConnectedComponents[i].Indices, bRemoveIsolatedVerts: true);
				}
			}
		}
		meshEditor.RemoveAllBowtieVertices(bRepeatUntilClean: true);
		MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh);
		bool flag = false;
		try
		{
			flag = meshBoundaryLoops.Compute();
		}
		catch (Exception)
		{
			return false;
		}
		if (!flag)
		{
			return false;
		}
		if (meshBoundaryLoops.Count > 1)
		{
			return false;
		}
		int[] vertices = meshBoundaryLoops[0].Vertices;
		MeshFaceSelection meshFaceSelection2 = new MeshFaceSelection(Mesh);
		meshFaceSelection2.SelectVertexOneRings(vertices);
		meshFaceSelection2.ExpandToOneRingNeighbours(RemeshBorderRings);
		RegionRemesher regionRemesher = new RegionRemesher(Mesh, meshFaceSelection2.ToArray());
		regionRemesher.Region.MapVerticesToSubmesh(vertices);
		double num = TargetEdgeLength;
		if (num <= 0.0)
		{
			MeshQueries.EdgeLengthStatsFromEdges(Mesh, meshBoundaryLoops[0].Edges, out var _, out var _, out var avgEdgeLen);
			num = avgEdgeLen;
		}
		MeshProjectionTarget meshProjectionTarget = new MeshProjectionTarget(Spatial.Mesh, Spatial);
		regionRemesher.SetProjectionTarget(meshProjectionTarget);
		regionRemesher.SetTargetEdgeLength(num);
		regionRemesher.SmoothSpeedT = SmoothingAlpha;
		DCurveProjectionTarget dCurveProjectionTarget = new DCurveProjectionTarget(TrimLine);
		SequentialProjectionTarget target = new SequentialProjectionTarget(dCurveProjectionTarget, meshProjectionTarget);
		int setID = 3;
		MeshConstraintUtil.ConstrainVtxLoopTo(regionRemesher, vertices, target, setID);
		for (int j = 0; j < RemeshRounds; j++)
		{
			regionRemesher.BasicRemeshPass();
		}
		regionRemesher.BackPropropagate();
		return true;
	}
}
