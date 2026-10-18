using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

internal sealed class FaceRebuildData
{
	public Face face;

	public List<Vertex> vertices;

	public List<int> sharedIndexes;

	public List<int> sharedIndexesUV;

	private int _appliedOffset;

	public int Offset()
	{
		return _appliedOffset;
	}

	public override string ToString()
	{
		return $"{ArrayUtility.ToString(vertices)}\n{ArrayUtility.ToString(sharedIndexes)}";
	}

	public static void Apply(IEnumerable<FaceRebuildData> newFaces, ProBuilderMesh mesh, List<Vertex> vertices = null, List<Face> faces = null)
	{
		if (faces == null)
		{
			faces = new List<Face>(mesh.facesInternal);
		}
		if (vertices == null)
		{
			vertices = new List<Vertex>(mesh.GetVertices());
		}
		Dictionary<int, int> sharedVertexLookup = mesh.sharedVertexLookup;
		Dictionary<int, int> sharedTextureLookup = mesh.sharedTextureLookup;
		Apply(newFaces, vertices, faces, sharedVertexLookup, sharedTextureLookup);
		mesh.SetVertices(vertices);
		mesh.faces = faces;
		mesh.SetSharedVertices(sharedVertexLookup);
		mesh.SetSharedTextures(sharedTextureLookup);
	}

	public static void Apply(IEnumerable<FaceRebuildData> newFaces, List<Vertex> vertices, List<Face> faces, Dictionary<int, int> sharedVertexLookup, Dictionary<int, int> sharedTextureLookup = null)
	{
		int num = vertices.Count;
		foreach (FaceRebuildData newFace in newFaces)
		{
			Face face = newFace.face;
			int count = newFace.vertices.Count;
			bool flag = sharedVertexLookup != null && newFace.sharedIndexes != null && newFace.sharedIndexes.Count == count;
			bool flag2 = sharedTextureLookup != null && newFace.sharedIndexesUV != null && newFace.sharedIndexesUV.Count == count;
			for (int i = 0; i < count; i++)
			{
				int num2 = i;
				sharedVertexLookup?.Add(num2 + num, flag ? newFace.sharedIndexes[num2] : (-1));
				if (sharedTextureLookup != null && flag2)
				{
					sharedTextureLookup.Add(num2 + num, newFace.sharedIndexesUV[num2]);
				}
			}
			newFace._appliedOffset = num;
			int[] indexesInternal = face.indexesInternal;
			int j = 0;
			for (int num3 = indexesInternal.Length; j < num3; j++)
			{
				indexesInternal[j] += num;
			}
			num += newFace.vertices.Count;
			face.indexesInternal = indexesInternal;
			faces.Add(face);
			vertices.AddRange(newFace.vertices);
		}
	}
}
