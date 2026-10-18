using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[Serializable]
public sealed class SharedVertex : ICollection<int>, IEnumerable<int>, IEnumerable
{
	[SerializeField]
	[FormerlySerializedAs("array")]
	[FormerlySerializedAs("m_Vertexes")]
	private int[] m_Vertices;

	internal int[] arrayInternal => m_Vertices;

	public int this[int i]
	{
		get
		{
			return m_Vertices[i];
		}
		set
		{
			m_Vertices[i] = value;
		}
	}

	public int Count => m_Vertices.Length;

	public bool IsReadOnly => m_Vertices.IsReadOnly;

	public SharedVertex(IEnumerable<int> indexes)
	{
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		m_Vertices = indexes.ToArray();
	}

	public SharedVertex(SharedVertex sharedVertex)
	{
		if (sharedVertex == null)
		{
			throw new ArgumentNullException("sharedVertex");
		}
		m_Vertices = new int[sharedVertex.Count];
		Array.Copy(sharedVertex.m_Vertices, m_Vertices, m_Vertices.Length);
	}

	public IEnumerator<int> GetEnumerator()
	{
		return ((IEnumerable<int>)m_Vertices).GetEnumerator();
	}

	public override string ToString()
	{
		return m_Vertices.ToString(",");
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(int item)
	{
		m_Vertices = m_Vertices.Add(item);
	}

	public void Clear()
	{
		m_Vertices = new int[0];
	}

	public bool Contains(int item)
	{
		return Array.IndexOf(m_Vertices, item) > -1;
	}

	public void CopyTo(int[] array, int arrayIndex)
	{
		m_Vertices.CopyTo(array, arrayIndex);
	}

	public bool Remove(int item)
	{
		if (Array.IndexOf(m_Vertices, item) < 0)
		{
			return false;
		}
		m_Vertices = m_Vertices.RemoveAt(item);
		return true;
	}

	public static void GetSharedVertexLookup(IList<SharedVertex> sharedVertices, Dictionary<int, int> lookup)
	{
		lookup.Clear();
		int i = 0;
		for (int count = sharedVertices.Count; i < count; i++)
		{
			foreach (int item in sharedVertices[i])
			{
				if (!lookup.ContainsKey(item))
				{
					lookup.Add(item, i);
				}
			}
		}
	}

	internal void ShiftIndexes(int offset)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			m_Vertices[i] += offset;
		}
	}

	internal static SharedVertex[] ToSharedVertices(IEnumerable<KeyValuePair<int, int>> lookup)
	{
		if (lookup == null)
		{
			return new SharedVertex[0];
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		List<List<int>> list = new List<List<int>>();
		foreach (KeyValuePair<int, int> item in lookup)
		{
			if (item.Value < 0)
			{
				list.Add(new List<int> { item.Key });
				continue;
			}
			int value = -1;
			if (dictionary.TryGetValue(item.Value, out value))
			{
				list[value].Add(item.Key);
				continue;
			}
			dictionary.Add(item.Value, list.Count);
			list.Add(new List<int> { item.Key });
		}
		return ToSharedVertices(list);
	}

	private static SharedVertex[] ToSharedVertices(List<List<int>> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		SharedVertex[] array = new SharedVertex[list.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new SharedVertex(list[i]);
		}
		return array;
	}

	public static SharedVertex[] GetSharedVerticesWithPositions(IList<Vector3> positions)
	{
		if (positions == null)
		{
			throw new ArgumentNullException("positions");
		}
		Dictionary<IntVec3, List<int>> dictionary = new Dictionary<IntVec3, List<int>>();
		for (int i = 0; i < positions.Count; i++)
		{
			if (dictionary.TryGetValue(positions[i], out var value))
			{
				value.Add(i);
				continue;
			}
			dictionary.Add(new IntVec3(positions[i]), new List<int> { i });
		}
		SharedVertex[] array = new SharedVertex[dictionary.Count];
		int num = 0;
		foreach (KeyValuePair<IntVec3, List<int>> item in dictionary)
		{
			array[num++] = new SharedVertex(item.Value.ToArray());
		}
		return array;
	}

	internal static SharedVertex[] RemoveAndShift(Dictionary<int, int> lookup, IEnumerable<int> remove)
	{
		List<int> list = new List<int>(remove);
		list.Sort();
		return SortedRemoveAndShift(lookup, list);
	}

	internal static SharedVertex[] SortedRemoveAndShift(Dictionary<int, int> lookup, List<int> remove)
	{
		foreach (int item in remove)
		{
			lookup[item] = -1;
		}
		SharedVertex[] array = ToSharedVertices(lookup.Where((KeyValuePair<int, int> x) => x.Value > -1));
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			int j = 0;
			for (int count = array[i].Count; j < count; j++)
			{
				int num2 = ArrayUtility.NearestIndexPriorToValue(remove, array[i][j]);
				array[i][j] -= num2 + 1;
			}
		}
		return array;
	}

	internal static void SetCoincident(ref Dictionary<int, int> lookup, IEnumerable<int> vertices)
	{
		int count = lookup.Count;
		foreach (int vertex in vertices)
		{
			lookup[vertex] = count;
		}
	}
}
