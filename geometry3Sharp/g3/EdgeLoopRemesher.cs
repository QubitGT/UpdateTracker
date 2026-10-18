using System.Collections.Generic;

namespace g3;

public class EdgeLoopRemesher : Remesher
{
	public EdgeLoop InputLoop;

	public EdgeLoop OutputLoop;

	public int LocalSmoothingRings;

	private List<int> CurrentLoopE;

	private List<int> CurrentLoopV;

	private List<int> RemainingE;

	private const int nPrime = 31337;

	private HashSet<int> smoothV = new HashSet<int>();

	public EdgeLoopRemesher(DMesh3 m, EdgeLoop loop)
		: base(m)
	{
		UpdateLoop(loop);
		EnableFlips = false;
		CustomSmoothF = loop_smooth_vertex;
	}

	public void UpdateLoop(EdgeLoop loop)
	{
		InputLoop = loop;
		OutputLoop = null;
		CurrentLoopE = new List<int>(loop.Edges);
		CurrentLoopV = new List<int>(loop.Vertices);
	}

	public override void Precompute()
	{
		base.Precompute();
	}

	protected override int start_edges()
	{
		RemainingE = new List<int>(CurrentLoopE.Count);
		int num = 31337;
		int num2 = 0;
		do
		{
			RemainingE.Add(CurrentLoopE[num2]);
			num2 = (num2 + num) % CurrentLoopE.Count;
		}
		while (num2 != 0);
		int result = RemainingE[RemainingE.Count - 1];
		RemainingE.RemoveAt(RemainingE.Count - 1);
		return result;
	}

	protected override int next_edge(int cur_eid, out bool bDone)
	{
		if (RemainingE.Count == 0)
		{
			bDone = true;
			return 0;
		}
		bDone = false;
		int result = RemainingE[RemainingE.Count - 1];
		RemainingE.RemoveAt(RemainingE.Count - 1);
		return result;
	}

	protected override void end_pass()
	{
		OutputLoop = new EdgeLoop(mesh, CurrentLoopV.ToArray(), CurrentLoopE.ToArray(), bCopyArrays: false);
	}

	protected override void begin_smooth()
	{
		base.begin_smooth();
		if (LocalSmoothingRings <= 0)
		{
			return;
		}
		smoothV.Clear();
		if (LocalSmoothingRings == 1)
		{
			for (int i = 0; i < CurrentLoopV.Count; i++)
			{
				smoothV.Add(CurrentLoopV[i]);
				foreach (int item in mesh.VtxVerticesItr(CurrentLoopV[i]))
				{
					smoothV.Add(item);
				}
			}
			return;
		}
		MeshVertexSelection meshVertexSelection = new MeshVertexSelection(mesh);
		meshVertexSelection.Select(CurrentLoopV);
		meshVertexSelection.ExpandToOneRingNeighbours(LocalSmoothingRings);
		foreach (int item2 in meshVertexSelection)
		{
			smoothV.Add(item2);
		}
	}

	protected override IEnumerable<int> smooth_vertices()
	{
		if (LocalSmoothingRings > 0)
		{
			return smoothV;
		}
		return CurrentLoopV;
	}

	private Vector3d loop_smooth_vertex(DMesh3 mesh, int vid, double alpha)
	{
		if (LocalSmoothingRings > 0 && !CurrentLoopV.Contains(vid))
		{
			bool bModified = false;
			return base.ComputeSmoothedVertexPos(vid, MeshUtil.UniformSmooth, out bModified);
		}
		int num = CurrentLoopV.FindIndex((int i) => i == vid);
		if (num < 0)
		{
			return mesh.GetVertex(vid);
		}
		int index = (num + CurrentLoopV.Count - 1) % CurrentLoopV.Count;
		int index2 = (num + 1) % CurrentLoopV.Count;
		Vector3d vector3d = mesh.GetVertex(CurrentLoopV[index]) + mesh.GetVertex(CurrentLoopV[index2]);
		vector3d *= 0.5;
		return (1.0 - alpha) * mesh.GetVertex(vid) + alpha * vector3d;
	}

	protected override IEnumerable<int> project_vertices()
	{
		if (LocalSmoothingRings > 0)
		{
			return smoothV;
		}
		return CurrentLoopV;
	}

	protected override void OnEdgeSplit(int edgeID, int va, int vb, DMesh3.EdgeSplitInfo splitInfo)
	{
		int num = CurrentLoopV.FindIndex((int i) => i == va);
		int num2 = CurrentLoopV.FindIndex((int i) => i == vb);
		if (CurrentLoopE.FindIndex((int i) => i == edgeID) == CurrentLoopE.Count - 1)
		{
			CurrentLoopV.Add(splitInfo.vNew);
		}
		else if (num < num2)
		{
			CurrentLoopV.Insert(num2, splitInfo.vNew);
		}
		else
		{
			CurrentLoopV.Insert(num, splitInfo.vNew);
		}
		rebuild_edge_list();
	}

	protected override void OnEdgeCollapse(int edgeID, int va, int vb, DMesh3.EdgeCollapseInfo collapseInfo)
	{
		int num = CurrentLoopV.FindIndex((int i) => i == collapseInfo.vRemoved);
		CurrentLoopV.RemoveAt(num);
		int num2 = CurrentLoopE.FindIndex((int i) => i == edgeID);
		CurrentLoopE.RemoveAt(num2);
		if (num == 0 && num2 == CurrentLoopE.Count)
		{
			rebuild_edge_list();
		}
	}

	private bool check_loop()
	{
		for (int i = 0; i < CurrentLoopV.Count; i++)
		{
			mesh.FindEdge(CurrentLoopV[i], CurrentLoopV[(i + 1) % CurrentLoopV.Count]);
		}
		return true;
	}

	private void rebuild_edge_list()
	{
		CurrentLoopE.Clear();
		int count = CurrentLoopV.Count;
		for (int i = 0; i < count; i++)
		{
			CurrentLoopE.Add(mesh.FindEdge(CurrentLoopV[i], CurrentLoopV[(i + 1) % count]));
		}
	}
}
