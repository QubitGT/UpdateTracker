using System;
using System.Collections.Generic;

namespace g3;

public class DMesh3Builder : IMeshBuilder
{
	public enum AddTriangleFailBehaviors
	{
		DiscardTriangle,
		DuplicateAllVertices
	}

	public AddTriangleFailBehaviors NonManifoldTriBehavior = AddTriangleFailBehaviors.DuplicateAllVertices;

	public AddTriangleFailBehaviors DuplicateTriBehavior;

	public List<DMesh3> Meshes;

	public List<GenericMaterial> Materials;

	public List<int> MaterialAssignment;

	public List<Dictionary<string, object>> Metadata;

	private int nActiveMesh;

	public bool SupportsMetaData => true;

	public DMesh3Builder()
	{
		Meshes = new List<DMesh3>();
		Materials = new List<GenericMaterial>();
		MaterialAssignment = new List<int>();
		Metadata = new List<Dictionary<string, object>>();
		nActiveMesh = -1;
	}

	public int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups)
	{
		int count = Meshes.Count;
		DMesh3 item = new DMesh3(bHaveVtxNormals, bHaveVtxColors, bHaveVtxUVs, bHaveFaceGroups);
		Meshes.Add(item);
		MaterialAssignment.Add(-1);
		Metadata.Add(new Dictionary<string, object>());
		nActiveMesh = count;
		return count;
	}

	public int AppendNewMesh(DMesh3 existingMesh)
	{
		int count = Meshes.Count;
		Meshes.Add(existingMesh);
		MaterialAssignment.Add(-1);
		Metadata.Add(new Dictionary<string, object>());
		nActiveMesh = count;
		return count;
	}

	public void SetActiveMesh(int id)
	{
		if (id >= 0 && id < Meshes.Count)
		{
			nActiveMesh = id;
			return;
		}
		throw new ArgumentOutOfRangeException("active mesh id is out of range");
	}

	public int AppendTriangle(int i, int j, int k)
	{
		return AppendTriangle(i, j, k, -1);
	}

	public int AppendTriangle(int i, int j, int k, int g)
	{
		int num = Meshes[nActiveMesh].FindTriangle(i, j, k);
		if (num != -1)
		{
			if (DuplicateTriBehavior == AddTriangleFailBehaviors.DuplicateAllVertices)
			{
				return append_duplicate_triangle(i, j, k, g);
			}
			return num;
		}
		int num2 = Meshes[nActiveMesh].AppendTriangle(i, j, k, g);
		if (num2 == -2)
		{
			if (NonManifoldTriBehavior == AddTriangleFailBehaviors.DuplicateAllVertices)
			{
				return append_duplicate_triangle(i, j, k, g);
			}
			return -2;
		}
		return num2;
	}

	private int append_duplicate_triangle(int i, int j, int k, int g)
	{
		NewVertexInfo vinfo = default(NewVertexInfo);
		Meshes[nActiveMesh].GetVertex(i, ref vinfo, bWantNormals: true, bWantColors: true, bWantUVs: true);
		int v = Meshes[nActiveMesh].AppendVertex(vinfo);
		Meshes[nActiveMesh].GetVertex(j, ref vinfo, bWantNormals: true, bWantColors: true, bWantUVs: true);
		int v2 = Meshes[nActiveMesh].AppendVertex(vinfo);
		Meshes[nActiveMesh].GetVertex(k, ref vinfo, bWantNormals: true, bWantColors: true, bWantUVs: true);
		int v3 = Meshes[nActiveMesh].AppendVertex(vinfo);
		return Meshes[nActiveMesh].AppendTriangle(v, v2, v3, g);
	}

	public int AppendVertex(double x, double y, double z)
	{
		return Meshes[nActiveMesh].AppendVertex(new Vector3d(x, y, z));
	}

	public int AppendVertex(NewVertexInfo info)
	{
		return Meshes[nActiveMesh].AppendVertex(info);
	}

	public void AppendMetaData(string identifier, object data)
	{
		Metadata[nActiveMesh].Add(identifier, data);
	}

	public void SetVertexUV(int vID, Vector2f UV)
	{
		Meshes[nActiveMesh].SetVertexUV(vID, UV);
	}

	public int BuildMaterial(GenericMaterial m)
	{
		int count = Materials.Count;
		Materials.Add(m);
		return count;
	}

	public void AssignMaterial(int materialID, int meshID)
	{
		if (meshID >= MaterialAssignment.Count || materialID >= Materials.Count)
		{
			throw new ArgumentOutOfRangeException("[SimpleMeshBuilder::AssignMaterial] meshID or materialID are out-of-range");
		}
		MaterialAssignment[meshID] = materialID;
	}

	public static DMesh3 Build<VType, TType, NType>(IEnumerable<VType> Vertices, IEnumerable<TType> Triangles, IEnumerable<NType> Normals = null, IEnumerable<int> TriGroups = null)
	{
		DMesh3 dMesh = new DMesh3(Normals != null, bWantColors: false, bWantUVs: false, TriGroups != null);
		Vector3d[] array = BufferUtil.ToVector3d(Vertices);
		for (int i = 0; i < array.Length; i++)
		{
			dMesh.AppendVertex(array[i]);
		}
		if (Normals != null)
		{
			Vector3f[] array2 = BufferUtil.ToVector3f(Normals);
			if (array2.Length != array.Length)
			{
				throw new Exception("DMesh3Builder.Build: incorrect number of normals provided");
			}
			for (int j = 0; j < array2.Length; j++)
			{
				dMesh.SetVertexNormal(j, array2[j]);
			}
		}
		Index3i[] array3 = BufferUtil.ToIndex3i(Triangles);
		for (int k = 0; k < array3.Length; k++)
		{
			dMesh.AppendTriangle(array3[k]);
		}
		if (TriGroups != null)
		{
			List<int> list = new List<int>(TriGroups);
			if (list.Count != array3.Length)
			{
				throw new Exception("DMesh3Builder.Build: incorect number of triangle groups");
			}
			for (int l = 0; l < array3.Length; l++)
			{
				dMesh.SetTriangleGroup(l, list[l]);
			}
		}
		return dMesh;
	}
}
