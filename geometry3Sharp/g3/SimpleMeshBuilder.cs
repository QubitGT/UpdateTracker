using System;
using System.Collections.Generic;

namespace g3;

public class SimpleMeshBuilder : IMeshBuilder
{
	public List<SimpleMesh> Meshes;

	public List<GenericMaterial> Materials;

	public List<int> MaterialAssignment;

	private int nActiveMesh;

	public bool SupportsMetaData => false;

	public SimpleMeshBuilder()
	{
		Meshes = new List<SimpleMesh>();
		Materials = new List<GenericMaterial>();
		MaterialAssignment = new List<int>();
		nActiveMesh = -1;
	}

	public int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups)
	{
		int count = Meshes.Count;
		SimpleMesh simpleMesh = new SimpleMesh();
		simpleMesh.Initialize(bHaveVtxNormals, bHaveVtxColors, bHaveVtxUVs, bHaveFaceGroups);
		Meshes.Add(simpleMesh);
		MaterialAssignment.Add(-1);
		nActiveMesh = count;
		return count;
	}

	public int AppendNewMesh(DMesh3 existingMesh)
	{
		int count = Meshes.Count;
		SimpleMesh item = new SimpleMesh(existingMesh);
		Meshes.Add(item);
		MaterialAssignment.Add(-1);
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
		return Meshes[nActiveMesh].AppendTriangle(i, j, k);
	}

	public int AppendTriangle(int i, int j, int k, int g)
	{
		return Meshes[nActiveMesh].AppendTriangle(i, j, k, g);
	}

	public int AppendVertex(double x, double y, double z)
	{
		return Meshes[nActiveMesh].AppendVertex(x, y, z);
	}

	public int AppendVertex(NewVertexInfo info)
	{
		return Meshes[nActiveMesh].AppendVertex(info);
	}

	public void AppendMetaData(string identifier, object data)
	{
		throw new NotImplementedException("SimpleMeshBuilder: metadata not supported");
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

	public void SetVertexUV(int vID, Vector2f UV)
	{
		Meshes[nActiveMesh].SetVertexUV(vID, UV);
	}
}
