using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using g3;

namespace gs;

public class RemoveOccludedTriangles
{
	public enum CalculationMode
	{
		RayParity,
		AnalyticWindingNumber,
		FastWindingNumber,
		SimpleOcclusionTest
	}

	public DMesh3 Mesh;

	public DMeshAABBTree3 Spatial;

	public List<int> RemovedT;

	public bool RemoveFailed;

	public bool PerVertex;

	public double NormalOffset = 1E-08;

	public double WindingIsoValue = 0.5;

	public CalculationMode InsideMode;

	public ProgressCancel Progress;

	protected virtual bool Cancelled()
	{
		if (Progress != null)
		{
			return Progress.Cancelled();
		}
		return false;
	}

	public RemoveOccludedTriangles(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public RemoveOccludedTriangles(DMesh3 mesh, DMeshAABBTree3 spatial)
	{
		Mesh = mesh;
		Spatial = spatial;
	}

	public virtual bool Apply()
	{
		DMesh3 dMesh = Mesh;
		if (InsideMode == CalculationMode.RayParity)
		{
			MeshBoundaryLoops meshBoundaryLoops = new MeshBoundaryLoops(dMesh);
			if (meshBoundaryLoops.Count > 0)
			{
				dMesh = new DMesh3(Mesh);
				foreach (EdgeLoop item in meshBoundaryLoops)
				{
					if (Cancelled())
					{
						return false;
					}
					new SimpleHoleFiller(dMesh, item).Fill();
				}
			}
		}
		DMeshAABBTree3 spatial = ((Spatial != null && dMesh == Mesh) ? Spatial : new DMeshAABBTree3(dMesh, autoBuild: true));
		if (InsideMode == CalculationMode.AnalyticWindingNumber)
		{
			spatial.WindingNumber(Vector3d.Zero);
		}
		else if (InsideMode == CalculationMode.FastWindingNumber)
		{
			spatial.FastWindingNumber(Vector3d.Zero);
		}
		if (Cancelled())
		{
			return false;
		}
		List<Vector3d> ray_dirs = null;
		int NR = 0;
		if (InsideMode == CalculationMode.SimpleOcclusionTest)
		{
			ray_dirs = new List<Vector3d>();
			ray_dirs.Add(Vector3d.AxisX);
			ray_dirs.Add(-Vector3d.AxisX);
			ray_dirs.Add(Vector3d.AxisY);
			ray_dirs.Add(-Vector3d.AxisY);
			ray_dirs.Add(Vector3d.AxisZ);
			ray_dirs.Add(-Vector3d.AxisZ);
			NR = ray_dirs.Count;
		}
		Func<Vector3d, bool> isOccludedF = delegate(Vector3d pt)
		{
			if (InsideMode == CalculationMode.RayParity)
			{
				return spatial.IsInside(pt);
			}
			if (InsideMode == CalculationMode.AnalyticWindingNumber)
			{
				return spatial.WindingNumber(pt) > WindingIsoValue;
			}
			if (InsideMode == CalculationMode.FastWindingNumber)
			{
				return spatial.FastWindingNumber(pt) > WindingIsoValue;
			}
			for (int i = 0; i < NR; i++)
			{
				if (spatial.FindNearestHitTriangle(new Ray3d(pt, ray_dirs[i])) == -1)
				{
					return false;
				}
			}
			return true;
		};
		bool cancel = false;
		BitArray vertices = null;
		if (PerVertex)
		{
			vertices = new BitArray(Mesh.MaxVertexID);
			MeshNormals normals = null;
			if (!Mesh.HasVertexNormals)
			{
				normals = new MeshNormals(Mesh);
				normals.Compute();
			}
			gParallel.ForEach(Mesh.VertexIndices(), delegate(int vid)
			{
				if (!cancel)
				{
					if (vid % 10 == 0)
					{
						cancel = Cancelled();
					}
					Vector3d vertex = Mesh.GetVertex(vid);
					Vector3d vector3d = ((normals == null) ? ((Vector3d)Mesh.GetVertexNormal(vid)) : normals[vid]);
					vertex += vector3d * NormalOffset;
					vertices[vid] = isOccludedF(vertex);
				}
			});
		}
		if (Cancelled())
		{
			return false;
		}
		RemovedT = new List<int>();
		SpinLock removeLock = default(SpinLock);
		gParallel.ForEach(Mesh.TriangleIndices(), delegate(int tid)
		{
			if (!cancel)
			{
				if (tid % 10 == 0)
				{
					cancel = Cancelled();
				}
				bool flag2 = false;
				if (PerVertex)
				{
					Index3i triangle = Mesh.GetTriangle(tid);
					flag2 = vertices[triangle.a] || vertices[triangle.b] || vertices[triangle.c];
				}
				else
				{
					Vector3d triCentroid = Mesh.GetTriCentroid(tid);
					Vector3d triNormal = Mesh.GetTriNormal(tid);
					triCentroid += triNormal * NormalOffset;
					flag2 = isOccludedF(triCentroid);
				}
				if (flag2)
				{
					bool lockTaken = false;
					removeLock.Enter(ref lockTaken);
					RemovedT.Add(tid);
					removeLock.Exit();
				}
			}
		});
		if (Cancelled())
		{
			return false;
		}
		if (RemovedT.Count > 0)
		{
			bool flag = new MeshEditor(Mesh).RemoveTriangles(RemovedT, bRemoveIsolatedVerts: true);
			RemoveFailed = !flag;
		}
		return true;
	}
}
