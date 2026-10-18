using System;
using System.Collections.Generic;

namespace g3;

public class AddTrianglesMeshChange
{
	protected DVector<int> AddedV;

	protected DVector<Vector3d> Positions;

	protected DVector<Vector3f> Normals;

	protected DVector<Vector3f> Colors;

	protected DVector<Vector2f> UVs;

	protected DVector<int> AddedT;

	protected DVector<Index4i> Triangles;

	public Action<IEnumerable<int>, IEnumerable<int>> OnApplyF;

	public Action<IEnumerable<int>, IEnumerable<int>> OnRevertF;

	public void InitializeFromExisting(DMesh3 mesh, IEnumerable<int> added_v, IEnumerable<int> added_t)
	{
		initialize_buffers(mesh);
		bool hasTriangleGroups = mesh.HasTriangleGroups;
		if (added_v != null)
		{
			foreach (int item in added_v)
			{
				append_vertex(mesh, item);
			}
		}
		foreach (int item2 in added_t)
		{
			Index3i triangle = mesh.GetTriangle(item2);
			Index4i value = new Index4i(triangle.a, triangle.b, triangle.c, hasTriangleGroups ? mesh.GetTriangleGroup(item2) : (-1));
			AddedT.Add(item2);
			Triangles.Add(value);
		}
	}

	public void Apply(DMesh3 mesh)
	{
		int size = AddedV.size;
		if (size > 0)
		{
			NewVertexInfo info = new NewVertexInfo(Positions[0]);
			mesh.BeginUnsafeVerticesInsert();
			for (int i = 0; i < size; i++)
			{
				int vid = AddedV[i];
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
					throw new Exception("AddTrianglesMeshChange.Revert: error in InsertVertex(" + vid + "): " + meshResult);
				}
			}
			mesh.EndUnsafeVerticesInsert();
		}
		int size2 = AddedT.size;
		if (size2 > 0)
		{
			mesh.BeginUnsafeTrianglesInsert();
			for (int j = 0; j < size2; j++)
			{
				int tid = AddedT[j];
				Index4i index4i = Triangles[j];
				MeshResult meshResult2 = mesh.InsertTriangle(tv: new Index3i(index4i.a, index4i.b, index4i.c), tid: tid, gid: index4i.d, bUnsafe: true);
				if (meshResult2 != 0)
				{
					throw new Exception("AddTrianglesMeshChange.Revert: error in InsertTriangle(" + tid + "): " + meshResult2);
				}
			}
			mesh.EndUnsafeTrianglesInsert();
		}
		if (OnApplyF != null)
		{
			OnApplyF(AddedV, AddedT);
		}
	}

	public void Revert(DMesh3 mesh)
	{
		int size = AddedT.size;
		for (int i = 0; i < size; i++)
		{
			int num = AddedT[i];
			MeshResult meshResult = mesh.RemoveTriangle(AddedT[i]);
			if (meshResult != 0)
			{
				throw new Exception("AddTrianglesMeshChange.Apply: error in RemoveTriangle(" + num + "): " + meshResult);
			}
		}
		if (OnRevertF != null)
		{
			OnRevertF(AddedV, AddedT);
		}
	}

	private void append_vertex(DMesh3 mesh, int vid)
	{
		AddedV.Add(vid);
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
	}

	private void initialize_buffers(DMesh3 mesh)
	{
		AddedV = new DVector<int>();
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
		AddedT = new DVector<int>();
		Triangles = new DVector<Index4i>();
	}
}
