using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace g3;

public class Reducer : MeshRefinerBase
{
	public enum TargetProjectionMode
	{
		NoProjection,
		AfterRefinement,
		Inline
	}

	protected enum TargetModes
	{
		TriangleCount,
		VertexCount,
		MinEdgeLength
	}

	protected struct QEdge
	{
		public int eid;

		public QuadricError q;

		public Vector3d collapse_pt;

		public QEdge(int edge_id, ref QuadricError qin, ref Vector3d pt)
		{
			eid = edge_id;
			q = qin;
			collapse_pt = pt;
		}
	}

	protected enum ProcessResult
	{
		Ok_Collapsed,
		Ignored_CannotCollapse,
		Ignored_EdgeIsFullyConstrained,
		Ignored_EdgeTooLong,
		Ignored_Constrained,
		Ignored_CreatesFlip,
		Failed_OpNotSuccessful,
		Failed_NotAnEdge
	}

	protected IProjectionTarget target;

	public bool MinimizeQuadricPositionError = true;

	public bool PreserveBoundaryShape = true;

	public List<int> DebugEdges = new List<int>();

	public TargetProjectionMode ProjectionMode = TargetProjectionMode.AfterRefinement;

	public bool ENABLE_PROFILING;

	protected double MinEdgeLength = double.MaxValue;

	protected int TargetCount = int.MaxValue;

	protected TargetModes ReduceMode;

	protected QuadricError[] vertQuadrics;

	protected QEdge[] EdgeQuadrics;

	protected IndexPriorityQueue EdgeQueue;

	protected bool HaveBoundary;

	protected bool[] IsBoundaryVtxCache;

	private const int nPrime = 31337;

	private int nMaxEdgeID;

	public bool ENABLE_DEBUG_CHECKS;

	private int COUNT_COLLAPSES;

	private int COUNT_ITERATIONS;

	private Stopwatch AllOpsW;

	private Stopwatch SetupW;

	private Stopwatch ProjectW;

	private Stopwatch CollapseW;

	private bool EnableInlineProjection => ProjectionMode == TargetProjectionMode.Inline;

	public Reducer(DMesh3 m)
		: base(m)
	{
	}

	protected Reducer()
	{
	}

	public void SetProjectionTarget(IProjectionTarget target)
	{
		this.target = target;
	}

	public virtual void DoReduce()
	{
		if (mesh.TriangleCount == 0)
		{
			return;
		}
		begin_pass();
		begin_setup();
		Precompute();
		if (Cancelled())
		{
			return;
		}
		InitializeVertexQuadrics();
		if (Cancelled())
		{
			return;
		}
		InitializeQueue();
		if (Cancelled())
		{
			return;
		}
		end_setup();
		begin_ops();
		begin_collapse();
		while (EdgeQueue.Count > 0)
		{
			if (ReduceMode == TargetModes.VertexCount)
			{
				if (mesh.VertexCount <= TargetCount)
				{
					break;
				}
			}
			else if (mesh.TriangleCount <= TargetCount)
			{
				break;
			}
			COUNT_ITERATIONS++;
			int num = EdgeQueue.Dequeue();
			if (mesh.IsEdge(num))
			{
				if (Cancelled())
				{
					return;
				}
				if (CollapseEdge(num, EdgeQuadrics[num].collapse_pt, out var collapseToV) == ProcessResult.Ok_Collapsed)
				{
					vertQuadrics[collapseToV] = EdgeQuadrics[num].q;
					UpdateNeighbours(collapseToV);
				}
			}
		}
		end_collapse();
		end_ops();
		if (!Cancelled())
		{
			Reproject();
			end_pass();
		}
	}

	public virtual void ReduceToTriangleCount(int nCount)
	{
		ReduceMode = TargetModes.TriangleCount;
		TargetCount = Math.Max(1, nCount);
		MinEdgeLength = double.MaxValue;
		DoReduce();
	}

	public virtual void ReduceToVertexCount(int nCount)
	{
		ReduceMode = TargetModes.VertexCount;
		TargetCount = Math.Max(3, nCount);
		MinEdgeLength = double.MaxValue;
		DoReduce();
	}

