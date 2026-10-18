using System;
using System.Threading;

namespace g3;

public class MeshNormals
{
	public enum NormalsTypes
	{
		Vertex_OneRingFaceAverage_AreaWeighted
	}

	public IMesh Mesh;

	public DVector<Vector3d> Normals;

	public Func<int, Vector3d> VertexF;

	public NormalsTypes NormalType;

	public Vector3d this[int vid] => Normals[vid];

	public MeshNormals(IMesh mesh, NormalsTypes eType = NormalsTypes.Vertex_OneRingFaceAverage_AreaWeighted)
	{
		Mesh = mesh;
		NormalType = eType;
		Normals = new DVector<Vector3d>();
		VertexF = Mesh.GetVertex;
	}

	public void Compute()
	{
		Compute_FaceAvg_AreaWeighted();
	}

	public void CopyTo(DMesh3 SetMesh)
	{
		if (SetMesh.MaxVertexID < Mesh.MaxVertexID)
		{
			throw new Exception("MeshNormals.Set: SetMesh does not have enough vertices!");
		}
		if (!SetMesh.HasVertexNormals)
		{
			SetMesh.EnableVertexNormals(Vector3f.AxisY);
		}
		int maxVertexID = Mesh.MaxVertexID;
		for (int i = 0; i < maxVertexID; i++)
		{
			if (Mesh.IsVertex(i) && SetMesh.IsVertex(i))
			{
				SetMesh.SetVertexNormal(i, (Vector3f)Normals[i]);
			}
		}
	}

	private void Compute_FaceAvg_AreaWeighted()
	{
		int maxVertexID = Mesh.MaxVertexID;
		if (maxVertexID != Normals.size)
		{
			Normals.resize(maxVertexID);
		}
		for (int i = 0; i < maxVertexID; i++)
		{
			Normals[i] = Vector3d.Zero;
		}
		SpinLock Normals_lock = default(SpinLock);
		gParallel.ForEach(Mesh.TriangleIndices(), delegate(int ti)
		{
			Index3i triangle = Mesh.GetTriangle(ti);
			Vector3d v = Mesh.GetVertex(triangle.a);
			Vector3d v2 = Mesh.GetVertex(triangle.b);
			Vector3d v3 = Mesh.GetVertex(triangle.c);
			Vector3d vector3d = MathUtil.Normal(ref v, ref v2, ref v3);
			double num = MathUtil.Area(ref v, ref v2, ref v3);
			bool lockTaken = false;
			Normals_lock.Enter(ref lockTaken);
			Normals[triangle.a] += num * vector3d;
			Normals[triangle.b] += num * vector3d;
			Normals[triangle.c] += num * vector3d;
			Normals_lock.Exit();
		});
		gParallel.BlockStartEnd(0, maxVertexID - 1, delegate(int vi_start, int vi_end)
		{
			for (int j = vi_start; j <= vi_end; j++)
			{
				if (Normals[j].LengthSquared > 9.999999974752427E-07)
				{
					Normals[j] = Normals[j].Normalized;
				}
			}
		});
	}

	public static void QuickCompute(DMesh3 mesh)
	{
		MeshNormals meshNormals = new MeshNormals(mesh);
		meshNormals.Compute();
		meshNormals.CopyTo(mesh);
	}

	public static Vector3d QuickCompute(DMesh3 mesh, int vid, NormalsTypes type = NormalsTypes.Vertex_OneRingFaceAverage_AreaWeighted)
	{
		Vector3d zero = Vector3d.Zero;
		foreach (int item in mesh.VtxTrianglesItr(vid))
		{
			mesh.GetTriInfo(item, out var normal, out var fArea, out var _);
			zero += fArea * normal;
		}
		return zero.Normalized;
	}
}
