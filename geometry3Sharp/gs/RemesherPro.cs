using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using g3;

namespace gs;

public class RemesherPro : Remesher
{
	private struct SettingState
	{
		public bool EnableFlips;

		public bool EnableCollapses;

		public bool EnableSplits;

		public bool EnableSmoothing;

		public double MinEdgeLength;

		public double MaxEdgeLength;

		public double SmoothSpeedT;

		public SmoothTypes SmoothType;

		public TargetProjectionMode ProjectionMode;
	}

	public bool UseFaceAlignedProjection;

	public int FaceProjectionPassesPerIteration = 1;

	private HashSet<int> modified_edges;

	private SpinLock modified_edges_lock;

	private Action<int, int, int, int> SplitF;

	private List<int> edges_buffer = new List<int>();

	protected DVector<double> vBufferVWeights = new DVector<double>();

	private List<SettingState> stateStack = new List<SettingState>();

	public RemesherPro(DMesh3 m)
		: base(m)
	{
	}

	protected IEnumerable<int> EdgesIterator()
	{
		int cur_eid = start_edges();
		bool bDone;
		do
		{
			yield return cur_eid;
			cur_eid = next_edge(cur_eid, out bDone);
		}
		while (!bDone);
	}

	private void queue_one_ring_safe(int vid)
	{
		if (!mesh.IsVertex(vid))
		{
			return;
		}
		bool lockTaken = false;
		modified_edges_lock.Enter(ref lockTaken);
		foreach (int item in mesh.VtxEdgesItr(vid))
		{
			modified_edges.Add(item);
		}
		modified_edges_lock.Exit();
	}

	private void queue_one_ring(int vid)
	{
		if (!mesh.IsVertex(vid))
		{
			return;
		}
		foreach (int item in mesh.VtxEdgesItr(vid))
		{
			modified_edges.Add(item);
		}
	}

	private void queue_edge_safe(int eid)
	{
		bool lockTaken = false;
		modified_edges_lock.Enter(ref lockTaken);
		modified_edges.Add(eid);
		modified_edges_lock.Exit();
	}

	private void queue_edge(int eid)
	{
		modified_edges.Add(eid);
	}

	protected override void OnEdgeSplit(int edgeID, int va, int vb, DMesh3.EdgeSplitInfo splitInfo)
	{
		if (SplitF != null)
		{
			SplitF(edgeID, va, vb, splitInfo.vNew);
		}
	}

	public void FastestRemesh(int nMaxIterations = 25, bool bDoFastSplits = true)
	{
		ResetQueue();
		int num = 0;
		if (bDoFastSplits)
		{
			if (Cancelled())
			{
				return;
			}
			bool flag = true;
			while (flag)
			{
				int num2 = FastSplitIteration();
				if (num++ > nMaxIterations)
				{
					flag = false;
				}
				if ((double)num2 / (double)mesh.EdgeCount < 0.01)
				{
					flag = false;
				}
				if (Cancelled())
				{
					return;
				}
			}
			ResetQueue();
		}
		TargetProjectionMode projectionMode = ProjectionMode;
		for (int i = 0; i < nMaxIterations - 1; i++)
		{
			if (Cancelled())
			{
				break;
			}
			ProjectionMode = ((i % 2 != 0) ? projectionMode : TargetProjectionMode.NoProjection);
			RemeshIteration();
		}
		ProjectionMode = projectionMode;
		if (!Cancelled())
		{
			RemeshIteration();
		}
	}

	public void SharpEdgeReprojectionRemesh(int nRemeshIterations, int nTuneIterations, bool bDoFastSplits = true)
	{
		if (base.ProjectionTarget == null || !(base.ProjectionTarget is IOrientedProjectionTarget))
		{
			throw new Exception("RemesherPro.SharpEdgeReprojectionRemesh: cannot call this without a ProjectionTarget that has normals");
		}
		ResetQueue();
		int num = 0;
		if (bDoFastSplits)
		{
			if (Cancelled())
			{
				return;
			}
			bool flag = true;
			while (flag)
			{
				int num2 = FastSplitIteration();
				if (num++ > nRemeshIterations)
				{
					flag = false;
				}
				if ((double)num2 / (double)mesh.EdgeCount < 0.01)
				{
					flag = false;
				}
				if (Cancelled())
				{
					return;
				}
			}
			ResetQueue();
		}
		bool useFaceAlignedProjection = UseFaceAlignedProjection;
		UseFaceAlignedProjection = true;
		FaceProjectionPassesPerIteration = 1;
		double smoothSpeedT = SmoothSpeedT;
		for (int i = 0; i < nRemeshIterations; i++)
		{
			if (Cancelled())
			{
				break;
			}
			RemeshIteration();
			if (i > nRemeshIterations / 2)
			{
				SmoothSpeedT *= 0.8999999761581421;
			}
		}
		for (int j = 0; j < nTuneIterations; j++)
		{
			if (Cancelled())
			{
				break;
			}
			TrackedFaceProjectionPass();
		}
		SmoothSpeedT = smoothSpeedT;
		UseFaceAlignedProjection = useFaceAlignedProjection;
	}