	public virtual void ReduceToEdgeLength(double minEdgeLen)
	{
		ReduceMode = TargetModes.MinEdgeLength;
		TargetCount = 1;
		MinEdgeLength = minEdgeLen;
		DoReduce();
	}

	public virtual void FastCollapsePass(double fMinEdgeLength, int nRounds = 1, bool MeshIsClosedHint = false)
	{
		if (mesh.TriangleCount == 0)
		{
			return;
		}
		MinEdgeLength = fMinEdgeLength;
		double num = MinEdgeLength * MinEdgeLength;
		HaveBoundary = false;
		begin_pass();
		begin_setup();
		Precompute(MeshIsClosedHint);
		if (Cancelled())
		{
			return;
		}
		end_setup();
		begin_ops();
		begin_collapse();
		int maxEdgeID = mesh.MaxEdgeID;
		int num2 = 0;
		for (int i = 0; i < nRounds; i++)
		{
			num2 = 0;
			Vector3d a = Vector3d.Zero;
			Vector3d b = Vector3d.Zero;
			for (int j = 0; j < maxEdgeID; j++)
			{
				if (!mesh.IsEdge(j) || mesh.IsBoundaryEdge(j))
				{
					continue;
				}
				if (Cancelled())
				{
					return;
				}
				mesh.GetEdgeV(j, ref a, ref b);
				if (!(a.DistanceSquared(ref b) > num))
				{
					COUNT_ITERATIONS++;
					Vector3d vNewPos = (a + b) * 0.5;
					if (CollapseEdge(j, vNewPos, out var _) == ProcessResult.Ok_Collapsed)
					{
						num2++;
					}
				}
			}
			if (num2 == 0)
			{
				break;
			}
		}
		end_collapse();
		end_ops();
		if (!Cancelled())
		{
			Reproject();
			end_pass();
		}
	}

	protected virtual void InitializeVertexQuadrics()
	{
		int maxTriangleID = mesh.MaxTriangleID;
		QuadricError[] triQuadrics = new QuadricError[maxTriangleID];
		double[] triAreas = new double[maxTriangleID];
		gParallel.BlockStartEnd(0, mesh.MaxTriangleID - 1, delegate(int start_tid, int end_tid)
		{
			for (int j = start_tid; j <= end_tid; j++)
			{
				if (mesh.IsTriangle(j))
				{
					mesh.GetTriInfo(j, out var normal, out triAreas[j], out var vCentroid);
					triQuadrics[j] = new QuadricError(ref normal, ref vCentroid);
				}
			}
		});
		int maxVertexID = mesh.MaxVertexID;
		vertQuadrics = new QuadricError[maxVertexID];
		gParallel.BlockStartEnd(0, mesh.MaxVertexID - 1, delegate(int start_vid, int end_vid)
		{
			for (int i = start_vid; i <= end_vid; i++)
			{
				vertQuadrics[i] = QuadricError.Zero;
				if (mesh.IsVertex(i))
				{
					foreach (int item in mesh.VtxTrianglesItr(i))
					{
						vertQuadrics[i].Add(triAreas[item], ref triQuadrics[item]);
					}
				}
			}
		});
	}

