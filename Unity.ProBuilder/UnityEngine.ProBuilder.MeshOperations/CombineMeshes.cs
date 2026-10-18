using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class CombineMeshes
{
	[Obsolete("Combine(IEnumerable<ProBuilderMesh> meshes) is deprecated. Plase use Combine(IEnumerable<ProBuilderMesh> meshes, ProBuilderMesh meshTarget).")]
	public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes)
	{
		return CombineToNewMeshes(meshes);
	}

	public static List<ProBuilderMesh> Combine(IEnumerable<ProBuilderMesh> meshes, ProBuilderMesh meshTarget)
	{
		if (meshes == null)
		{
			throw new ArgumentNullException("meshes");
		}
		if (meshTarget == null)
		{
			throw new ArgumentNullException("meshTarget");
		}
		if (!meshes.Any() || meshes.Count() < 2)
		{
			return null;
		}
		if (!meshes.Contains(meshTarget))
		{
			return null;
		}
		List<Vertex> vertices = new List<Vertex>(meshTarget.GetVertices());
		List<Face> faces = new List<Face>(meshTarget.facesInternal);
		List<SharedVertex> sharedVertices = new List<SharedVertex>(meshTarget.sharedVertices);
		List<SharedVertex> sharedTextures = new List<SharedVertex>(meshTarget.sharedTextures);
		int vertexCount = meshTarget.vertexCount;
		List<Material> materialMap = new List<Material>(meshTarget.renderer.sharedMaterials);
		Transform transform = meshTarget.transform;
		List<ProBuilderMesh> list = new List<ProBuilderMesh>();
		List<ProBuilderMesh> list2 = new List<ProBuilderMesh>();
		int num = vertexCount;
		foreach (ProBuilderMesh mesh in meshes)
		{
			if (mesh != meshTarget)
			{
				if ((long)(num + mesh.vertexCount) < 65535L)
				{
					num += mesh.vertexCount;
					list.Add(mesh);
				}
				else
				{
					list2.Add(mesh);
				}
			}
		}
		List<Face> autoUvFaces = new List<Face>();
		AccumulateMeshesInfo(list, vertexCount, ref vertices, ref faces, ref autoUvFaces, ref sharedVertices, ref sharedTextures, ref materialMap, transform);
		meshTarget.SetVertices(vertices);
		meshTarget.faces = faces;
		meshTarget.sharedVertices = sharedVertices;
		meshTarget.sharedTextures = sharedTextures?.ToArray();
		meshTarget.renderer.sharedMaterials = materialMap.ToArray();
		meshTarget.ToMesh();
		meshTarget.Refresh();
		UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(meshTarget, autoUvFaces);
		MeshValidation.EnsureMeshIsValid(meshTarget, out var removedVertices);
		List<ProBuilderMesh> list3 = new List<ProBuilderMesh> { meshTarget };
		if (list2.Count > 1)
		{
			foreach (ProBuilderMesh item in CombineToNewMeshes(list2))
			{
				MeshValidation.EnsureMeshIsValid(item, out removedVertices);
				list3.Add(item);
			}
		}
		else if (list2.Count == 1)
		{
			list3.Add(list2[0]);
		}
		return list3;
	}

	private static List<ProBuilderMesh> CombineToNewMeshes(IEnumerable<ProBuilderMesh> meshes)
	{
		if (meshes == null)
		{
			throw new ArgumentNullException("meshes");
		}
		if (!meshes.Any() || meshes.Count() < 2)
		{
			return null;
		}
		List<Vertex> vertices = new List<Vertex>();
		List<Face> faces = new List<Face>();
		List<Face> autoUvFaces = new List<Face>();
		List<SharedVertex> sharedVertices = new List<SharedVertex>();
		List<SharedVertex> sharedTextures = new List<SharedVertex>();
		int offset = 0;
		List<Material> materialMap = new List<Material>();
		AccumulateMeshesInfo(meshes, offset, ref vertices, ref faces, ref autoUvFaces, ref sharedVertices, ref sharedTextures, ref materialMap);
		List<ProBuilderMesh> list = SplitByMaxVertexCount(vertices, faces, sharedVertices, sharedTextures);
		Vector3 position = meshes.LastOrDefault().transform.position;
		foreach (ProBuilderMesh item in list)
		{
			item.renderer.sharedMaterials = materialMap.ToArray();
			InternalMeshUtility.FilterUnusedSubmeshIndexes(item);
			item.SetPivot(position);
			UvUnwrapping.SetAutoAndAlignUnwrapParamsToUVs(item, autoUvFaces);
		}
		return list;
	}

	private static void AccumulateMeshesInfo(IEnumerable<ProBuilderMesh> meshes, int offset, ref List<Vertex> vertices, ref List<Face> faces, ref List<Face> autoUvFaces, ref List<SharedVertex> sharedVertices, ref List<SharedVertex> sharedTextures, ref List<Material> materialMap, Transform targetTransform = null)
	{
		foreach (ProBuilderMesh mesh in meshes)
		{
			int vertexCount = mesh.vertexCount;
			Transform transform = mesh.transform;
			Vertex[] vertices2 = mesh.GetVertices();
			Face[] facesInternal = mesh.facesInternal;
			IList<SharedVertex> sharedVertices2 = mesh.sharedVertices;
			SharedVertex[] sharedTextures2 = mesh.sharedTextures;
			Material[] sharedMaterials = mesh.renderer.sharedMaterials;
			int num = sharedMaterials.Length;
			for (int i = 0; i < vertexCount; i++)
			{
				Vertex vertex = transform.TransformVertex(vertices2[i]);
				if (targetTransform != null)
				{
					vertices.Add(targetTransform.InverseTransformVertex(vertex));
				}
				else
				{
					vertices.Add(vertex);
				}
			}
			Face[] array = facesInternal;
			foreach (Face face in array)
			{
				Face face2 = new Face(face);
				face2.ShiftIndexes(offset);
				if (!face2.manualUV && !face2.uv.useWorldSpace)
				{
					face2.manualUV = true;
					autoUvFaces.Add(face2);
				}
				Material material = ((num > 0) ? sharedMaterials[Math.Clamp(face.submeshIndex, 0, num - 1)] : null);
				int num2 = materialMap.IndexOf(material);
				if (num2 > -1)
				{
					face2.submeshIndex = num2;
				}
				else if (material == null)
				{
					face2.submeshIndex = 0;
				}
				else
				{
					face2.submeshIndex = materialMap.Count;
					materialMap.Add(material);
				}
				faces.Add(face2);
			}
			foreach (SharedVertex item in sharedVertices2)
			{
				SharedVertex sharedVertex = new SharedVertex(item);
				sharedVertex.ShiftIndexes(offset);
				sharedVertices.Add(sharedVertex);
			}
			SharedVertex[] array2 = sharedTextures2;
			for (int j = 0; j < array2.Length; j++)
			{
				SharedVertex sharedVertex2 = new SharedVertex(array2[j]);
				sharedVertex2.ShiftIndexes(offset);
				sharedTextures.Add(sharedVertex2);
			}
			offset += vertexCount;
		}
	}

	private static ProBuilderMesh CreateMeshFromSplit(List<Vertex> vertices, List<Face> faces, Dictionary<int, int> sharedVertexLookup, Dictionary<int, int> sharedTextureLookup, Dictionary<int, int> remap, Material[] materials)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		foreach (Face face in faces)
		{
			int i = 0;
			for (int num = face.indexesInternal.Length; i < num; i++)
			{
				face.indexesInternal[i] = remap[face.indexesInternal[i]];
			}
			face.InvalidateCache();
		}
		foreach (KeyValuePair<int, int> item in remap)
		{
			if (sharedVertexLookup.TryGetValue(item.Key, out var value))
			{
				dictionary.Add(item.Value, value);
			}
			if (sharedTextureLookup.TryGetValue(item.Key, out value))
			{
				dictionary2.Add(item.Value, value);
			}
		}
		return ProBuilderMesh.Create(vertices, faces, SharedVertex.ToSharedVertices(dictionary), (dictionary2.Count > 0) ? SharedVertex.ToSharedVertices(dictionary2) : null, materials);
	}

	internal static List<ProBuilderMesh> SplitByMaxVertexCount(IList<Vertex> vertices, IList<Face> faces, IList<SharedVertex> sharedVertices, IList<SharedVertex> sharedTextures, uint maxVertexCount = 65535u)
	{
		uint count = (uint)vertices.Count;
		uint num = System.Math.Max(1u, count / maxVertexCount);
		int num2 = faces.Max((Face x) => x.submeshIndex) + 1;
		if (num < 2)
		{
			return new List<ProBuilderMesh> { ProBuilderMesh.Create(vertices, faces, sharedVertices, sharedTextures, new Material[num2]) };
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		SharedVertex.GetSharedVertexLookup(sharedVertices, dictionary);
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		SharedVertex.GetSharedVertexLookup(sharedTextures, dictionary2);
		List<ProBuilderMesh> list = new List<ProBuilderMesh>();
		List<Vertex> list2 = new List<Vertex>();
		List<Face> list3 = new List<Face>();
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
		foreach (Face face in faces)
		{
			if (list2.Count + face.distinctIndexes.Count > maxVertexCount)
			{
				list.Add(CreateMeshFromSplit(list2, list3, dictionary, dictionary2, dictionary3, new Material[num2]));
				list2.Clear();
				list3.Clear();
				dictionary3.Clear();
			}
			foreach (int distinctIndex in face.distinctIndexes)
			{
				list2.Add(vertices[distinctIndex]);
				dictionary3.Add(distinctIndex, list2.Count - 1);
			}
			list3.Add(face);
		}
		if (list2.Count > 0)
		{
			list.Add(CreateMeshFromSplit(list2, list3, dictionary, dictionary2, dictionary3, new Material[num2]));
		}
		return list;
	}
}
