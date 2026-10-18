using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[Serializable]
public sealed class Face
{
	[FormerlySerializedAs("_indices")]
	[SerializeField]
	private int[] m_Indexes;

	[SerializeField]
	[FormerlySerializedAs("_smoothingGroup")]
	private int m_SmoothingGroup;

	[SerializeField]
	[FormerlySerializedAs("_uv")]
	private AutoUnwrapSettings m_Uv;

	[SerializeField]
	[FormerlySerializedAs("_mat")]
	private Material m_Material;

	[SerializeField]
	private int m_SubmeshIndex;

	[SerializeField]
	[FormerlySerializedAs("manualUV")]
	private bool m_ManualUV;

	[SerializeField]
	internal int elementGroup;

	[SerializeField]
	private int m_TextureGroup;

	[NonSerialized]
	private int[] m_DistinctIndexes;

	[NonSerialized]
	private Edge[] m_Edges;

	public bool manualUV
	{
		get
		{
			return m_ManualUV;
		}
		set
		{
			m_ManualUV = value;
		}
	}

	public int textureGroup
	{
		get
		{
			return m_TextureGroup;
		}
		set
		{
			m_TextureGroup = value;
		}
	}

	internal int[] indexesInternal
	{
		get
		{
			return m_Indexes;
		}
		set
		{
			if (m_Indexes == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_Indexes.Length % 3 != 0)
			{
				throw new ArgumentException("Face indexes must be a multiple of 3.");
			}
			m_Indexes = value;
			InvalidateCache();
		}
	}

	public ReadOnlyCollection<int> indexes => new ReadOnlyCollection<int>(m_Indexes);

	internal int[] distinctIndexesInternal
	{
		get
		{
			if (m_DistinctIndexes != null)
			{
				return m_DistinctIndexes;
			}
			return CacheDistinctIndexes();
		}
	}

	public ReadOnlyCollection<int> distinctIndexes => new ReadOnlyCollection<int>(distinctIndexesInternal);

	internal Edge[] edgesInternal
	{
		get
		{
			if (m_Edges != null)
			{
				return m_Edges;
			}
			return CacheEdges();
		}
	}

	public ReadOnlyCollection<Edge> edges => new ReadOnlyCollection<Edge>(edgesInternal);

	public int smoothingGroup
	{
		get
		{
			return m_SmoothingGroup;
		}
		set
		{
			m_SmoothingGroup = value;
		}
	}