	protected virtual void InitializeQueue()
	{
		_ = mesh.EdgeCount;
		int maxEdgeID = mesh.MaxEdgeID;
		EdgeQuadrics = new QEdge[maxEdgeID];
		EdgeQueue = new IndexPriorityQueue(maxEdgeID);
		float[] edgeErrors = new float[maxEdgeID];
		gParallel.BlockStartEnd(0, maxEdgeID - 1, delegate(int start_eid, int end_eid)
		{
			for (int k = start_eid; k <= end_eid; k++)
			{
				if (mesh.IsEdge(k))
				{
					Index2i edgeV = mesh.GetEdgeV(k);
					QuadricError q = new QuadricError(ref vertQuadrics[edgeV.a], ref vertQuadrics[edgeV.b]);
					Vector3d pt = OptimalPoint(k, ref q, edgeV.a, edgeV.b);
					edgeErrors[k] = (float)q.Evaluate(ref pt);
					EdgeQuadrics[k] = new QEdge(k, ref q, ref pt);
				}
			}
		});
		int[] array = new int[maxEdgeID];
		for (int i = 0; i < maxEdgeID; i++)
		{
			array[i] = i;
		}
		Array.Sort(edgeErrors, array);
		for (int j = 0; j < edgeErrors.Length; j++)
		{
			int num = array[j];
			if (mesh.IsEdge(num))
			{
				QEdge qEdge = EdgeQuadrics[num];
				EdgeQueue.Insert(qEdge.eid, edgeErrors[j]);
			}
		}
	}

	protected Vector3d OptimalPoint(int eid, ref QuadricError q, int ea, int eb)
	{
		if (HaveBoundary && PreserveBoundaryShape)
		{
			if (mesh.IsBoundaryEdge(eid))
			{
				return (mesh.GetVertex(ea) + mesh.GetVertex(eb)) * 0.5;
			}
			if (IsBoundaryV(ea))
			{
				return mesh.GetVertex(ea);
			}
			if (IsBoundaryV(eb))
			{
				return mesh.GetVertex(eb);
			}
		}
		if (!MinimizeQuadricPositionError)
		{
			return project((mesh.GetVertex(ea) + mesh.GetVertex(eb)) * 0.5);
		}
		Vector3d result = Vector3d.Zero;
		if (q.OptimalPoint(ref result))
		{
			return project(result);
		}
		Vector3d pt = mesh.GetVertex(ea);
		Vector3d pt2 = mesh.GetVertex(eb);
		Vector3d pt3 = project((pt + pt2) * 0.5);
		double num = q.Evaluate(ref pt);
		double num2 = q.Evaluate(ref pt2);
		double c = q.Evaluate(ref pt3);
		double num3 = MathUtil.Min(num, num2, c);
		if (num3 == num)
		{
			return pt;
		}
		if (num3 == num2)
		{
			return pt2;
		}
		return pt3;
	}

	private Vector3d project(Vector3d pos)
	{
		if (EnableInlineProjection && target != null)
		{
			return target.Project(pos);
		}
		return pos;
	}

