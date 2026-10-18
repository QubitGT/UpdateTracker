using System;
using System.Collections.Generic;

namespace g3;

public class MeshPlaneCut
{
	public DMesh3 Mesh;

	public Vector3d PlaneOrigin;

	public Vector3d PlaneNormal;

	public bool CollapseDegenerateEdgesOnCut = true;

	public double DegenerateEdgeTol = 9.999999974752427E-07;

	public MeshFaceSelection CutFaceSet;

	public List<EdgeLoop> CutLoops;

	public List<EdgeSpan> CutSpans;

	public bool CutLoopsFailed;

	public bool FoundOpenSpans;

	public List<int[]> LoopFillTriangles;

	public MeshPlaneCut(DMesh3 mesh, Vector3d origin, Vector3d normal)
	{
		Mesh = mesh;
		PlaneOrigin = origin;
		PlaneNormal = normal;
	}

	public virtual ValidationStatus Validate()
	{
		return ValidationStatus.Ok;
	}

	public virtual bool Cut()
	{
		double invalidDist = double.MinValue;
		MeshEdgeSelection meshEdgeSelection = null;
		MeshVertexSelection meshVertexSelection = null;
		if (CutFaceSet != null)
		{
			meshEdgeSelection = new MeshEdgeSelection(Mesh, CutFaceSet);
			meshVertexSelection = new MeshVertexSelection(Mesh, meshEdgeSelection);
		}
		int maxVertexID = Mesh.MaxVertexID;
		double[] signs = new double[maxVertexID];
		gParallel.ForEach(Interval1i.Range(maxVertexID), delegate(int vid)
		{
			if (Mesh.IsVertex(vid))
			{
				Vector3d vertex = Mesh.GetVertex(vid);
				signs[vid] = (vertex - PlaneOrigin).Dot(PlaneNormal);
			}
			else
			{
				signs[vid] = invalidDist;
			}
		});
		HashSet<int> ZeroEdges = new HashSet<int>();
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> OnCutEdges = new HashSet<int>();
		int maxEdgeID = Mesh.MaxEdgeID;
		HashSet<int> hashSet2 = new HashSet<int>();
		IEnumerable<int> enumerable = Interval1i.Range(maxEdgeID);
		if (meshEdgeSelection != null)
		{
			enumerable = meshEdgeSelection;
		}
		foreach (int item in enumerable)
		{
			if (!Mesh.IsEdge(item) || item >= maxEdgeID || hashSet2.Contains(item))
			{
				continue;
			}
			Index2i edgeV = Mesh.GetEdgeV(item);
			double num = signs[edgeV.a];
			double num2 = signs[edgeV.b];
			int num3 = ((Math.Abs(num) < 2.220446049250313E-16) ? 1 : 0);
			int num4 = ((Math.Abs(num2) < 2.220446049250313E-16) ? 1 : 0);
			if (num3 + num4 > 0)
			{
				if (num3 + num4 == 2)
				{
					ZeroEdges.Add(item);
				}
				else
				{
					hashSet.Add((num3 == 1) ? edgeV[0] : edgeV[1]);
				}
			}
			else if (!(num * num2 > 0.0))
			{
				if (Mesh.SplitEdge(item, out var split) != 0)
				{
					throw new Exception("MeshPlaneCut.Cut: failed in SplitEdge");
				}
				double num5 = num / (num - num2);
				Vector3d vNewPos = (1.0 - num5) * Mesh.GetVertex(edgeV.a) + num5 * Mesh.GetVertex(edgeV.b);
				Mesh.SetVertex(split.vNew, vNewPos);
				hashSet2.Add(split.eNewBN);
				hashSet2.Add(split.eNewCN);
				OnCutEdges.Add(split.eNewCN);
				if (split.eNewDN != -1)
				{
					hashSet2.Add(split.eNewDN);
					OnCutEdges.Add(split.eNewDN);
				}
			}
		}
		IEnumerable<int> enumerable2 = Interval1i.Range(maxVertexID);
		if (meshVertexSelection != null)
		{
			enumerable2 = meshVertexSelection;
		}
		foreach (int item2 in enumerable2)
		{
			if (signs[item2] > 0.0 && Mesh.IsVertex(item2))
			{
				Mesh.RemoveVertex(item2);
			}
		}
		if (CollapseDegenerateEdgesOnCut)
		{
			collapse_degenerate_edges(OnCutEdges, ZeroEdges);
		}
		Func<int, bool> edgeFilterF = (int eid) => (OnCutEdges.Contains(eid) || ZeroEdges.Contains(eid)) ? true : false;
		try
		{
			MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh, bAutoCompute: false);
			meshBoundaryLoops.EdgeFilterF = edgeFilterF;
			meshBoundaryLoops.Compute();
			CutLoops = meshBoundaryLoops.Loops;
			CutSpans = meshBoundaryLoops.Spans;
			CutLoopsFailed = false;
			FoundOpenSpans = CutSpans.Count > 0;
		}
		catch
		{
			CutLoops = new List<EdgeLoop>();
			CutLoopsFailed = true;
		}
		return true;
	}

	protected void collapse_degenerate_edges(HashSet<int> OnCutEdges, HashSet<int> ZeroEdges)
	{
		HashSet<int>[] array = new HashSet<int>[2] { OnCutEdges, ZeroEdges };
		double num = DegenerateEdgeTol * DegenerateEdgeTol;
		Vector3d a = Vector3d.Zero;
		Vector3d b = Vector3d.Zero;
		int num2 = 0;
		do
		{
			num2 = 0;
			HashSet<int>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				foreach (int item in array2[i])
				{
					if (!Mesh.IsEdge(item))
					{
						continue;
					}
					Mesh.GetEdgeV(item, ref a, ref b);
					if (!(a.DistanceSquared(b) > num))
					{
						Index2i edgeV = Mesh.GetEdgeV(item);
						if (Mesh.CollapseEdge(edgeV.a, edgeV.b, out var _) == MeshResult.Ok)
						{
							num2++;
						}
					}
				}
			}
		}
		while (num2 != 0);
	}

	public bool FillHoles(int constantGroupID = -1)
	{
		bool result = true;
		LoopFillTriangles = new List<int[]>(CutLoops.Count);
		foreach (EdgeLoop cutLoop in CutLoops)
		{
			SimpleHoleFiller simpleHoleFiller = new SimpleHoleFiller(Mesh, cutLoop);
			int group_id = ((constantGroupID >= 0) ? constantGroupID : Mesh.AllocateTriangleGroup());
			if (simpleHoleFiller.Fill(group_id))
			{
				result = false;
				LoopFillTriangles.Add(simpleHoleFiller.NewTriangles);
			}
			else
			{
				LoopFillTriangles.Add(null);
			}
		}
		return result;
	}
}
