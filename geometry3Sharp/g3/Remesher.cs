using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace g3;

public class Remesher : MeshRefinerBase
{
	public enum SmoothTypes
	{
		Uniform,
		Cotan,
		MeanValue
	}

	[Flags]
	public enum VertexControl
	{
		AllowAll = 0,
		NoSmooth = 1,
		NoProject = 2,
		NoMovement = 3
	}

	public enum TargetProjectionMode
	{
		NoProjection,
		AfterRefinement,
		Inline
	}

	protected enum ProcessResult
	{
		Ok_Collapsed,
		Ok_Flipped,
		Ok_Split,
		Ignored_EdgeIsFine,
		Ignored_EdgeIsFullyConstrained,
		Failed_OpNotSuccessful,
		Failed_NotAnEdge
	}

	private IProjectionTarget target;

	public bool EnableFlips = true;

	public bool EnableCollapses = true;

	public bool EnableSplits = true;

	public bool EnableSmoothing = true;

	public bool PreventNormalFlips;

	public double MinEdgeLength = 0.0010000000474974513;

	public double MaxEdgeLength = 0.10000000149011612;

	public double SmoothSpeedT = 0.10000000149011612;

	public SmoothTypes SmoothType;

	public Func<DMesh3, int, double, Vector3d> CustomSmoothF;

	public Func<int, VertexControl> VertexControlF;

	public List<int> DebugEdges = new List<int>();

	public TargetProjectionMode ProjectionMode = TargetProjectionMode.AfterRefinement;

	public bool EnableParallelProjection = true;

	public bool EnableParallelSmooth = true;

	public bool EnableSmoothInPlace;

	public bool ENABLE_PROFILING;

	private bool MeshIsClosed;

	public int ModifiedEdgesLastPass;

	private const int nPrime = 31337;

	private int nMaxEdgeID;

	protected DVector<Vector3d> vBufferV = new DVector<Vector3d>();

	protected BitArray vModifiedV = new BitArray(4096);

	public bool ENABLE_DEBUG_CHECKS;

	private int COUNT_SPLITS;

	private int COUNT_COLLAPSES;

	private int COUNT_FLIPS;

	private Stopwatch AllOpsW;

	private Stopwatch SmoothW;

	private Stopwatch ProjectW;

	private Stopwatch FlipW;

	private Stopwatch SplitW;

	private Stopwatch CollapseW;

	private bool EnableInlineProjection => ProjectionMode == TargetProjectionMode.Inline;

	public IProjectionTarget ProjectionTarget => target;

	public Remesher(DMesh3 m)
		: base(m)
	{
	}

	protected Remesher()
	{
	}

	public void SetProjectionTarget(IProjectionTarget target)
	{
		this.target = target;
	}

	public void SetTargetEdgeLength(double fLength)
	{
		MinEdgeLength = fLength * 0.66;
		MaxEdgeLength = fLength * 1.33;
	}

	public virtual void Precompute()
	{
		MeshIsClosed = true;
		foreach (int item in mesh.EdgeIndices())
		{
			if (mesh.IsBoundaryEdge(item))
			{
				MeshIsClosed = false;
				break;
			}
		}
	}

	public virtual void BasicRemeshPass()
	{
		if (mesh.TriangleCount == 0)
		{
			return;
		}
		begin_pass();
		begin_ops();
		int num = start_edges();
		bool bDone = false;
		ModifiedEdgesLastPass = 0;
		do
		{
			if (mesh.IsEdge(num))
			{
				ProcessResult processResult = ProcessEdge(num);
				if (processResult == ProcessResult.Ok_Collapsed || processResult == ProcessResult.Ok_Flipped || processResult == ProcessResult.Ok_Split)
				{
					ModifiedEdgesLastPass++;
				}
			}
			if (Cancelled())
			{
				return;
			}
			num = next_edge(num, out bDone);
		}
		while (!bDone);
		end_ops();
		if (Cancelled())
		{
			return;
		}
		begin_smooth();
		if (EnableSmoothing && SmoothSpeedT > 0.0)
		{
			if (EnableSmoothInPlace)
			{
				FullSmoothPass_InPlace(EnableParallelSmooth);
			}
			else
			{
				FullSmoothPass_Buffer(EnableParallelSmooth);
			}
			DoDebugChecks();
		}
		end_smooth();
		if (!Cancelled())
		{
			begin_project();
			if (target != null && ProjectionMode == TargetProjectionMode.AfterRefinement)
			{
				FullProjectionPass();
				DoDebugChecks();
			}
			end_project();
			if (!Cancelled())
			{
				end_pass();
			}
		}
	}