	protected virtual void UpdateNeighbours(int vid)
	{
		foreach (int item in mesh.VtxEdgesItr(vid))
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			QuadricError q = new QuadricError(ref vertQuadrics[edgeV.a], ref vertQuadrics[edgeV.b]);
			Vector3d pt = OptimalPoint(item, ref q, edgeV.a, edgeV.b);
			double num = q.Evaluate(ref pt);
			EdgeQuadrics[item] = new QEdge(item, ref q, ref pt);
			if (EdgeQueue.Contains(item))
			{
				EdgeQueue.Update(item, (float)num);
			}
			else
			{
				EdgeQueue.Insert(item, (float)num);
			}
		}
	}

	protected virtual void Reproject()
	{
		begin_project();
		if (target != null && ProjectionMode == TargetProjectionMode.AfterRefinement)
		{
			FullProjectionPass();
			DoDebugChecks();
		}
		end_project();
	}

	protected virtual void Precompute(bool bMeshIsClosed = false)
	{
		HaveBoundary = false;
		IsBoundaryVtxCache = new bool[mesh.MaxVertexID];
		if (bMeshIsClosed)
		{
			return;
		}
		foreach (int item in mesh.BoundaryEdgeIndices())
		{
			Index2i edgeV = mesh.GetEdgeV(item);
			IsBoundaryVtxCache[edgeV.a] = true;
			IsBoundaryVtxCache[edgeV.b] = true;
			HaveBoundary = true;
		}
	}

	protected bool IsBoundaryV(int vid)
	{
		return IsBoundaryVtxCache[vid];
	}

	protected virtual void OnEdgeCollapse(int edgeID, int va, int vb, DMesh3.EdgeCollapseInfo collapseInfo)
	{
	}

	protected virtual int start_edges()
	{
		nMaxEdgeID = mesh.MaxEdgeID;
		return 0;
	}

	protected virtual int next_edge(int cur_eid, out bool bDone)
	{
		int num = (cur_eid + 31337) % nMaxEdgeID;
		bDone = num == 0;
		return num;
	}

	protected virtual IEnumerable<int> project_vertices()
	{
		return mesh.VertexIndices();
	}

	protected virtual ProcessResult CollapseEdge(int edgeID, Vector3d vNewPos, out int collapseToV)
	{
		collapseToV = -1;
		EdgeConstraint edgeConstraint = ((constraints == null) ? EdgeConstraint.Unconstrained : constraints.GetEdgeConstraint(edgeID));
		if (edgeConstraint.NoModifications)
		{
			return ProcessResult.Ignored_EdgeIsFullyConstrained;
		}
		if (!edgeConstraint.CanCollapse)
		{
			return ProcessResult.Ignored_EdgeIsFullyConstrained;
		}
		int a = 0;
		int b = 0;
		int t = 0;
		int t2 = 0;
		if (!mesh.GetEdge(edgeID, ref a, ref b, ref t, ref t2))
		{
			return ProcessResult.Failed_NotAnEdge;
		}
		bool num = t2 == -1;
		int c = IndexUtil.find_tri_other_vtx(tri_verts: mesh.GetTriangle(t), a: a, b: b);
		Index3i tri_verts = (num ? DMesh3.InvalidTriangle : mesh.GetTriangle(t2));
		int d = (num ? (-1) : IndexUtil.find_tri_other_vtx(a, b, tri_verts));
		Vector3d vertex = mesh.GetVertex(a);
		Vector3d vertex2 = mesh.GetVertex(b);
		if ((vertex - vertex2).LengthSquared > MinEdgeLength * MinEdgeLength)
		{
			return ProcessResult.Ignored_EdgeTooLong;
		}
		begin_collapse();
		int collapse_to = -1;
		if (!can_collapse_constraints(edgeID, a, b, c, d, t, t2, out collapse_to))
		{
			return ProcessResult.Ignored_Constrained;
		}
		if (PreserveBoundaryShape && HaveBoundary)
		{
			if (collapse_to != -1 && ((IsBoundaryV(b) && collapse_to != b) || (IsBoundaryV(a) && collapse_to != a)))
			{
				return ProcessResult.Ignored_Constrained;
			}
			if (IsBoundaryV(b))
			{
				collapse_to = b;
			}
			else if (IsBoundaryV(a))
			{
				collapse_to = a;
			}
		}
		ProcessResult result = ProcessResult.Failed_OpNotSuccessful;
		int num2 = b;
		int num3 = a;
		if (collapse_to == b)
		{
			vNewPos = vertex2;
		}
		else if (collapse_to == a)
		{
			num2 = a;
			num3 = b;
			vNewPos = vertex;
		}
		else
		{
			vNewPos = get_projected_collapse_position(num2, vNewPos);
		}
		if (collapse_creates_flip_or_invalid(a, b, ref vNewPos, t, t2) || collapse_creates_flip_or_invalid(b, a, ref vNewPos, t, t2))
		{
			result = ProcessResult.Ignored_CreatesFlip;
		}
		else
		{
			COUNT_COLLAPSES++;
			if (mesh.CollapseEdge(num2, num3, out var collapse) == MeshResult.Ok)
			{
				collapseToV = num2;
				mesh.SetVertex(num2, vNewPos);
				if (constraints != null)
				{
					constraints.ClearEdgeConstraint(edgeID);
					constraints.ClearEdgeConstraint(collapse.eRemoved0);
					if (collapse.eRemoved1 != -1)
					{
						constraints.ClearEdgeConstraint(collapse.eRemoved1);
					}
					constraints.ClearVertexConstraint(num3);
				}
				OnEdgeCollapse(edgeID, num2, num3, collapse);
				DoDebugChecks();
				result = ProcessResult.Ok_Collapsed;
			}
		}
		end_collapse();
		return result;
	}

	protected void project_vertex(int vID, IProjectionTarget targetIn)
	{
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d vNewPos = targetIn.Project(vertex, vID);
		mesh.SetVertex(vID, vNewPos);
	}

	protected Vector3d get_projected_collapse_position(int vid, Vector3d vNewPos)
	{
		if (constraints != null)
		{
			VertexConstraint vertexConstraint = constraints.GetVertexConstraint(vid);
			if (vertexConstraint.Target != null)
			{
				return vertexConstraint.Target.Project(vNewPos, vid);
			}
			_ = vertexConstraint.Fixed;
			return vNewPos;
		}
		return vNewPos;
	}

	protected virtual void FullProjectionPass()
	{
		Action<int> body = delegate(int vID)
		{
			if (!vertex_is_constrained(vID))
			{
				Vector3d vertex = mesh.GetVertex(vID);
				Vector3d vNewPos = target.Project(vertex, vID);
				mesh.SetVertex(vID, vNewPos);
			}
		};
		gParallel.ForEach(project_vertices(), body);
	}

	[Conditional("DEBUG")]
	protected virtual void RuntimeDebugCheck(int eid)
	{
		if (DebugEdges.Contains(eid))
		{
			Debugger.Break();
		}
	}

	protected virtual void DoDebugChecks()
	{
		if (ENABLE_DEBUG_CHECKS)
		{
			DebugCheckVertexConstraints();
		}
	}

	protected virtual void DebugCheckVertexConstraints()
	{
		if (constraints == null)
		{
			return;
		}
		foreach (KeyValuePair<int, VertexConstraint> item in constraints.VertexConstraintsItr())
		{
			int key = item.Key;
			if (item.Value.Target != null)
			{
				Vector3d vertex = mesh.GetVertex(key);
				Vector3d v = item.Value.Target.Project(vertex, key);
				if (vertex.DistanceSquared(v) > 9.999999747378752E-05)
				{
					Util.gBreakToDebugger();
				}
			}
		}
	}

	protected virtual void begin_pass()
	{
		if (ENABLE_PROFILING)
		{
			COUNT_COLLAPSES = 0;
			COUNT_ITERATIONS = 0;
			AllOpsW = new Stopwatch();
			SetupW = new Stopwatch();
			ProjectW = new Stopwatch();
			CollapseW = new Stopwatch();
		}
	}

	protected virtual void end_pass()
	{
		if (ENABLE_PROFILING)
		{
			Console.WriteLine($"ReducePass: T {mesh.TriangleCount} V {mesh.VertexCount} collapses {COUNT_COLLAPSES}  iterations {COUNT_ITERATIONS}");
			Console.WriteLine($"           Timing1: setup {Util.ToSecMilli(SetupW.Elapsed)} ops {Util.ToSecMilli(AllOpsW.Elapsed)} project {Util.ToSecMilli(ProjectW.Elapsed)}");
		}
	}

	protected virtual void begin_ops()
	{
		if (ENABLE_PROFILING)
		{
			AllOpsW.Start();
		}
	}

	protected virtual void end_ops()
	{
		if (ENABLE_PROFILING)
		{
			AllOpsW.Stop();
		}
	}

	protected virtual void begin_setup()
	{
		if (ENABLE_PROFILING)
		{
			SetupW.Start();
		}
	}

	protected virtual void end_setup()
	{
		if (ENABLE_PROFILING)
		{
			SetupW.Stop();
		}
	}

	protected virtual void begin_project()
	{
		if (ENABLE_PROFILING)
		{
			ProjectW.Start();
		}
	}

	protected virtual void end_project()
	{
		if (ENABLE_PROFILING)
		{
			ProjectW.Stop();
		}
	}

	protected virtual void begin_collapse()
	{
		if (ENABLE_PROFILING)
		{
			CollapseW.Start();
		}
	}

	protected virtual void end_collapse()
	{
		if (ENABLE_PROFILING)
		{
			CollapseW.Stop();
		}
	}
}