	public void ResetQueue()
	{
		if (modified_edges != null)
		{
			modified_edges.Clear();
			modified_edges = null;
		}
	}

	public int FastSplitIteration()
	{
		if (mesh.TriangleCount == 0)
		{
			return 0;
		}
		PushState();
		EnableFlips = (EnableCollapses = (EnableSmoothing = false));
		ProjectionMode = TargetProjectionMode.NoProjection;
		begin_pass();
		begin_ops();
		IEnumerable<int> enumerable = EdgesIterator();
		if (modified_edges == null)
		{
			modified_edges = new HashSet<int>();
		}
		else
		{
			edges_buffer.Clear();
			edges_buffer.AddRange(modified_edges);
			enumerable = edges_buffer;
			modified_edges.Clear();
		}
		_ = base.Mesh.EdgeCount;
		int num = 0;
		double max_edge_len_sqr = MaxEdgeLength * MaxEdgeLength;
		SplitF = delegate(int edgeID, int a, int b, int vNew)
		{
			Vector3d v = base.Mesh.GetVertex(vNew);
			foreach (int item in base.Mesh.VtxEdgesItr(vNew))
			{
				Index2i edgeV = base.Mesh.GetEdgeV(item);
				int vID = ((edgeV.a == vNew) ? edgeV.b : edgeV.a);
				if (mesh.GetVertex(vID).DistanceSquared(ref v) > max_edge_len_sqr)
				{
					queue_edge(item);
				}
			}
		};
		ModifiedEdgesLastPass = 0;
		int num2 = 0;
		foreach (int item2 in enumerable)
		{
			if (Cancelled())
			{
				goto IL_015e;
			}
			if (mesh.IsEdge(item2))
			{
				mesh.GetEdgeV(item2);
				mesh.GetEdgeOpposingV(item2);
				num2++;
				if (ProcessEdge(item2) == ProcessResult.Ok_Split)
				{
					ModifiedEdgesLastPass++;
					num++;
				}
			}
		}
		end_ops();
		goto IL_015e;
		IL_015e:
		SplitF = null;
		PopState();
		end_pass();
		return num;
	}

	public virtual void RemeshIteration()
	{
		if (mesh.TriangleCount == 0)
		{
			return;
		}
		begin_pass();
		begin_ops();
		IEnumerable<int> enumerable = EdgesIterator();
		if (modified_edges == null)
		{
			modified_edges = new HashSet<int>();
		}
		else
		{
			edges_buffer.Clear();
			edges_buffer.AddRange(modified_edges);
			enumerable = edges_buffer;
			modified_edges.Clear();
		}
		_ = base.Mesh.EdgeCount;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		ModifiedEdgesLastPass = 0;
		int num4 = 0;
		foreach (int item in enumerable)
		{
			if (Cancelled())
			{
				return;
			}
			if (mesh.IsEdge(item))
			{
				Index2i edgeV = mesh.GetEdgeV(item);
				Index2i edgeOpposingV = mesh.GetEdgeOpposingV(item);
				num4++;
				switch (ProcessEdge(item))
				{
				case ProcessResult.Ok_Collapsed:
					queue_one_ring(edgeV.a);
					queue_one_ring(edgeV.b);
					queue_one_ring(edgeOpposingV.a);
					queue_one_ring(edgeOpposingV.b);
					ModifiedEdgesLastPass++;
					num3++;
					break;
				case ProcessResult.Ok_Split:
					queue_one_ring(edgeV.a);
					queue_one_ring(edgeV.b);
					queue_one_ring(edgeOpposingV.a);
					queue_one_ring(edgeOpposingV.b);
					ModifiedEdgesLastPass++;
					num2++;
					break;
				case ProcessResult.Ok_Flipped:
					queue_one_ring(edgeV.a);
					queue_one_ring(edgeV.b);
					queue_one_ring(edgeOpposingV.a);
					queue_one_ring(edgeOpposingV.b);
					ModifiedEdgesLastPass++;
					num++;
					break;
				}
			}
		}
		end_ops();
		if (Cancelled())
		{
			return;
		}
		begin_smooth();
		if (EnableSmoothing && SmoothSpeedT > 0.0)
		{
			TrackedSmoothPass(EnableParallelSmooth);
			DoDebugChecks();
		}
		end_smooth();
		if (Cancelled())
		{
			return;
		}
		begin_project();
		if (base.ProjectionTarget != null && ProjectionMode == TargetProjectionMode.AfterRefinement)
		{
			if (UseFaceAlignedProjection)
			{
				for (int i = 0; i < FaceProjectionPassesPerIteration; i++)
				{
					TrackedFaceProjectionPass();
				}
			}
			else
			{
				TrackedProjectionPass(EnableParallelProjection);
			}
			DoDebugChecks();
		}
		end_project();
		end_pass();
	}

