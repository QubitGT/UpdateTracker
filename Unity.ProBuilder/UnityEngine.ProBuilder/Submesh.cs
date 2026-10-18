using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityEngine.ProBuilder;

[Serializable]
public sealed class Submesh
{
	[SerializeField]
	internal int[] m_Indexes;

	[SerializeField]
	internal MeshTopology m_Topology;

	[SerializeField]
	internal int m_SubmeshIndex;

	public IEnumerable<int> indexes
	{
		get
		{
			return new ReadOnlyCollection<int>(m_Indexes);
		}
		set
		{
			m_Indexes = value.ToArray();
		}
	}

	public MeshTopology topology
	{
		get
		{
			return m_Topology;
		}
		set
		{
			m_Topology = value;
		}
	}

	public int submeshIndex
	{
		get
		{
			return m_SubmeshIndex;
		}
		set
		{
			m_SubmeshIndex = value;
		}
	}

	public Submesh(int submeshIndex, MeshTopology topology, IEnumerable<int> indexes)
	{
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		m_Indexes = indexes.ToArray();
		m_Topology = topology;
		m_SubmeshIndex = submeshIndex;
	}

	public Submesh(Mesh mesh, int subMeshIndex)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		m_Indexes = mesh.GetIndices(subMeshIndex);
		m_Topology = mesh.GetTopology(subMeshIndex);
		m_SubmeshIndex = subMeshIndex;
	}

	public override string ToString()
	{
		return string.Format("{0}, {1}, {2}", m_SubmeshIndex, m_Topology.ToString(), (m_Indexes != null) ? m_Indexes.Length.ToString() : "0");
	}

	internal static int GetSubmeshCount(ProBuilderMesh mesh)
	{
		int num = 0;
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			num = Mathf.Max(num, face.submeshIndex);
		}
		return num + 1;
	}

	public static Submesh[] GetSubmeshes(IEnumerable<Face> faces, int submeshCount, MeshTopology preferredTopology = MeshTopology.Triangles)
	{
		if (preferredTopology != 0 && preferredTopology != MeshTopology.Quads)
		{
			throw new NotImplementedException("Currently only Quads and Triangles are supported.");
		}
		if (faces == null)
		{
			throw new ArgumentNullException("faces");
		}
		bool flag = preferredTopology == MeshTopology.Quads;
		List<int>[] array = (flag ? new List<int>[submeshCount] : null);
		List<int>[] array2 = new List<int>[submeshCount];
		int upperBound = submeshCount - 1;
		int num = -1;
		for (int i = 0; i < submeshCount; i++)
		{
			if (flag)
			{
				array[i] = new List<int>();
			}
			array2[i] = new List<int>();
		}
		foreach (Face face in faces)
		{
			if (face.indexesInternal != null && face.indexesInternal.Length >= 1)
			{
				int num2 = Math.Clamp(face.submeshIndex, 0, upperBound);
				num = Mathf.Max(num2, num);
				if (flag && face.IsQuad())
				{
					array[num2].AddRange(face.ToQuad());
				}
				else
				{
					array2[num2].AddRange(face.indexesInternal);
				}
			}
		}
		submeshCount = num + 1;
		Submesh[] array3 = new Submesh[submeshCount];
		switch (preferredTopology)
		{
		case MeshTopology.Triangles:
		{
			for (int l = 0; l < submeshCount; l++)
			{
				array3[l] = new Submesh(l, MeshTopology.Triangles, array2[l]);
			}
			break;
		}
		case MeshTopology.Quads:
		{
			for (int j = 0; j < submeshCount; j++)
			{
				if (array2[j].Count > 0)
				{
					List<int> list = array2[j];
					List<int> list2 = array[j];
					int count = list.Count;
					int count2 = list2.Count;
					int[] array4 = new int[count + count2 / 4 * 6];
					for (int k = 0; k < count; k++)
					{
						array4[k] = list[k];
					}
					int num3 = 0;
					int num4 = count;
					while (num3 < count2)
					{
						array4[num4] = list2[num3];
						array4[num4 + 1] = list2[num3 + 1];
						array4[num4 + 2] = list2[num3 + 2];
						array4[num4 + 3] = list2[num3 + 2];
						array4[num4 + 4] = list2[num3 + 3];
						array4[num4 + 5] = list2[num3];
						num3 += 4;
						num4 += 6;
					}
					array3[j] = new Submesh(j, MeshTopology.Triangles, array4);
				}
				else
				{
					array3[j] = new Submesh(j, MeshTopology.Quads, array[j]);
				}
			}
			break;
		}
		}
		return array3;
	}

	internal static void MapFaceMaterialsToSubmeshIndex(ProBuilderMesh mesh)
	{
		Material[] sharedMaterials = mesh.renderer.sharedMaterials;
		int num = sharedMaterials.Length;
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			if (!(face.material == null))
			{
				int value = Array.IndexOf(sharedMaterials, face.material);
				face.submeshIndex = Math.Clamp(value, 0, num - 1);
				face.material = null;
			}
		}
	}
}