	protected virtual void OnEdgeSplit(int edgeID, int va, int vb, DMesh3.EdgeSplitInfo splitInfo)
	{
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

	protected virtual IEnumerable<int> smooth_vertices()
	{
		return mesh.VertexIndices();
	}

	protected virtual IEnumerable<int> project_vertices()
	{
		return mesh.VertexIndices();
	}

	protected virtual ProcessResult ProcessEdge(int edgeID)
	{
		EdgeConstraint edgeConstraint = ((constraints == null) ? EdgeConstraint.Unconstrained : constraints.GetEdgeConstraint(edgeID));
		if (edgeConstraint.NoModifications)
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
		bool flag = t2 == -1;
		Index2i edgeOpposingV = mesh.GetEdgeOpposingV(edgeID);
		int a2 = edgeOpposingV.a;
		int b2 = edgeOpposingV.b;
		Vector3d vertex = mesh.GetVertex(a);
		Vector3d vertex2 = mesh.GetVertex(b);
		double num = vertex.DistanceSquared(vertex2);
		begin_collapse();
		int collapse_to = -1;
		bool num2 = EnableCollapses && edgeConstraint.CanCollapse && num < MinEdgeLength * MinEdgeLength && can_collapse_constraints(edgeID, a, b, a2, b2, t, t2, out collapse_to);
		bool flag2 = false;
		if (num2)
		{
			int num3 = b;
			int num4 = a;
			Vector3d vNewPos = (vertex + vertex2) * 0.5;
			if (collapse_to == b)
			{
				vNewPos = vertex2;
			}
			else if (collapse_to == a)
			{
				num3 = a;
				num4 = b;
				vNewPos = vertex;
			}
			else
			{
				vNewPos = get_projected_collapse_position(num3, vNewPos);
			}
			if (!PreventNormalFlips || (!collapse_creates_flip_or_invalid(a, b, ref vNewPos, t, t2) && !collapse_creates_flip_or_invalid(b, a, ref vNewPos, t, t2)))
			{
				COUNT_COLLAPSES++;
				if (mesh.CollapseEdge(num3, num4, out var collapse) == MeshResult.Ok)
				{
					mesh.SetVertex(num3, vNewPos);
					if (constraints != null)
					{
						constraints.ClearEdgeConstraint(edgeID);
						constraints.ClearEdgeConstraint(collapse.eRemoved0);
						if (collapse.eRemoved1 != -1)
						{
							constraints.ClearEdgeConstraint(collapse.eRemoved1);
						}
						constraints.ClearVertexConstraint(num4);
					}
					OnEdgeCollapse(edgeID, num3, num4, collapse);
					DoDebugChecks();
					return ProcessResult.Ok_Collapsed;
				}
				flag2 = true;
			}
		}
		end_collapse();
		begin_flip();
		bool flag3 = false;
		if (EnableFlips && edgeConstraint.CanFlip && !flag)
		{
			bool flag4 = !MeshIsClosed && (flag || mesh.IsBoundaryVertex(a));
			bool flag5 = !MeshIsClosed && (flag || mesh.IsBoundaryVertex(b));
			bool flag6 = !MeshIsClosed && mesh.IsBoundaryVertex(a2);
			bool num5 = !MeshIsClosed && mesh.IsBoundaryVertex(b2);
			int vtxEdgeCount = mesh.GetVtxEdgeCount(a);
			int vtxEdgeCount2 = mesh.GetVtxEdgeCount(b);
			int vtxEdgeCount3 = mesh.GetVtxEdgeCount(a2);
			int vtxEdgeCount4 = mesh.GetVtxEdgeCount(b2);
			int num6 = (flag4 ? vtxEdgeCount : 6);
			int num7 = (flag5 ? vtxEdgeCount2 : 6);
			int num8 = (flag6 ? vtxEdgeCount3 : 6);
			int num9 = (num5 ? vtxEdgeCount4 : 6);
			int num10 = Math.Abs(vtxEdgeCount - num6) + Math.Abs(vtxEdgeCount2 - num7) + Math.Abs(vtxEdgeCount3 - num8) + Math.Abs(vtxEdgeCount4 - num9);
			bool flag7 = Math.Abs(vtxEdgeCount - 1 - num6) + Math.Abs(vtxEdgeCount2 - 1 - num7) + Math.Abs(vtxEdgeCount3 + 1 - num8) + Math.Abs(vtxEdgeCount4 + 1 - num9) < num10;
			if (flag7 && PreventNormalFlips && flip_inverts_normals(a, b, a2, b2, t))
			{
				flag7 = false;
			}
			if (flag7)
			{
				COUNT_FLIPS++;
				if (mesh.FlipEdge(edgeID, out var _) == MeshResult.Ok)
				{
					DoDebugChecks();
					return ProcessResult.Ok_Flipped;
				}
				flag3 = true;
			}
		}
		end_flip();
		begin_split();
		bool flag8 = false;
		if (EnableSplits && edgeConstraint.CanSplit && num > MaxEdgeLength * MaxEdgeLength)
		{
			COUNT_SPLITS++;
			if (mesh.SplitEdge(edgeID, out var split) == MeshResult.Ok)
			{
				update_after_split(edgeID, a, b, ref split);
				OnEdgeSplit(edgeID, a, b, split);
				DoDebugChecks();
				return ProcessResult.Ok_Split;
			}
			flag8 = true;
		}
		end_split();
		if (flag3 || flag8 || flag2)
		{
			return ProcessResult.Failed_OpNotSuccessful;
		}
		return ProcessResult.Ignored_EdgeIsFine;
	}

	protected virtual void update_after_split(int edgeID, int va, int vb, ref DMesh3.EdgeSplitInfo splitInfo)
	{
		bool flag = false;
		if (constraints != null && constraints.HasEdgeConstraint(edgeID))
		{
			constraints.SetOrUpdateEdgeConstraint(splitInfo.eNewBN, constraints.GetEdgeConstraint(edgeID));
			VertexConstraint vertexConstraint = constraints.GetVertexConstraint(va);
			VertexConstraint vertexConstraint2 = constraints.GetVertexConstraint(vb);
			if (vertexConstraint.Fixed && vertexConstraint2.Fixed)
			{
				int setID = ((vertexConstraint.FixedSetID > 0 && vertexConstraint.FixedSetID == vertexConstraint2.FixedSetID) ? vertexConstraint.FixedSetID : (-1));
				constraints.SetOrUpdateVertexConstraint(splitInfo.vNew, new VertexConstraint(isFixed: true, setID));
				flag = true;
			}
			if (vertexConstraint.Target != null || vertexConstraint2.Target != null)
			{
				IProjectionTarget projectionTarget = constraints.GetEdgeConstraint(edgeID).Target;
				IProjectionTarget projectionTarget2 = null;
				if (vertexConstraint.Target == vertexConstraint2.Target && vertexConstraint.Target == projectionTarget)
				{
					projectionTarget2 = projectionTarget;
				}
				else if (vertexConstraint.Target == projectionTarget && vertexConstraint2.Fixed)
				{
					projectionTarget2 = projectionTarget;
				}
				else if (vertexConstraint2.Target == projectionTarget && vertexConstraint.Fixed)
				{
					projectionTarget2 = projectionTarget;
				}
				if (projectionTarget2 != null)
				{
					constraints.SetOrUpdateVertexConstraint(splitInfo.vNew, new VertexConstraint(projectionTarget2));
					project_vertex(splitInfo.vNew, projectionTarget2);
					flag = true;
				}
			}
		}
		if (EnableInlineProjection && !flag && target != null)
		{
			project_vertex(splitInfo.vNew, target);
		}
	}

	protected virtual void project_vertex(int vID, IProjectionTarget targetIn)
	{
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d vNewPos = targetIn.Project(vertex, vID);
		mesh.SetVertex(vID, vNewPos);
	}

	protected virtual Vector3d get_projected_collapse_position(int vid, Vector3d vNewPos)
	{
		if (constraints != null)
		{
			VertexConstraint vertexConstraint = constraints.GetVertexConstraint(vid);
			if (vertexConstraint.Target != null)
			{
				return vertexConstraint.Target.Project(vNewPos, vid);
			}
			if (vertexConstraint.Fixed)
			{
				return vNewPos;
			}
		}
		if (EnableInlineProjection && target != null && (VertexControlF == null || (VertexControlF(vid) & VertexControl.NoProject) == 0))
		{
			return target.Project(vNewPos, vid);
		}
		return vNewPos;
	}

	protected virtual void FullSmoothPass_InPlace(bool bParallel)
	{
		Func<DMesh3, int, double, Vector3d> smoothFunc = MeshUtil.UniformSmooth;
		if (CustomSmoothF != null)
		{
			smoothFunc = CustomSmoothF;
		}
		else if (SmoothType == SmoothTypes.MeanValue)
		{
			smoothFunc = MeshUtil.MeanValueSmooth;
		}
		else if (SmoothType == SmoothTypes.Cotan)
		{
			smoothFunc = MeshUtil.CotanSmooth;
		}
		Action<int> action = delegate(int vID)
		{
			bool bModified = false;
			Vector3d vNewPos = ComputeSmoothedVertexPos(vID, smoothFunc, out bModified);
			if (bModified)
			{
				mesh.SetVertex(vID, vNewPos);
			}
		};
		if (bParallel)
		{
			gParallel.ForEach(smooth_vertices(), action);
			return;
		}
		foreach (int item in smooth_vertices())
		{
			action(item);
		}
	}

	protected virtual void FullSmoothPass_Buffer(bool bParallel)
	{
		InitializeVertexBufferForPass();
		Func<DMesh3, int, double, Vector3d> smoothFunc = MeshUtil.UniformSmooth;
		if (CustomSmoothF != null)
		{
			smoothFunc = CustomSmoothF;
		}
		else if (SmoothType == SmoothTypes.MeanValue)
		{
			smoothFunc = MeshUtil.MeanValueSmooth;
		}
		else if (SmoothType == SmoothTypes.Cotan)
		{
			smoothFunc = MeshUtil.CotanSmooth;
		}
		Action<int> action = delegate(int vID)
		{
			bool bModified = false;
			Vector3d value = ComputeSmoothedVertexPos(vID, smoothFunc, out bModified);
			if (bModified)
			{
				vModifiedV[vID] = true;
				vBufferV[vID] = value;
			}
		};
		if (bParallel)
		{
			gParallel.ForEach(smooth_vertices(), action);
		}
		else
		{
			foreach (int item in smooth_vertices())
			{
				action(item);
			}
		}
		ApplyVertexBuffer(bParallel);
	}

	protected virtual void InitializeVertexBufferForPass()
	{
		if (vBufferV.size < mesh.MaxVertexID)
		{
			vBufferV.resize(mesh.MaxVertexID + mesh.MaxVertexID / 5);
		}
		if (vModifiedV.Length < mesh.MaxVertexID)
		{
			vModifiedV = new BitArray(2 * mesh.MaxVertexID);
		}
		else
		{
			vModifiedV.SetAll(value: false);
		}
	}

	protected virtual void ApplyVertexBuffer(bool bParallel)
	{
		if (bParallel)
		{
			gParallel.BlockStartEnd(0, mesh.MaxVertexID - 1, delegate(int a, int b)
			{
				for (int i = a; i <= b; i++)
				{
					if (vModifiedV[i])
					{
						base.Mesh.SetVertex(i, vBufferV[i]);
					}
				}
			});
			return;
		}
		foreach (int item in mesh.VertexIndices())
		{
			if (vModifiedV[item])
			{
				base.Mesh.SetVertex(item, vBufferV[item]);
			}
		}
	}

	protected virtual Vector3d ComputeSmoothedVertexPos(int vID, Func<DMesh3, int, double, Vector3d> smoothFunc, out bool bModified)
	{
		bModified = false;
		VertexConstraint vc = VertexConstraint.Unconstrained;
		get_vertex_constraint(vID, ref vc);
		if (vc.Fixed)
		{
			return base.Mesh.GetVertex(vID);
		}
		VertexControl vertexControl = ((VertexControlF != null) ? VertexControlF(vID) : VertexControl.AllowAll);
		if ((vertexControl & VertexControl.NoSmooth) != 0)
		{
			return base.Mesh.GetVertex(vID);
		}
		Vector3d vector3d = smoothFunc(mesh, vID, SmoothSpeedT);
		if (vc.Target != null)
		{
			vector3d = vc.Target.Project(vector3d, vID);
		}
		else if (EnableInlineProjection && target != null && (vertexControl & VertexControl.NoProject) == 0)
		{
			vector3d = target.Project(vector3d, vID);
		}
		bModified = true;
		return vector3d;
	}

	protected virtual void FullProjectionPass()
	{
		Action<int> action = delegate(int vID)
		{
			if (!vertex_is_constrained(vID) && (VertexControlF == null || (VertexControlF(vID) & VertexControl.NoProject) == 0))
			{
				Vector3d vertex = mesh.GetVertex(vID);
				Vector3d vNewPos = target.Project(vertex, vID);
				mesh.SetVertex(vID, vNewPos);
			}
		};
		if (EnableParallelProjection)
		{
			gParallel.ForEach(project_vertices(), action);
			return;
		}
		foreach (int item in project_vertices())
		{
			action(item);
		}
	}

	[Conditional("DEBUG")]
	private void RuntimeDebugCheck(int eid)
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

	private void DebugCheckVertexConstraints()
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
			COUNT_SPLITS = (COUNT_COLLAPSES = (COUNT_FLIPS = 0));
			AllOpsW = new Stopwatch();
			SmoothW = new Stopwatch();
			ProjectW = new Stopwatch();
			FlipW = new Stopwatch();
			SplitW = new Stopwatch();
			CollapseW = new Stopwatch();
		}
	}

	protected virtual void end_pass()
	{
		if (ENABLE_PROFILING)
		{
			Console.WriteLine($"RemeshPass: T {mesh.TriangleCount} V {mesh.VertexCount} splits {COUNT_SPLITS} flips {COUNT_FLIPS} collapses {COUNT_COLLAPSES}");
			Console.WriteLine($"           Timing1:  ops {Util.ToSecMilli(AllOpsW.Elapsed)} smooth {Util.ToSecMilli(SmoothW.Elapsed)} project {Util.ToSecMilli(ProjectW.Elapsed)}");
			Console.WriteLine($"           Timing2:  collapse {Util.ToSecMilli(CollapseW.Elapsed)} flip {Util.ToSecMilli(FlipW.Elapsed)} split {Util.ToSecMilli(SplitW.Elapsed)}");
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

	protected virtual void begin_smooth()
	{
		if (ENABLE_PROFILING)
		{
			SmoothW.Start();
		}
	}

	protected virtual void end_smooth()
	{
		if (ENABLE_PROFILING)
		{
			SmoothW.Stop();
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

	protected virtual void begin_flip()
	{
		if (ENABLE_PROFILING)
		{
			FlipW.Start();
		}
	}

	protected virtual void end_flip()
	{
		if (ENABLE_PROFILING)
		{
			FlipW.Stop();
		}
	}

	protected virtual void begin_split()
	{
		if (ENABLE_PROFILING)
		{
			SplitW.Start();
		}
	}

	protected virtual void end_split()
	{
		if (ENABLE_PROFILING)
		{
			SplitW.Stop();
		}
	}
}