	protected virtual void TrackedSmoothPass(bool bParallel)
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
			Vector3d vertex = base.Mesh.GetVertex(vID);
			bool bModified = false;
			Vector3d value = ComputeSmoothedVertexPos(vID, smoothFunc, out bModified);
			if (bModified)
			{
				vModifiedV[vID] = true;
				vBufferV[vID] = value;
				foreach (int item in mesh.VtxEdgesItr(vID))
				{
					Index2i edgeV = base.Mesh.GetEdgeV(item);
					int vID2 = ((edgeV.a == vID) ? edgeV.b : edgeV.a);
					Vector3d vertex2 = mesh.GetVertex(vID2);
					vertex.Distance(vertex2);
					double num = value.Distance(vertex2);
					if (num < MinEdgeLength || num > MaxEdgeLength)
					{
						queue_edge_safe(item);
					}
				}
			}
		};
		if (bParallel)
		{
			gParallel.ForEach(smooth_vertices(), action);
		}
		else
		{
			foreach (int item2 in smooth_vertices())
			{
				action(item2);
			}
		}
		ApplyVertexBuffer(bParallel);
	}

	protected virtual void TrackedProjectionPass(bool bParallel)
	{
		InitializeVertexBufferForPass();
		Action<int> action = delegate(int vID)
		{
			Vector3d vertex = base.Mesh.GetVertex(vID);
			bool bModified = false;
			Vector3d vector3d = ComputeProjectedVertexPos(vID, out bModified);
			if (vertex.EpsilonEqual(vector3d, 9.999999974752427E-07))
			{
				bModified = false;
			}
			if (bModified)
			{
				vModifiedV[vID] = true;
				vBufferV[vID] = vector3d;
				foreach (int item in mesh.VtxEdgesItr(vID))
				{
					Index2i edgeV = base.Mesh.GetEdgeV(item);
					int vID2 = ((edgeV.a == vID) ? edgeV.b : edgeV.a);
					Vector3d vertex2 = mesh.GetVertex(vID2);
					vertex.Distance(vertex2);
					double num = vector3d.Distance(vertex2);
					if (num < MinEdgeLength || num > MaxEdgeLength)
					{
						queue_edge_safe(item);
					}
				}
			}
		};
		if (bParallel)
		{
			gParallel.ForEach(smooth_vertices(), action);
		}
		else
		{
			foreach (int item2 in smooth_vertices())
			{
				action(item2);
			}
		}
		ApplyVertexBuffer(bParallel);
	}

	protected virtual Vector3d ComputeProjectedVertexPos(int vID, out bool bModified)
	{
		bModified = false;
		if (vertex_is_constrained(vID))
		{
			return base.Mesh.GetVertex(vID);
		}
		if (VertexControlF != null && (VertexControlF(vID) & VertexControl.NoProject) != 0)
		{
			return base.Mesh.GetVertex(vID);
		}
		Vector3d vertex = mesh.GetVertex(vID);
		Vector3d result = base.ProjectionTarget.Project(vertex, vID);
		bModified = true;
		return result;
	}

	protected virtual void InitializeBuffersForFacePass()
	{
		base.InitializeVertexBufferForPass();
		if (vBufferVWeights.size < vBufferV.size)
		{
			vBufferVWeights.resize(vBufferV.size);
		}
		int maxVertexID = mesh.MaxVertexID;
		for (int i = 0; i < maxVertexID; i++)
		{
			vBufferV[i] = Vector3d.Zero;
			vBufferVWeights[i] = 0.0;
		}
	}

	protected virtual void TrackedFaceProjectionPass()
	{
		IOrientedProjectionTarget normalTarget = base.ProjectionTarget as IOrientedProjectionTarget;
		if (normalTarget == null)
		{
			throw new Exception("RemesherPro.TrackedFaceProjectionPass: projection target does not have normals!");
		}
		InitializeBuffersForFacePass();
		SpinLock buffer_lock = default(SpinLock);
		Action<int> body = delegate(int tid)
		{
			mesh.GetTriInfo(tid, out var normal, out var fArea, out var vCentroid);
			Vector3d vProjectNormal;
			Vector3d vector3d2 = normalTarget.Project(vCentroid, out vProjectNormal);
			Index3i triangle = mesh.GetTriangle(tid);
			Vector3d v = mesh.GetVertex(triangle.a);
			Vector3d v2 = mesh.GetVertex(triangle.b);
			Vector3d v3 = mesh.GetVertex(triangle.c);
			Frame3f frame3f = new Frame3f(vCentroid, normal);
			v = frame3f.ToFrameP(ref v);
			v2 = frame3f.ToFrameP(ref v2);
			v3 = frame3f.ToFrameP(ref v3);
			frame3f.AlignAxis(2, (Vector3f)vProjectNormal);
			frame3f.Origin = (Vector3f)vector3d2;
			v = frame3f.FromFrameP(ref v);
			v2 = frame3f.FromFrameP(ref v2);
			v3 = frame3f.FromFrameP(ref v3);
			double f = normal.Dot(vProjectNormal);
			f = MathUtil.Clamp(f, 0.0, 1.0);
			double num2 = fArea * (f * f * f);
			bool lockTaken = false;
			buffer_lock.Enter(ref lockTaken);
			vBufferV[triangle.a] += num2 * v;
			vBufferVWeights[triangle.a] += num2;
			vBufferV[triangle.b] += num2 * v2;
			vBufferVWeights[triangle.b] += num2;
			vBufferV[triangle.c] += num2 * v3;
			vBufferVWeights[triangle.c] += num2;
			buffer_lock.Exit();
		};
		gParallel.ForEach(mesh.TriangleIndices(), body);
		gParallel.ForEach(mesh.VertexIndices(), delegate(int vID)
		{
			vModifiedV[vID] = false;
			if (!(vBufferVWeights[vID] < 1E-08) && !vertex_is_constrained(vID) && (VertexControlF == null || (VertexControlF(vID) & VertexControl.NoProject) == 0))
			{
				Vector3d vertex = mesh.GetVertex(vID);
				Vector3d vector3d = vBufferV[vID] / vBufferVWeights[vID];
				if (!vertex.EpsilonEqual(vector3d, 9.999999974752427E-07))
				{
					vModifiedV[vID] = true;
					vBufferV[vID] = vector3d;
					foreach (int item in mesh.VtxEdgesItr(vID))
					{
						Index2i edgeV = base.Mesh.GetEdgeV(item);
						int vID2 = ((edgeV.a == vID) ? edgeV.b : edgeV.a);
						Vector3d vertex2 = mesh.GetVertex(vID2);
						vertex.Distance(vertex2);
						double num = vector3d.Distance(vertex2);
						if (num < MinEdgeLength || num > MaxEdgeLength)
						{
							queue_edge_safe(item);
						}
					}
				}
			}
		});
		ApplyVertexBuffer(bParallel: true);
	}

	public void PushState()
	{
		SettingState settingState = default(SettingState);
		settingState.EnableFlips = EnableFlips;
		settingState.EnableCollapses = EnableCollapses;
		settingState.EnableSplits = EnableSplits;
		settingState.EnableSmoothing = EnableSmoothing;
		settingState.MinEdgeLength = MinEdgeLength;
		settingState.MaxEdgeLength = MaxEdgeLength;
		settingState.SmoothSpeedT = SmoothSpeedT;
		settingState.SmoothType = SmoothType;
		settingState.ProjectionMode = ProjectionMode;
		SettingState item = settingState;
		stateStack.Add(item);
	}

	public void PopState()
	{
		SettingState settingState = stateStack.Last();
		stateStack.RemoveAt(stateStack.Count - 1);
		EnableFlips = settingState.EnableFlips;
		EnableCollapses = settingState.EnableCollapses;
		EnableSplits = settingState.EnableSplits;
		EnableSmoothing = settingState.EnableSmoothing;
		MinEdgeLength = settingState.MinEdgeLength;
		MaxEdgeLength = settingState.MaxEdgeLength;
		SmoothSpeedT = settingState.SmoothSpeedT;
		SmoothType = settingState.SmoothType;
		ProjectionMode = settingState.ProjectionMode;
	}
}
