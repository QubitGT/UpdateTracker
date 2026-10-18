using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UnityEngine.Rendering.Universal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct EdgeDictionary : IEdgeStore
{
	private class EdgeComparer : IEqualityComparer<ShadowEdge>
	{
		public bool Equals(ShadowEdge edge0, ShadowEdge edge1)
		{
			if (edge0.v0 != edge1.v0 || edge0.v1 != edge1.v1)
			{
				if (edge0.v1 == edge1.v0)
				{
					return edge0.v0 == edge1.v1;
				}
				return false;
			}
			return true;
		}

		public int GetHashCode(ShadowEdge edge)
		{
			int num = edge.v0;
			int num2 = edge.v1;
			if (edge.v1 < edge.v0)
			{
				num = edge.v1;
				num2 = edge.v0;
			}
			return ((num << 15) | num2).GetHashCode();
		}
	}

	private static Dictionary<ShadowEdge, int> m_EdgeDictionary = new Dictionary<ShadowEdge, int>(new EdgeComparer());

	public NativeArray<ShadowEdge> GetOutsideEdges(NativeArray<Vector3> vertices, NativeArray<int> indices)
	{
		m_EdgeDictionary.Clear();
		m_EdgeDictionary.EnsureCapacity(indices.Length);
		for (int i = 0; i < indices.Length; i += 3)
		{
			int num = indices[i];
			int num2 = indices[i + 1];
			int num3 = indices[i + 2];
			ShadowEdge key = new ShadowEdge(num, num2);
			ShadowEdge key2 = new ShadowEdge(num2, num3);
			ShadowEdge key3 = new ShadowEdge(num3, num);
			if (m_EdgeDictionary.ContainsKey(key))
			{
				m_EdgeDictionary[key] += 1;
			}
			else
			{
				m_EdgeDictionary.Add(key, 1);
			}
			if (m_EdgeDictionary.ContainsKey(key2))
			{
				m_EdgeDictionary[key2] += 1;
			}
			else
			{
				m_EdgeDictionary.Add(key2, 1);
			}
			if (m_EdgeDictionary.ContainsKey(key3))
			{
				m_EdgeDictionary[key3] += 1;
			}
			else
			{
				m_EdgeDictionary.Add(key3, 1);
			}
		}
		int num4 = 0;
		foreach (KeyValuePair<ShadowEdge, int> item in m_EdgeDictionary)
		{
			if (item.Value == 1)
			{
				num4++;
			}
		}
		int num5 = 0;
		NativeArray<ShadowEdge> result = new NativeArray<ShadowEdge>(num4, Allocator.Temp);
		foreach (KeyValuePair<ShadowEdge, int> item2 in m_EdgeDictionary)
		{
			if (item2.Value == 1)
			{
				result[num5++] = item2.Key;
			}
		}
		return result;
	}
}
