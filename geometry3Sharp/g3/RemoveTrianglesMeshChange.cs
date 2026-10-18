using System;
using System.Collections.Generic;

namespace g3;

public class RemoveTrianglesMeshChange
{
	protected DVector<int> RemovedV;

	protected DVector<Vector3d> Positions;

	protected DVector<Vector3f> Normals;

	protected DVector<Vector3f> Colors;

	protected DVector<Vector2f> UVs;

	protected DVector<int> RemovedT;

	protected DVector<Index4i> Triangles;

	public Action<IEnumerable<int>, IEnumerable<int>> OnApplyF;

	public Action<IEnumerable<int>, IEnumerable<int>> OnRevertF;

	public void InitializeFromApply(DMesh3 mesh, IEnumerable<int> triangles)
	{
		initialize_buffers(mesh);
		bool hasTriangleGroups = mesh.HasTriangleGroups;
		foreach (int triangle2 in triangles)
		{
			if (mesh.IsTriangle(triangle2))
			{
				Index3i triangle = mesh.GetTriangle(triangle2);
				save_vertex(mesh, triangle.a);
				save_vertex(mesh, triangle.b);
				save_vertex(mesh, triangle.c);
				Index4i value = new Index4i(triangle.a, triangle.b, triangle.c, hasTriangleGroups ? mesh.GetTriangleGroup(triangle2) : (-1));
				RemovedT.Add(triangle2);
				Triangles.Add(value);
				MeshResult meshResult = mesh.RemoveTriangle(triangle2);
				if (meshResult != 0)
				{
					throw new Exception("RemoveTrianglesMeshChange.Initialize: exception in RemoveTriangle(" + triangle2 + "): " + meshResult);
				}
			}
		}
	}

	public void InitializeFromExisting(DMesh3 mesh, IEnumerable<int> remove_t)
	{
		initialize_buffers(mesh);
		bool hasTriangleGroups = mesh.HasTriangleGroups;
		HashSet<int> hashSet = new HashSet<int>(remove_t);
		HashSet<int> hashSet2 = new HashSet<int>();
		IndexUtil.TrianglesToVertices(mesh, remove_t, hashSet2);
		List<int> list = new List<int>();
		foreach (int item in hashSet2)
		{
			bool flag = true;
			foreach (int item2 in mesh.VtxTrianglesItr(item))
			{
				if (!hashSet.Contains(item2))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list.Add(item);
			}
		}
		foreach (int item3 in list)
		{
			save_vertex(mesh, item3, force: true);
		}
		foreach (int item4 in remove_t)
		{
			Index3i triangle = mesh.GetTriangle(item4);
			Index4i value = new Index4i(triangle.a, triangle.b, triangle.c, hasTriangleGroups ? mesh.GetTriangleGroup(item4) : (-1));
			RemovedT.Add(item4);
			Triangles.Add(value);
		}
	}

	public void Apply(DMesh3 mesh)
	{
		int size = RemovedT.size;
		for (int i = 0; i < size; i++)
		{
			int num = RemovedT[i];
			MeshResult meshResult = mesh.RemoveTriangle(RemovedT[i]);
			if (meshResult != 0)
			{
				throw new Exception("RemoveTrianglesMeshChange.Apply: error in RemoveTriangle(" + num + "): " + meshResult);
			}
		}
		if (OnApplyF != null)
		{
			OnApplyF(RemovedV, RemovedT);
		}
	}

	public void Revert(DMesh3 mesh)
	{
		int size = RemovedV.size;
		if (size > 0)
		{
			NewVertexInfo info = new NewVertexInfo(Positions[0]);
			mesh.BeginUnsafeVerticesInsert();
			for (int i = 0; i < size; i++)
			{
				int vid = RemovedV[i];
				info.v = Positions[i];
				if (Normals != null)
				{
					info.bHaveN = true;
					info.n = Normals[i];
				}
				if (Colors != null)
				{
					info.bHaveC = true;
					info.c = Colors[i];
				}
				if (UVs != null)
				{
					info.bHaveUV = true;
					info.uv = UVs[i];
				}
				MeshResult meshResult = mesh.InsertVertex(vid, ref info, bUnsafe: true);
				if (meshResult != 0)
				{
					throw new Exception("RemoveTrianglesMeshChange.Revert: error in InsertVertex(" + vid + "): " + meshResult);
				}
			}
			mesh.EndUnsafeVerticesInsert();
		}
		int size2 = RemovedT.size;
		if (size2 > 0)
		{
			mesh.BeginUnsafeTrianglesInsert();
			for (int j = 0; j < size2; j++)
			{
				int tid = RemovedT[j];
				Index4i index4i = Triangles[j];
				MeshResult meshResult2 = mesh.InsertTriangle(tv: new Index3i(index4i.a, index4i.b, index4i.c), tid: tid, gid: index4i.d, bUnsafe: true);
				if (meshResult2 != 0)
				{
					throw new Exception("RemoveTrianglesMeshChange.Revert: error in InsertTriangle(" + tid + "): " + meshResult2);
				}
			}
			mesh.EndUnsafeTrianglesInsert();
		}
		if (OnRevertF != null)
		{
			OnRevertF(RemovedV, RemovedT);
		}
	}

	private bool save_vertex(DMesh3 mesh, int vid, bool force = false)
	{
		if (force || mesh.VerticesRefCounts.refCount(vid) == 2)
		{
			RemovedV.Add(vid);
			Positions.Add(mesh.GetVertex(vid));
			if (Normals != null)
			{
				Normals.Add(mesh.GetVertexNormal(vid));
			}
			if (Colors != null)
			{
				Colors.Add(mesh.GetVertexColor(vid));
			}
			if (UVs != null)
			{
				UVs.Add(mesh.GetVertexUV(vid));
			}
			return false;
		}
		return true;
	}

	private void initialize_buffers(DMesh3 mesh)
	{
		RemovedV = new DVector<int>();
		Positions = new DVector<Vector3d>();
		if (mesh.HasVertexNormals)
		{
			Normals = new DVector<Vector3f>();
		}
		if (mesh.HasVertexColors)
		{
			Colors = new DVector<Vector3f>();
		}
		if (mesh.HasVertexUVs)
		{
			UVs = new DVector<Vector2f>();
		}
		RemovedT = new DVector<int>();
		Triangles = new DVector<Index4i>();
	}
}
