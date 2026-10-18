using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Technie.PhysicsCreator;

public class CuttableMesh
{
	private MeshRenderer inputMeshRenderer;

	private bool hasUvs;

	private bool hasUv1s;

	private bool hasColours;

	private List<CuttableSubMesh> subMeshes;

	public CuttableMesh(Mesh inputMesh)
	{
		Init(inputMesh, inputMesh.name);
	}

	public CuttableMesh(MeshRenderer input)
	{
		inputMeshRenderer = input;
		Mesh sharedMesh = input.GetComponent<MeshFilter>().sharedMesh;
		Init(sharedMesh, input.name);
	}

	private void Init(Mesh inputMesh, string debugName)
	{
		subMeshes = new List<CuttableSubMesh>();
		if (inputMesh.isReadable)
		{
			Vector3[] vertices = inputMesh.vertices;
			Vector3[] normals = inputMesh.normals;
			Vector2[] uv = inputMesh.uv;
			Vector2[] uv2 = inputMesh.uv2;
			Color32[] colors = inputMesh.colors32;
			hasUvs = uv != null && uv.Length != 0;
			hasUv1s = uv2 != null && uv2.Length != 0;
			hasColours = colors != null && colors.Length != 0;
			for (int i = 0; i < inputMesh.subMeshCount; i++)
			{
				CuttableSubMesh item = new CuttableSubMesh(inputMesh.GetIndices(i), vertices, normals, colors, uv, uv2);
				subMeshes.Add(item);
			}
		}
		else
		{
			Debug.LogError("CuttableMesh's input mesh is not readable: " + debugName, inputMesh);
		}
	}

	public CuttableMesh(CuttableMesh inputMesh, List<CuttableSubMesh> newSubMeshes)
	{
		inputMeshRenderer = inputMesh.inputMeshRenderer;
		hasUvs = inputMesh.hasUvs;
		hasUv1s = inputMesh.hasUv1s;
		hasColours = inputMesh.hasColours;
		subMeshes = new List<CuttableSubMesh>();
		subMeshes.AddRange(newSubMeshes);
	}

	public void Add(CuttableMesh other)
	{
		if (subMeshes.Count != other.subMeshes.Count)
		{
			throw new Exception("Mismatched submesh count");
		}
		for (int i = 0; i < subMeshes.Count; i++)
		{
			subMeshes[i].Add(other.subMeshes[i]);
		}
	}

	public int NumSubMeshes()
	{
		return subMeshes.Count;
	}

	public bool HasUvs()
	{
		return hasUvs;
	}

	public bool HasColours()
	{
		return hasColours;
	}

	public List<CuttableSubMesh> GetSubMeshes()
	{
		return subMeshes;
	}

	public CuttableSubMesh GetSubMesh(int index)
	{
		return subMeshes[index];
	}

	public Transform GetTransform()
	{
		if (inputMeshRenderer != null)
		{
			return inputMeshRenderer.transform;
		}
		return null;
	}

	public MeshRenderer ConvertToRenderer(string newObjectName)
	{
		Mesh mesh = CreateMesh();
		if (mesh.vertexCount == 0)
		{
			return null;
		}
		GameObject gameObject = new GameObject(newObjectName);
		gameObject.transform.SetParent(inputMeshRenderer.transform);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = inputMeshRenderer.shadowCastingMode;
		meshRenderer.reflectionProbeUsage = inputMeshRenderer.reflectionProbeUsage;
		meshRenderer.lightProbeUsage = inputMeshRenderer.lightProbeUsage;
		meshRenderer.sharedMaterials = inputMeshRenderer.sharedMaterials;
		return meshRenderer;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		int num = 0;
		for (int i = 0; i < subMeshes.Count; i++)
		{
			num += subMeshes[i].NumIndices();
		}
		mesh.indexFormat = ((num > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Color32> list3 = (hasColours ? new List<Color32>() : null);
		List<Vector2> list4 = (hasUvs ? new List<Vector2>() : null);
		List<Vector2> list5 = (hasUv1s ? new List<Vector2>() : null);
		List<int> list6 = new List<int>();
		foreach (CuttableSubMesh subMesh in subMeshes)
		{
			list6.Add(list.Count);
			subMesh.AddTo(list, list2, list3, list4, list5);
		}
		mesh.vertices = list.ToArray();
		mesh.normals = list2.ToArray();
		mesh.colors32 = (hasColours ? list3.ToArray() : null);
		mesh.uv = (hasUvs ? list4.ToArray() : null);
		mesh.uv2 = (hasUv1s ? list5.ToArray() : null);
		mesh.subMeshCount = subMeshes.Count;
		for (int j = 0; j < subMeshes.Count; j++)
		{
			CuttableSubMesh cuttableSubMesh = subMeshes[j];
			int baseVertex = list6[j];
			int[] triangles = cuttableSubMesh.GenIndices();
			mesh.SetTriangles(triangles, j, calculateBounds: true, baseVertex);
		}
		return mesh;
	}
}