	[Obsolete("Face.material is deprecated. Please use submeshIndex instead.")]
	public Material material
	{
		get
		{
			return m_Material;
		}
		set
		{
			m_Material = value;
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

	public AutoUnwrapSettings uv
	{
		get
		{
			return m_Uv;
		}
		set
		{
			m_Uv = value;
		}
	}

	public int this[int i] => indexesInternal[i];

	public void SetIndexes(IEnumerable<int> indices)
	{
		if (indices == null)
		{
			throw new ArgumentNullException("indices");
		}
		int[] array = indices.ToArray();
		if (array.Length % 3 != 0)
		{
			throw new ArgumentException("Face indexes must be a multiple of 3.");
		}
		m_Indexes = array;
		InvalidateCache();
	}

	public Face()
	{
		m_SubmeshIndex = 0;
	}

	public Face(IEnumerable<int> indices)
	{
		SetIndexes(indices);
		m_Uv = AutoUnwrapSettings.tile;
		m_Material = BuiltinMaterials.defaultMaterial;
		m_SmoothingGroup = 0;
		m_SubmeshIndex = 0;
		textureGroup = -1;
		elementGroup = 0;
	}

	[Obsolete("Face.material is deprecated. Please use \"submeshIndex\" instead.")]
	internal Face(int[] triangles, Material m, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
	{
		SetIndexes(triangles);
		m_Uv = new AutoUnwrapSettings(u);
		m_Material = m;
		m_SmoothingGroup = smoothing;
		textureGroup = texture;
		elementGroup = element;
		manualUV = manualUVs;
		m_SubmeshIndex = 0;
	}

	internal Face(IEnumerable<int> triangles, int submeshIndex, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
	{
		SetIndexes(triangles);
		m_Uv = new AutoUnwrapSettings(u);
		m_SmoothingGroup = smoothing;
		textureGroup = texture;
		elementGroup = element;
		manualUV = manualUVs;
		m_SubmeshIndex = submeshIndex;
	}

	public Face(Face other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(Face other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		int num = other.indexesInternal.Length;
		m_Indexes = new int[num];
		Array.Copy(other.indexesInternal, m_Indexes, num);
		m_SmoothingGroup = other.smoothingGroup;
		m_Uv = new AutoUnwrapSettings(other.uv);
		m_Material = other.material;
		manualUV = other.manualUV;
		m_TextureGroup = other.textureGroup;
		elementGroup = other.elementGroup;
		m_SubmeshIndex = other.m_SubmeshIndex;
		InvalidateCache();
	}

	internal void InvalidateCache()
	{
		m_Edges = null;
		m_DistinctIndexes = null;
	}

	private Edge[] CacheEdges()
	{
		if (m_Indexes == null)
		{
			return null;
		}
		HashSet<Edge> hashSet = new HashSet<Edge>();
		List<Edge> list = new List<Edge>();
		for (int i = 0; i < indexesInternal.Length; i += 3)
		{
			Edge item = new Edge(indexesInternal[i], indexesInternal[i + 1]);
			Edge item2 = new Edge(indexesInternal[i + 1], indexesInternal[i + 2]);
			Edge item3 = new Edge(indexesInternal[i + 2], indexesInternal[i]);
			if (!hashSet.Add(item))
			{
				list.Add(item);
			}
			if (!hashSet.Add(item2))
			{
				list.Add(item2);
			}
			if (!hashSet.Add(item3))
			{
				list.Add(item3);
			}
		}
		hashSet.ExceptWith(list);
		m_Edges = hashSet.ToArray();
		return m_Edges;
	}

	private int[] CacheDistinctIndexes()
	{
		if (m_Indexes == null)
		{
			return null;
		}
		m_DistinctIndexes = m_Indexes.Distinct().ToArray();
		return distinctIndexesInternal;
	}

	public bool Contains(int a, int b, int c)
	{
		int i = 0;
		for (int num = indexesInternal.Length; i < num; i += 3)
		{
			if (a == indexesInternal[i] && b == indexesInternal[i + 1] && c == indexesInternal[i + 2])
			{
				return true;
			}
		}
		return false;
	}

	public bool IsQuad()
	{
		if (edgesInternal != null)
		{
			return edgesInternal.Length == 4;
		}
		return false;
	}

	public int[] ToQuad()
	{
		if (!IsQuad())
		{
			throw new InvalidOperationException("Face is not representable as a quad. Use Face.IsQuad to check for validity.");
		}
		int[] array = new int[4]
		{
			edgesInternal[0].a,
			edgesInternal[0].b,
			-1,
			-1
		};
		if (edgesInternal[1].a == array[1])
		{
			array[2] = edgesInternal[1].b;
		}
		else if (edgesInternal[2].a == array[1])
		{
			array[2] = edgesInternal[2].b;
		}
		else if (edgesInternal[3].a == array[1])
		{
			array[2] = edgesInternal[3].b;
		}
		if (edgesInternal[1].a == array[2])
		{
			array[3] = edgesInternal[1].b;
		}
		else if (edgesInternal[2].a == array[2])
		{
			array[3] = edgesInternal[2].b;
		}
		else if (edgesInternal[3].a == array[2])
		{
			array[3] = edgesInternal[3].b;
		}
		return array;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < indexesInternal.Length; i += 3)
		{
			stringBuilder.Append("[");
			stringBuilder.Append(indexesInternal[i]);
			stringBuilder.Append(", ");
			stringBuilder.Append(indexesInternal[i + 1]);
			stringBuilder.Append(", ");
			stringBuilder.Append(indexesInternal[i + 2]);
			stringBuilder.Append("]");
			if (i < indexesInternal.Length - 3)
			{
				stringBuilder.Append(", ");
			}
		}
		return stringBuilder.ToString();
	}

	public void ShiftIndexes(int offset)
	{
		int i = 0;
		for (int num = m_Indexes.Length; i < num; i++)
		{
			m_Indexes[i] += offset;
		}
		InvalidateCache();
	}

	private int SmallestIndexValue()
	{
		int num = m_Indexes[0];
		for (int i = 1; i < m_Indexes.Length; i++)
		{
			if (m_Indexes[i] < num)
			{
				num = m_Indexes[i];
			}
		}
		return num;
	}

	public void ShiftIndexesToZero()
	{
		int num = SmallestIndexValue();
		for (int i = 0; i < m_Indexes.Length; i++)
		{
			m_Indexes[i] -= num;
		}
		InvalidateCache();
	}

	public void Reverse()
	{
		Array.Reverse(m_Indexes);
		InvalidateCache();
	}

	internal static void GetIndices(IEnumerable<Face> faces, List<int> indices)
	{
		indices.Clear();
		foreach (Face face in faces)
		{
			int i = 0;
			for (int num = face.indexesInternal.Length; i < num; i++)
			{
				indices.Add(face.indexesInternal[i]);
			}
		}
	}

	internal static void GetDistinctIndices(IEnumerable<Face> faces, List<int> indices)
	{
		indices.Clear();
		foreach (Face face in faces)
		{
			int i = 0;
			for (int num = face.distinctIndexesInternal.Length; i < num; i++)
			{
				indices.Add(face.distinctIndexesInternal[i]);
			}
		}
	}

	internal bool TryGetNextEdge(Edge source, int index, ref Edge nextEdge, ref int nextIndex)
	{
		int i = 0;
		for (int num = edgesInternal.Length; i < num; i++)
		{
			if (!(edgesInternal[i] == source))
			{
				nextEdge = edgesInternal[i];
				if (nextEdge.Contains(index))
				{
					nextIndex = ((nextEdge.a == index) ? nextEdge.b : nextEdge.a);
					return true;
				}
			}
		}
		return false;
	}
}
