using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityEngine.ProBuilder.MeshOperations;

public static class DeleteElements
{
	public static void DeleteVertices(this ProBuilderMesh mesh, IEnumerable<int> distinctIndexes)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (distinctIndexes == null || !distinctIndexes.Any())
		{
			return;
		}
		Vertex[] vertices = mesh.GetVertices();
		int num = vertices.Length;
		int[] offset = new int[num];
		List<int> sorted = new List<int>(distinctIndexes);
		sorted.Sort();
		vertices = vertices.SortedRemoveAt(sorted);
		for (int i = 0; i < num; i++)
		{
			offset[i] = ArrayUtility.NearestIndexPriorToValue(sorted, i) + 1;
		}
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			int[] indexesInternal = face.indexesInternal;
			for (int k = 0; k < indexesInternal.Length; k++)
			{
				indexesInternal[k] -= offset[indexesInternal[k]];
			}
			face.InvalidateCache();
		}
		IEnumerable<KeyValuePair<int, int>> sharedVertices = from x in mesh.sharedVertexLookup
			where sorted.BinarySearch(x.Key) < 0
			select x into y
			select new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value);
		IEnumerable<KeyValuePair<int, int>> sharedTextures = from x in mesh.sharedTextureLookup
			where sorted.BinarySearch(x.Key) < 0
			select x into y
			select new KeyValuePair<int, int>(y.Key - offset[y.Key], y.Value);
		mesh.SetVertices(vertices);
		mesh.SetSharedVertices(sharedVertices);
		mesh.SetSharedTextures(sharedTextures);
	}

	public static int[] DeleteFace(this ProBuilderMesh mesh, Face face)
	{
		return mesh.DeleteFaces(new Face[1] { face });
	}

	public static int[] DeleteFaces(this ProBuilderMesh mesh, IEnumerable<Face> faces)
	{
		return mesh.DeleteFaces(faces.Select((Face x) => Array.IndexOf(mesh.facesInternal, x)).ToList());
	}

	public static int[] DeleteFaces(this ProBuilderMesh mesh, IList<int> faceIndexes)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (faceIndexes == null)
		{
			throw new ArgumentNullException("faceIndexes");
		}
		Face[] array = new Face[faceIndexes.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = mesh.facesInternal[faceIndexes[i]];
		}
		List<int> list = array.SelectMany((Face x) => x.distinctIndexesInternal).Distinct().ToList();
		list.Sort();
		int num = mesh.positionsInternal.Length;
		Face[] array2 = mesh.facesInternal.RemoveAt(faceIndexes);
		Vertex[] vertices = mesh.GetVertices().SortedRemoveAt(list);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int j = 0; j < num; j++)
		{
			dictionary.Add(j, ArrayUtility.NearestIndexPriorToValue(list, j) + 1);
		}
		for (int k = 0; k < array2.Length; k++)
		{
			int[] indexesInternal = array2[k].indexesInternal;
			for (int l = 0; l < indexesInternal.Length; l++)
			{
				indexesInternal[l] -= dictionary[indexesInternal[l]];
			}
			array2[k].indexesInternal = indexesInternal;
		}
		mesh.SetVertices(vertices);
		mesh.sharedVerticesInternal = SharedVertex.SortedRemoveAndShift(mesh.sharedVertexLookup, list);
		mesh.sharedTextures = SharedVertex.SortedRemoveAndShift(mesh.sharedTextureLookup, list);
		mesh.facesInternal = array2;
		return list.ToArray();
	}

	[Obsolete("Use MeshValidation.RemoveDegenerateTriangles")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static int[] RemoveDegenerateTriangles(this ProBuilderMesh mesh)
	{
		List<int> list = new List<int>();
		MeshValidation.RemoveDegenerateTriangles(mesh, list);
		return list.ToArray();
	}

	[Obsolete("Use MeshValidation.RemoveUnusedVertices")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static int[] RemoveUnusedVertices(this ProBuilderMesh mesh)
	{
		List<int> list = new List<int>();
		MeshValidation.RemoveUnusedVertices(mesh, list);
		return list.ToArray();
	}
}
