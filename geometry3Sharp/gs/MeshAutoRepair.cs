using System;
using System.Linq;
using g3;

namespace gs;

public class MeshAutoRepair
{
	public enum RemoveModes
	{
		None,
		Interior,
		Occluded
	}

	public double RepairTolerance = 9.999999974752427E-07;

	public double MinEdgeLengthTol = 0.0001;

	public int ErosionIterations = 5;

	public RemoveModes RemoveMode;

	public ProgressCancel Progress;

	public DMesh3 Mesh;

	protected virtual bool Cancelled()
	{
		if (Progress != null)
		{
			return Progress.Cancelled();
		}
		return false;
	}

	public MeshAutoRepair(DMesh3 mesh3)
	{
		Mesh = mesh3;
	}

	public bool Apply()
	{
		bool flag = false;
		if (flag)
		{
			Mesh.CheckValidity();
		}
		do_remove_inside();
		if (Cancelled())
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			repair_orientation(bGlobal: false);
			if (Cancelled())
			{
				return false;
			}
			repair_cracks(bUniqueOnly: true, RepairTolerance);
			if (Mesh.IsClosed())
			{
				break;
			}
			if (Cancelled())
			{
				return false;
			}
			collapse_all_degenerate_edges(RepairTolerance * 0.5, bBoundaryOnly: true);
			if (Cancelled())
			{
				return false;
			}
			repair_cracks(bUniqueOnly: true, 2.0 * RepairTolerance);
			if (Cancelled())
			{
				return false;
			}
			repair_cracks(bUniqueOnly: false, 2.0 * RepairTolerance);
			if (Cancelled())
			{
				return false;
			}
			if (Mesh.IsClosed())
			{
				break;
			}
			repair_orientation(bGlobal: false);
			if (Cancelled())
			{
				return false;
			}
			if (flag)
			{
				Mesh.CheckValidity();
			}
			remove_loners();
			int nRemaining = 0;
			fill_trivial_holes(out var nRemaining2, out var saw_spans);
			if (Cancelled())
			{
				return false;
			}
			if (Mesh.IsClosed())
			{
				break;
			}
			fill_any_holes(out nRemaining2, out saw_spans);
			if (Cancelled())
			{
				return false;
			}
			if (saw_spans)
			{
				disconnect_bowties(out nRemaining);
				fill_any_holes(out nRemaining2, out saw_spans);
			}
			if (Cancelled())
			{
				return false;
			}
			if (Mesh.IsClosed())
			{
				break;
			}
			disconnect_bowties(out nRemaining);
			if (Cancelled())
			{
				return false;
			}
			if (num == 0 && !Mesh.IsClosed())
			{
				num++;
				continue;
			}
			if (num > ErosionIterations || Mesh.IsClosed())
			{
				break;
			}
			num++;
			MeshFaceSelection meshFaceSelection = new MeshFaceSelection(Mesh);
			foreach (int item in MeshIterators.BoundaryEdges(Mesh))
			{
				meshFaceSelection.SelectEdgeTris(item);
			}
			MeshEditor.RemoveTriangles(Mesh, meshFaceSelection);
		}
		if (MinEdgeLengthTol > 0.0)
		{
			collapse_all_degenerate_edges(MinEdgeLengthTol, bBoundaryOnly: false);
		}
		if (Cancelled())
		{
			return false;
		}
		repair_orientation(bGlobal: true);
		if (Cancelled())
		{
			return false;
		}
		if (flag)
		{
			Mesh.CheckValidity();
		}
		Mesh = new DMesh3(Mesh, bCompact: true);
		MeshNormals.QuickCompute(Mesh);
		return true;
	}

	private void fill_trivial_holes(out int nRemaining, out bool saw_spans)
	{
		MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh);
		nRemaining = 0;
		saw_spans = meshBoundaryLoops.SawOpenSpans;
		foreach (EdgeLoop item in meshBoundaryLoops)
		{
			if (Cancelled())
			{
				break;
			}
			bool flag = false;
			if (item.VertexCount == 3)
			{
				flag = new SimpleHoleFiller(Mesh, item).Fill();
			}
			else if (item.VertexCount == 4)
			{
				flag = new MinimalHoleFill(Mesh, item).Apply();
				if (!flag)
				{
					flag = new SimpleHoleFiller(Mesh, item).Fill();
				}
			}
			if (!flag)
			{
				nRemaining++;
			}
		}
	}

	private void fill_any_holes(out int nRemaining, out bool saw_spans)
	{
		MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(Mesh);
		nRemaining = 0;
		saw_spans = meshBoundaryLoops.SawOpenSpans;
		foreach (EdgeLoop item in meshBoundaryLoops)
		{
			if (Cancelled())
			{
				break;
			}
			if (!new MinimalHoleFill(Mesh, item).Apply())
			{
				if (Cancelled())
				{
					break;
				}
				new SimpleHoleFiller(Mesh, item).Fill();
			}
		}
	}

	private bool repair_cracks(bool bUniqueOnly, double mergeDist)
	{
		try
		{
			return new MergeCoincidentEdges(Mesh)
			{
				OnlyUniquePairs = bUniqueOnly,
				MergeDistance = mergeDist
			}.Apply();
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool remove_duplicate_faces(double vtxTolerance, out int nRemoved)
	{
		nRemoved = 0;
		try
		{
			RemoveDuplicateTriangles removeDuplicateTriangles = new RemoveDuplicateTriangles(Mesh);
			removeDuplicateTriangles.VertexTolerance = vtxTolerance;
			bool result = removeDuplicateTriangles.Apply();
			nRemoved = removeDuplicateTriangles.Removed;
			return result;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool collapse_degenerate_edges(double minLength, bool bBoundaryOnly, out int collapseCount)
	{
		collapseCount = 0;
		foreach (int item in MathUtil.ModuloIteration(Mesh.MaxEdgeID))
		{
			if (Cancelled())
			{
				break;
			}
			if (!Mesh.IsEdge(item))
			{
				continue;
			}
			bool flag = Mesh.IsBoundaryEdge(item);
			if (bBoundaryOnly && !flag)
			{
				continue;
			}
			Index2i edgeV = Mesh.GetEdgeV(item);
			Vector3d vertex = Mesh.GetVertex(edgeV.a);
			Vector3d vertex2 = Mesh.GetVertex(edgeV.b);
			if (!(vertex.Distance(vertex2) < minLength))
			{
				continue;
			}
			int num = (Mesh.IsBoundaryVertex(edgeV.a) ? edgeV.a : edgeV.b);
			int vRemove = ((num == edgeV.a) ? edgeV.b : edgeV.a);
			if (Mesh.CollapseEdge(num, vRemove, out var _) == MeshResult.Ok)
			{
				collapseCount++;
				if (!Mesh.IsBoundaryVertex(num) || flag)
				{
					Mesh.SetVertex(num, (vertex + vertex2) * 0.5);
				}
			}
		}
		return true;
	}

	private bool collapse_all_degenerate_edges(double minLength, bool bBoundaryOnly)
	{
		bool flag = true;
		while (flag && !Cancelled())
		{
			collapse_degenerate_edges(minLength, bBoundaryOnly, out var collapseCount);
			if (collapseCount == 0)
			{
				flag = false;
			}
		}
		return true;
	}

	private bool disconnect_bowties(out int nRemaining)
	{
		MeshEditor meshEditor = new MeshEditor(Mesh);
		nRemaining = meshEditor.DisconnectAllBowties();
		return true;
	}

	private void repair_orientation(bool bGlobal)
	{
		MeshRepairOrientation meshRepairOrientation = new MeshRepairOrientation(Mesh);
		meshRepairOrientation.OrientComponents();
		if (!Cancelled() && bGlobal)
		{
			meshRepairOrientation.SolveGlobalOrientation();
		}
	}

	private bool remove_interior(out int nRemoved)
	{
		RemoveOccludedTriangles removeOccludedTriangles = new RemoveOccludedTriangles(Mesh);
		removeOccludedTriangles.PerVertex = true;
		removeOccludedTriangles.InsideMode = RemoveOccludedTriangles.CalculationMode.FastWindingNumber;
		removeOccludedTriangles.Apply();
		nRemoved = removeOccludedTriangles.RemovedT.Count();
		return true;
	}

	private bool remove_occluded(out int nRemoved)
	{
		RemoveOccludedTriangles removeOccludedTriangles = new RemoveOccludedTriangles(Mesh);
		removeOccludedTriangles.PerVertex = true;
		removeOccludedTriangles.InsideMode = RemoveOccludedTriangles.CalculationMode.SimpleOcclusionTest;
		removeOccludedTriangles.Apply();
		nRemoved = removeOccludedTriangles.RemovedT.Count();
		return true;
	}

	private bool do_remove_inside()
	{
		int nRemoved = 0;
		if (RemoveMode == RemoveModes.Interior)
		{
			return remove_interior(out nRemoved);
		}
		if (RemoveMode == RemoveModes.Occluded)
		{
			return remove_occluded(out nRemoved);
		}
		return true;
	}

	private bool remove_loners()
	{
		MeshEditor.RemoveIsolatedTriangles(Mesh);
		return true;
	}
}
